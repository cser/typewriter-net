using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor
{
	public class Line
	{
		public readonly List<Char> chars = new List<Char>();
		public int tabSize;
		public int cachedSize = -1;
		public string cachedText = null;
		public Rules.Context[] startState;
		public Rules.Context[] endState;
		public int wwSizeX = 0;
		public PredictableList<CutOff> cutOffs = new PredictableList<CutOff>(2);
		public int lastSublineSizeX;

		public void SetStyle(int index, short style)
		{
			chars[index] = new Char(chars[index].c, style);
		}

		public void SetRangeStyle(int startIndex, int count, short style)
		{
			int endIndex = startIndex + count;
			for (int i = startIndex; i < endIndex; i++)
			{
				chars[i] = new Char(chars[i].c, style);
			}
		}

		public Char this[int index]
		{
			get { return chars[index]; }
		}

		public int Size
		{
			get
			{
				if (cachedSize == -1)
				{
					cachedSize = 0;
					int count = chars.Count;
					for (int i = 0; i < count; i++)
					{
						if (chars[i].c == '\t')
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
					StringBuilder builder = new StringBuilder(chars.Count);
					int count = chars.Count;
					for (int i = 0; i < count; i++)
					{
						builder.Append(chars[i].c);
					}
					cachedText = builder.ToString();
				}
				return cachedText;
			}
		}

		public int IndexOfPos(int pos)
		{
			int count = chars.Count;
			int iPos = 0;
			int i = 0;
			for (; i < count; i++)
			{
				if (chars[i].c == '\t')
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
				return chars.Count;

			CutOff cutOff;
			if (iSubline <= 0)
			{
				cutOff = new CutOff(0, 0, 0);
			}
			else
			{
				cutOff = cutOffs.buffer[iSubline - 1];
			}

			int count = iSubline < cutOffs.count ? cutOffs.buffer[iSubline].iChar - 1 : chars.Count;
			int iPos = cutOff.left;
			for (int i = cutOff.iChar; i < count; i++)
			{
				if (chars[i].c == '\t')
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
			if (index > chars.Count)
				index = chars.Count;
			int pos = 0;
			for (int i = 0; i < index; i++)
			{
				if (chars[i].c == '\t')
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
				if (chars[i].c == '\t')
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
				int count = chars.Count;
				if (count > 0)
				{
					char c = chars[count - 1].c;
					if (c == '\n')
					{
						count--;
						if (count > 0 && chars[count - 1].c == '\r')
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
			int count = chars.Count;
			int size = 0;
			int lastLength = 0;
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < count; i++)
			{
				char c = chars[i].c;
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
			int count = chars.Count;
			int size = 0;
			iChar = 0;
			for (; iChar < count; iChar++)
			{
				char c = chars[iChar].c;
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
			int count = chars.Count;
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
					char c = chars[i].c;
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
			for (int i = chars.Count; i-- > 0; )
			{
				char c = chars[i].c;
				if (c != '\n' && c != '\r')
					break;
				count++;
				text = c + text;
			}
			chars.RemoveRange(chars.Count - count, count);
			return text;
		}

		public int GetFirstSpaces()
		{
			int count = chars.Count;
			for (int i = 0; i < count; i++)
			{
				char c = chars[i].c;
				if (c != ' ' && c != '\t')
					return i;
			}
			return count;
		}
		
		public string GetRN()
		{
			char c0 = chars.Count > 1 ? chars[chars.Count - 2].c : '\0';
			char c1 = chars.Count > 0 ? chars[chars.Count - 1].c : '\0';
			string result = "";
			if (c0 == '\r' && c1 == '\n')
				result = "\r\n";
			else if (c1 == '\r')
				result = "\r";
			else if (c1 == '\n')
				result = "\n";
			return result;
		}
	}
}
