using System;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class ViReceiver : AReceiver
	{
		public override ViMode ViMode { get { return ViMode.Normal; } }
		
		private ViReceiverData startData;
		private ViCommands.ICommand lastCommand;
		private bool offsetOnStart;
		
		public ViReceiver(ViReceiverData startData, bool offsetOnStart)
		{
			this.startData = startData;
			this.offsetOnStart = offsetOnStart;
		}
		
		public override bool AltMode { get { return true; } }
		
		public override void DoOn()
		{
			foreach (Selection selection in controller.Selections)
			{
				selection.SetEmpty();
			}
			ViReceiverData startData = this.startData;
			this.startData = null;
			if (startData != null)
			{
				if (startData.action == 'o' || startData.action == 'O')
				{
					for (int i = 1; i < startData.count; i++)
					{
						controller.InsertLineBreak();
						foreach (char c in startData.inputChars)
						{
							ProcessInputChar(c);
						}
					}
				}
				else
				{
					for (int i = 1; i < startData.count; i++)
					{
						foreach (char c in startData.inputChars)
						{
							ProcessInputChar(c);
						}
					}
				}
			}
			if (offsetOnStart)
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
							if (selection.preferredPos > 0)
							{
								selection.preferredPos--;
							}
						}
					}
				}
			}
		}
		
		private readonly ViCommandParser parser = new ViCommandParser(false);
		
		public override void DoKeyPress(char code, out string viShortcut, out bool scrollToCursor)
		{
			code = context.GetMapped(code);
			ProcessKey(new ViChar(code, false), out viShortcut, out scrollToCursor);
		}
		
		public override bool DoKeyDown(Keys keysData, out bool scrollToCursor)
		{
			if (((keysData & Keys.Control) == Keys.Control) &&
				((keysData & Keys.OemOpenBrackets) == Keys.OemOpenBrackets))
			{
				if (controller.ClearMinorSelections())
				{
					scrollToCursor = true;
					return true;
				}
			}
			string viShortcut;
			switch (keysData)
			{
				case Keys.Left:
					ProcessKey(new ViChar('h', false), out viShortcut, out scrollToCursor);
					return true;
				case Keys.Right:
					ProcessKey(new ViChar('l', false), out viShortcut, out scrollToCursor);
					return true;
				case Keys.Down:
					ProcessKey(new ViChar('j', false), out viShortcut, out scrollToCursor);
					return true;
				case Keys.Up:
					ProcessKey(new ViChar('k', false), out viShortcut, out scrollToCursor);
					return true;
				case Keys.Control | Keys.R:
					ProcessKey(new ViChar('r', true), out viShortcut, out scrollToCursor);
					return true;
				case Keys.Control | Keys.F:
					ProcessKey(new ViChar('f', true), out viShortcut, out scrollToCursor);
					return true;
				case Keys.Control | Keys.B:
					ProcessKey(new ViChar('b', true), out viShortcut, out scrollToCursor);
					return true;
				case Keys.Control | Keys.J:
					ProcessKey(new ViChar('j', true), out viShortcut, out scrollToCursor);
					return true;
				case Keys.Control | Keys.K:
					ProcessKey(new ViChar('k', true), out viShortcut, out scrollToCursor);
					return true;
				case Keys.Control | Keys.D:
					ProcessKey(new ViChar('d', true), out viShortcut, out scrollToCursor);
					return true;
				case Keys.Control | Keys.Shift | Keys.D:
					ProcessKey(new ViChar('D', true), out viShortcut, out scrollToCursor);
					return true;
				case Keys.Control | Keys.Shift | Keys.J:
					ProcessKey(new ViChar('J', true), out viShortcut, out scrollToCursor);
					return true;
				case Keys.Control | Keys.Shift | Keys.K:
					ProcessKey(new ViChar('K', true), out viShortcut, out scrollToCursor);
					return true;
				default:
					scrollToCursor = false;
					return false;
			}
		}
		
		private void ProcessKey(ViChar code, out string viShortcut, out bool scrollToCursor)
		{
			viShortcut = null;
			if (!parser.AddKey(code))
			{
				scrollToCursor = false;
				return;
			}
			if (parser.shortcut != null)
			{
				viShortcut = parser.shortcut;
				scrollToCursor = false;
				return;
			}
			scrollToCursor = true;
			ViMoves.IMove move = null;
			int count = parser.FictiveCount;
			bool needInput = false;
			switch (parser.move.Index)
			{
				case 'f' + ViChar.ControlIndex:
					move = new ViMoves.PageUpDown(false);
					break;
				case 'b' + ViChar.ControlIndex:
					move = new ViMoves.PageUpDown(true);
					break;
				case 'h':
					move = new ViMoves.MoveStep(Direction.Left);
					break;
				case 'l':
					move = new ViMoves.MoveStep(Direction.Right);
					break;
				case 'j':
					if (parser.moveChar.c == 'g')
					{
						move = new ViMoves.SublineMoveStep(Direction.Down);
					}
					else
					{
						move = new ViMoves.MoveStep(Direction.Down);
					}
					break;
				case 'k':
					if (parser.moveChar.c == 'g')
					{
						move = new ViMoves.SublineMoveStep(Direction.Up);
					}
					else
					{
						move = new ViMoves.MoveStep(Direction.Up);
					}
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
					move = new ViMoves.Find(parser.move.c, parser.moveChar.c, count);
					count = 1;
					break;
				case '0':
					move = new ViMoves.Home(false);
					break;
				case '^':
					move = new ViMoves.Home(true);
					break;
				case '$':
					move = new ViMoves.End(count);
					count = 1;
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
					count = 1;
					break;
				case 'g':
					if (parser.moveChar.IsChar('g'))
					{
						move = new ViMoves.DocumentStart();
					}
					count = 1;
					break;
				case 'i':
				case 'a':
					move = new ViMoves.MoveObject(parser.moveChar.c, parser.move.c == 'i');
					break;
				case 'n':
					move = new ViMoves.FindForwardPattern();
					break;
				case 'N':
					move = new ViMoves.FindBackwardPattern();
					break;
			}
			ViCommands.ICommand command = null;
			if (move != null)
			{
				switch (parser.action.Index)
				{
					case 'd':
						command = new ViCommands.Delete(move, count, false, parser.register);
						count = 1;
						break;
					case 'c':
						command = new ViCommands.Delete(move, count, true, parser.register);
						count = 1;
						needInput = true;
						break;
					case 'y':
						ProcessCopy(move, parser.register, count);
						count = 1;
						break;
					default:
						command = new ViCommands.Empty(move, count);
						count = 1;
						break;
				}
			}
			else
			{
				switch (parser.action.Index)
				{
					case 'u':
						ProcessUndo(count);
						break;
					case 'r':
						command = new ViCommands.ReplaceChar(parser.moveChar.c, count);
						break;
					case 'x':
						command = new ViCommands.Delete(
							new ViMoves.MoveStep(Direction.Right), count, false, parser.register);
						count = 1;
						break;
					case 'p':
						command = new ViCommands.Paste(Direction.Right, parser.register, count);
						count = 1;
						break;
					case 'P':
						command = new ViCommands.Paste(Direction.Left, parser.register, count);
						count = 1;
						break;
					case 'J':
						command = new ViCommands.J();
						break;
					case 'd':
						if (parser.move.IsChar('d'))
						{
							command = new ViCommands.DeleteLine(count, parser.register);
							count = 1;
						}
						break;
					case 'c':
						if (parser.move.IsChar('c'))
						{
							controller.ViDeleteLineForChange(parser.register, count);
							context.SetState(new InputReceiver(new ViReceiverData('c', 1), false));
							count = 1;
						}
						break;
					case 'd' + ViChar.ControlIndex:
						controller.SelectNextText();
						if (!controller.AllSelectionsEmpty)
						{
							context.SetState(new ViReceiverVisual(false));
						}
						break;
					case 'D' + ViChar.ControlIndex:
						controller.SelectAllMatches();
						if (!controller.AllSelectionsEmpty)
						{
							context.SetState(new ViReceiverVisual(false));
						}
						break;
					case 'J' + ViChar.ControlIndex:
						for (int i = 0; i < count; i++)
						{
							controller.PutCursorDown();
						}
						break;
					case 'K' + ViChar.ControlIndex:
						for (int i = 0; i < count; i++)
						{
							controller.PutCursorUp();
						}
						break;
					case 'y':
						if (parser.move.IsChar('y'))
						{
							controller.ViCopyLine(parser.register, count);
						}
						break;
					case '>':
						if (parser.move.IsChar('>'))
						{
							controller.ViShift(1, count, false);
						}
						break;
					case '<':
						if (parser.move.IsChar('<'))
						{
							controller.ViShift(1, count, true);
						}
						break;
					case '.':
						if (lastCommand != null)
						{	
							lastCommand.Execute(controller);
						}
						break;
					case 'r' + ViChar.ControlIndex:
						ProcessRedo(count);
						break;
					case 'i':
						context.SetState(new InputReceiver(new ViReceiverData('i', count), false));
						break;
					case 'a':
						controller.ViMoveRightFromCursor();
						context.SetState(new InputReceiver(new ViReceiverData('a', count), false));
						break;
					case 's':
						controller.ViSelectRight(count);
						controller.EraseSelection();
						context.SetState(new InputReceiver(new ViReceiverData('s', 1), false));
						break;
					case 'I':
						controller.ViMoveHome(false, true);
						context.SetState(new InputReceiver(new ViReceiverData('I', count), false));
						break;
					case 'A':
						controller.ViMoveEnd(false, 1);
						controller.ViMoveRightFromCursor();
						context.SetState(new InputReceiver(new ViReceiverData('A', count), false));
						break;
					case 'o':
						controller.ViMoveEnd(false, 1);
						controller.ViMoveRightFromCursor();
						controller.InsertLineBreak();
						context.SetState(new InputReceiver(new ViReceiverData('o', count), false));
						break;
					case 'O':
						controller.ViMoveHome(false, true);
						controller.InsertLineBreak();
						controller.ViLogicMoveUp(false);
						if (lines.autoindent)
						{
							controller.ViAutoindentByBottom();
						}
						context.SetState(new InputReceiver(new ViReceiverData('O', count), false));
						break;
					case 'C':
						controller.ViMoveEnd(true, parser.FictiveCount);
						count = 1;
						controller.ViCut(parser.register, false);
						context.SetState(new InputReceiver(new ViReceiverData('C', count), false));
						break;
					case 'D':
						controller.ViMoveEnd(true, parser.FictiveCount);
						count = 1;
						controller.ViCut(parser.register, true);
						break;
					case 'j' + ViChar.ControlIndex:
						for (int i = 0; i < count; i++)
						{
							controller.ScrollRelative(0, 1);
						}
						scrollToCursor = false;
						break;
					case 'k' + ViChar.ControlIndex:
						for (int i = 0; i < count; i++)
						{
							controller.ScrollRelative(0, -1);
						}
						scrollToCursor = false;
						break;
					case 'v':
						context.SetState(new ViReceiverVisual(false));
						break;
					case 'V':
						context.SetState(new ViReceiverVisual(true));
						break;
					case '*':
						string text = controller.GetWord(controller.Lines.PlaceOf(controller.LastSelection.caret));
						if (!string.IsNullOrEmpty(text))
						{
							DoFind(new Pattern("\\b" + text + "\\b", true, false));
						}
						break;
					case '\b':
						for (int i = 0; i < count; i++)
						{
							if (lines.AllSelectionsEmpty)
							{
								controller.Backspace();
							}
							else
							{
								controller.EraseSelection();
							}
						}
						break;
				}
			}
			if (command != null && count != 1)
			{
				command = new ViCommands.Repeat(command, count);
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
		
		public override bool DoFind(Pattern pattern)
		{
			ClipboardExecutor.PutToSearch(pattern);
			if (ClipboardExecutor.ViRegex != null)
			{
				controller.ViFindForward(ClipboardExecutor.ViRegex);
			}
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
