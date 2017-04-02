using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor
{
	public class Line
	{
		public int charsCount;
		public char[] chars;
		public short[] styles;
		
		public Line(int capacity)
		{
			if (capacity < 1)
			{
				capacity = 1;
			}
			chars = new char[capacity];
			styles = new short[capacity];
		}
		
		public void Chars_Add(Char c)
		{
			Chars_Resize(charsCount + 1);
			chars[charsCount] = c.c;
			styles[charsCount] = c.style;
			charsCount++;
		}
		
		public void Chars_RemoveAt(int index)
		{
			Array.Copy(chars, index + 1, chars, index, charsCount - index - 1);
			Array.Copy(styles, index + 1, styles, index, charsCount - index - 1);
			charsCount--;
		}
		
		public void Chars_AddRange(Line line)
		{
			Chars_Resize(charsCount + line.charsCount);
			Array.Copy(line.chars, 0, chars, charsCount, line.charsCount);
			Array.Copy(line.styles, 0, styles, charsCount, line.charsCount);
			charsCount += line.charsCount;
		}
		
		public void Chars_AddRange(Line line, int index, int count)
		{
			Chars_Resize(charsCount + count);
			Array.Copy(line.chars, index, chars, charsCount, count);
			Array.Copy(line.styles, index, styles, charsCount, count);
			charsCount += count;
		}
		
		public void Chars_InsertRange(int index, Line line, int lineIndex, int count)
		{
			Chars_Resize(charsCount + count);
			Array.Copy(chars, index, chars, index + count, charsCount - index);
			Array.Copy(styles, index, styles, index + count, charsCount - index);
			Array.Copy(line.chars, lineIndex, chars, index, count);
			Array.Copy(line.styles, lineIndex, styles, index, count);
			charsCount += count;
		}
		
		public void Chars_InsertRange(int index, char[] text)
		{
			int length = text.Length;
			Chars_Resize(charsCount + length);
			Array.Copy(chars, index, chars, index + length, charsCount - index);
			Array.Copy(text, 0, chars, index, length);
			Array.Copy(styles, index, styles, index + length, charsCount - index);
			Array.Clear(styles, index, length);
			charsCount += length;
		}
		
		public void Chars_RemoveRange(int index, int length)
		{
			if (charsCount - index - length > 0)
			{
				Array.Copy(chars, index + length, chars, index, charsCount - index - length);
				Array.Copy(styles, index + length, styles, index, charsCount - index - length);
			}
			Chars_Resize(charsCount);
			charsCount -= length;
		}
		
		private void Chars_Resize(int count)
		{
			if (count > chars.Length)
			{
				int nextLength = chars.Length << 1;
				while (nextLength < count)
				{
					nextLength = nextLength << 1;
				}
				char[] newChars = new char[nextLength];
				short[] newStyles = new short[nextLength];
				Array.Copy(chars, newChars, charsCount);
				Array.Copy(styles, newStyles, charsCount);
				chars = newChars;
				styles = newStyles;
			}
		}
		
		public int tabSize;
		public int cachedSize = -1;
		public string cachedText = null;
		public Rules.Context[] startState;
		public Rules.Context[] endState;
		public int wwSizeX = 0;
		public PredictableList<CutOff> cutOffs = new PredictableList<CutOff>(2);
		public int lastSublineSizeX;

		public void SetRangeStyle(int startIndex, int count, short style)
		{
			int endIndex = startIndex + count;
			for (int i = startIndex; i < endIndex; i++)
			{
				styles[i] = style;
			}
		}

		public Char this[int index]
		{
			get { return new Char(chars[index], styles[index]); }
		}

		public int Size
		{
			get
			{
				if (cachedSize == -1)
				{
					cachedSize = 0;
					int count = charsCount;
					for (int i = 0; i < count; i++)
					{
						if (chars[i] == '\t')
						{
							cachedSize = ((cachedSize + tabSize) / tabSize) * tabSize;
						}
						else
						{
							cachedSize++;
						}
					}
				}
				return cachedSize;
			}
		}

		public string Text
		{
			get
			{
				if (cachedText == null)
				{
					cachedText = new string(chars, 0, charsCount);
				}
				return cachedText;
			}
		}

		public int IndexOfPos(int pos)
		{
			int count = charsCount;
			int iPos = 0;
			int i = 0;
			for (; i < count; i++)
			{
				if (chars[i] == '\t')
				{
					int prevPos = iPos;
					iPos = ((iPos + tabSize) / tabSize) * tabSize;
					if (iPos > pos)
						return i + (2 * (pos - prevPos) - 1) / (iPos - prevPos);
				}
				else
				{
					iPos++;
					if (iPos > pos)
						return i;
				}
			}
			return count;
		}

		public int WWIndexOfPos(int pos, int iSubline)
		{
			if (iSubline > cutOffs.count)
				return charsCount;

			CutOff cutOff;
			if (iSubline <= 0)
			{
				cutOff = new CutOff(0, 0, 0);
			}
			else
			{
				cutOff = cutOffs.buffer[iSubline - 1];
			}

			int count = iSubline < cutOffs.count ? cutOffs.buffer[iSubline].iChar - 1 : charsCount;
			int iPos = cutOff.left;
			for (int i = cutOff.iChar; i < count; i++)
			{
				if (chars[i] == '\t')
				{
					int prevPos = iPos;
					iPos = ((iPos + tabSize) / tabSize) * tabSize;
					if (iPos > pos)
						return i + (2 * (pos - prevPos) - 1) / (iPos - prevPos);
				}
				else
				{
					iPos++;
					if (iPos > pos)
						return i;
				}
			}
			return count;
		}

		public int NormalIndexOfPos(int pos)
		{
			int index = IndexOfPos(pos);
			return Math.Min(NormalCount, index);
		}

		public int WWNormalIndexOfPos(int pos, int iSubline)
		{
			int index = WWIndexOfPos(pos, iSubline);
			return Math.Min(NormalCount, index);
		}

		public int PosOfIndex(int index)
		{
			if (index > charsCount)
				index = charsCount;
			int pos = 0;
			for (int i = 0; i < index; i++)
			{
				if (chars[i] == '\t')
				{
					pos = ((pos + tabSize) / tabSize) * tabSize;
				}
				else
				{
					pos++;
				}
			}
			return pos;
		}

		public Pos WWPosOfIndex(int index)
		{
			CutOff cutOff = new CutOff(0, 0, 0);
			int iy = 0;
			for (; iy < cutOffs.count && cutOffs.buffer[iy].iChar <= index; iy++)
			{
				cutOff = cutOffs.buffer[iy];
			}
			int pos = 0;
			for (int i = cutOff.iChar; i < index; i++)
			{
				if (chars[i] == '\t')
				{
					pos = ((pos + tabSize) / tabSize) * tabSize;
				}
				else
				{
					pos++;
				}
			}
			pos += cutOff.left;
			return new Pos(pos, iy);
		}

		public int NormalCount
		{
			get
			{
				int count = charsCount;
				if (count > 0)
				{
					char c = chars[count - 1];
					if (c == '\n')
					{
						count--;
						if (count > 0 && chars[count - 1] == '\r')
							count--;
					}
					else if (c == '\r')
					{
						count--;
					}
				}
				return count;
			}
		}

		public void GetFirstIntegerTabs(out string text, out int tabsCount)
		{
			int count = charsCount;
			int size = 0;
			int lastLength = 0;
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < count; i++)
			{
				char c = chars[i];
				if (c == '\t')
				{
					builder.Append(c);
					size = ((size + tabSize) / tabSize) * tabSize;
					lastLength = builder.Length;
				}
				else if (c == ' ')
				{
					builder.Append(c);
					size++;
					if (size % tabSize == 0)
						lastLength = builder.Length;
				}
				else
				{
					break;
				}
			}
			text = builder.ToString(0, lastLength);
			tabsCount = size / tabSize;
		}

		public int GetFirstSpaceSize(out int iChar)
		{
			int count = charsCount;
			int size = 0;
			iChar = 0;
			for (; iChar < count; iChar++)
			{
				char c = chars[iChar];
				if (c == '\t')
				{
					size = ((size + tabSize) / tabSize) * tabSize;
				}
				else if (c == ' ')
				{
					size++;
				}
				else
				{
					break;
				}
			}
			return size;
		}

		public void CalcCutOffs(int wwSizeX)
		{
			this.wwSizeX = wwSizeX;
			cutOffs.Clear();
			int count = charsCount;
			if (count > 4)
			{
				int i;
				int left = GetFirstSpaceSize(out i);
				int pos = left;
				int lastSeparator = 0;
				int prevLastSeparator = 0;
				int lastPos = 0;
				char prev = '\0';
				while (i < count)
				{
					char c = chars[i];
					if (!char.IsWhiteSpace(c) && (char.IsWhiteSpace(prev) || !char.IsLetterOrDigit(c)))
					{
						prevLastSeparator = lastSeparator;
						lastSeparator = i;
						lastPos = pos;
					}
					if (pos >= wwSizeX)
					{
						if (lastSeparator > prevLastSeparator)
						{
							i = lastSeparator;
						}
						else
						{
							if (left > 0 && cutOffs.count > 0)
							{
								int index = cutOffs.count - 1;
								CutOff prevCutOff = cutOffs.buffer[index];
								cutOffs.buffer[index] = new CutOff(prevCutOff.iChar, 0, prevCutOff.sizeX);
								pos -= left;
								left = 0;

								prev = c;
								pos = c == '\t' ? ((pos + tabSize) / tabSize) * tabSize : pos + 1;
								i++;
								continue;
							}
							else
							{
								lastSeparator = i;
								lastPos = pos;
							}
						}
						cutOffs.Add(new CutOff(lastSeparator, left, lastPos));
						pos = left;
						prevLastSeparator = lastSeparator;
						prev = '\0';
					}
					else
					{
						prev = c;
						pos = c == '\t' ? ((pos + tabSize) / tabSize) * tabSize : pos + 1;
						i++;
					}
				}
				lastSublineSizeX = pos;
			}
			else
			{
				lastSublineSizeX = Size;
			}
		}

		public int GetSublineLeft(int iSubline)
		{
			if (iSubline < 0 || iSubline > cutOffs.count)
				return 0;
			return iSubline == 0 ? 0 : cutOffs.buffer[iSubline - 1].left;
		}

		public int GetSublineSize(int iSubline)
		{
			if (iSubline < 0 || iSubline > cutOffs.count)
				return 0;
			return iSubline == cutOffs.count ? lastSublineSizeX : cutOffs.buffer[iSubline].sizeX;
		}

		public string RemoveRN()
		{
			int count = 0;
			string text = "";
			for (int i = charsCount; i-- > 0; )
			{
				char c = chars[i];
				if (c != '\n' && c != '\r')
					break;
				count++;
				text = c + text;
			}
			Chars_RemoveRange(charsCount - count, count);
			return text;
		}

		public int GetFirstSpaces()
		{
			int count = charsCount;
			for (int i = 0; i < count; i++)
			{
				char c = chars[i];
				if (c != ' ' && c != '\t')
					return i;
			}
			return count;
		}
		
		public string GetRN()
		{
			char c0 = charsCount > 1 ? chars[charsCount - 2] : '\0';
			char c1 = charsCount > 0 ? chars[charsCount - 1] : '\0';
			string result = "";
			if (c0 == '\r' && c1 == '\n')
				result = "\r\n";
			else if (c1 == '\r')
				result = "\r";
			else if (c1 == '\n')
				result = "\n";
			return result;
		}
		
		public int IndexOfChar(char c, int index)
		{
			for (int i = index; i < charsCount; i++)
			{
				if (chars[i] == c)
				{
					return i;
				}
			}
			return -1;
		}
		
		public int LeftIndexOfChar(char c, int index)
		{
			if (index >= charsCount)
			{
				index = charsCount - 1;
			}
			for (int i = index; i >= 0; i--)
			{
				if (chars[i] == c)
				{
					return i;
				}
			}
			return -1;
		}
	}
}
