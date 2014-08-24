using System;
using System.Text;

namespace MulticaretEditor
{
	public static class CommonHelper
	{
		public static int Clamp(int value, int min, int max)
		{
			if (value > max)
				value = max;
			if (value < min)
				value = min;
			return value;
		}

		public static string GetOneLine(string text)
		{
			int index0 = text.IndexOf('\n');
			int index1 = text.IndexOf('\r');
			int index;
			if (index0 != -1 && index1 != -1)
				index = Math.Min(index0, index1);
			else if (index0 != -1)
				index = index0;
			else if (index1 != -1)
				index = index1;
			else
				return text;
			return text.Substring(0, index);
		}

		public static int GetFirstSpaces(string text)
		{
			int length = text.Length;
			for (int i = 0; i < length; i++)
			{
				char c = text[i];
				if (c != ' ' && c != '\t')
					return i;
			}
			return text.Length;
		}
	}
}
