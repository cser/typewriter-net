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
				int prev = 0;
				int indexN = -1;
				int indexR = -1;
				while (true)
				{			
					indexN = indexN > prev ? indexN : text.IndexOf('\n', prev);
					indexR = indexR > prev ? indexR : text.IndexOf('\r', prev);
					int index = indexN != -1 && (indexR != -1 && indexN < indexR || indexR == -1) ? indexN : indexR;
					if (index == -1)
						break;
					char c = text[index];
					if (c == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
					{
						prev = index + 2;
					}
					else
					{
						prev = index + 1;
					}
					linesCount++;
				}
				linesCount++;
			}
			if (noLastEmpty &&
				linesCount > 1 && text.Length > 0 && (text[text.Length - 1] == '\r' || text[text.Length - 1] == '\n'))
			{
				linesCount--;
			}
			return linesCount;
		}
		
		public string[] GetLines()
		{
			string[] lines = new string[GetLinesCount()];
			int prev = 0;
			int i = 0;
			int indexN = -1;
			int indexR = -1;
			while (true)
			{
				indexN = indexN > prev ? indexN : text.IndexOf('\n', prev);
				indexR = indexR > prev ? indexR : text.IndexOf('\r', prev);
				int index = indexN != -1 && (indexR != -1 && indexN < indexR || indexR == -1) ? indexN : indexR;
				if (index == -1)
					break;
				char c = text[index];
				if (c == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
				{
					lines[i++] = text.Substring(prev, index + 2 - prev);
					prev = index + 2;
				}
				else
				{
					lines[i++] = text.Substring(prev, index + 1 - prev);
					prev = index + 1;
				}
			}
			if (i < lines.Length)
			{
				lines[i] = text.Substring(prev);
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
