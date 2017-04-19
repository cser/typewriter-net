using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public struct LineSubdivider
	{
		public readonly string text;
		
		private readonly bool noLastEmpty;
		
		public LineSubdivider(string text)
		{
			this.text = text;
			noLastEmpty = false;
			linesCount = -1;
		}
		
		public LineSubdivider(string text, bool noLastEmpty)
		{
			this.text = text;
			this.noLastEmpty = noLastEmpty;
			linesCount = -1;
		}
		
		private int linesCount;
		
		public int GetLinesCount()
		{
			if (linesCount == -1)
			{
				linesCount = 0;
				int length = text.Length;
				for (int i = 0; i < length; ++i)
				{
					char c = text[i];
					if (c == '\r')
					{
						if (i + 1 < length && text[i + 1] == '\n')
							++i;
						++linesCount;
					}
					else if (c == '\n')
					{
						++linesCount;
					}
				}
				++linesCount;
				if (noLastEmpty &&
					linesCount > 1 && text.Length > 0 && (text[text.Length - 1] == '\r' || text[text.Length - 1] == '\n'))
				{
					linesCount--;
				}
			}
			return linesCount;
		}
		
		public string[] GetLines()
		{
			string[] lines = new string[GetLinesCount()];
			int length = text.Length;
			int lineIndex = 0;
			int lineStart = 0;
			for (int i = 0; i < length; i++)
			{
				char c = text[i];
				if (c == '\r')
				{
					if (i + 1 < length && text[i + 1] == '\n')
						i++;
					lines[lineIndex++] = text.Substring(lineStart, i + 1 - lineStart);
					lineStart = i + 1;
				}
				else if (c == '\n')
				{
					lines[lineIndex++] = text.Substring(lineStart, i + 1 - lineStart);
					lineStart = i + 1;
				}
			}
			if (lineIndex < lines.Length)
			{
				lines[lineIndex] = text.Substring(lineStart, length - lineStart);
			}
			return lines;
		}
		
		public static string GetWithoutEndRN(string text)
		{
			int length = text.Length;
			string result = text;
			if (length > 0)
			{
				if (text[length - 1] == '\n')
				{
					if (length > 1 && text[length - 2] == '\r')
					{
						result = text.Substring(0, length - 2);
					}
					else
					{
						result = text.Substring(0, length - 1);
					}
				}
				else if (text[length - 1] == '\r')
				{
					result = text.Substring(0, length - 1);
				}
			}
			return result;
		}
	}
}
