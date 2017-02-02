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
			bool ignoreRepeat = false;
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
					case 'F':
					case 't':
					case 'T':
						move = new ViFind(parser.move.c, parser.moveChar.c, parser.count);
						ignoreRepeat = true;
						break;
					case '0':
						move = new ViHome(false);
						break;
					case '^':
						move = new ViHome(true);
						break;
					case '$':
						move = new ViEnd(parser.count);
						ignoreRepeat = true;
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
			if (command != null && parser.count != 1 && !ignoreRepeat)
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
			
			public override string ToString()
			{
				return "ViCommand()";
			}
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
			
			public override string ToString()
			{
				return "ViRepeat(" + command + ", " + count + ")";
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
			
			public override string ToString()
			{
				return "Empty(" + move + ", " + count + ")";
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
			
			public override string ToString()
			{
				return "Delete(" + move + ")";
			}
		}
		
		public class ViUndo : ViCommand
		{
			public override void Execute(Controller controller)
			{
				controller.Undo();
				controller.ViCollapseSelections();
			}
			
			public override string ToString()
			{
				return "Undo()";
			}
		}
		
		public class ViRedo : ViCommand
		{
			public override void Execute(Controller controller)
			{
				controller.Redo();
				controller.ViCollapseSelections();
			}
			
			public override string ToString()
			{
				return "Redo()";
			}
		}
		
		public abstract class ViMove
		{
			public abstract void Move(Controller controller, bool shift);
			
			public override string ToString()
			{
				return "Move";
			}
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
			
			public override string ToString()
			{
				return "MoveStep:" + direction;
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
			
			public override string ToString()
			{
				return "MoveWord:" + direction;
			}
		}
		
		public class ViFind : ViMove
		{
			private char type;
			private char charToFind;
			private int count;
			
			public ViFind(char type, char charToFind, int count)
			{
				this.type = type;
				this.charToFind = charToFind;
				this.count = count;
			}
			
			public override void Move(Controller controller, bool shift)
			{
				switch (type)
				{
					case 'f':
						controller.ViMoveToCharRight(charToFind, shift, count, false);
						break;
					case 't':
						controller.ViMoveToCharRight(charToFind, shift, count, true);
						break;
					case 'F':
						controller.ViMoveToCharLeft(charToFind, shift, count, false);
						break;
					case 'T':
						controller.ViMoveToCharLeft(charToFind, shift, count, true);
						break;
				}
			}
			
			public override string ToString()
			{
				return count + ":" + type + ":" + charToFind;
			}
		}
		
		public class ViHome : ViMove
		{
			private bool indented;
			
			public ViHome(bool indented)
			{
				this.indented = indented;
			}
			
			public override void Move(Controller controller, bool shift)
			{
				controller.MoveHome(shift);
				controller.ViMoveHome(shift, indented);
			}
			
			public override string ToString()
			{
				return "Home:" + indented;
			}
		}
		
		public class ViEnd : ViMove
		{
			private int count;
			
			public ViEnd(int count)
			{
				this.count = count;
			}
			
			public override void Move(Controller controller, bool shift)
			{
				controller.ViMoveEnd(shift, count);
			}
			
			public override string ToString()
			{
				return "End:" + count;
			}
		}
	}
}