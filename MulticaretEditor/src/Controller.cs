using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using MulticaretEditor.Commands;
using MulticaretEditor.Highlighting;

namespace MulticaretEditor
{
	public class Controller
	{
		private readonly LineArray lines;
		private readonly List<Selection> selections;

		public readonly History history;

		public Controller(LineArray lines)
		{
			this.lines = lines;
			this.selections = lines.selections;
			history = new History();
			ResetCommandsBatching();
		}

		public bool isReadonly;

		public LineArray Lines { get { return lines; } }

		public void InitText(string text)
		{
			lines.SetText(text);
			history.Reset();
			history.MarkAsSaved();
		}

		public bool MoveRight(bool shift)
		{
			return MoveRight(lines, shift);
		}

		public bool MoveLeft(bool shift)
		{
			return MoveLeft(lines, shift);
		}

		public static bool MoveRight(LineArray lines, bool shift)
		{
			bool result = false;
			foreach (Selection selection in lines.selections)
			{
				if (!shift && !selection.Empty)
				{
					if (selection.caret != selection.anchor)
						result = true;
					int index = selection.Right;
					selection.caret = index;
					selection.anchor = index;
				}
				else
				{
					if (selection.caret < lines.charsCount - 1 && lines.GetText(selection.caret, 2) == "\r\n")
					{
						selection.caret += 2;
						result = true;
					}
					else if (selection.caret < lines.charsCount)
					{
						selection.caret++;
						result = true;
					}
					if (!shift && selection.anchor != selection.caret)
					{
						selection.anchor = selection.caret;
						result = true;
					}
				}
				Place place = lines.PlaceOf(selection.caret);
				lines.SetPreferredPos(selection, place);
			}
			return result;
		}

		public static bool MoveLeft(LineArray lines, bool shift)
		{
			bool result = false;
			foreach (Selection selection in lines.selections)
			{
				if (!shift && !selection.Empty)
				{
					if (selection.caret != selection.anchor)
						result = true;
					int index = selection.Left;
					selection.caret = index;
					selection.anchor = index;
				}
				else
				{
					if (selection.caret > 1 && lines.GetText(selection.caret - 2, 2) == "\r\n")
					{
						selection.caret -= 2;
						result = true;
					}
					else if (selection.caret > 0)
					{
						selection.caret--;
						result = true;
					}
					if (!shift && selection.anchor != selection.caret)
					{
						selection.anchor = selection.caret;
						result = true;
					}
				}
				Place place = lines.PlaceOf(selection.caret);
				lines.SetPreferredPos(selection, place);
			}
			return result;
		}

		public bool MoveUp(bool shift)
		{
			bool result = false;
			if (lines.wordWrap && selections.Count == 1)
			{
				Selection selection = lines.LastSelection;
				Place place = lines.PlaceOf(selection.caret);
				Line line = lines[place.iLine];
				Pos pos = line.WWPosOfIndex(place.iChar);
				Place newPlace = place;
				if (pos.iy > 0)
				{
					newPlace = new Place(line.WWNormalIndexOfPos(selection.wwPreferredPos, pos.iy - 1), place.iLine);
					result = true;
				}
				else if (place.iLine > 0)
				{
					line = lines[place.iLine - 1];
					newPlace = new Place(line.WWNormalIndexOfPos(selection.wwPreferredPos, line.cutOffs.count), place.iLine - 1);
					result = true;
				}
				selection.caret = lines.IndexOf(newPlace);
				if (!shift && selection.anchor != selection.caret)
				{
					selection.anchor = selection.caret;
					result = true;
				}
			}
			else
			{
				foreach (Selection selection in selections)
				{
					Place place = lines.PlaceOf(selection.caret);
					if (place.iLine > 0)
					{
						Line line = lines[place.iLine - 1];
						place = new Place(Math.Min(line.chars.Count, line.NormalIndexOfPos(selection.preferredPos)), place.iLine - 1);
						result = true;
					}
					selection.caret = lines.IndexOf(place);
					if (!shift && selection.anchor != selection.caret)
					{
						selection.anchor = selection.caret;
						result = true;
					}
				}
			}
			return result;
		}

		public bool MoveDown(bool shift)
		{
			bool result = false;
			if (lines.wordWrap && selections.Count == 1)
			{
				Selection selection = lines.LastSelection;
				Place place = lines.PlaceOf(selection.caret);
				Line line = lines[place.iLine];
				Pos pos = line.WWPosOfIndex(place.iChar);
				Place newPlace = place;
				if (pos.iy < line.cutOffs.count)
				{
					newPlace = new Place(line.WWNormalIndexOfPos(selection.wwPreferredPos, pos.iy + 1), place.iLine);
					result = true;
				}
				else if (place.iLine < lines.LinesCount - 1)
				{
					line = lines[place.iLine + 1];
					newPlace = new Place(line.WWNormalIndexOfPos(selection.wwPreferredPos, 0), place.iLine + 1);
					result = true;
				}
				selection.caret = lines.IndexOf(newPlace);
				if (!shift && selection.anchor != selection.caret)
				{
					selection.anchor = selection.caret;
					result = true;
				}
			}
			else
			{
				foreach (Selection selection in selections)
				{
					Place place = lines.PlaceOf(selection.caret);
					if (place.iLine < lines.LinesCount - 1)
					{
						Line line = lines[place.iLine + 1];
						place = new Place(Math.Min(line.chars.Count, line.NormalIndexOfPos(selection.preferredPos)), place.iLine + 1);
						result = true;
					}
					selection.caret = lines.IndexOf(place);
					if (!shift && selection.anchor != selection.caret)
					{
						selection.anchor = selection.caret;
						result = true;
					}
				}
			}
			return result;
		}

		public void MoveEnd(bool shift)
		{
			if (lines.wordWrap && selections.Count == 1)
			{
				Selection selection = lines.LastSelection;
				Place place = lines.PlaceOf(selection.caret);
				Line line = lines[place.iLine];
				if (line.cutOffs.count > 0)
				{
					Pos pos = line.WWPosOfIndex(place.iChar);
					if (pos.iy < line.cutOffs.count)
					{
						int sublineStart = line.cutOffs.buffer[pos.iy].iChar;
						if (place.iChar < sublineStart - 1)
						{
							Place newPlace = new Place(sublineStart - 1, place.iLine);
							selection.caret = lines.IndexOf(newPlace);
							if (!shift)
								selection.anchor = selection.caret;
							lines.SetPreferredPos(selection, newPlace);
							return;
						}
					}
				}
			}
			foreach (Selection selection in selections)
			{
				Place caret = lines.PlaceOf(selection.caret);
				caret.iChar = lines[caret.iLine].NormalCount;
				selection.caret = lines.IndexOf(caret);
				if (!shift)
					selection.anchor = selection.caret;
				lines.SetPreferredPos(selection, caret);
			}
		}

		public void MoveHome(bool shift)
		{
			if (lines.wordWrap && selections.Count == 1)
			{
				Selection selection = lines.LastSelection;
				Place place = lines.PlaceOf(selection.caret);
				Line line = lines[place.iLine];
				if (line.cutOffs.count > 0)
				{
					Pos pos = line.WWPosOfIndex(place.iChar);
					if (pos.iy > 0)
					{
						int sublineStart = line.cutOffs.buffer[pos.iy - 1].iChar;
						if (place.iChar - sublineStart > 0)
						{
							Place newPlace = new Place(sublineStart, place.iLine);
							selection.caret = lines.IndexOf(newPlace);
							if (!shift)
								selection.anchor = selection.caret;
							lines.SetPreferredPos(selection, newPlace);
							return;
						}
					}
				}
			}
			foreach (Selection selection in selections)
			{
				Place caret = lines.PlaceOf(selection.caret);
				Line line = lines[caret.iLine];
				int charsCount = line.NormalCount;
				int minIChar = 0;
				while (minIChar < charsCount && char.IsWhiteSpace(line.chars[minIChar].c))
				{
					minIChar++;
				}
				caret.iChar = caret.iChar > minIChar ? minIChar : 0;
				selection.caret = lines.IndexOf(caret);
				if (!shift)
					selection.anchor = selection.caret;
				lines.SetPreferredPos(selection, caret);
			}
		}

		public void DocumentStart(bool shift)
		{
			foreach (Selection selection in selections)
			{
				selection.caret = 0;
				if (!shift)
					selection.anchor = selection.caret;
			}
			lines.JoinSelections();
			lines.LastSelection.preferredPos = 0;
		}

		public void DocumentEnd(bool shift)
		{
			foreach (Selection selection in selections)
			{
				selection.caret = lines.charsCount;
				if (!shift)
					selection.anchor = selection.caret;
			}
			lines.JoinSelections();
			Place place = lines.PlaceOf(lines.charsCount);
			lines.SetPreferredPos(lines.LastSelection, place);
		}

		public void PutCursor(Pos pos, bool moving)
		{
			PutCursor(lines.PlaceOf(pos), moving);
		}

		public void PutCursor(Place place, bool moving)
		{
			Selection selection = selections[selections.Count - 1];
			Place caret = lines.Normalize(place);
			selection.caret = lines.IndexOf(caret);
			if (!moving)
				selection.anchor = selection.caret;
			Line line = lines[caret.iLine];
			lines.SetPreferredPos(selection, caret);
		}

		public enum CharType
		{
			Identifier,
			Space,
			Punctuation,
			Special
		}

		private static CharType GetCharType(char c)
        {
			if (c == ' ' || c == '\t')
				return CharType.Space;
			if (c == '\r' || c == '\n' || c == '\0')
				return CharType.Special;
			return char.IsLetterOrDigit(c) || c == '_' ? CharType.Identifier : CharType.Punctuation;
        }

		public void MoveWordRight(bool shift)
		{
			MoveWordRight(lines, shift);
		}

		public void MoveWordLeft(bool shift)
		{
			MoveWordLeft(lines, shift);
		}

		public static void MoveWordRight(LineArray lines, bool shift)
		{
			foreach (Selection selection in lines.selections)
			{
				PlaceIterator iterator = lines.GetCharIterator(selection.caret);

				bool wasSpace = false;
				while (GetCharType(iterator.RightChar) == CharType.Space)
				{
					wasSpace = true;
					if (!iterator.MoveRightWithRN())
						break;
				}
				bool wasIdentifier = false;
				CharType type = GetCharType(iterator.RightChar);
				if (type == CharType.Identifier || type == CharType.Punctuation)
				{
					CharType typeI = type;
					while (typeI == type)
					{
						wasIdentifier = true;
						if (!iterator.MoveRightWithRN())
							break;
						typeI = GetCharType(iterator.RightChar);
					}
				}
				if (!wasIdentifier && (!wasSpace || iterator.RightChar != '\n' && iterator.RightChar != '\r'))
					iterator.MoveRightWithRN();

				selection.caret = iterator.Position;
				if (!shift)
					selection.anchor = iterator.Position;
				lines.SetPreferredPos(selection, iterator.Place);
			}
		}

		public static void MoveWordLeft(LineArray lines, bool shift)
		{
			foreach (Selection selection in lines.selections)
			{
				PlaceIterator iterator = lines.GetCharIterator(selection.caret);

				bool wasSpace = false;
				while (GetCharType(iterator.LeftChar) == CharType.Space)
				{
					wasSpace = true;
					if (!iterator.MoveLeftWithRN())
						break;
				}
				bool wasIdentifier = false;
				CharType type = GetCharType(iterator.LeftChar);
				if (type == CharType.Identifier || type == CharType.Punctuation)
				{
					CharType typeI = type;
					while (typeI == type)
					{
						wasIdentifier = true;
						if (!iterator.MoveLeftWithRN())
							break;
						typeI = GetCharType(iterator.LeftChar);
					}
				}
				if (!wasIdentifier && (!wasSpace || iterator.LeftChar != '\n' && iterator.LeftChar != '\r'))
					iterator.MoveLeftWithRN();

				selection.caret = iterator.Position;
				if (!shift)
					selection.anchor = iterator.Position;
				lines.SetPreferredPos(selection, iterator.Place);
			}
		}

		public void PutCursorDown()
		{
			if (lines.selections.Count > 1)
			{
				if (lines.selections[lines.selections.Count - 2].caret > lines.LastSelection.caret)
				{
					lines.selections.RemoveAt(lines.selections.Count - 1);
					return;
				}
			}
			int preferredPos = lines.LastSelection.preferredPos;
			int wwPreferredPos = lines.LastSelection.wwPreferredPos;
			Pos pos = lines.PosOf(lines.PlaceOf(lines.LastSelection.caret));
			if (pos.iy < lines.LinesCount - 1)
			{
				pos.ix = preferredPos;
				pos.iy++;
				PutNewCursor(pos);
				lines.LastSelection.preferredPos = preferredPos;
				lines.LastSelection.wwPreferredPos = wwPreferredPos;
			}
		}

		public void PutCursorUp()
		{
			if (lines.selections.Count > 1)
			{
				if (lines.selections[lines.selections.Count - 2].caret < lines.LastSelection.caret)
				{
					lines.selections.RemoveAt(lines.selections.Count - 1);
					return;
				}
			}
			int preferredPos = lines.LastSelection.preferredPos;
			int wwPreferredPos = lines.LastSelection.wwPreferredPos;
			Pos pos = lines.PosOf(lines.PlaceOf(lines.LastSelection.caret));
			if (pos.iy > 0)
			{
				pos.ix = preferredPos;
				pos.iy--;
				PutNewCursor(pos);
				lines.LastSelection.preferredPos = preferredPos;
				lines.LastSelection.wwPreferredPos = wwPreferredPos;
			}
		}

		public void PutNewCursor(Pos pos)
		{
			PutNewCursor(lines.PlaceOf(pos));
		}

		public void PutNewCursor(Place place)
		{
			int caret = lines.IndexOf(place);
			bool contains = false;
			foreach (Selection selection in selections)
			{
				if (selection.Contains(caret))
				{
					contains = true;
					break;
				}
			}
			if (!contains)
			{
				lines.selections.Add(new Selection());
			}
			else
			{
				ClearMinorSelections();
			}
			PutCursor(place, false);
		}

		public bool ClearMinorSelections()
		{
			if (lines.selections.Count > 1)
			{
				lines.selections.RemoveRange(1, lines.selections.Count - 1);
				return true;
			}
			return false;
		}

		public bool ClearFirstMinorSelections()
		{
			if (lines.selections.Count > 1)
			{
				lines.selections.RemoveRange(0, lines.selections.Count - 1);
				return true;
			}
			return false;
		}

		public void SelectAll()
		{
			ClearMinorSelections();
			Selection selection = lines.LastSelection;
			selection.anchor = lines.charsCount;
			selection.caret = 0;
		}

		public void SelectAllToEnd()
		{
			ClearMinorSelections();
			Selection selection = lines.LastSelection;
			selection.anchor = 0;
			selection.caret = lines.charsCount;
		}

		private CommandType lastCommandType;
		private long lastTime;

		private void ResetCommandsBatching()
		{
			lastCommandType = CommandType.None;
			lastTime = 0;
		}

		private bool Execute(Command command)
		{
			if (isReadonly && command.type.changesText)
				return false;
			command.lines = lines;
			command.selections = selections;
			long time = DateTime.UtcNow.Ticks;
			if (command.type != lastCommandType)
			{
				if (history.LastCommand != null)
					history.LastCommand.marked = true;
				lastCommandType = command.type;
				lastTime = time;
			}
			else if (new TimeSpan(time - lastTime).TotalMilliseconds > 1000)
			{
				if (history.LastCommand != null)
					history.LastCommand.marked = true;
				lastCommandType = command.type;
				lastTime = time;
			}
			bool result = command.Init();
			if (result)
				history.ExecuteInited(command);
			return result;
		}

		public void Undo()
		{
			ResetCommandsBatching();
			while (true)
			{
				history.Undo();
				if (history.LastCommand == null || history.LastCommand.marked)
					break;
			}
		}

		public void Redo()
		{
			ResetCommandsBatching();
			while (true)
			{
				history.Redo();
				if (history.NextCommand == null)
					break;
				if (history.NextCommand.marked)
				{
					history.Redo();
					break;
				}
			}
		}

		public void Backspace()
		{
			Execute(new BackspaceCommand());
		}

		public void Delete()
		{
			Execute(new DeleteCommand());
		}

		public void InsertText(string text)
		{
			Execute(new InsertTextCommand(text, null));
		}

		public void InsertLineBreak()
		{
			lines.JoinSelections();
			string[] texts = new string[selections.Count];
			for (int i = 0; i < selections.Count; i++)
			{
				Selection selection = selections[i];
				Place place = lines.PlaceOf(selection.Left);
				Line line = lines[place.iLine];
				texts[i] = lines.lineBreak + GetLineBreakFirstSpaces(line, place.iChar);
			}
			Execute(new InsertTextCommand(null, texts));
		}

		private static string GetLineBreakFirstSpaces(Line line, int iChar)
		{
			int count = line.chars.Count;
			int spacesCount = 0;
			for (int i = 0; i < count; i++)
			{
				char c = line.chars[i].c;
				if (c != '\t' && c != ' ')
					break;
				spacesCount++;
			}
			if (iChar >= spacesCount)
			{
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < count; i++)
				{
					char c = line.chars[i].c;
					if (c != '\t' && c != ' ')
						break;
					builder.Append(c);
				}
				return builder.ToString();
			}
			return "";
		}

		public void Copy()
		{
			Execute(new CopyCommand());
		}

		public void Cut()
		{
			Copy();
			EraseSelection();
		}

		public void EraseSelection()
		{
			Execute(new EraseSelectionCommand());
		}

		public void Paste()
		{
			Execute(new PasteCommand());
		}

		public bool ShiftLeft()
		{
			return Execute(new ShiftCommand(true));
		}

		public bool ShiftRight()
		{
			return Execute(new ShiftCommand(false));
		}

		public bool RemoveWordLeft()
		{
			return Execute(new RemoveWordCommand(true));
		}

		public bool RemoveWordRight()
		{
			return Execute(new RemoveWordCommand(false));
		}

		public bool MoveLineUp()
		{
			return Execute(new MoveLineCommand(true));
		}

		public bool MoveLineDown()
		{
			return Execute(new MoveLineCommand(false));
		}

		private void GetWordSelection(Place place, out int position, out int count)
		{
			Line line = lines[place.iLine];
			int normalCount = line.NormalCount;
			int left;
			int right;
			if (normalCount > 0)
			{
				int iChar = place.iChar;
				if (iChar >= normalCount)
					iChar = normalCount - 1;
				CharType charType = GetCharType(line.chars[iChar].c);
				left = iChar;
				while (left > 0 && GetCharType(line.chars[left - 1].c) == charType)
				{
					left--;
				}
				right = iChar + 1;
				while (right < normalCount && GetCharType(line.chars[right].c) == charType)
				{
					right++;
				}
			}
			else
			{
				left = 0;
				right = 0;
			}
			position = lines.IndexOf(new Place(left, place.iLine));
			count = right - left;
		}

		public void SelectWordAtPlace(Place place, bool newSelection)
		{
			int position;
			int count;
			GetWordSelection(place, out position, out count);
			if (newSelection)
			{
				selections.Add(new Selection());
			}
			else
			{
				ClearMinorSelections();
			}
			Selection selection = lines.LastSelection;
			selection.anchor = position;
			selection.caret = position + count;
			lines.JoinSelections();
		}

		public void SelectNextText()
		{
			if (lines.LastSelection.Empty)
			{
				foreach (Selection selection in selections)
				{
					if (selection.Empty)
					{
						Place place = lines.PlaceOf(selection.caret);
						int position;
						int count;
						GetWordSelection(place, out position, out count);
						selection.anchor = position;
						selection.caret = position + count;
						Place caret = lines.PlaceOf(selection.caret);
						lines.SetPreferredPos(selection, caret);
					}
				}
			}
			else
			{
				Selection lastSelection = lines.LastSelection;
				string text = lines.GetText(lastSelection.Left, lastSelection.Count);
				int position = lines.IndexOf(text, lastSelection.Right);
				if (position == -1)
					position = lines.IndexOf(text, 0);
				while (true)
				{
					if (position == -1 || !lines.IntersectSelections(position, position + text.Length))
						break;
					int newPosition = lines.IndexOf(text, position + text.Length);
					if (newPosition == position)
						break;
					position = newPosition;
				}
				if (position != -1)
				{
					Selection selection = new Selection();
					selection.anchor = position;
					selection.caret = position + text.Length;
					Place caret = lines.PlaceOf(selection.caret);
					lines.SetPreferredPos(selection, caret);
					selections.Add(selection);
				}
			}
		}

		public bool AllSelectionsEmpty { get { return lines.AllSelectionsEmpty; } }

		public void ScrollPage(bool isUp, bool withSelection)
		{
			lines.scroller.ScrollPage(isUp, this, withSelection);
		}

		public void Scroll(int x, int y)
		{
			lines.scroller.Scroll(x, y);
		}

		public void NeedScrollToCaret()
		{
			lines.scroller.needScrollToCaret = true;
		}

		public Place SoftNormalizedPlaceOf(int index)
		{
			return lines.SoftNormalizedPlaceOf(index);
		}

		public IEnumerable<Selection> Selections { get { return selections; } }
		public Selection LastSelection { get { return lines.LastSelection; } }

		public void RemoveSelections(List<Selection> selections)
		{
			foreach (Selection selection in selections)
			{
				lines.selections.Remove(selection);
			}
			lines.JoinSelections();
		}

		public void JoinSelections()
		{
			lines.JoinSelections();
		}

		public void SetStyleRange(StyleRange range)
		{
			lines.SetStyleRange(range);
		}

		public void SetStyleRanges(List<StyleRange> ranges)
		{
			foreach (StyleRange range in ranges)
			{
				lines.SetStyleRange(range);
			}
		}
	}
}
