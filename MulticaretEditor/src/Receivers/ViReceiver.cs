using System;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class ViReceiver : AReceiver
	{
		private ViReceiverData startData;
		
		public ViReceiver(ViReceiverData startData)
		{
			this.startData = startData;
		}
		
		public override bool AltMode { get { return true; } }
		
		public override void DoOn()
		{
			ViReceiverData startData = this.startData;
			this.startData = null;
			if (startData != null)
			{
				for (int i = 1; i < startData.count; i++)
				{
					foreach (char c in startData.inputChars)
					{
						ProcessInputChar(c);
					}
				}
			}
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
						if (selection.preferredPos > 0)
						{
							selection.preferredPos--;
						}
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
				case Keys.Control | Keys.F:
					ProcessKey(new ViChar('f', true));
					break;
				case Keys.Control | Keys.B:
					ProcessKey(new ViChar('b', true));
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
				context.SetState(new InputReceiver(new ViReceiverData(parser.FictiveCount)));
				return;
			}
			if (parser.action.c == 'a')
			{
				controller.ViMoveRightFromCursor();
				context.SetState(new InputReceiver(new ViReceiverData(parser.FictiveCount)));
				return;
			}
			if (parser.action.c == 'I')
			{
				controller.ViMoveHome(false, true);
				context.SetState(new InputReceiver(new ViReceiverData(parser.FictiveCount)));
				return;
			}
			if (parser.action.c == 'A')
			{
				controller.ViMoveEnd(false, 1);
				controller.ViMoveRightFromCursor();
				context.SetState(new InputReceiver(new ViReceiverData(parser.FictiveCount)));
				return;
			}
			ViMoves.IMove move = null;
			bool ignoreRepeat = false;
			bool needInput = false;
			if (parser.move.control)
			{
				switch (parser.move.c)
				{
					case 'f':
						move = new ViMoves.PageUpDown(false);
						break;
					case 'b':
						move = new ViMoves.PageUpDown(true);
						break;
				}
			}
			else
			{
				switch (parser.move.c)
				{
					case 'h':
						move = new ViMoves.MoveStep(Direction.Left);
						break;
					case 'l':
						move = new ViMoves.MoveStep(Direction.Right);
						break;
					case 'j':
						move = new ViMoves.MoveStep(Direction.Down);
						break;
					case 'k':
						move = new ViMoves.MoveStep(Direction.Up);
						break;
					case 'w':
						move = new ViMoves.MoveWord(Direction.Right);
						break;
					case 'b':
						move = new ViMoves.MoveWord(Direction.Left);
						break;
					case 'f':
					case 'F':
					case 't':
					case 'T':
						move = new ViMoves.Find(parser.move.c, parser.moveChar.c, parser.FictiveCount);
						ignoreRepeat = true;
						break;
					case '0':
						move = new ViMoves.Home(false);
						break;
					case '^':
						move = new ViMoves.Home(true);
						break;
					case '$':
						move = new ViMoves.End(parser.FictiveCount);
						ignoreRepeat = true;
						break;
					case 'G':
						if (parser.rawCount == -1)
						{
							move = new ViMoves.DocumentEnd();
						}
						else
						{
							move = new ViMoves.GoToLine(parser.rawCount);
						}
						break;
					case 'g':
						if (parser.moveChar.IsChar('g'))
						{
							move = new ViMoves.DocumentStart();
						}
						break;
				}
			}
			//Console.WriteLine("ACTION: " + parser.action + " MOVE: " + parser.move + " - " + parser.moveChar);
			ViCommands.ICommand command = null;
			if (move != null)
			{
				if (!parser.action.control)
				{
					switch (parser.action.c)
					{
						case 'd':
							command = new ViCommands.Delete(move, parser.FictiveCount, false);
							break;
						case 'c':
							command = new ViCommands.Delete(move, parser.FictiveCount, true);
							ignoreRepeat = true;
							needInput = true;
							break;
						case 'y':
							command = new ViCommands.Copy(move, parser.FictiveCount);
							ignoreRepeat = true;
							break;
						default:
							command = new ViCommands.Empty(move, parser.FictiveCount);
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
							command = new ViCommands.Undo();
							break;
						case 'r':
							command = new ViCommands.ReplaceChar(parser.moveChar.c, parser.FictiveCount);
							break;
						case 'x':
							command = new ViCommands.Delete(
								new ViMoves.MoveStep(Direction.Right), parser.FictiveCount, false);
							ignoreRepeat = true;
							break;
						case 'p':
							command = new ViCommands.Paste();
							break;
					}
				}
				else
				{
					switch (parser.action.c)
					{
						case 'r':
							command = new ViCommands.Redo();
							break;
					}
				}
			}
			if (command != null && parser.FictiveCount != 1 && !ignoreRepeat)
			{
				command = new ViCommands.Repeat(command, parser.FictiveCount);
			}
			if (command != null)
			{
				command.Execute(controller);
				controller.ViResetCommandsBatching();
				if (needInput)
				{
					context.SetState(new InputReceiver(new ViReceiverData(parser.FictiveCount)));
				}
			}
		}
	}
}