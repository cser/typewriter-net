using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor
{
	public class LineArray : FSBArray<Line, LineBlock>
	{
		public readonly WordWrapValidator wwValidator;
		public readonly Scroller scroller;

		public LineArray(int blockSize) : base(blockSize)
		{
			SetText("");
			selections = new List<Selection>();
			selections.Add(new Selection());
			wwValidator = new WordWrapValidator(this);
			scroller = new Scroller(this);
		}

		public LineArray() : this(200)
		{
		}

		public bool IsEmpty { get { return LinesCount == 1 && this[0].charsCount == 0; } }

		override protected LineBlock NewBlock()
		{
			return new LineBlock(blockSize);
		}

		public int tabSize = 4;
		public bool spacesInsteadTabs = false;
		public string lineBreak = "\r\n";
		public bool autoindent = false;
		
		public TabSettings TabSettings { get { return new TabSettings(spacesInsteadTabs, tabSize); } }

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
			//System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			//sw.Start();
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
			wwSizeX = 0;

			//sw.Stop();
			//Console.WriteLine(sw.ElapsedMilliseconds + " ms - " + text.Length + " chars");
			ResetTextCache();
		}
		
		public void ClearAllUnsafely()
		{
			ClearValues();
		}
		
		public void AddLineUnsafely(Line line)
		{
			AddValue(line);
			charsCount += line.charsCount;
		}
		
		public void CutLastLineBreakUnsafely()
		{
			Line line = GetValue(LinesCount - 1);
			if (line.chars[line.charsCount - 1].c == '\n' || line.chars[line.charsCount - 1].c == '\r')
			{
				line.Chars_RemoveAt(line.charsCount - 1);
				--charsCount;
			}
			if (line.chars[line.charsCount - 1].c == '\n' || line.chars[line.charsCount - 1].c == '\r')
			{
				line.Chars_RemoveAt(line.charsCount - 1);
				--charsCount;
			}
		}

		private string cachedText;
		private bool charsValid;
		private CharsRegularExpressions.Regex highlightRegex;

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
		
		private CharBuffer _charBuffer;
		
		public char[] GetChars()
		{
			if (_charBuffer == null)
			{
				_charBuffer = new CharBuffer();
			}
			if (!charsValid)
			{
				charsValid = true;
				_charBuffer.Resize(charsCount);
				_charBuffer.Realocate();
				int index = 0;
				for (int i = 0; i < blocksCount; i++)
				{
					LineBlock block = blocks[i];
					for (int j = 0; j < block.count; j++)
					{
						Line line = block.array[j];
						Char[] chars = line.chars;
						for (int k = 0, count = line.charsCount; k < count; k++)
						{
							_charBuffer.buffer[index++] = chars[k].c;
						}
					}
				}
			}
			return _charBuffer.buffer;
		}
		
		public void ResetTextCache()
		{
			cachedText = null;
			charsValid = false;
			frameValid = false;
			highlightRegex = null;
		}
		
		private bool frameValid;
		private readonly CharBuffer _frameChars = new CharBuffer();
		private int _frameCharsIndex;
		private int _frameCharsCount;
		
		public void UpdateHighlight(int index, int count)
		{
			FixRange(ref index, ref count);
			bool isOutFrame = index < _frameCharsIndex || index + count > _frameCharsIndex + _frameCharsCount;
			if (highlightRegex != ClipboardExecutor.ViRegex || isOutFrame || !frameValid)
			{
				frameValid = true;
				highlightRegex = ClipboardExecutor.ViRegex;
				matches.Clear();
				if (highlightRegex != null)
				{
					int frameCharsIndex = index - count / 2;
					int frameCharsCount = count * 2;
					FixRange(ref frameCharsIndex, ref frameCharsCount);
					if (_frameCharsIndex != frameCharsIndex || _frameCharsCount != frameCharsCount)
					{
						_frameCharsIndex = frameCharsIndex;
						_frameCharsCount = frameCharsCount;
						_frameChars.Resize(_frameCharsCount);
					}
					if (_frameCharsCount > 0)
					{
						GetText(_frameCharsIndex, _frameCharsCount, _frameChars.buffer);
					}
					char[] chars = _frameChars.buffer;
					int ii = 0;
					while (ii < _frameCharsCount)
					{
						CharsRegularExpressions.Match match = null;
						try
						{
							match = highlightRegex.Match(chars, ii, _frameCharsCount - ii);
						}
						catch
						{
						}
						if (match == null || !match.IsMatched(0))
						{
							break;
						}
						SimpleRange range = new SimpleRange();
						range.index = _frameCharsIndex + match.Index;
						range.count = match.Length;
						matches.Add(range);
						ii = match.Index + (match.Length > 0 ? match.Length : 1);
					}
				}
			}
		}
		
		private void FixRange(ref int index, ref int count)
		{
			if (index + count > charsCount)
			{
				index = charsCount - count;
			}
			if (index < 0)
			{
				index = 0;
				if (index + count > charsCount)
				{
					count = charsCount - index;
				}
			}
		}

		private Line NewLine(string text, int index, int count)
		{
			Line line = new Line(count);
			line.tabSize = tabSize;
			for (int j = 0; j < count; j++)
			{
				line.chars[j].c = text[index + j];
			}
			line.charsCount = count;
			return line;
		}
		
		public TextChangeHook hook;
		public TextChangeHook hook2;

		public void InsertText(int index, string text)
		{
			if (index < 0 || index > charsCount)
				throw new IndexOutOfRangeException(
					"text index=" + index + ", count=" + text.Length + " is out of [0, " + charsCount + "]");
			if (text.Length == 0)
				return;
			int blockI;
			int blockIChar;
			Place place = PlaceOf(index, out blockI, out blockIChar);
			LineBlock block = blocks[blockI];
			int startJ = place.iLine - block.offset;
			Line start = block.array[startJ];
			if (place.iChar == start.charsCount - 1 &&
				start.charsCount >= 2 &&
				start.chars[start.charsCount - 2].c == '\r' &&
				start.chars[start.charsCount - 1].c == '\n')
			{
				start.Chars_RemoveAt(start.charsCount - 1);
				start.cachedText = null;
				start.cachedSize = -1;
				start.endState = null;
				start.wwSizeX = 0;
				block.valid = 0;
				block.wwSizeX = 0;
				
				start = NewLine("\n", 0, 1);
				place.iLine++;
				place.iChar = 0;
				InsertValue(place.iLine, start);
			}
			if (text.IndexOf('\n') == -1 && text.IndexOf('\r') == -1)
			{
				start.Chars_InsertRange(place.iChar, text);
				start.cachedText = null;
				start.cachedSize = -1;
				start.endState = null;
				start.wwSizeX = 0;
				block.valid = 0;
				block.wwSizeX = 0;
			}
			else
			{
				int textLength = text.Length;
				int lineStart = 0;
				List<Line> lines = new List<Line>();
				for (int i = 0; i < textLength; i++)
				{
					char c = text[i];
					if (c == '\r')
					{
						if (i + 1 < textLength && text[i + 1] == '\n')
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
				lines.Add(NewLine(text, lineStart, textLength - lineStart));
				Line line0 = lines[0];
				line0.Chars_InsertRange(0, start, 0, place.iChar);
				line0.cachedText = null;
				line0.cachedSize = -1;
				line0.wwSizeX = 0;
				if (place.iLine > 0 && line0.charsCount > 0 && line0.chars[0].c == '\n')
				{
					int prevBlockI = GetBlockIndex(place.iLine - 1);
					LineBlock prevBlock = blocks[prevBlockI];
					Line prev = prevBlock.array[place.iLine - 1 - prevBlock.offset];
					if (prev.chars[prev.charsCount - 1].c == '\r')
					{
						line0.Chars_RemoveAt(0);
						prev.Chars_Add(new Char('\n'));
						prev.cachedText = null;
						prev.cachedSize = -1;
						prev.endState = null;
						prev.wwSizeX = 0;
						prevBlock.valid = 0;
						prevBlock.wwSizeX = 0;
						if (line0.charsCount == 0)
						{
							lines.RemoveAt(0);
						}
					}
				}
				Line line1 = lines[lines.Count - 1];
				line1.Chars_AddRange(start, place.iChar, start.charsCount - place.iChar);
				line1.cachedText = null;
				line1.cachedSize = -1;
				line1.wwSizeX = 0;
				if (line1.charsCount == 1 && line1.chars[0].c == '\n' && lines.Count > 1)
				{
					Line prevLine1 = lines[lines.Count - 2];
					if (prevLine1.chars[prevLine1.charsCount - 1].c == '\r')
					{
						prevLine1.Chars_Add(new Char('\n'));
						lines.RemoveAt(lines.Count - 1);
					}
				}
				RemoveValueAt(place.iLine);
				InsertValuesRange(place.iLine, lines.ToArray());
			}
			charsCount += text.Length;
			size = null;
			wwSizeX = 0;
			ResetTextCache();
			if (hook != null)
				hook.InsertText(index, text);
			if (hook2 != null)
				hook2.InsertText(index, text);
		}

		public void RemoveText(int index, int count)
		{
			if (index < 0 || index + count > charsCount)
				throw new IndexOutOfRangeException("text index=" + index + ", count=" + count + " is out of [0, " + charsCount + "]");
			if (count == 0)
			{
				return;
			}
			int blockI;
			int blockIChar;
			Place place = PlaceOf(index, out blockI, out blockIChar);
			LineBlock block = blocks[blockI];
			int startJ = place.iLine - block.offset;
			Line start = block.array[startJ];
			if (place.iChar + count <= start.charsCount)
			{
				start.Chars_RemoveRange(place.iChar, count);
				start.cachedText = null;
				start.cachedSize = -1;
				start.endState = null;
				start.wwSizeX = 0;
				block.valid = 0;
				block.wwSizeX = 0;
				int startCharsCount = start.charsCount;
				bool mergeNext = true;
				if (startCharsCount == 1 && start.chars[0].c == '\n')
				{
					mergeNext = false;
					if (place.iLine > 0)
					{
						Line prev;
						if (startJ - 1 >= 0)
						{
							prev = block.array[startJ - 1];
						}
						else
						{
							block = blocks[blockI - 1];
							block.valid = 0;
							block.wwSizeX = 0;
							prev = block.array[block.count - 1];
						}
						if (prev.chars[prev.charsCount - 1].c == '\r')
						{
							RemoveValueAt(place.iLine);
							prev.Chars_Add(new Char('\n'));
							prev.cachedText = null;
							prev.cachedSize = -1;
							prev.endState = null;
							prev.wwSizeX = 0;
						}
					}
				}
				else if (startCharsCount > 0)
				{
					char c = start.chars[startCharsCount - 1].c;
					mergeNext = c != '\n' && c != '\r';
				}
				if (mergeNext && place.iLine + 1 < valuesCount)
				{
					Line line = startJ + 1 < block.count ? block.array[startJ + 1] : blocks[blockI + 1].array[0];
					start.Chars_AddRange(line);
					RemoveValueAt(place.iLine + 1);
				}
			}
			else
			{
				LineBlock prevBlock = block;
				int prevBlockI = blockI;
				block.valid = 0;
				block.wwSizeX = 0;
				int lineJ = startJ;
				int k = start.charsCount - place.iChar;
				int countToRemove = -1;
				Line line = start;
				while (k < count)
				{
					lineJ++;
					++countToRemove;
					if (lineJ >= block.count)
					{
						blockI++;
						block = blocks[blockI];
						lineJ = 0;
					}
					line = block.array[lineJ];
					k += line.charsCount;
				}
				Line end = line;
				start.Chars_RemoveRange(place.iChar, start.charsCount - place.iChar);// fails
				start.cachedText = null;
				start.cachedSize = -1;
				start.endState = null;
				start.wwSizeX = 0;
				end.Chars_RemoveRange(0, count + end.charsCount - k);
				end.cachedText = null;
				end.cachedSize = -1;
				end.wwSizeX = 0;
				end.Chars_ReduceBuffer();
				start.Chars_AddRange(end);

				int removeStart = place.iLine + 1;
				int startCharsCount = start.charsCount;
				bool mergeNext = true;
				if (startCharsCount == 1 && start.chars[0].c == '\n')
				{
					mergeNext = false;
					if (place.iLine > 0)
					{
						Line prev;
						if (startJ - 1 >= 0)
						{
							prev = prevBlock.array[startJ - 1];
						}
						else
						{
							prevBlock = blocks[prevBlockI - 1];
							prevBlock.valid = 0;
							prevBlock.wwSizeX = 0;
							prev = prevBlock.array[prevBlock.count - 1];
						}
						if (prev.chars[prev.charsCount - 1].c == '\r')
						{
							--removeStart;
							++countToRemove;
							prev.Chars_Add(new Char('\n'));
							prev.cachedText = null;
							prev.cachedSize = -1;
							prev.endState = null;
							prev.wwSizeX = 0;
						}
					}
				}
				else if (startCharsCount > 0)
				{
					char c = start.chars[startCharsCount - 1].c;
					mergeNext = c != '\n' && c != '\r';
				}
				if (mergeNext && block.offset + lineJ + 1 < valuesCount)
				{
					Line next = lineJ + 1 < block.count ? block.array[lineJ + 1] : blocks[blockI + 1].array[0];
					start.Chars_AddRange(next);
					++countToRemove;
				}

				++countToRemove;
				RemoveValuesRange(removeStart, countToRemove);
			}
			start.Chars_ReduceBuffer();
			charsCount -= count;
			size = null;
			wwSizeX = 0;
			ResetTextCache();
			if (hook != null)
				hook.RemoveText(index, count);
			if (hook2 != null)
				hook2.RemoveText(index, count);
		}

		public string GetText(int index, int count)
		{
			if (count == 0)
				return "";
			char[] chars = new char[count];
			GetText(index, count, chars);
			return new string(chars);
		}
		
		private void GetText(int index, int count, char[] outChars)
		{
			if (index < 0 || index + count > charsCount)
			{
				throw new IndexOutOfRangeException("text index=" + index + ", count=" + count + " is out of [0, " + charsCount + "]");
			}
			int blockI;
			int blockIChar;
			Place place = PlaceOf(index, out blockI, out blockIChar);
			LineBlock block = blocks[blockI];
			int frameI = 0;
			int i = place.iLine - block.offset;
			int j = index - blockIChar;
			for (int ii = 0; ii < i; ii++)
			{
				j -= block.array[ii].charsCount;
			}
			Line line = block.array[i];
			for (int k = 0; k < count; k++)
			{
				if (j < line.charsCount)
				{
					outChars[frameI++] = line.chars[j].c;
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
					outChars[frameI++] = line.chars[j].c;
				}
				j++;
			}
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
						block.charsCount += block.array[j].charsCount;
					}
				}
				if (place.iLine >= block.offset && place.iLine < block.offset + block.count)
				{
					int j1 = place.iLine - block.offset;
					for (int j = 0; j < j1; j++)
					{
						charOffset += block.array[j].charsCount;
					}
					if (place.iChar > block.array[j1].charsCount)
						place.iChar = block.array[j1].charsCount;
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
						block.charsCount += block.array[j].charsCount;
					}
				}
				if (index >= charOffset && index < charOffset + block.charsCount)
				{
					blockIChar = charOffset;
					int currentJ = 0;
					for (int j = 0; j < block.count; j++)
					{
						charOffset += block.array[j].charsCount;
						currentJ = j;
						if (index < charOffset)
							break;
					}
					blockI = i;
					Place place = new Place(index - charOffset + block.array[currentJ].charsCount, block.offset + currentJ);
					return place;
				}
				charOffset += block.charsCount;
			}
			blockIChar = charOffset;
			blockI = blocksCount - 1;
			{
				Place place = new Place(block.array[block.count - 1].charsCount, block.offset + block.count - 1);
				return place;
			}
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
			return new PlaceIterator(
				blocks, blocksCount, charsCount, blockI, place.iLine - blocks[blockI].offset, place.iChar, position);
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
				return SingleLineIndexOf(text, startIndex, length, place);
			}
			LineSubdivider subdivider = new LineSubdivider(text, true);
			if (subdivider.GetLinesCount() == 1)
			{
				return SingleLineIndexOf(text, startIndex, length, place);
			}
			string[] texts = subdivider.GetLines();
			string text0 = texts[0];
			LineIterator iteratorI = GetLineRange(place.iLine, linesCount - place.iLine);
			iteratorI.MoveNext();
			if (place.iChar > iteratorI.current.charsCount - text0.Length)
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
			return -1;
		}
		
		private int SingleLineIndexOf(string text, int startIndex, int length, Place place)
		{
			int linesCount = LinesCount;
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
			return -1;
		}

		public readonly List<Selection> selections;
		public readonly List<SimpleRange> matches = new List<SimpleRange>();

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
					int right = currentRight >= selectionsBuffer.buffer[i].Right ?
						currentRight : selectionsBuffer.buffer[i].Right;
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
		
		public void ResizeSelections(int count)
		{
			if (selections.Count > count)
			{
				selections.RemoveRange(count, selections.Count - count);
			}
			else
			{
				while (selections.Count < count)
				{
					Selection prev = selections[selections.Count - 1];
					Selection selection = new Selection();
					selection.anchor = prev.anchor;
					selection.caret = prev.caret;
					selection.preferredPos = prev.preferredPos;
					selection.wwPreferredPos = prev.preferredPos;
					selections.Add(selection);
				}
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
			if (start.iChar + range.count <= line.charsCount)
			{
				line.SetRangeStyle(start.iChar, range.count, style);
			}
			else
			{
				Place end = PlaceOf(range.start + range.count);
				LineIterator iterator = GetLineRange(start.iLine, end.iLine - start.iLine + 1);
				if (iterator.MoveNext())
				{
					iterator.current.SetRangeStyle(start.iChar, iterator.current.charsCount - start.iChar, style);
					for (int i = end.iLine - start.iLine - 1; i-- > 0;)
					{
						iterator.MoveNext();
						iterator.current.SetRangeStyle(0, iterator.current.charsCount, style);
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
		
		public void ResetColor()
		{
			for (int i = 0; i < blocksCount; i++)
			{
				LineBlock block = blocks[i];
				for (int j = 0; j < block.count; j++)
				{
					Line line = block.array[j];
					line.SetRangeStyle(0, line.charsCount, 0);
				}
			}
		}

		public string markedWord;

		public readonly Dictionary<int, int[]> marksByLine = new Dictionary<int, int[]>();

		public bool markedBracket;
		public Place markedBracket0;
		public Place markedBracket1;
		public SelectionMemento[] mementos;

		public List<StyleRange> ranges;
		
		public string Debug_CheckConsistency()
		{
			int offsetChars = 0;
			for (int i = 0; i < blocksCount; i++)
			{
				LineBlock block = blocks[i];
				if ((block.valid & LineBlock.CharsCountValid) == 0)
					return "OK [i=" + i + "] (without validation)";
				offsetChars += block.charsCount;
				int offsetCharsI = 0;
				for (int j = 0; j < block.count; j++)
				{
					offsetCharsI += block.array[j].charsCount;
				}
				if (offsetCharsI != block.charsCount)
					return "ERROR [i=" + i + "] (offsetCharsI=" + offsetCharsI + " != block.charsCount=" + block.charsCount + ")\n" + Debug_GetBlockInfo(block, "");
			}
			if (offsetChars != charsCount)
				return "ERROR (offsetChars=" + offsetChars + " != charsCount=" + charsCount + ")";
			return "OK";
		}
		
		public string Debug_GetBlockInfo(LineBlock block, string name)
		{
			string text = "BLOCK[" + name + "]" + ((block.valid & LineBlock.CharsCountValid) != 0 ? " - valid" : "") + " [\n";
			int charsCount = 0;
			for (int i = 0; i < block.count; i++)
			{
				Line line = block.array[i];
				text += "\t[" + i + ":" + line.charsCount + "] " + line.Text.Replace("\n", "\\n").Replace("\r", "\\r") + "\n";
				charsCount += line.charsCount;
			}
			text += "] expected:" + block.charsCount + ", was:" + charsCount;
			return text;
		}
	}
}
