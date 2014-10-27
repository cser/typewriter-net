using System;
using System.Collections.Generic;
using System.Text;
using MulticaretEditor.Highlighting;

namespace MulticaretEditor
{
	public class LineArray : FSBArray<Line, LineBlock>
	{
		public readonly WordWrapValidator wwValidator;
		public readonly Scroller scroller;

		public LineArray(int blockSize) : base(blockSize)
		{
			SetText("");
			selections = new RWList<Selection>();
			selections.Add(new Selection());
			wwValidator = new WordWrapValidator(this);
			scroller = new Scroller(this);
		}

		public LineArray() : this(200)
		{
		}

		public bool IsEmpty { get { return LinesCount == 1 && this[0].chars.Count == 0; } }

		override protected LineBlock NewBlock()
		{
			return new LineBlock(blockSize);
		}

		public int tabSize = 4;
		public bool spacesInsteadTabs = false;
		public string lineBreak = "\r\n";

		private void ValidateSize()
		{
			if (this.size != null)
				return;
			IntSize size = new IntSize(0, 0);
			size.y = LinesCount;
			for (int i = 0; i < blocksCount; i++)
			{
				LineBlock block = blocks[i];
				if ((block.valid & LineBlock.MaxSizeXValid) == 0)
				{
					block.valid |= LineBlock.MaxSizeXValid;
					block.maxSizeX = 0;
					for (int j = 0; j < block.count; j++)
					{
						int sizeI = block.array[j].Size;
						if (block.maxSizeX < sizeI)
							block.maxSizeX = sizeI;
					}
				}
				if (size.x < block.maxSizeX)
					size.x = block.maxSizeX;
			}
			this.size = size;
		}

		private IntSize? size = null;
		public IntSize Size
		{
			get
			{
				if (size == null)
					ValidateSize();
				return size.Value;
			}
		}

		public int LinesCount { get { return valuesCount; } }

		public int charsCount = 0;

		public Line this[int index]
		{
			get { return GetValue(index); }
		}

		public void SetText(string text)
		{
			ClearValues();
			int length = text.Length;
			int lineStart = 0;
			for (int i = 0; i < length; i++)
			{
				char c = text[i];
				if (c == '\r')
				{
					if (i + 1 < length && text[i + 1] == '\n')
						i++;
					AddValue(NewLine(text, lineStart, i + 1 - lineStart));
					lineStart = i + 1;
				}
				else if (c == '\n')
				{
					AddValue(NewLine(text, lineStart, i + 1 - lineStart));
					lineStart = i + 1;
				}
			}
			AddValue(NewLine(text, lineStart, length - lineStart));
			charsCount = length;
			size = null;
			cachedText = null;
			wwSizeX = 0;
		}

		public string cachedText;

		public string GetText()
		{
			if (cachedText == null)
			{
				StringBuilder builder = new StringBuilder(charsCount);
				for (int i = 0; i < blocksCount; i++)
				{
					LineBlock block = blocks[i];
					for (int j = 0; j < block.count; j++)
					{
						builder.Append(block.array[j].Text);
					}
				}
				cachedText = builder.ToString();
			}
			return cachedText;
		}

		private Line NewLine(string text, int index, int count)
		{
			Line line = new Line();
			line.tabSize = tabSize;
			line.chars.Capacity = count;
			int end = index + count;
			for (int j = index; j < end; j++)
			{
				line.chars.Add(new Char(text[j]));
			}
			return line;
		}

		public void InsertText(int index, string text)
		{
			if (index < 0 || index > charsCount)
				throw new IndexOutOfRangeException("text index=" + index + ", count=" + text.Length + " is out of [0, " + charsCount + "]");
			if (text.Length == 0)
				return;
			int blockI;
			int blockIChar;
			Place place = PlaceOf(index, out blockI, out blockIChar);
			LineBlock block = blocks[blockI];
			int startJ = place.iLine - block.offset;
			Line start = block.array[startJ];
			if (text.IndexOf("\n") == -1 && text.IndexOf("\r") == -1)
			{
				Char[] chars = new Char[text.Length];
				for (int i = 0; i < chars.Length; i++)
				{
					chars[i] = new Char(text[i]);
				}
				start.chars.InsertRange(place.iChar, chars);
				start.cachedText = null;
				start.cachedSize = -1;
				start.endState = null;
				start.wwSizeX = 0;
				block.valid = 0;
				block.wwSizeX = 0;
			}
			else
			{
				int length = text.Length;
				int lineStart = 0;
				List<Line> lines = new List<Line>();
				for (int i = 0; i < length; i++)
				{
					char c = text[i];
					if (c == '\r')
					{
						if (i + 1 < length && text[i + 1] == '\n')
							i++;
						lines.Add(NewLine(text, lineStart, i + 1 - lineStart));
						lineStart = i + 1;
					}
					else if (c == '\n')
					{
						lines.Add(NewLine(text, lineStart, i + 1 - lineStart));
						lineStart = i + 1;
					}
				}
				lines.Add(NewLine(text, lineStart, length - lineStart));
				Line line0 = lines[0];
				line0.chars.InsertRange(0, start.chars.GetRange(0, place.iChar));
				line0.cachedText = null;
				line0.cachedSize = -1;
				line0.wwSizeX = 0;
				Line line1 = lines[lines.Count - 1];
				line1.chars.AddRange(start.chars.GetRange(place.iChar, start.chars.Count - place.iChar));
				line1.cachedText = null;
				line1.cachedSize = -1;
				line1.wwSizeX = 0;
				RemoveValueAt(place.iLine);
				InsertValuesRange(place.iLine, lines.ToArray());
			}
			charsCount += text.Length;
			size = null;
			cachedText = null;
			wwSizeX = 0;
		}

		public void RemoveText(int index, int count)
		{
			if (index < 0 || index + count > charsCount)
				throw new IndexOutOfRangeException("text index=" + index + ", count=" + count + " is out of [0, " + charsCount + "]");
			if (count == 0)
				return;
			int blockI;
			int blockIChar;
			Place place = PlaceOf(index, out blockI, out blockIChar);
			LineBlock block = blocks[blockI];
			int startJ = place.iLine - block.offset;
			Line start = block.array[startJ];
			if (place.iChar + count <= start.chars.Count)
			{
				start.chars.RemoveRange(place.iChar, count);
				start.cachedText = null;
				start.cachedSize = -1;
				start.endState = null;
				start.wwSizeX = 0;
				block.valid = 0;
				block.wwSizeX = 0;
				int startCharsCount = start.chars.Count;
				bool needMerge;
				if (startCharsCount > 0)
				{
					char c = start.chars[startCharsCount - 1].c;
					needMerge = c != '\n' && c != '\r';
				}
				else
				{
					needMerge = true;
				}
				if (needMerge && place.iLine + 1 < valuesCount)
				{
					Line line = startJ + 1 < block.count ? block.array[startJ + 1] : blocks[blockI + 1].array[0];
					start.chars.AddRange(line.chars);
					RemoveValueAt(place.iLine + 1);
				}
			}
			else
			{
				int lineJ = startJ;
				int k = start.chars.Count - place.iChar;
				int countToRemove = -1;
				Line line = start;
				while (k < count)
				{
					lineJ++;
					countToRemove++;
					if (lineJ >= block.count)
					{
						blockI++;
						block = blocks[blockI];
						lineJ = 0;
					}
					line = block.array[lineJ];
					k += line.chars.Count;
				}
				Line end = line;
				start.chars.RemoveRange(place.iChar, start.chars.Count - place.iChar);// fails
				start.cachedText = null;
				start.cachedSize = -1;
				start.endState = null;
				start.wwSizeX = 0;
				end.chars.RemoveRange(0, count + end.chars.Count - k);
				end.cachedText = null;
				end.cachedSize = -1;
				end.wwSizeX = 0;
				start.chars.AddRange(end.chars);

				int startCharsCount = start.chars.Count;
				bool needMerge;
				if (startCharsCount > 0)
				{
					char c = start.chars[startCharsCount - 1].c;
					needMerge = c != '\n' && c != '\r';
				}
				else
				{
					needMerge = true;
				}
				if (needMerge && block.offset + lineJ + 1 < valuesCount)
				{
					Line next = lineJ + 1 < block.count ? block.array[lineJ + 1] : blocks[lineJ + 1].array[0];// fails
					start.chars.AddRange(next.chars);
					countToRemove++;
				}

				countToRemove++;
				RemoveValuesRange(place.iLine + 1, countToRemove);
			}
			charsCount -= count;
			size = null;
			cachedText = null;
			wwSizeX = 0;
		}

		public string GetText(int index, int count)
		{
			if (index < 0 || index + count > charsCount)
				throw new IndexOutOfRangeException("text index=" + index + ", count=" + count + " is out of [0, " + charsCount + "]");
			if (count == 0)
				return "";
			int blockI;
			int blockIChar;
			Place place = PlaceOf(index, out blockI, out blockIChar);
			LineBlock block = blocks[blockI];
			StringBuilder builder = new StringBuilder(count);
			int i = place.iLine - block.offset;
			int j = index - blockIChar;
			for (int ii = 0; ii < i; ii++)
			{
				j -= block.array[ii].chars.Count;
			}
			Line line = block.array[i];
			for (int k = 0; k < count; k++)
			{
				if (j < line.chars.Count)
				{
					builder.Append(line.chars[j].c);
				}
				else
				{
					i++;
					if (i >= block.count)
					{
						blockI++;
						block = blocks[blockI];
						i = 0;
					}
					line = block.array[i];
					j = 0;
					builder.Append(line.chars[j].c);
				}
				j++;
			}
			return builder.ToString();
		}

		public int IndexOf(Place place)
		{
			int charOffset = 0;
			for (int i = 0; i < blocksCount; i++)
			{
				LineBlock block = blocks[i];
				if ((block.valid & LineBlock.CharsCountValid) == 0)
				{
					block.valid |= LineBlock.CharsCountValid;
					block.charsCount = 0;
					for (int j = 0; j < block.count; j++)
					{
						block.charsCount += block.array[j].chars.Count;
					}
				}
				if (place.iLine >= block.offset && place.iLine < block.offset + block.count)
				{
					int j1 = place.iLine - block.offset;
					for (int j = 0; j < j1; j++)
					{
						charOffset += block.array[j].chars.Count;
					}
					if (place.iChar > block.array[j1].chars.Count)
						place.iChar = block.array[j1].chars.Count;
					return charOffset + place.iChar;
				}
				charOffset += block.charsCount;
			}
			return charOffset;
		}

		public Place UniversalPlaceOf(Pos pos)
		{
			return wordWrap ? wwValidator.PlaceOf(pos) : PlaceOf(pos);
		}

		public Pos UniversalPosOf(Place place)
		{
			return wordWrap ? wwValidator.PosOf(place) : PosOf(place);
		}

		private Place PlaceOf(int index, out int blockI, out int blockIChar)
		{
			if (index < 0 || index > charsCount)
				throw new IndexOutOfRangeException("text index=" + index + " is out of [0, " + charsCount + "]");
			int charOffset = 0;
			LineBlock block = null;
			for (int i = 0; i < blocksCount; i++)
			{
				block = blocks[i];
				if ((block.valid & LineBlock.CharsCountValid) == 0)
				{
					block.valid |= LineBlock.CharsCountValid;
					block.charsCount = 0;
					for (int j = 0; j < block.count; j++)
					{
						block.charsCount += block.array[j].chars.Count;
					}
				}
				if (index >= charOffset && index < charOffset + block.charsCount)
				{
					blockIChar = charOffset;
					int currentJ = 0;
					for (int j = 0; j < block.count; j++)
					{
						charOffset += block.array[j].chars.Count;
						currentJ = j;
						if (index < charOffset)
							break;
					}
					blockI = i;
					return new Place(index - charOffset + block.array[currentJ].chars.Count, block.offset + currentJ);
				}
				charOffset += block.charsCount;
			}
			blockIChar = charOffset;
			blockI = blocksCount - 1;
			return new Place(block.array[block.count - 1].chars.Count, block.offset + block.count - 1);
		}

		public Place PlaceOf(int index)
		{
			int blockI;
			int blockIChar;
			return PlaceOf(index, out blockI, out blockIChar);
		}

		public Place PlaceOf(Pos pos)
		{
			int iLine = pos.iy;
			if (iLine < 0)
			{
				iLine = 0;
			}
			else if (iLine >= LinesCount)
			{
				iLine = LinesCount - 1;
			}
			return new Place(this[iLine].IndexOfPos(pos.ix), iLine);
		}

		public Place SoftNormalizedPlaceOf(int index)
		{
			return Normalize(PlaceOf(Math.Max(0, Math.Min(charsCount, index))));
		}

		public Place Normalize(Place place)
		{
			if (place.iLine < 0)
			{
				place.iLine = 0;
			}
			else if (place.iLine >= valuesCount)
			{
				place.iLine = valuesCount - 1;
			}
			return new Place(Math.Min(place.iChar, this[place.iLine].NormalCount), place.iLine);
		}

		public Pos PosOf(Place place)
		{
			int iLine = place.iLine;
			if (iLine < 0)
			{
				iLine = 0;
			}
			else if (iLine >= LinesCount)
			{
				iLine = LinesCount - 1;
			}
			return new Pos(this[iLine].PosOfIndex(place.iChar), iLine);
		}

		public PlaceIterator GetCharIterator(int position)
		{
			int blockI;
			int blockIChar;
			Place place = PlaceOf(position, out blockI, out blockIChar);
			return new PlaceIterator(blocks, blocksCount, charsCount, blockI, place.iLine - blocks[blockI].offset, place.iChar, position);
		}

		public int IndexOf(string text, int startIndex)
		{
			return IndexOf(text, startIndex, charsCount - startIndex);
		}

		public int IndexOf(string text, int startIndex, int length)
		{
			Place place = PlaceOf(startIndex);
			int linesCount = LinesCount;
			if (text.IndexOf('\n') == -1 && text.IndexOf('\r') == -1)
			{
				LineIterator iterator = GetLineRange(place.iLine, linesCount - place.iLine);
				iterator.MoveNext();
				{
					int index = iterator.current.Text.IndexOf(text, place.iChar);
					if (index != -1)
						return IndexOf(new Place(index, place.iLine));
				}
				while (iterator.MoveNext())
				{
					int index = iterator.current.Text.IndexOf(text);
					if (index != -1)
						return IndexOf(new Place(index, iterator.Index));
				}
			}
			else
			{
				string[] texts = new LineSubdivider(text).GetLines();
				string text0 = texts[0];
				LineIterator iteratorI = GetLineRange(place.iLine, linesCount - place.iLine);
				iteratorI.MoveNext();
				if (place.iChar > iteratorI.current.chars.Count - text0.Length)
					iteratorI.MoveNext();
				while (true)
				{
					Line line = iteratorI.current;
					if (line.Text.EndsWith(text0))
					{
						int i = iteratorI.Index;
						bool correct = i + texts.Length <= linesCount;
						if (correct)
						{
							LineIterator iteratorJ = iteratorI.GetNextRange(texts.Length - 1);
							for (int j = 1; j < texts.Length - 1 && correct; j++)
							{
								iteratorJ.MoveNext();
								if (texts[j] != iteratorJ.current.Text)
									correct = false;
							}
							iteratorJ.MoveNext();
							if (correct && iteratorJ.current.Text.StartsWith(texts[texts.Length - 1]))
								return IndexOf(new Place(line.Text.Length - text0.Length, i));
						}
					}
					if (!iteratorI.MoveNext())
						break;
				}

			}
			return -1;
		}

		public readonly List<Selection> selections;

		private PredictableList<Selection> selectionsBuffer = new PredictableList<Selection>();
		private SelectionComparer selectionComparer = new SelectionComparer();

		public bool IntersectSelections(int anchor, int caret)
		{
			int left = Math.Min(anchor, caret);
			int right = Math.Max(anchor, caret);
			for (int i = 0, count = selections.Count; i < count; i++)
			{
				Selection selection = selections[i];
				int selectionLeft = selection.Left;
				int selectionRight = selection.Right;
				if (selectionLeft == selectionRight && selectionLeft >= left && selectionLeft <= right ||
					left == right && left >= selectionLeft && left <= selectionRight ||
					selectionRight > left && selectionLeft < right)
					return true;
			}
			return false;
		}

		public Selection LastSelection { get { return selections[selections.Count - 1]; } }

		public int wwSizeX;
		public int wwSizeY;
		public bool wordWrap;

		public void JoinSelections()
		{
			if (selections.Count == 0)
				return;

			selectionsBuffer.Resize(selections.Count);

			for (int i = 0; i < selections.Count; i++)
			{
				Selection selection = selections[i];
				selection.needRemove = false;
				selectionsBuffer.buffer[i] = selection;
			}

			Array.Sort(selectionsBuffer.buffer, 0, selectionsBuffer.count, selectionComparer);

			int count;

			count = selectionsBuffer.count;
			Selection current = selectionsBuffer.buffer[0];
			for (int i = 1; i < count; i++)
			{
				int currentRight = current.Right;
				int iLeft = selectionsBuffer.buffer[i].Left;
				if (currentRight > iLeft ||
					(current.Empty || selectionsBuffer.buffer[i].Empty) && currentRight >= iLeft)
				{
					selectionsBuffer.buffer[i].needRemove = true;
					int right = currentRight >= selectionsBuffer.buffer[i].Right ? currentRight : selectionsBuffer.buffer[i].Right;
					if (current.caret >= current.anchor)
					{
						current.caret = right;
					}
					else
					{
						current.anchor = right;
					}
				}
				else
				{
					current = selectionsBuffer.buffer[i];
				}
			}

			selectionsBuffer.Clear();

			for (int i = selections.Count; i-- > 0;)
			{
				if (selections[i].needRemove)
					selections.RemoveAt(i);
			}
		}

		public class SelectionComparer : IComparer<Selection>
		{
			public int Compare(Selection a, Selection b)
			{
				if (a.Left == b.Left)
					return a.Right - b.Right;
				return a.Left - b.Left;
			}
		}

		public bool AllSelectionsEmpty
		{
			get
			{
				foreach (Selection selection in selections)
				{
					if (!selection.Empty)
						return false;
				}
				return true;
			}
		}

		public LineIterator GetLineRange(int index, int count)
		{
			return new LineIterator(this, index, count, -1);
		}

		public void SetPreferredPos(Selection selection, Place place)
		{
			Line line = this[place.iLine];
			selection.preferredPos = line.PosOfIndex(place.iChar);
			selection.wwPreferredPos = wordWrap ? line.WWPosOfIndex(place.iChar).ix : selection.preferredPos;
		}

		public string[] Debug_GetLinesText()
		{
			string[] array = new string[LinesCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = this[i].Text;
			}
			return array;
		}

		public string Debug_GetBlocksInfo()
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < blocksCount; i++)
			{
				if (i != 0)
					builder.Append("; ");
				LineBlock block = blocks[i];
				builder.Append(block.charsCount + ":" + block.offset + ":[");
				bool first = true;
				for (int j = 0; j < block.count; j++)
				{
					if (!first)
						builder.Append("; ");
					first = false;
					builder.Append(Debug_GetOneLineText(block.array[j] != null ? block.array[j].Text : ""));
				}
				for (int j = block.count; j < blockSize; j++)
				{
					if (!first)
						builder.Append("; ");
					first = false;
					builder.Append("(" + Debug_GetOneLineText(block.array[j] != null ? block.array[j].Text : "") + ")");
				}
				builder.Append("]");
			}
			return builder.ToString();
		}

		public string Debug_GetSelections()
		{
			return ListUtil.ToString<Selection>(selections);
		}

		public static string Debug_GetOneLineText(string text)
		{
			return string.Join("\\r", string.Join("\\n", text.Split('\n')).Split('\r'));
		}

		public void SetTabSize(int value)
		{
			if (tabSize != value)
			{
				tabSize = value;
				size = null;
				for (int i = 0; i < blocksCount; i++)
				{
					LineBlock block = blocks[i];
					for (int j = 0; j < block.count; j++)
					{
						Line line = block.array[j];
						if (line.tabSize != value)
						{
							line.tabSize = value;
							line.cachedSize = -1;
							line.wwSizeX = 0;
						}
					}
					block.valid &= (~LineBlock.MaxSizeXValid);
				}
			}
		}

		public void SetStyleRange(StyleRange range)
		{
			short style = range.style;
			Place start = PlaceOf(range.start);
			Line line = this[start.iLine];
			if (start.iChar + range.count <= line.chars.Count)
			{
				line.SetRangeStyle(start.iChar, range.count, style);
			}
			else
			{
				Place end = PlaceOf(range.start + range.count);
				LineIterator iterator = GetLineRange(start.iLine, end.iLine - start.iLine + 1);
				if (iterator.MoveNext())
				{
					iterator.current.SetRangeStyle(start.iChar, iterator.current.chars.Count - start.iChar, style);
					for (int i = end.iLine - start.iLine - 1; i-- > 0;)
					{
						iterator.MoveNext();
						iterator.current.SetRangeStyle(0, iterator.current.chars.Count, style);
					}
					iterator.MoveNext();
					iterator.current.SetRangeStyle(0, end.iChar, style);
				}
			}
		}

		public void ResetHighlighting()
		{
			for (int i = 0; i < blocksCount; i++)
			{
				LineBlock block = blocks[i];
				block.valid &= (~LineBlock.ColorValid);
				for (int j = 0; j < block.count; j++)
				{
					Line line = block.array[j];
					line.startState = null;
					line.endState = null;
				}
			}
		}

		public string markedWord;

		public readonly Dictionary<int, int[]> marksByLine = new Dictionary<int, int[]>();

		public bool markedBracket;
		public Place markedBracket0;
		public Place markedBracket1;

		public List<StyleRange> ranges;
	}
}
