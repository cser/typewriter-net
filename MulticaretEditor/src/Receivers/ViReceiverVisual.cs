using System;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class ViReceiverVisual : AReceiver
	{
		public override ViMode ViMode { get { return _lineMode ? ViMode.LinesVisual : ViMode.Visual; } }
		
		private bool _lineMode;
		
		public ViReceiverVisual(bool lineMode)
		{
			_lineMode = lineMode;
		}
		
		public override bool AltMode { get { return true; } }
		
		public override void DoOn()
		{
		}
		
		private readonly ViCommandParser parser = new ViCommandParser(true);
		
		public override void DoKeyPress(char code, out string viShortcut, out bool scrollToCursor)
		{
			code = context.GetMapped(code);
			ProcessKey(new ViChar(code, false), out viShortcut, out scrollToCursor);
		}
		
		public override bool DoKeyDown(Keys keysData, out string viShortcut, out bool scrollToCursor)
		{
			viShortcut = null;
			if (((keysData & Keys.Control) == Keys.Control) &&
				((keysData & Keys.OemOpenBrackets) == Keys.OemOpenBrackets))
			{
				controller.JoinSelections();
				scrollToCursor = true;
				controller.ViStoreSelections();
				SetViMode();
				return true;
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
						count = 1;
					}
					break;
				case 'i':
				case 'a':
					move = new ViMoves.MoveObject(parser.moveChar.c, parser.move.c == 'i', count);
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
			bool needViMode = false;
			if (move != null)
			{
				for (int i = 0; i < count; i++)
				{
					move.Move(controller, true, false);
				}
			}
			else
			{
				switch (parser.action.Index)
				{
					case 'u':
						controller.ViStoreSelections();
						controller.ChangeCase(false);
						controller.MoveLeft(false);
						context.SetState(new ViReceiver(null, false));
						count = 1;
						break;
					case 'U':
						controller.ViStoreSelections();
						controller.ChangeCase(true);
						controller.MoveLeft(false);
						context.SetState(new ViReceiver(null, false));
						count = 1;
						break;
					case 'r':
						command = new ViCommands.ReplaceChar(parser.moveChar.c, count);
						needViMode = true;
						count = 1;
						break;
					case ' ':
						context.SetState(new ViJumpReceiver(parser.moveChar.c,
							_lineMode ? ViJumpReceiver.Mode.LinesSelection : ViJumpReceiver.Mode.Selection));
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
					case 'x':
						if (_lineMode)
						{
							controller.ViDeleteLine(parser.register, 1);
						}
						else
						{
							controller.ViCut(parser.register, true);
						}
						SetViMode();
						break;
					case 'c':
						if (_lineMode)
						{
							controller.ViStoreSelections();
							controller.ViCopyLine('0', 1);
							controller.ViDeleteLine('0', 1);
						}
						else
						{
							controller.ViCut(parser.register, false);
						}
						context.SetState(new InputReceiver(null, false));
						break;
					case 'y':
						if (_lineMode)
						{
							controller.ViStoreSelections();
							controller.ViCopyLine(parser.register, count);
						}
						else
						{
							controller.ViStoreSelections();
							controller.ViCopy(parser.register);
						}
						SetViMode();
						break;
					case 'd' + ViChar.ControlIndex:
						controller.SelectNextText();
						break;
					case 'D' + ViChar.ControlIndex:
						controller.SelectAllMatches();
						break;
					case 'J' + ViChar.ControlIndex:
						controller.PutCursorDown();
						break;
					case 'K' + ViChar.ControlIndex:
						controller.PutCursorUp();
						break;
					case '>':
						controller.ViShift(count, 1, false);
						SetViMode();
						break;
					case '<':
						controller.ViShift(count, 1, true);
						SetViMode();
						break;
					case 'r' + ViChar.ControlIndex:
						ProcessRedo(count);
						break;
					case 's':
						controller.ViSelectRight(count);
						controller.EraseSelection();
						context.SetState(new InputReceiver(new ViReceiverData('s', 1), false));
						break;
					case 'I':
						if (!controller.AllSelectionsEmpty)
						{
							controller.ViStoreSelections();
							foreach (Selection selection in controller.Selections)
							{
								int left = selection.Left;
								selection.anchor = left;
								selection.caret = left;
							}
						}
						context.SetState(new InputReceiver(new ViReceiverData('i', count), false));
						break;
					case 'A':
						if (!controller.AllSelectionsEmpty)
						{
							controller.ViStoreSelections();
							foreach (Selection selection in controller.Selections)
							{
								int right = selection.Right;
								selection.anchor = right;
								selection.caret = right;
							}
							context.SetState(new InputReceiver(new ViReceiverData('i', count), false));
						}
						break;
					case 'o':
					case 'O':
						foreach (Selection selection in controller.Selections)
						{
							int position = selection.anchor;
							selection.anchor = selection.caret;
							selection.caret = position;
						}
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
						if (_lineMode)
						{
							context.SetState(new ViReceiverVisual(false));
						}
						else
						{
							SetViMode();
						}
						break;
					case 'V':
						if (!_lineMode)
						{
							context.SetState(new ViReceiverVisual(true));
						}
						else
						{
							SetViMode();
						}
						break;
					case '*':
						if (!controller.LastSelection.Empty)
						{
							string text = controller.Lines.GetText(
								controller.LastSelection.Left, controller.LastSelection.Count);
							DoFind(new Pattern(text, false, false));
							SetViMode();
						}
						else
						{
							string text = controller.GetWord(controller.Lines.PlaceOf(controller.LastSelection.caret));
							if (!string.IsNullOrEmpty(text))
							{
								DoFind(new Pattern("\\b" + text + "\\b", true, false));
							}
							SetViMode();
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
					case '\r':
						for (int i = 0; i < count; i++)
						{
							controller.InsertLineBreak();
						}
						break;
				}
			}
			if (command != null)
			{
				command.Execute(controller);
				controller.ViResetCommandsBatching();
				if (needViMode)
				{
					SetViMode();
				}
			}
		}
		
		private void SetViMode()
		{
			foreach (Selection selection in controller.Selections)
			{
				if (selection.Count > 0)
				{
					if (selection.caret > selection.anchor)
					{
						--selection.caret;
					}
					selection.SetEmpty();
				}
			}
			context.SetState(new ViReceiver(null, false));
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
	}
}