using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace MulticaretEditor
{
	public class Controller
	{
		private readonly LineArray lines;
		private readonly List<Selection> selections;

		public Controller(LineArray lines)
		{
			this.lines = lines;
			this.selections = lines.selections;
			processor = new CommandProcessor(this, lines, selections);
			processor.ResetCommandsBatching();
		}

		public readonly CommandProcessor processor;
		public bool isReadonly;
		public MacrosExecutor macrosExecutor;

		public LineArray Lines { get { return lines; } }

		public void InitText(string text)
		{
			lines.SetText(text);
			processor.DoAfterInitText();
		}
		
		private void DoAfterMove()
		{
			processor.ResetCommandsBatching();
		}

		public bool MoveRight(bool shift)
		{
			bool result = PrivateMoveRight(lines, shift);
			DoAfterMove();
			return result;
		}

		public bool MoveLeft(bool shift)
		{
			bool result = PrivateMoveLeft(lines, shift);
			DoAfterMove();
			return result;
		}

		private bool PrivateMoveRight(LineArray lines, bool shift)
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
						++selection.caret;
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

		private bool PrivateMoveLeft(LineArray lines, bool shift)
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
						--selection.caret;
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
						place = new Place(Math.Min(line.charsCount, line.NormalIndexOfPos(selection.preferredPos)), place.iLine - 1);
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
			DoAfterMove();
			return result;
		}
		
		public bool ViLogicMoveUp(bool shift)
		{
			bool result = false;
			foreach (Selection selection in selections)
			{
				Place place = lines.PlaceOf(selection.caret);
				if (place.iLine > 0)
				{
					Line line = lines[place.iLine - 1];
					place = new Place(Math.Min(line.NormalCount, place.iChar), place.iLine - 1);
					result = true;
				}
				selection.caret = lines.IndexOf(place);
				if (!shift && selection.anchor != selection.caret)
				{
					selection.anchor = selection.caret;
					result = true;
				}
			}
			DoAfterMove();
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
				else
				{
					place.iLine = lines.LinesCount - 1;
					line = lines[place.iLine];
					if (place.iChar != line.NormalCount)
					{
						place.iChar = line.NormalCount;
						newPlace = place;
						result = true;
					}
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
						place = new Place(Math.Min(line.charsCount, line.NormalIndexOfPos(selection.preferredPos)), place.iLine + 1);
						result = true;
					}
					else
					{
						place.iLine = lines.LinesCount - 1;
						Line line = lines[place.iLine];
						if (place.iChar != line.NormalCount)
						{
							place.iChar = line.NormalCount;
							result = true;
						}
					}
					selection.caret = lines.IndexOf(place);
					if (!shift && selection.anchor != selection.caret)
					{
						selection.anchor = selection.caret;
						result = true;
					}
				}
			}
			DoAfterMove();
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
							selection.SetEmptyIfNotShift(shift);
							lines.SetPreferredPos(selection, newPlace);
							DoAfterMove();
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
				selection.SetEmptyIfNotShift(shift);
				lines.SetPreferredPos(selection, caret);
			}
			DoAfterMove();
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
							selection.SetEmptyIfNotShift(shift);
							lines.SetPreferredPos(selection, newPlace);
							DoAfterMove();
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
					++minIChar;
				}
				caret.iChar = caret.iChar > minIChar ? minIChar : 0;
				selection.caret = lines.IndexOf(caret);
				selection.SetEmptyIfNotShift(shift);
				lines.SetPreferredPos(selection, caret);
			}
			DoAfterMove();
		}

		public void DocumentStart(bool shift)
		{
			foreach (Selection selection in selections)
			{
				selection.caret = 0;
				selection.SetEmptyIfNotShift(shift);
			}
			lines.JoinSelections();
			lines.LastSelection.preferredPos = 0;
			DoAfterMove();
		}

		public void DocumentEnd(bool shift)
		{
			foreach (Selection selection in selections)
			{
				selection.caret = lines.charsCount;
				selection.SetEmptyIfNotShift(shift);
			}
			lines.JoinSelections();
			Place place = lines.PlaceOf(lines.charsCount);
			lines.SetPreferredPos(lines.LastSelection, place);
			DoAfterMove();
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
			lines.SetPreferredPos(selection, caret);
			DoAfterMove();
		}
		
		public void PutCursorForcedly(Place place, bool moving)
		{
			Selection selection = selections[selections.Count - 1];
			selection.caret = lines.IndexOf(place);
			if (!moving)
				selection.anchor = selection.caret;
			lines.SetPreferredPos(selection, place);
			DoAfterMove();
		}

		private static CharType GetCharType(char c)
        {
			if (c == ' ' || c == '\t')
				return CharType.Space;
			if (c == '\r' || c == '\n' || c == '\0')
				return CharType.Special;
			return char.IsLetterOrDigit(c) || c == '_' ? CharType.Identifier : CharType.Punctuation;
        }
        
        public static bool IsSpaceOrNewLine(char c)
        {
			return c == ' ' || c == '\t' || c == '\r' || c == '\n';
        }

		public void MoveWordRight(bool shift)
		{
			PrivateMoveWordRight(shift, shift);
			DoAfterMove();
		}

		public void MoveWordLeft(bool shift)
		{
			PrivateMoveWordLeft(shift);
			DoAfterMove();
		}

		private void PrivateMoveWordRight(bool shift, bool shiftMove)
		{
			foreach (Selection selection in lines.selections)
			{
				Moves moves = new Moves(lines, selection.caret);
				moves.NPP_WordRight(shiftMove);
				moves.Apply(selection, shift);
			}
		}

		private void PrivateMoveWordLeft(bool shift)
		{
			foreach (Selection selection in lines.selections)
			{
				Moves moves = new Moves(lines, selection.caret);
				moves.NPP_WordLeft();
				moves.Apply(selection, shift);
			}
		}

		public void PutCursorDown()
		{
			if (lines.selections.Count > 1)
			{
				if (lines.selections[lines.selections.Count - 2].caret > lines.LastSelection.caret)
				{
					lines.selections.RemoveAt(lines.selections.Count - 1);
					DoAfterMove();
					return;
				}
			}
			int preferredPos = lines.LastSelection.preferredPos;
			int wwPreferredPos = lines.LastSelection.wwPreferredPos;
			Pos pos = lines.PosOf(lines.PlaceOf(lines.LastSelection.caret));
			if (pos.iy < lines.LinesCount - 1)
			{
				pos.ix = preferredPos;
				++pos.iy;
				PutNewCursor(pos);
				lines.LastSelection.preferredPos = preferredPos;
				lines.LastSelection.wwPreferredPos = wwPreferredPos;
			}
			DoAfterMove();
		}

		public void PutCursorUp()
		{
			if (lines.selections.Count > 1)
			{
				if (lines.selections[lines.selections.Count - 2].caret < lines.LastSelection.caret)
				{
					lines.selections.RemoveAt(lines.selections.Count - 1);
					DoAfterMove();
					return;
				}
			}
			int preferredPos = lines.LastSelection.preferredPos;
			int wwPreferredPos = lines.LastSelection.wwPreferredPos;
			Pos pos = lines.PosOf(lines.PlaceOf(lines.LastSelection.caret));
			if (pos.iy > 0)
			{
				pos.ix = preferredPos;
				--pos.iy;
				PutNewCursor(pos);
				lines.LastSelection.preferredPos = preferredPos;
				lines.LastSelection.wwPreferredPos = wwPreferredPos;
			}
			DoAfterMove();
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
			DoAfterMove();
		}

		public void SelectAllToEnd()
		{
			ClearMinorSelections();
			Selection selection = lines.LastSelection;
			selection.anchor = 0;
			selection.caret = lines.charsCount;
			DoAfterMove();
		}

		public void Backspace()
		{
			processor.Execute(new BackspaceCommand());
		}

		public void Delete()
		{
			processor.Execute(new DeleteCommand());
		}

		public void InsertText(string text)
		{
			if (lines.autoindent && text == "}")
			{
				processor.Execute(new InsertIndentedCketCommand());
				return;
			}
			if (lines.spacesInsteadTabs && text == "\t")
			{
				text = new string(' ', lines.tabSize);
			}
			processor.Execute(new InsertTextCommand(text, null, true));
		}

		public void InsertLineBreak()
		{
			lines.JoinSelections();
			string[] texts = new string[selections.Count];
			for (int i = 0; i < selections.Count; ++i)
			{
				Selection selection = selections[i];
				Place place = lines.PlaceOf(selection.Left);
				Line line = lines[place.iLine];
				string text = line.GetLineBreakFirstSpaces(place.iChar);
				if (lines.autoindent)
				{
					int firstTrimmedLength = line.GetLastLeftSpace(place.iChar);
					if (firstTrimmedLength > 0 && line.chars[firstTrimmedLength - 1].c == '{')
					{
						text = (lines.spacesInsteadTabs ? new string(' ', lines.tabSize) : "\t") + text;
					}
				}
				texts[i] = lines.lineBreak + text;
			}
			processor.Execute(new InsertTextCommand(null, texts, true));
		}
		
		public void ReplaceText(SimpleRange[] orderedRanges, string newText)
		{
			processor.Execute(new ReplaceTextCommand(orderedRanges, newText));
		}

		public void Copy()
		{
			if (lines.AllSelectionsEmpty)
			{
				CopyLines('*', GetLineRangesByLefts());
			}
			else
			{
				ViCopy('*');
			}
		}

		public void Cut()
		{
			if (lines.AllSelectionsEmpty)
			{
				List<SimpleRange> ranges = GetLineRangesByLefts();
				CopyLines('*', ranges);
				processor.Execute(new EraseLinesCommand(ranges));
			}
			else
			{
				ViCopy('*');
				EraseSelection();
			}
		}
		
		private void CopyLines(char register, List<SimpleRange> ranges)
		{
			lines.JoinSelections();
			StringBuilder text = new StringBuilder();
			foreach (SimpleRange range  in ranges)
			{
				if (range.count > 0)
				{
					LineIterator iterator = lines.GetLineRange(range.index, range.count);
					while (iterator.MoveNext())
					{
						Line line = iterator.current;
						int normalCount = line.NormalCount;
						Char[] chars = line.chars;
						for (int i = 0; i < normalCount; ++i)
						{
							text.Append(chars[i].c);
						}
						if (normalCount < line.charsCount)
						{
							text.Append(line.GetRN());
						}
						else
						{
							text.Append(lines.lineBreak);
						}
					}
				}
			}
			ClipboardExecutor.PutToRegister(register, text.ToString());
		}

		public void EraseSelection()
		{
			processor.Execute(new EraseSelectionCommand());
		}

		public void Paste()
		{
			processor.Execute(new PasteCommand('*'));
		}

		public bool ShiftLeft()
		{
			return processor.Execute(new ShiftCommand(true));
		}

		public bool ShiftRight()
		{
			return processor.Execute(new ShiftCommand(false));
		}

		public bool RemoveWordLeft()
		{
			if (!lines.AllSelectionsEmpty)
			{
				return false;
			}
			processor.BeginBatch();
			processor.Execute(new SavePositions());
			PrivateMoveWordLeft(true);
			bool result = processor.Execute(new EraseSelectionCommand());
			processor.EndBatch();
			return result;
		}

		public bool RemoveWordRight()
		{
			if (!lines.AllSelectionsEmpty)
			{
				return false;
			}
			processor.BeginBatch();
			processor.Execute(new SavePositions());
			PrivateMoveWordRight(true, false);
			bool result = processor.Execute(new EraseSelectionCommand());
			processor.EndBatch();
			return result;
		}

		public bool MoveLineUp()
		{
			return processor.Execute(new MoveLineCommand(true));
		}

		public bool MoveLineDown()
		{
			return processor.Execute(new MoveLineCommand(false));
		}
		
		public string GetWord(Place place)
		{
			int position;
			int count;
			GetWordSelection(place, out position, out count);
			return lines.GetText(position, count);
		}
		
		public string GetWord(Place place, out int position)
		{
			int count;
			GetWordSelection(place, out position, out count);
			return lines.GetText(position, count);
		}
		
		public string GetLeftWord(Place place)
		{
			Line line = lines[place.iLine];
			int normalCount = line.NormalCount;
			int left;
			if (normalCount > 0)
			{
				int iChar = place.iChar;
				if (iChar > normalCount)
					iChar = normalCount;
				left = iChar;
				while (left > 0 && GetCharType(line.chars[left - 1].c) == CharType.Identifier)
				{
					--left;
				}
				if (left < iChar)
				{
					StringBuilder builder = new StringBuilder();
					for (int i = left; i < iChar; ++i)
					{
						builder.Append(line.chars[i].c);
					}
					return builder.ToString();
				}
			}
			return "";
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
					--left;
				}
				right = iChar + 1;
				while (right < normalCount && GetCharType(line.chars[right].c) == charType)
				{
					++right;
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
		
		public bool UnselectPrevText()
		{
			if (selections.Count > 1)
			{
				selections.RemoveAt(selections.Count - 2);
				return true;
			}
			return false;
		}
		
		public void SelectAllMatches()
		{
			Selection lastSelection = lines.LastSelection;
			if (lastSelection.Empty)
			{
				SelectNextText();
			}
			if (lastSelection.Empty)
			{
				return;
			}
			string all = lines.GetText();
			string text = all.Substring(lastSelection.Left, lastSelection.Count);
			int start = 0;
			bool first = true;
			while (true)
			{
				int length = text.Length;
				int index =  all.IndexOf(text, start);
				if (index == -1)
				{
					break;
				}
				if (first)
				{
					first = false;
					ClearMinorSelections();
				}
				else
				{
					selections.Add(new Selection());
				}
				Selection selection = selections[selections.Count - 1];
				selection.anchor = index;
				selection.caret = index + length;
				lines.SetPreferredPos(selection, lines.PlaceOf(selection.caret));
				start = index + length;
			}
		}

		public void ChangeCase(bool upper)
		{
			lines.JoinSelections();
			string[] texts = new string[selections.Count];
			bool needChange = false;
			for (int i = 0; i < selections.Count; ++i)
			{
				Selection selection = selections[i];
				string text = lines.GetText(selection.Left, selection.Count);
				if (text.Length > 0)
					needChange = true;
				texts[i] = upper ? text.ToUpperInvariant() : text.ToLowerInvariant();
			}
			if (needChange)
				processor.Execute(new InsertTextCommand(null, texts, false));
		}

		public bool AllSelectionsEmpty { get { return lines.AllSelectionsEmpty; } }

		public void ScrollPage(bool isUp, bool withSelection)
		{
			lines.scroller.ScrollPage(isUp, this, withSelection);
		}

		public void ScrollRelative(int x, int y)
		{
			lines.scroller.ScrollRelative(x, y);
		}

		public void NeedScrollToCaret()
		{
			lines.scroller.needScrollToCaret = true;
		}

		public Place SoftNormalizedPlaceOf(int index)
		{
			return lines.SoftNormalizedPlaceOf(index);
		}

		public int SelectionsCount { get { return selections.Count; } }
		public IEnumerable<Selection> Selections { get { return selections; } }
		public Selection LastSelection { get { return lines.LastSelection; } }

		public void RemoveEmptyOrMinorSelections()
		{
			bool allEmpty = true;
			for (int i = selections.Count; i-- > 0;)
			{
				if (!selections[i].Empty)
				{
					allEmpty = false;
					break;
				}
			}
			if (allEmpty)
			{
				if (selections.Count > 1)
				{
					selections.RemoveRange(0, selections.Count - 1);
				}
			}
			else
			{
				for (int i = selections.Count; i-- > 0;)
				{
					if (selections[i].Empty)
					{
						selections.RemoveAt(i);
					}
				}
			}
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

		private int markLeft = -1;
		private int markRight = -1;
		private bool markEnabled = true;
		
		private string GetWordForSelection(Selection selection)
		{
			Place leftPlace = lines.PlaceOf(selection.Left);
			Place rightPlace = lines.PlaceOf(selection.Right);
			if (leftPlace.iLine != rightPlace.iLine)
			{
				return null;
			}
			Line line = lines[leftPlace.iLine];
			if ((leftPlace.iChar == 0 || GetCharType(line.chars[leftPlace.iChar - 1].c) != CharType.Identifier) &&
				(rightPlace.iChar == line.charsCount || GetCharType(line.chars[rightPlace.iChar].c) != CharType.Identifier))
			{
				for (int i = leftPlace.iChar; i < rightPlace.iChar; ++i)
				{
					if (IsWordSeparator(line.chars[i].c))
					{
						return null;
					}
				}
				int offset = leftPlace.iChar;
				int length = rightPlace.iChar - offset;
				char[] buffer = new char[length];
				for (int i = 0; i < length; ++i)
				{
					buffer[i] = line.chars[offset + i].c;
				}
				return new string(buffer);
			}
			return null;
		}

		public void MarkWordOnPaint(bool enabled)
		{
			//System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			//sw.Start();
			Selection selection = selections[0];
			if (selection.Empty || !enabled)
			{
				if (lines.marksByLine.Count != 0)
					lines.marksByLine.Clear();
				lines.markedWord = null;
				markLeft = -1;
				markRight = -1;
				return;
			}
			if (selection.Left == markLeft && selection.Right == markRight && markEnabled == enabled)
				return;
			markLeft = selection.Left;
			markRight = selection.Right;
			markEnabled = enabled;
			string word = GetWordForSelection(selection);
			if (word == null)
			{
				if (lines.marksByLine.Count != 0)
					lines.marksByLine.Clear();
				lines.markedWord = null;
				return;
			}
			lines.markedWord = word;
			char c0 = word[0];
			int wordLength = word.Length;

			PredictableList<int> indexList = new PredictableList<int>();
			int lineIndex = 0;
			for (int i = 0; i < lines.blocksCount; ++i)
			{
				LineBlock block = lines.blocks[i];
				for (int j = 0; j < block.count; ++j)
				{
					Line lineI = block.array[j];
					Char[] chars = lineI.chars;
					int charsCount = lineI.NormalCount;
					indexList.Clear();
					int k = 0;
					if (k + wordLength <= charsCount)
					{
						bool matched = chars[k].c == c0;
						if (matched)
						{
							for (int wordK = 1; wordK < wordLength; ++wordK)
							{
								if (chars[k + wordK].c != word[wordK])
								{
									matched = false;
									k += wordK;
									break;
								}
							}
						}
						if (matched &&
							(k + wordLength >= charsCount || IsWordSeparator(chars[k + wordLength].c)))
						{
							indexList.Add(k);
						}
					}
					if (k == 0)
					{
						++k;
					}
					for (; k < charsCount; ++k)
					{
						if (chars[k].c == c0 && IsWordSeparator(chars[k - 1].c))
						{
							if (k + wordLength <= charsCount)
							{
								bool matched = true;
								for (int wordK = 1; wordK < wordLength; ++wordK)
								{
									if (chars[k + wordK].c != word[wordK])
									{
										matched = false;
										k += wordK;
										break;
									}
								}
								if (matched &&
									(k + wordLength >= charsCount || IsWordSeparator(chars[k + wordLength].c)))
								{
									indexList.Add(k);
								}
							}
						}
					}
					if (indexList.count > 0)
						lines.marksByLine[lineIndex] = indexList.ToArray();
					++lineIndex;
				}
			}
			//sw.Stop();
			//Console.WriteLine(sw.ElapsedMilliseconds + " ms");
		}
		
		private static bool IsWordSeparator(char c)
		{
			switch (c)
			{
				case ' ':
				case '\t':
				case '!':
				case '%':
				case '&':
				case '(':
				case ')':
				case '*':
				case '+':
				case ',':
				case '-':
				case '.':
				case '/':
				case ':':
				case ';':
				case '<':
				case '=':
				case '>':
				case '?':
				case '[':
				case '\\':
				case ']':
				case '^':
				case '{':
				case '|':
				case '}':
				case '~':
				case '"':
				case '\'':
				case '@':
				case '#':
				case '$':
				case '`':
					return true;
				default:
					return false;
			}
		}
		
		private int markedBracketCaret = -1;
		private bool markedBracketEnabled = true;

		public void MarkBracketOnPaint(bool enabled)
		{
			if (!enabled)
			{
				markedBracketCaret = -1;
				lines.markedBracket = false;
				return;
			}
			Selection selection = selections[0];
			if (!selection.Empty || selections.Count != 1)
			{
				markedBracketCaret = -1;
				lines.markedBracket = false;
				return;
			}
			if (markedBracketCaret == selection.caret && markedBracketEnabled == enabled)
				return;
			markedBracketCaret = selection.caret;
			markedBracketEnabled = enabled;

			Place place = lines.PlaceOf(selection.caret);
			Line line = lines[place.iLine];
			int iChar = -1;
			int position = selection.caret;
			char c0 = '\0';
			if (place.iChar > 0)
			{
				c0 = line.chars[place.iChar - 1].c;
				if (c0 == '{' || c0 == '}' || c0 == '(' || c0 == ')')
				{
					iChar = place.iChar - 1;
					--position;
				}
			}
			if (iChar == -1 && place.iChar < line.charsCount)
			{
				c0 = line.chars[place.iChar].c;
				if (c0 == '{' || c0 == '}' || c0 == '(' || c0 == ')')
					iChar = place.iChar;
			}
			if (iChar == -1)
			{
				lines.markedBracket = false;
				return;
			}
			char c1;
			bool direct;
			if (c0 == '{')
			{
				c1 = '}';
				direct = true;
			}
			else if (c0 == '}')
			{
				c1 = '{';
				direct = false;
			}
			else if (c0 == '(')
			{
				c1 = ')';
				direct = true;
			}
			else
			{
				c1 = '(';
				direct = false;
			}
			PlaceIterator iterator = lines.GetCharIterator(position);
			int depth = 1;
			while (direct ? iterator.MoveRight() : iterator.MoveLeft())
			{
				char c = iterator.RightChar;
				if (c == c0)
					++depth;
				else if (c == c1)
					--depth;
				if (depth <= 0)
					break;
			}
			if (depth <= 0)
			{
				lines.markedBracket = true;
				lines.markedBracket0 = new Place(iChar, place.iLine);
				lines.markedBracket1 = iterator.Place;
				return;
			}
			lines.markedBracket = false;
		}
		
		public bool FixLineBreaks()
		{
			return processor.Execute(new FixLineBreaksCommand());
		}
		
		private ControllerDialogsExtension dialogsExtension;
		
		public ControllerDialogsExtension DialogsExtension
		{
			get
			{
				if (dialogsExtension == null)
				{
					dialogsExtension = new ControllerDialogsExtension(this);
				}
				return dialogsExtension;
			}
		}
		
		public void ViAutoindentByBottom()
		{
			processor.Execute(new InsertIndentedBeforeCommand());
		}

		public void ViResetCommandsBatching()
		{
			processor.ResetCommandsBatching();
		}
		
		public void ViMoveToCharLeft(char charToFind, bool shift, int count, bool at)
		{
			foreach (Selection selection in lines.selections)
			{
				Place place = lines.PlaceOf(selection.caret);
				Line line = lines[place.iLine];
				bool needAfterMove = at;
				for (int i = 0; i < count; i++)
				{
					int iChar = line.LeftIndexOfChar(charToFind, place.iChar - 1);
					if (iChar != -1)
					{
						place.iChar = iChar;
						selection.caret = lines.IndexOf(place);
						lines.SetPreferredPos(selection, place);
						needAfterMove &= true;
					}
					else
					{
						break;
					}
				}
				if (needAfterMove)
				{
					selection.caret++;
					lines.SetPreferredPos(selection, place);
				}
				if (!shift)
				{
					selection.anchor = selection.caret;
				}
			}
		}
		
		public void ViMoveToCharRight(char charToFind, bool shift, int count, bool at)
		{
			foreach (Selection selection in lines.selections)
			{
				Place place = lines.PlaceOf(selection.caret);
				Line line = lines[place.iLine];
				bool needAfterMove = at;
				for (int i = 0; i < count; i++)
				{
					int iChar = line.IndexOfChar(charToFind, place.iChar + 1);
					if (iChar != -1)
					{
						if (shift)
						{
							iChar++;
						}
						place.iChar = iChar;
						selection.caret = lines.IndexOf(place);
						lines.SetPreferredPos(selection, place);
						needAfterMove &= true;
					}
					else
					{
						break;
					}
				}
				if (needAfterMove)
				{
					selection.caret--;
					lines.SetPreferredPos(selection, place);
				}
				selection.SetEmptyIfNotShift(shift);
			}
		}
		
		public void ViCollapseSelections()
		{
			foreach (Selection selection in selections)
			{
				selection.caret = selection.anchor;
			}
			JoinSelections();
		}
		
		public void ViMove_w(bool shift, bool change)
		{
			foreach (Selection selection in lines.selections)
			{
				Moves moves = new Moves(lines, selection.caret);
				moves.Vi_w(change);
				moves.Apply(selection, shift);
			}
		}
		
		public void ViMove_b(bool shift, bool change)
		{
			foreach (Selection selection in lines.selections)
			{
				Moves moves = new Moves(lines, selection.caret);
				moves.Vi_b();
				moves.Apply(selection, shift);
			}
		}
		
		public void ViMove_W(bool shift, bool change)
		{
			foreach (Selection selection in lines.selections)
			{
				Moves moves = new Moves(lines, selection.caret);
				moves.Vi_W(change);
				moves.Apply(selection, shift);
			}
		}
		
		public void ViMove_B(bool shift, bool change)
		{
			foreach (Selection selection in lines.selections)
			{
				Moves moves = new Moves(lines, selection.caret);
				moves.Vi_B();
				moves.Apply(selection, shift);
			}
		}
		
		public void ViMove_e(bool shift)
		{
			foreach (Selection selection in lines.selections)
			{
				Moves moves = new Moves(lines, selection.caret);
				moves.Vi_e(shift);
				moves.Apply(selection, shift);
			}
		}
		
		public void ViMove_E(bool shift)
		{
			foreach (Selection selection in lines.selections)
			{
				Moves moves = new Moves(lines, selection.caret);
				moves.Vi_E(shift);
				moves.Apply(selection, shift);
			}
		}
		
		public void ViMoveInWord(bool shift, bool inside)
		{
			foreach (Selection selection in lines.selections)
			{
				Moves moves = new Moves(lines, selection.caret);
				moves.Vi_WordStart();
				selection.caret = moves.Position;
				selection.SetEmpty();
				moves.Vi_w(inside);
				moves.Apply(selection, true);
			}
		}
		
		public void ViMoveInBrackets(bool shift, bool inside, char bra, char ket)
		{
			foreach (Selection selection in lines.selections)
			{
				Moves moves = new Moves(lines, selection.caret);
				if (moves.Vi_BracketStart(bra, ket))
				{
					int position = moves.Position;
					if (moves.Vi_BracketEnd(bra, ket))
					{
						selection.caret = position;
						selection.SetEmpty();
						moves.Apply(selection, true);
					}
				}
			}
		}
		
		public void ViMoveHome(bool shift, bool indented)
		{
			foreach (Selection selection in selections)
			{
				Place caret = lines.PlaceOf(selection.caret);
				Line line = lines[caret.iLine];
				int minIChar = 0;
				int charsCount = line.NormalCount;
				if (indented)
				{
					while (minIChar < charsCount && char.IsWhiteSpace(line.chars[minIChar].c))
					{
						minIChar++;
					}
				}
				caret.iChar = minIChar;
				if (!shift && caret.iChar >= line.NormalCount)
				{
					caret.iChar = line.NormalCount - 1;
				}
				if (caret.iChar < 0)
				{
					caret.iChar = 0;
				}
				selection.caret = lines.IndexOf(caret);
				selection.SetEmptyIfNotShift(shift);
				lines.SetPreferredPos(selection, caret);
			}
		}
		
		public void ViMoveEnd(bool shift, int count)
		{
			foreach (Selection selection in selections)
			{
				Place caret = lines.PlaceOf(selection.caret);
				if (count > 1)
				{
					caret.iLine += count - 1;
					if (caret.iLine >= lines.LinesCount)
					{
						caret.iLine = lines.LinesCount - 1;
					}
				}
				caret.iChar = lines[caret.iLine].NormalCount;
				selection.caret = lines.IndexOf(caret);
				selection.SetEmptyIfNotShift(shift);
			}
			ViFixPositions(true);
		}
		
		public void ViDocumentEnd(bool shift)
		{
			Place place = new Place(0, lines.LinesCount - 1);
			int position = lines.IndexOf(place);
			foreach (Selection selection in selections)
			{
				selection.caret = position;
				selection.SetEmptyIfNotShift(shift);
			}
			lines.JoinSelections();
			lines.SetPreferredPos(lines.LastSelection, place);
		}
		
		public void ViMoveLeft(bool shift)
		{
			foreach (Selection selection in lines.selections)
			{
				Place place = lines.PlaceOf(selection.caret);
				if (place.iChar > 0)
				{
					selection.caret--;
					selection.SetEmptyIfNotShift(shift);
				}
			}
			ViFixPositions(true);
		}
		
		public void ViMoveRight(bool shift)
		{
			foreach (Selection selection in lines.selections)
			{
				Place place = lines.PlaceOf(selection.caret);
				Line line = lines[place.iLine];
				if (place.iChar < line.NormalCount)
				{
					place.iChar++;
					selection.caret = lines.IndexOf(place);
					selection.SetEmptyIfNotShift(shift);
				}
			}
			ViFixPositions(true);
		}
		
		public void ViMoveUp(bool shift)
		{
			foreach (Selection selection in selections)
			{
				Place place = lines.PlaceOf(selection.caret);
				if (place.iLine > 0)
				{
					place.iLine--;
					place = ViGetPreferredPlace(selection, place);
					selection.caret = lines.IndexOf(place);
					selection.SetEmptyIfNotShift(shift);
				}
			}
			ViFixPositions(false);
		}
		
		public void ViMoveDown(bool shift)
		{
			foreach (Selection selection in selections)
			{
				Place place = lines.PlaceOf(selection.caret);
				if (place.iLine < lines.LinesCount - 1)
				{
					place.iLine++;
					place = ViGetPreferredPlace(selection, place);
					selection.caret = lines.IndexOf(place);
					selection.SetEmptyIfNotShift(shift);
				}
			}
			ViFixPositions(false);
		}
		
		public void ViFixPositions(bool setPreferredPos)
		{
			foreach (Selection selection in lines.selections)
			{
				Place place = lines.PlaceOf(selection.caret);
				if (selection.Empty)
				{
					Line line = lines[place.iLine];
					int count = line.NormalCount;
					if (count > 0 && place.iChar >= count)
					{
						place.iChar = count - 1;
						selection.caret = lines.IndexOf(place);
						selection.anchor = selection.caret;
					}
				}
				if (setPreferredPos)
				{
					lines.SetPreferredPos(selection, place);
				}
			}
		}
		
		public void ViNormal_FixCaret(bool setPreferredPos)
		{
			foreach (Selection selection in lines.selections)
			{
				Place place = lines.PlaceOf(selection.caret);
				Line line = lines[place.iLine];
				int count = line.NormalCount;
				if (count > 0 && place.iChar >= count)
				{
					place.iChar = count - 1;
					selection.caret = lines.IndexOf(place);
					if (setPreferredPos)
					{
						lines.SetPreferredPos(selection, place);
					}
				}
			}
		}
		
		public void ViSelectRight(int count)
		{
			foreach (Selection selection in lines.selections)
			{
				Place place = lines.PlaceOf(selection.caret);
				Line line = lines[place.iLine];
				for (int i = 0; i < count; i++)
				{
					if (place.iChar < line.NormalCount)
					{
						place.iChar++;
						selection.caret = lines.IndexOf(place);
					}
				}
				lines.SetPreferredPos(selection, place);
			}
		}
		
		public void ViMoveRightFromCursor()
		{
			foreach (Selection selection in lines.selections)
			{
				Place place = lines.PlaceOf(selection.caret);
				if (selection.Empty)
				{
					Line line = lines[place.iLine];
					if (place.iChar < line.NormalCount)
					{
						place.iChar++;
						selection.caret = lines.IndexOf(place);
						selection.anchor = selection.caret;
						lines.SetPreferredPos(selection, place);
					}
				}
			}
		}
		
		public void ViReplaceChar(char c, int count)
		{
			bool allEmpty = true;
			foreach (Selection selection in selections)
			{
				if (selection.Empty)
				{
					Place place = lines.PlaceOf(selection.anchor);
					Line line = lines[place.iLine];
					if (place.iChar + count <= line.NormalCount)
					{
						selection.caret += count;
					}
				}
				else
				{
					allEmpty = false;
				}
			}
			lines.JoinSelections();
			string[] texts = new string[selections.Count];
			for (int i = 0, selectionsCount = selections.Count; i < selectionsCount; i++)
			{
				Selection selection = lines.selections[i];
				if (selection.Count == 1)
				{
					texts[i] = c + "";
				}
				else
				{
					string text = lines.GetText(selection.Left, selection.Count);
					StringBuilder builder = new StringBuilder();
					for (int j = 0; j < text.Length; j++)
					{
						if (text[j] == '\n' || text[j] == '\r')
						{
							builder.Append(text[j]);
						}
						else
						{
							builder.Append(c);
						}
					}
					texts[i] = builder.ToString();
				}
			}
			processor.Execute(new InsertTextCommand(null, texts, true));
			if (selections.Count == texts.Length)
			{
				for (int i = 0, selectionsCount = selections.Count; i < selectionsCount; i++)
				{
					Selection selection = lines.selections[i];
					int textLength = texts[i].Length;
					selection.caret -= allEmpty && textLength > 0 ? 1 : textLength;
					selection.anchor = selection.caret;
					Place place = lines.PlaceOf(selection.caret);
					lines.SetPreferredPos(selection, place);
				}
			}
		}
		
		public void ViCut(char register, bool fixPositions)
		{
			ViCopy(register);
			EraseSelection();
			if (fixPositions)
			{
				ViFixPositions(true);
			}
		}
		
		public void ViCopy(char register)
		{
			lines.JoinSelections();
			if (lines.AllSelectionsEmpty)
			{
				return;
			}
			StringBuilder text = new StringBuilder();
			SelectionMemento[] mementos = GetSelectionMementos();
			bool first = true;
			foreach (SelectionMemento memento  in mementos)
			{
				if (!first)
					text.Append('\n');
				first = false;
				text.Append(lines.GetText(memento.Left, memento.Count));
			}
			ClipboardExecutor.PutToRegister(register, text.ToString());
		}
		
		private SelectionMemento[] GetSelectionMementos()
		{
			SelectionMemento[] mementos = new SelectionMemento[selections.Count];
			for (int i = 0; i < mementos.Length; i++)
			{
				mementos[i] = selections[i].Memento;
				mementos[i].index = i;
			}
			Array.Sort(mementos, SelectionMemento.CompareSelections);
			return mementos;
		}
		
		public void ViCopyLine(char register, int count)
		{
			List<SimpleRange> ranges = ViGetLineRanges(count);
			CopyLines(register, ranges);
		}
		
		public void ViShift(int indents, int count, bool isLeft)
		{
			List<SimpleRange> ranges = ViGetLineRanges(count);
			for (int i = 0; i < indents; ++i)
			{
				processor.Execute(new ViShiftCommand(ranges, isLeft));
			}
		}
		
		public void SavePositions()
		{
			processor.Execute(new SavePositions());
		}
		
		public void ViJ()
		{
			ViCollapseSelections();
			string[] texts = new string[selections.Count];
			for (int i = 0, selectionsCount = selections.Count; i < selectionsCount; i++)
			{
				Selection selection = lines.selections[i];
				if (selection.Empty)
				{
					Place place = lines.PlaceOf(selection.anchor);
					if (place.iLine < lines.LinesCount - 1)
					{
						Line line = lines[place.iLine];
						place.iChar = line.NormalCount;
						selection.anchor = lines.IndexOf(place);
						++place.iLine;
						if (place.iLine < lines.LinesCount)
						{
							Line nextLine = lines[place.iLine];
							selection.caret = selection.anchor + line.GetRN().Length + nextLine.GetFirstSpaces();
							texts[i] = " ";
						}
						else
						{
							texts[i] = "";
						}
					}
					else
					{
						texts[i] = "";
					}
				}
			}
			processor.Execute(new InsertTextCommand(null, texts, true));
			ViMoveLeft(false);
		}
		
		public void ViPaste(char register, Direction direction, int count)
		{
			processor.BeginBatch();
			if (direction == Direction.Right)
			{
				SavePositions();
				ViMoveRightFromCursor();
			}
			string text = ClipboardExecutor.GetFromRegister(register);
			if (text == null || text == "")
			{
				processor.EndBatch();
				return;
			}
			bool isLineInsert = text.EndsWith("\n") || text.EndsWith("\r");
			if (isLineInsert)
			{
				StringBuilder builder = new StringBuilder();
				for (int ii = 0; ii < count; ii++)
				{
					builder.Append(text);
				}
				text = builder.ToString();
				foreach (Selection selection in selections)
				{
					Place caret = lines.PlaceOf(selection.caret);
					caret.iChar = direction == Direction.Right ? lines[caret.iLine].charsCount : 0;
					selection.caret = lines.IndexOf(caret);
					selection.SetEmpty();
				}
				processor.Execute(new InsertTextCommand(text, null, false));
				for (int i = 0, selectionsCount = selections.Count; i < selectionsCount; i++)
				{
					Selection selection = lines.selections[i];
					Place caret = lines.PlaceOf(selection.caret);
					caret.iChar = lines[caret.iLine].GetFirstSpaces();
					selection.caret = lines.IndexOf(caret);
					selection.SetEmpty();
				}
			}
			else
			{
				LineSubdivider subdivider = new LineSubdivider(text);
				for (int ii = 0; ii < count; ii++)
				{
					if (subdivider.GetLinesCount() != selections.Count)
					{
						processor.Execute(new InsertTextCommand(text, null, true));
					}
					else
					{
						string[] texts = subdivider.GetLines();
						for (int i = 0; i < texts.Length; i++)
						{
							texts[i] = LineSubdivider.GetWithoutEndRN(texts[i]);
						}
						processor.Execute(new InsertTextCommand(null, texts, true));
					}
				}
				for (int i = 0, selectionsCount = selections.Count; i < selectionsCount; i++)
				{
					Selection selection = lines.selections[i];
					selection.caret--;
					selection.SetEmpty();
				}
			}
			SavePositions();
			processor.EndBatch();
		}
		
		public void ViGoToLine(int iLine, bool shift)
		{
			ClearMinorSelections();
			Place place = new Place(0, CommonHelper.Clamp(iLine, 0, lines.LinesCount - 1));
			Line line = lines[place.iLine];
			place.iChar = line.GetFirstSpaces();
			LastSelection.caret = lines.IndexOf(place);
			LastSelection.SetEmptyIfNotShift(shift);
		}
		
		private Place ViGetPreferredPlace(Selection selection, Place place)
		{
			Line line = lines[place.iLine];
			return new Place(line.NormalIndexOfPos(selection.preferredPos), place.iLine);
		}
		
		public void ViFindForward(CharsRegularExpressions.Regex regex)
		{
			if (regex == null)
			{
				return;
			}
			char[] chars = lines.GetChars();
			int charsCount = lines.charsCount;
			int start = selections[0].caret;
			if (start < charsCount)
			{
				start++;
			}
			CharsRegularExpressions.Match match = regex.Match(chars, start, charsCount - start);
			if (match == null || !match.IsMatched(0))
			{
				try
				{
					match = regex.Match(chars, 0, charsCount);
				}
				catch
				{
				}
				if (match == null || !match.IsMatched(0))
				{
					return;
				}
			}
			ClearMinorSelections();
			selections[0].anchor = match.Index;
			selections[0].caret = match.Index;
			Place place = lines.PlaceOf(selections[0].caret);
			lines.SetPreferredPos(selections[0], place);
		}
		
		public void ViFindBackward(CharsRegularExpressions.Regex regex)
		{
			if (regex == null)
			{
				return;
			}
			char[] chars = lines.GetChars();
			int charsCount = lines.charsCount;
			int start = selections[0].caret;
			CharsRegularExpressions.Match match = regex.Match(chars, 0, start);
			if (match == null || !match.IsMatched(0))
			{
				try
				{
					match = regex.Match(chars, 0, charsCount);
				}
				catch
				{
				}
				if (match == null || !match.IsMatched(0))
				{
					return;
				}
			}
			ClearMinorSelections();
			selections[0].anchor = match.Index;
			selections[0].caret = match.Index;
			Place place = lines.PlaceOf(selections[0].caret);
			lines.SetPreferredPos(selections[0], place);
		}
		
		public void ViDeleteLine(char register, int count)
		{
			List<SimpleRange> ranges = ViGetLineRanges(count);
			CopyLines(register, ranges);
			processor.Execute(new ViEraseLinesCommand(this, ranges));
		}
		
		public void ViDeleteLineForChange(char register, int count)
		{
			List<SimpleRange> ranges = ViGetLineRanges(count);
			CopyLines(register, ranges);
			processor.Execute(new ViEraseLinesForChangeCommand(this, ranges));
		}
		
		public List<SimpleRange> GetLineRangesByLefts()
		{
			SimpleRange[] mementos = new SimpleRange[selections.Count];
			for (int i = 0; i < mementos.Length; i++)
			{
				Selection selection = selections[i];
				mementos[i] = new SimpleRange(selection.Left, selection.Count);
			}
			Array.Sort(mementos, SimpleRange.CompareLeftToRight);
			
			List<SimpleRange> ranges = new List<SimpleRange>();
			int startLine = -1;
			int endLine = -1;
			foreach (SimpleRange memento in mementos)
			{
				Place place = lines.PlaceOf(memento.index);
				if (startLine == -1)
				{
					startLine = place.iLine;
					endLine = startLine;
				}
				else if (place.iLine == endLine + 1)
				{
					endLine = place.iLine;
				}
				else if (place.iLine > endLine + 1)
				{
					ranges.Add(new SimpleRange(startLine, endLine - startLine + 1));
					startLine = place.iLine;
					endLine = startLine;
				}
			}
			if (startLine != -1)
			{
				ranges.Add(new SimpleRange(startLine, endLine - startLine + 1));
			}
			return ranges;
		}
		
		public List<SimpleRange> ViGetLineRanges(int count)
		{
			if (count < 1)
			{
				count = 1;
			}
			SimpleRange[] mementos = new SimpleRange[selections.Count];
			for (int i = 0; i < mementos.Length; i++)
			{
				Selection selection = selections[i];
				mementos[i] = new SimpleRange(selection.Left, selection.Count);
			}
			Array.Sort(mementos, SimpleRange.CompareLeftToRight);
			
			List<SimpleRange> ranges = new List<SimpleRange>();
			SimpleRange lastRange = new SimpleRange(-1, -1);
			int linesCount = lines.LinesCount;
			foreach (SimpleRange memento in mementos)
			{
				Place start = lines.PlaceOf(memento.index);
				Place end = memento.count == 0 ? start : lines.PlaceOf(memento.index + memento.count);
				SimpleRange rangeI = new SimpleRange(start.iLine, end.iLine - start.iLine + count);
				if (rangeI.index + rangeI.count > linesCount)
				{
					rangeI.count = linesCount - rangeI.index;
				}
				if (lastRange.index == -1)
				{
					lastRange = rangeI;
				}
				else
				{
					if (rangeI.index <= lastRange.index + lastRange.count)
					{
						lastRange.count = rangeI.index + rangeI.count - lastRange.index;
					}
					else
					{
						ranges.Add(lastRange);
						lastRange = rangeI;
					}
				}
			}
			if (lastRange.index != -1)
			{
				ranges.Add(lastRange);
			}
			return ranges;
		}
		
		private void SetSelectionMementos(SelectionMemento[] mementos)
		{
			for (int i = selections.Count; i < mementos.Length; i++)
			{
				selections.Add(new Selection());
			}
			if (selections.Count > mementos.Length)
				selections.RemoveRange(mementos.Length, selections.Count - mementos.Length);
			for (int i = 0; i < mementos.Length; i++)
			{
				selections[mementos[i].index].Memento = mementos[i];
			}
		}
		
		public void ViStoreSelections()
		{
			lines.mementos = GetSelectionMementos();
		}
		
		public void ViRecoverSelections()
		{
			if (lines.mementos != null)
			{
				SetSelectionMementos(lines.mementos);
			}
		}
	}
}
