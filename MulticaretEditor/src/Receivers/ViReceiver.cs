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
			int count = parser.FictiveCount;
			bool needInput = false;
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
					}
					count = 1;
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
				switch (parser.action.Index)
				{
					case (int)'d':
						command = new ViCommands.Delete(move, count, false, parser.register);
						count = 1;
						break;
					case (int)'c':
						command = new ViCommands.Delete(move, count, true, parser.register);
						count = 1;
						needInput = true;
						break;
					case (int)'y':
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
					case (int)'u':
						ProcessUndo(count);
						break;
					case (int)'r':
						command = new ViCommands.ReplaceChar(parser.moveChar.c, count);
						break;
					case (int)'x':
						command = new ViCommands.Delete(
							new ViMoves.MoveStep(Direction.Right), count, false, parser.register);
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
						if (parser.move.IsChar('d'))
						{
							command = new ViCommands.DeleteLine(count, parser.register);
							count = 1;
						}
						break;
					case (int)'d' + ViChar.ControlIndex:
					case (int)'n' + ViChar.ControlIndex:
						controller.SelectNextText();
						if (!controller.AllSelectionsEmpty)
						{
							context.SetState(new ViReceiverVisual(false));
						}
						break;
					case (int)'D' + ViChar.ControlIndex:
					case (int)'N' + ViChar.ControlIndex:
						controller.SelectAllMatches();
						if (!controller.AllSelectionsEmpty)
						{
							context.SetState(new ViReceiverVisual(false));
						}
						break;
					case (int)'y':
						if (parser.move.IsChar('y'))
						{
							controller.ViCopyLine(parser.register, count);
						}
						break;
					case (int)'>':
						if (parser.move.IsChar('>'))
						{
							controller.ViShift(1, count, false);
						}
						break;
					case (int)'<':
						if (parser.move.IsChar('<'))
						{
							controller.ViShift(1, count, true);
						}
						break;
					case (int)'.':
						if (lastCommand != null)
						{	
							lastCommand.Execute(controller);
						}
						break;
					case (int)'r' + ViChar.ControlIndex:
						ProcessRedo(count);
						break;
					case (int)'i':
						context.SetState(new InputReceiver(new ViReceiverData('i', count), false));
						break;
					case (int)'a':
						controller.ViMoveRightFromCursor();
						context.SetState(new InputReceiver(new ViReceiverData('a', count), false));
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
						controller.ViMoveEnd(false, 1);
						controller.ViMoveRightFromCursor();
						controller.InsertLineBreak();
						context.SetState(new InputReceiver(new ViReceiverData('o', count), false));
						break;
					case (int)'O':
						controller.ViMoveHome(false, true);
						controller.InsertLineBreak();
						controller.ViLogicMoveUp(false);
						if (lines.autoindent)
						{
							controller.ViAutoindentByBottom();
						}
						context.SetState(new InputReceiver(new ViReceiverData('O', count), false));
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
						context.SetState(new ViReceiverVisual(false));
						break;
					case (int)'V':
						context.SetState(new ViReceiverVisual(true));
						break;
					case (int)'*':
						string text = controller.GetWord(controller.Lines.PlaceOf(controller.LastSelection.caret));
						if (!string.IsNullOrEmpty(text))
						{
							DoFind("\\b" + text + "\\b");
						}
						context.SetState(new ViReceiver(null, false));
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
