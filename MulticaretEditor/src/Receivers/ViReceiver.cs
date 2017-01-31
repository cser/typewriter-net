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
			code = context.GetMapped(code);
			ProcessKey(new ViChar(code, false));
		}
		
		public override bool DoKeyDown(Keys keysData)
		{
			switch (keysData)
			{
				case Keys.Left:
					ProcessKey(new ViChar('h', false));
					break;
				case Keys.Right:
					ProcessKey(new ViChar('l', false));
					break;
				case Keys.Down:
					ProcessKey(new ViChar('j', false));
					break;
				case Keys.Up:
					ProcessKey(new ViChar('k', false));
					break;
				case Keys.Control | Keys.R:
					ProcessKey(new ViChar('r', true));
					break;
				default:
					return false;
			}
			return true;
		}
		
		private void ProcessKey(ViChar code)
		{
			if (!parser.AddKey(code))
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
			//Console.WriteLine("ACTION: " + parser.action + " MOVE: " + parser.move + " - " + parser.moveChar);
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
						controller.ViMoveWordLeft(shift);
						break;
					case Direction.Right:
						controller.ViMoveWordRight(shift);
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
}