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
		private bool offsetOnStart;
		
		public ViReceiver(ViReceiverData startData, bool offsetOnStart)
		{
			this.startData = startData;
			this.offsetOnStart = offsetOnStart;
		}
		
		public override bool AltMode { get { return true; } }
		
		public override void DoOn()
		{
			ViReceiverData startData = this.startData;
			this.startData = null;
			if (controller.macrosExecutor != null &&
				controller.macrosExecutor.lastCommand != null &&
				startData != null)
			{
				controller.macrosExecutor.lastCommand.startData = startData;
			}
			foreach (Selection selection in controller.Selections)
			{
				selection.SetEmpty();
			}
			if (startData != null)
			{
				ClipboardExecutor.viLastInputChars = startData.inputChars;
				if (startData.action == 'o' || startData.action == 'O')
				{
					for (int i = startData.forcedInput ? 0 : 1; i < startData.count; i++)
					{
						if (i > 0)
						{
							controller.InsertLineBreak();
						}
						foreach (char c in startData.inputChars)
						{
							ProcessInputChar(c);
						}
					}
				}
				else
				{
					for (int i = startData.forcedInput ? 0 : 1; i < startData.count; i++)
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
		
		public override bool IsIdle { get { return parser.IsIdle; } }
		
		public override void DoKeyPress(char code, out string viShortcut, out bool scrollToCursor)
		{
			ViChar viChar = new ViChar(code, false);
			viChar.c = context.GetMapped(code);
			ProcessKey(viChar, out viShortcut, out scrollToCursor);
		}
		
		public override bool DoKeyDown(Keys keysData, out string viShortcut, out bool scrollToCursor)
		{
			viShortcut = null;
			if (((keysData & Keys.Control) == Keys.Control) &&
				((keysData & Keys.OemOpenBrackets) == Keys.OemOpenBrackets))
			{
				if (!parser.IsIdle)
				{
					scrollToCursor = false;
					parser.Reset();
					return true;
				}
				if (controller.ClearMinorSelections())
				{
					scrollToCursor = true;
					return true;
				}
			}
			switch (keysData)
			{
				case Keys.Control | Keys.R:
					if (controller.isReadonly)
					{
						scrollToCursor = false;
						return false;
					}
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
				case Keys.Control | Keys.O:
					ProcessKey(new ViChar('o', true), out viShortcut, out scrollToCursor);
					return true;
				case Keys.Control | Keys.I:
					ProcessKey(new ViChar('i', true), out viShortcut, out scrollToCursor);
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
			ProcessParserCommand(out viShortcut, out scrollToCursor);
		}
		
		private void ProcessParserCommand(out string viShortcut, out bool scrollToCursor)
		{
			viShortcut = null;
			scrollToCursor = true;
			ViMoves.IMove move = null;
			bool needHistoryMove = true;
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
					needHistoryMove = count > 1;
					break;
				case 'l':
					move = new ViMoves.MoveStep(Direction.Right);
					needHistoryMove = count > 1;
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
					needHistoryMove = count > 1;
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
					needHistoryMove = count > 1;
					break;
				case 'w':
					move = new ViMoves.MoveWord(Direction.Right);
					break;
				case 'W':
					move = new ViMoves.BigMoveWord(Direction.Right);
					break;
				case 'b':
					move = new ViMoves.MoveWord(Direction.Left);
					break;
				case 'B':
					move = new ViMoves.BigMoveWord(Direction.Left);
					break;
				case 'e':
					move = new ViMoves.MoveWordE();
					break;
				case 'E':
					move = new ViMoves.BigMoveWordE();
					break;
				case 'f':
				case 'F':
				case 't':
				case 'T':
					move = new ViMoves.Find(parser.move.c, parser.moveChar.c, count);
					count = 1;
					break;
				case '`':
				case '\'':
					if (ViMoves.JumpBookmark.IsFileBased(parser.moveChar.c))
					{
						viShortcut = "" + parser.move.c + parser.moveChar.c;
						return;
					}
					move = new ViMoves.JumpBookmark(parser.move.c, parser.moveChar.c);
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
					if (parser.moveChar.IsChar('v'))
					{
						controller.ViRecoverSelections();
						if (!controller.AllSelectionsEmpty)
						{
							context.SetState(new ViReceiverVisual(false));
						}
					}
					count = 1;
					break;
				case 'i':
				case 'a':
					move = new ViMoves.MoveObject(parser.moveChar.c, parser.move.c == 'i', count);
					count = 1;
					break;
				case 'n':
					move = new ViMoves.FindForwardPattern();
					break;
				case 'N':
					move = new ViMoves.FindBackwardPattern();
					break;
				case '%':
					move = new ViMoves.FindPairBracket();
					break;
			}
			ViCommands.ICommand command = null;
			bool forceLastCommand = false;
			if (move != null)
			{
				switch (parser.action.Index)
				{
					case 'd':
						command = new ViCommands.Delete(move, count, false, parser.register);
						break;
					case 'c':
						command = new ViCommands.Delete(move, count, true, parser.register);
						context.SetState(new InputReceiver(new ViReceiverData('c', 1), false));
						break;
					case 'y':
						ProcessCopy(move, parser.register, count);
						break;
					default:
						for (int i = 0; i < count; i++)
						{
							move.Move(controller, false, MoveMode.Move);
						}
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
						command = new ViCommands.ReplaceChar(parser.moveChar.origin, count);
						break;
					case ' ':
						context.SetState(new ViJumpReceiver(parser.moveChar.c, ViJumpReceiver.Mode.Single));
						break;
					case ',':
						if (parser.move.IsChar(' '))
						{
							context.SetState(new ViJumpReceiver(parser.moveChar.c, ViJumpReceiver.Mode.New));
						}
						break;
					case 'x':
						command = new ViCommands.Delete(
							new ViMoves.MoveStep(Direction.Right), count, false, parser.register);
						break;
					case '~':
						command = new ViCommands.SwitchUpperLower(
							new ViMoves.MoveStep(Direction.Right), count);
						break;
					case 'p':
						command = new ViCommands.Paste(Direction.Right, parser.register, count);
						break;
					case 'P':
						command = new ViCommands.Paste(Direction.Left, parser.register, count);
						break;
					case 'J':
						command = new ViCommands.J();
						command = new ViCommands.Repeat(command, count);
						break;
					case 'd':
						if (parser.move.IsChar('d'))
						{
							command = new ViCommands.DeleteLine(count, parser.register);
						}
						break;
					case 'c':
						if (parser.move.IsChar('c'))
						{
							controller.ViDeleteLineForChange(parser.register, count);
							context.SetState(new InputReceiver(new ViReceiverData('c', 1), false));
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
					case 'Y':
						controller.ViCopyLine(parser.register, count);
						break;
					case '>':
						if (parser.move.IsChar('>'))
						{
							controller.ViShift(1, count, false);
							forceLastCommand = true;
						}
						break;
					case '<':
						if (parser.move.IsChar('<'))
						{
							controller.ViShift(1, count, true);
							forceLastCommand = true;
						}
						break;
					case '.':
						if (controller.macrosExecutor != null && controller.macrosExecutor.lastCommand != null)
						{	
							ViCommandParser.LastCommand lastCommand = null;
							lastCommand = controller.macrosExecutor.lastCommand;
							controller.macrosExecutor.lastCommand = null;
							try
							{
								controller.processor.BeginBatch();
								for (int i = 0; i < count; i++)
								{
									parser.SetLastCommand(lastCommand);
									string tempShortcut;
									bool tempScrollToCursor;
									ProcessParserCommand(out tempShortcut, out tempScrollToCursor);
									if (tempScrollToCursor)
									{
										scrollToCursor = true;
									}
									if (lastCommand.startData != null)
									{
										lastCommand.startData.forcedInput = true;
										context.SetState(new ViReceiver(lastCommand.startData, true));
									}
								}
							}
							finally
							{
								controller.processor.EndBatch();
							}
							return;
						}
						break;
					case 'r' + ViChar.ControlIndex:
						ProcessRedo(count);
						return;
					case 'o' + ViChar.ControlIndex:
						viShortcut = "C-o";
						return;
					case 'i' + ViChar.ControlIndex:
						viShortcut = "C-i";
						return;
					case 'i':
						context.SetState(new InputReceiver(new ViReceiverData('i', count), false));
						forceLastCommand = true;
						break;
					case 'a':
						controller.ViMoveRightFromCursor();
						context.SetState(new InputReceiver(new ViReceiverData('a', count), false));
						forceLastCommand = true;
						break;
					case 's':
						controller.ViSelectRight(count);
						controller.EraseSelection();
						context.SetState(new InputReceiver(new ViReceiverData('s', 1), false));
						forceLastCommand = true;
						break;
					case 'I':
						controller.ViMoveHome(false, true);
						context.SetState(new InputReceiver(new ViReceiverData('I', count), false));
						forceLastCommand = true;
						break;
					case 'A':
						controller.ViMoveEnd(false, 1);
						controller.ViMoveRightFromCursor();
						context.SetState(new InputReceiver(new ViReceiverData('A', count), false));
						forceLastCommand = true;
						break;
					case 'o':
						if (!controller.isReadonly)
						{
							controller.ViMoveEnd(false, 1);
							controller.ViMoveRightFromCursor();
							controller.InsertLineBreak();
							context.SetState(new InputReceiver(new ViReceiverData('o', count), false));
						}
						forceLastCommand = true;
						break;
					case 'O':
						if (!controller.isReadonly)
						{
							controller.ViMoveHome(false, true);
							controller.InsertLineBreak();
							controller.ViLogicMoveUp(false);
							if (lines.autoindent)
							{
								controller.ViAutoindentByBottom();
							}
							context.SetState(new InputReceiver(new ViReceiverData('O', count), false));
						}
						forceLastCommand = true;
						break;
					case 'C':
						controller.ViMoveEnd(true, parser.FictiveCount);
						controller.ViCut(parser.register, false);
						context.SetState(new InputReceiver(new ViReceiverData('C', count), false));
						forceLastCommand = true;
						break;
					case 'D':
						controller.ViMoveEnd(true, parser.FictiveCount);
						controller.ViCut(parser.register, true);
						forceLastCommand = true;
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
						forceLastCommand = true;
						break;
					case '\r':
						for (int i = 0; i < count; i++)
						{
							controller.InsertLineBreak();
						}
						forceLastCommand = true;
						break;
					case 'm':
						if (ViMoves.JumpBookmark.IsFileBased(parser.moveChar.c))
						{
							if (controller.macrosExecutor != null)
							{
								controller.macrosExecutor.SetBookmark(parser.moveChar.c, controller.Lines.viFullPath, controller.LastSelection.caret);
							}
							return;
						}
						controller.SetBookmark(parser.moveChar.c, controller.LastSelection.anchor);
						break;
				}
			}
			if ((move != null || needInput) && controller.macrosExecutor != null)
			{
				controller.ViAddHistoryPosition(needHistoryMove);
			}
			if (command != null)
			{
				command.Execute(controller);
				controller.ViResetCommandsBatching();
				if (needInput)
				{
					context.SetState(new InputReceiver(null, false));
				}
			}
			if (command != null || forceLastCommand)
			{
				if (controller.macrosExecutor != null)
				{
					controller.macrosExecutor.lastCommand = parser.GetLastCommand();
				}
				if (controller.isReadonly)
				{
					viShortcut = parser.GetFictiveShortcut();
				}
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
				controller.processor.Redo();
			}
			controller.ViFixPositions(true);
		}
		
		private void ProcessUndo(int count)
		{
			for (int i = 0; i < count; i++)
			{
				controller.processor.Undo();
			}
			controller.ViCollapseSelections();
			controller.ViFixPositions(true);
		}
		
		private void ProcessCopy(ViMoves.IMove move, char register, int count)
		{
			for (int i = 0; i < count; i++)
			{
				move.Move(controller, true, MoveMode.Copy);
			}
			controller.ViCopy(register);
			controller.ViCollapseSelections();
		}
	}
}
