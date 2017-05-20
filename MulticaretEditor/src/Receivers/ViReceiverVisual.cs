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
		
		public override bool DoKeyDown(Keys keysData, out bool scrollToCursor)
		{
			if (((keysData & Keys.Control) == Keys.Control) &&
				((keysData & Keys.OemOpenBrackets) == Keys.OemOpenBrackets))
			{
				controller.JoinSelections();
				foreach (Selection selection in controller.Selections)
				{
					selection.SetEmpty();
				}
				scrollToCursor = true;
				context.SetState(new ViReceiver(null));
				return true;
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
				case Keys.Control | Keys.N:
					ProcessKey(new ViChar('n', true), out viShortcut, out scrollToCursor);
					return true;
				case Keys.Control | Keys.Shift | Keys.N:
					ProcessKey(new ViChar('N', true), out viShortcut, out scrollToCursor);
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
			bool needInput = false;
			int count = parser.FictiveCount;
			switch (parser.move.Index)
			{
				case (int)'f' + ViChar.ControlIndex:
					move = new ViMoves.PageUpDown(false);
					break;
				case (int)'b' + ViChar.ControlIndex:
					move = new ViMoves.PageUpDown(true);
					break;
				case (int)'h':
					move = new ViMoves.MoveStep(Direction.Left);
					break;
				case (int)'l':
					move = new ViMoves.MoveStep(Direction.Right);
					break;
				case (int)'j':
					if (parser.moveChar.c == 'g')
					{
						move = new ViMoves.SublineMoveStep(Direction.Down);
					}
					else
					{
						move = new ViMoves.MoveStep(Direction.Down);
					}
					break;
				case (int)'k':
					if (parser.moveChar.c == 'g')
					{
						move = new ViMoves.SublineMoveStep(Direction.Up);
					}
					else
					{
						move = new ViMoves.MoveStep(Direction.Up);
					}
					break;
				case (int)'w':
					move = new ViMoves.MoveWord(Direction.Right);
					break;
				case (int)'b':
					move = new ViMoves.MoveWord(Direction.Left);
					break;
				case (int)'e':
					move = new ViMoves.MoveWordE();
					break;
				case (int)'f':
				case (int)'F':
				case (int)'t':
				case (int)'T':
					move = new ViMoves.Find(parser.move.c, parser.moveChar.c, count);
					count = 1;
					break;
				case (int)'0':
					move = new ViMoves.Home(false);
					break;
				case (int)'^':
					move = new ViMoves.Home(true);
					break;
				case (int)'$':
					move = new ViMoves.End(count);
					count = 1;
					break;
				case (int)'G':
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
				case (int)'g':
					if (parser.moveChar.IsChar('g'))
					{
						move = new ViMoves.DocumentStart();
						count = 1;
					}
					break;
				case (int)'i':
				case (int)'a':
					move = new ViMoves.MoveObject(parser.moveChar.c, parser.move.c == 'i');
					break;
				case (int)'n':
					move = new ViMoves.FindForwardPattern();
					break;
				case (int)'N':
					move = new ViMoves.FindBackwardPattern();
					break;
			}
			ViCommands.ICommand command = null;
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
					case (int)'u':
						ProcessUndo(count);
						count = 1;
						break;
					case (int)'r':
						command = new ViCommands.ReplaceChar(parser.moveChar.c, count);
						count = 1;
						break;
					case (int)'p':
						command = new ViCommands.Paste(Direction.Right, parser.register, count);
						count = 1;
						break;
					case (int)'P':
						command = new ViCommands.Paste(Direction.Left, parser.register, count);
						count = 1;
						break;
					case (int)'J':
						command = new ViCommands.J();
						break;
					case (int)'d':
					case (int)'x':
						if (_lineMode)
						{
							controller.ViDeleteLine(parser.register, 1);
						}
						else
						{
							controller.ViCut(parser.register);
						}
						context.SetState(new ViReceiver(null));
						break;
					case (int)'c':
						if (_lineMode)
						{
							controller.ViCopyLine('0', 1);
							controller.ViDeleteLine('0', 1);
						}
						else
						{
							controller.ViCut(parser.register);
						}
						context.SetState(new InputReceiver(null, false));
						break;
					case (int)'y':
						if (_lineMode)
						{
							controller.ViCopyLine(parser.register, count);
						}
						else
						{
							controller.ViCopy(parser.register);
						}
						context.SetState(new ViReceiver(null));
						break;
					case (int)'d' + ViChar.ControlIndex:
					case (int)'n' + ViChar.ControlIndex:
						controller.SelectNextText();
						break;
					case (int)'D' + ViChar.ControlIndex:
					case (int)'N' + ViChar.ControlIndex:
						controller.SelectAllMatches();
						break;
					case (int)'>':
						controller.ViShift(count, 1, false);
						context.SetState(new ViReceiver(null));
						break;
					case (int)'<':
						controller.ViShift(count, 1, true);
						context.SetState(new ViReceiver(null));
						break;
					case (int)'r' + ViChar.ControlIndex:
						ProcessRedo(count);
						break;
					case (int)'s':
						controller.ViSelectRight(count);
						controller.EraseSelection();
						context.SetState(new InputReceiver(new ViReceiverData('s', 1), false));
						break;
					case (int)'I':
						controller.ViMoveHome(false, true);
						context.SetState(new InputReceiver(new ViReceiverData('I', count), false));
						break;
					case (int)'A':
						controller.ViMoveEnd(false, 1);
						controller.ViMoveRightFromCursor();
						context.SetState(new InputReceiver(new ViReceiverData('A', count), false));
						break;
					case (int)'o':
					case (int)'O':
						foreach (Selection selection in controller.Selections)
						{
							int position = selection.anchor;
							selection.anchor = selection.caret;
							selection.caret = position;
						}
						break;
					case (int)'j' + ViChar.ControlIndex:
						for (int i = 0; i < count; i++)
						{
							controller.ScrollRelative(0, 1);
						}
						scrollToCursor = false;
						break;
					case (int)'k' + ViChar.ControlIndex:
						for (int i = 0; i < count; i++)
						{
							controller.ScrollRelative(0, -1);
						}
						scrollToCursor = false;
						break;
					case (int)'v':
						if (_lineMode)
						{
							context.SetState(new ViReceiverVisual(false));
						}
						else
						{
							context.SetState(new ViReceiver(null));
						}
						break;
					case (int)'V':
						if (!_lineMode)
						{
							context.SetState(new ViReceiverVisual(true));
						}
						else
						{
							context.SetState(new ViReceiver(null));
						}
						break;
					case (int)'*':
						if (!controller.LastSelection.Empty)
						{
							string text = controller.Lines.GetText(
								controller.LastSelection.Left, controller.LastSelection.Count);
							DoFind(Escape(text));
							context.SetState(new ViReceiver(null));
						}
						else
						{
							string text = controller.GetWord(controller.Lines.PlaceOf(controller.LastSelection.caret));
							if (!string.IsNullOrEmpty(text))
							{
								DoFind("\\b" + text + "\\b");
							}
							context.SetState(new ViReceiver(null));
						}
						break;
				}
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
		}
		
		private static string Escape(string text)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				switch (c)
				{
					case '\\':
						builder.Append("\\\\");
						break;
					case '(':
					case ')':
					case '[':
					case ']':
					case '.':
					case '$':
					case '?':
					case '{':
					case '}':
					case '+':
					case '-':
						builder.Append('\\');
						builder.Append(c);
						break;
					default:
						builder.Append(c);
						break;
				}
			}
			return builder.ToString();
		}
		
		public override bool DoFind(string text)
		{
			ClipboardExecuter.PutToRegister('/', text);
			if (ClipboardExecuter.ViRegex != null)
			{
				controller.ViFindForward(ClipboardExecuter.ViRegex);
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