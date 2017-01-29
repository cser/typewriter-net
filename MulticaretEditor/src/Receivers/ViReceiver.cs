using System;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class ViReceiver : AReceiver
	{
		public override bool AltMode { get { return true; } }
		
		public override void DoOn()
		{
			for (int i = 0; i < lines.selections.Count; i++)
			{
				Selection selection = lines.selections[i];
				if (selection.Empty)
				{
					Place place = lines.PlaceOf(selection.caret);
					if (place.iChar > 0)
					{
						selection.anchor--;
						selection.caret--;
					}
				}
			}
		}
		
		private readonly ViCommandParser parser = new ViCommandParser();
		
		public override void DoKeyPress(char code)
		{
			Console.WriteLine("!!DoKeyPress(" + (short)code + ")");
			code = context.GetMapped(code);
			parser.AddKey(new ViChar(code, false));
			ProcessKey();
		}
		
		public override bool DoKeyDown(Keys keysData)
		{
			Console.WriteLine("!!DoKeyDown(" + keysData + "/" + (short)keysData + ")");
			switch (keysData)
			{
				case Keys.Left:
					parser.AddKey(new ViChar('h', false));
					break;
				case Keys.Right:
					parser.AddKey(new ViChar('l', false));
					break;
				case Keys.Down:
					parser.AddKey(new ViChar('j', false));
					break;
				case Keys.Up:
					parser.AddKey(new ViChar('k', false));
					break;
				case Keys.Control | Keys.R:
					parser.AddKey(new ViChar('r', true));
					break;
				default:
					return false;
			}
			ProcessKey();
			return true;
		}
		
		private void ProcessKey()
		{
			if (!parser.TryComplete())
			{
				return;
			}
			if (parser.action.c == 'i')
			{
				context.SetState(new InputReceiver());
				return;
			}
			if (parser.action.c == 'a')
			{
				controller.MoveRight(false);
				context.SetState(new InputReceiver());
				return;
			}
			ViMove move = null;
			if (!parser.move.control)
			{
				switch (parser.move.c)
				{
					case 'h':
						move = new ViMoveStep(Direction.Left);
						break;
					case 'l':
						move = new ViMoveStep(Direction.Right);
						break;
					case 'j':
						move = new ViMoveStep(Direction.Down);
						break;
					case 'k':
						move = new ViMoveStep(Direction.Up);
						break;
					case 'w':
						move = new ViMoveWord(Direction.Right);
						break;
					case 'b':
						move = new ViMoveWord(Direction.Left);
						break;
					case 'f':
						move = new ViFind(parser.moveChar.c);
						break;
				}
			}
			Console.WriteLine("ACTION: " + parser.action + " MOVE: " + parser.move + " - " + parser.moveChar);
			ViCommand command = null;
			if (move != null)
			{
				if (!parser.action.control)
				{
					switch (parser.action.c)
					{
						case 'd':
							command = new ViDelete(move);
							break;
						default:
							command = new ViEmpty(move, parser.count);
							break;
					}
				}
			}
			else
			{
				if (!parser.action.control)
				{
					switch (parser.action.c)
					{
						case 'u':
							command = new ViUndo();
							break;
					}
				}
				else
				{
					switch (parser.action.c)
					{
						case 'r':
							command = new ViRedo();
							break;
					}
				}
			}
			if (command != null && parser.count != 1)
			{
				command = new ViRepeat(command, parser.count);
			}
			if (command != null)
			{
				command.Execute(controller);
				controller.ViResetCommandsBatching();
			}
		}
		
		public abstract class ViCommand
		{
			public abstract void Execute(Controller controller);
		}
		
		public class ViRepeat : ViCommand
		{
			private ViCommand command;
			private int count;
			
			public ViRepeat(ViCommand command, int count)
			{
				this.command = command;
				this.count = count;
			}
			
			public override void Execute(Controller controller)
			{
				for (int i = 0; i < count; i++)
				{
					command.Execute(controller);
				}
			}
		}
		
		public class ViEmpty : ViCommand
		{
			private ViMove move;
			private int count;
			
			public ViEmpty(ViMove move, int count)
			{
				this.move = move;
				this.count = count;
			}
			
			public override void Execute(Controller controller)
			{
				move.Move(controller, false);
			}
		}
		
		public class ViDelete : ViCommand
		{
			private ViMove move;
			
			public ViDelete(ViMove move)
			{
				this.move = move;
			}
			
			public override void Execute(Controller controller)
			{
				move.Move(controller, true);
				controller.Cut();
			}
		}
		
		public class ViUndo : ViCommand
		{
			public override void Execute(Controller controller)
			{
				controller.Undo();
				controller.ViCollapseSelections();
			}
		}
		
		public class ViRedo : ViCommand
		{
			public override void Execute(Controller controller)
			{
				controller.Redo();
				controller.ViCollapseSelections();
			}
		}
		
		public abstract class ViMove
		{
			public abstract void Move(Controller controller, bool shift);
		}
		
		public class ViMoveStep : ViMove
		{
			private Direction direction;
			
			public ViMoveStep(Direction direction)
			{
				this.direction = direction;
			}
			
			public override void Move(Controller controller, bool shift)
			{
				switch (direction)
				{
					case Direction.Left:
						controller.MoveLeft(shift);
						break;
					case Direction.Right:
						controller.MoveRight(shift);
						break;
					case Direction.Up:
						controller.MoveUp(shift);
						break;
					case Direction.Down:
						controller.MoveDown(shift);
						break;
				}
			}
		}
		
		public class ViMoveWord : ViMove
		{
			private Direction direction;
			
			public ViMoveWord(Direction direction)
			{
				this.direction = direction;
			}
			
			public override void Move(Controller controller, bool shift)
			{
				switch (direction)
				{
					case Direction.Left:
						controller.MoveWordLeft(shift);
						break;
					case Direction.Right:
						controller.MoveWordRight(shift);
						break;
				}
			}
		}
		
		public class ViFind : ViMove
		{
			private char charToFind;
			
			public ViFind(char charToFind)
			{
				this.charToFind = charToFind;
			}
			
			public override void Move(Controller controller, bool shift)
			{
				controller.ViMoveToChar(charToFind, shift);
			}
		}
	}
	
	/*
	NORMAL
	COMMAND
	VISUAL
	LINE_VISUAL
	
	10w 
	10{move{WORD}}
	10W
	10{move{LONG_WORD}}
	10.
	10{replay}
	10i
	switch{INPUT}, wait_switch_out, (10-1){replay}
	i
	switch{INPUT}, wait_switch_out
	a
	move{RIGHT}, switch{INPUT}, wait_switch_out
	10dw
	10{delete{move{WORD}}}
	10diw
	10{delete{object{WORD}}}
	10v
	10{select{move{RIGHT}}}
	v
	switch{VISUAL}
	
	COMMAND
	10w
	Do(10, Move(WORD, false))
	10dw
	Do(10, And(Move(WORD, true), Delete()))
	10diw
	Do(10, And(Select(WORD), Delete()))
	10W
	Do(10, Move(BIG_WORD, false))
	10fa
	Do(10, Move(Find('a'), false))
	%
	Move(BRACKET, false)
	di%
	And(Select(BRACKET_INSIDE, true), Delete())
	da%
	And(Select(BRACKET_OUTSIDE, true), Delete())
	*/
}