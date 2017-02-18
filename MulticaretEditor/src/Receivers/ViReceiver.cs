using System;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class ViReceiver : AReceiver
	{
		private ViReceiverData startData;
		private ViCommands.ICommand lastCommand;
		
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
		
		public override void DoKeyPress(char code, out string viShortcut)
		{
			code = context.GetMapped(code);
			ProcessKey(new ViChar(code, false), out viShortcut);
		}
		
		public override bool DoKeyDown(Keys keysData)
		{
			string viShortcut;
			switch (keysData)
			{
				case Keys.Left:
					ProcessKey(new ViChar('h', false), out viShortcut);
					return true;
				case Keys.Right:
					ProcessKey(new ViChar('l', false), out viShortcut);
					return true;
				case Keys.Down:
					ProcessKey(new ViChar('j', false), out viShortcut);
					return true;
				case Keys.Up:
					ProcessKey(new ViChar('k', false), out viShortcut);
					return true;
				case Keys.Control | Keys.R:
					ProcessKey(new ViChar('r', true), out viShortcut);
					return true;
				case Keys.Control | Keys.F:
					ProcessKey(new ViChar('f', true), out viShortcut);
					return true;
				case Keys.Control | Keys.B:
					ProcessKey(new ViChar('b', true), out viShortcut);
					return true;
				default:
					return false;
			}
		}
		
		private void ProcessKey(ViChar code, out string viShortcut)
		{
			viShortcut = null;
			if (!parser.AddKey(code))
			{
				return;
			}
			if (parser.shortcut != null)
			{
				viShortcut = parser.shortcut;
				return;
			}
			if (parser.action.c == 'i')
			{
				context.SetState(new InputReceiver(new ViReceiverData(parser.FictiveCount), false));
				return;
			}
			if (parser.action.c == 'a')
			{
				controller.ViMoveRightFromCursor();
				context.SetState(new InputReceiver(new ViReceiverData(parser.FictiveCount), false));
				return;
			}
			if (parser.action.c == 's')
			{
				controller.ViShiftRight(parser.FictiveCount);
				controller.EraseSelection();
				context.SetState(new InputReceiver(new ViReceiverData(1), false));
				return;
			}
			if (parser.action.c == 'I')
			{
				controller.ViMoveHome(false, true);
				context.SetState(new InputReceiver(new ViReceiverData(parser.FictiveCount), false));
				return;
			}
			if (parser.action.c == 'A')
			{
				controller.ViMoveEnd(false, 1);
				controller.ViMoveRightFromCursor();
				context.SetState(new InputReceiver(new ViReceiverData(parser.FictiveCount), false));
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
					case 'e':
						move = new ViMoves.MoveWordE();
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
					case 'i':
					case 'a':
						move = new ViMoves.MoveObject(parser.moveChar.c, parser.move.c == 'i');
						break;
				}
			}
			ViCommands.ICommand command = null;
			if (move != null)
			{
				if (!parser.action.control)
				{
					switch (parser.action.c)
					{
						case 'd':
							command = new ViCommands.Delete(move, parser.FictiveCount, false, parser.register);
							ignoreRepeat = true;
							break;
						case 'c':
							command = new ViCommands.Delete(move, parser.FictiveCount, true, parser.register);
							ignoreRepeat = true;
							needInput = true;
							break;
						case 'y':
							ProcessCopy(move, parser.register, parser.FictiveCount);
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
							ProcessUndo(parser.FictiveCount);
							break;
						case 'r':
							command = new ViCommands.ReplaceChar(parser.moveChar.c, parser.FictiveCount);
							break;
						case 'x':
							command = new ViCommands.Delete(
								new ViMoves.MoveStep(Direction.Right), parser.FictiveCount, false, parser.register);
							ignoreRepeat = true;
							break;
						case 'p':
							command = new ViCommands.Paste(Direction.Right, parser.register, parser.FictiveCount);
							ignoreRepeat = true;
							break;
						case 'P':
							command = new ViCommands.Paste(Direction.Left, parser.register, parser.FictiveCount);
							ignoreRepeat = true;
							break;
						case 'J':
							command = new ViCommands.J();
							break;
						case 'd':
							if (parser.move.IsChar('d'))
							{
								command = new ViCommands.DeleteLine(parser.FictiveCount, parser.register);
								ignoreRepeat = true;
							}
							break;
						case 'y':
							if (parser.move.IsChar('y'))
							{
								controller.ViCopyLine(parser.register, parser.FictiveCount);
							}
							break;
						case '.':
							command = lastCommand;
							lastCommand = null;
							break;
					}
				}
				else
				{
					switch (parser.action.c)
					{
						case 'r':
							ProcessRedo(parser.FictiveCount);
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
					context.SetState(new InputReceiver(null, false));
				}
				lastCommand = command;
			}
		}
		
		public override bool DoFind(string text)
		{
			ClipboardExecuter.PutToRegister('/', text);
			return true;
		}
		
		private void ProcessRedo(int count)
		{
			for (int i = 0; i < count; i++)
			{
				controller.Redo();
			}
			controller.ViCollapseSelections();
		}
		
		private void ProcessUndo(int count)
		{
			for (int i = 0; i < count; i++)
			{
				controller.Undo();
			}
			controller.ViCollapseSelections();
		}
		
		private void ProcessCopy(ViMoves.IMove move, char register, int count)
		{
			for (int i = 0; i < count; i++)
			{
				move.Move(controller, true, false);
			}
			controller.ViCopy(register);
			controller.ViCollapseSelections();
		}
	}
}