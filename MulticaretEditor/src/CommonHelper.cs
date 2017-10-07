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
		
		public static int GetFirstSpaces(string text, int start, int length)
		{
			int end = start + length;
			for (int i = start; i < end; i++)
			{
				char c = text[i];
				if (c != ' ' && c != '\t')
					return i - start;
			}
			return length;
		}
		
		public static string GetShortText(string text, int maxLength)
		{
			if (text == null || text.Length <= maxLength)
				return text;
			if (maxLength <= 0)
				return "";
			int left = maxLength / 2;
			int right = maxLength - left - 1;
			return text.Substring(0, left) + "…" + text.Substring(text.Length - right);
		}
		
		public static bool IsIdentifier(string text)
		{
			if (text != null && text != "" && (text[0] == '_' || char.IsLetter(text[0])))
			{
				for (int i = 1; i < text.Length; ++i)
				{
					char c = text[i];
					if (c != '_' && !char.IsLetterOrDigit(c))
						return false;
				}
				return true;
			}
			return false;
		}
		
		public static int MatchesCount(string text, char c)
		{
			int count = 0;
			int index = 0;
			while (true)
			{
				index = text.IndexOf(c, index);
				if (index == -1)
					break;
				++count;
				++index;
				if (index >= text.Length)
					break;
			}
			return count;
		}
		
        private static int RomanNumberOfChar(char c)
        {
	        switch (c)
	        {
				case 'I': return 1;
				case 'V': return 5;
				case 'X': return 10;
				case 'L': return 50;
				case 'C': return 100;
				case 'D': return 500;
				case 'M': return 1000;
			}
			return 0;
        }

		// https://stackoverflow.com/questions/7040289/converting-integers-to-roman-numerals
		public static string RomanOf(int number)
		{
			if (number <= 0 || number > 3999)
			{
				return number + "";
			}
			StringBuilder builder = new StringBuilder();
			while (number > 0)
			{
				if (number >= 1000)
				{
					builder.Append("M");
					number -= 1000;
				}
				else if (number >= 900)
				{
					builder.Append("CM");
					number -= 900;
				}
				else if (number >= 500)
				{
					builder.Append("D");
					number -= 500;
				}
				else if (number >= 400)
				{
					builder.Append("CD");
					number -= 400;
				}
				else if (number >= 100)
				{
					builder.Append("C");
					number -= 100;
				}
				else if (number >= 90)
				{
					builder.Append("XC");
					number -= 90;
				}
				else if (number >= 50)
				{
					builder.Append("L");
					number -= 50;
				}
				else if (number >= 40)
				{
					builder.Append("XL");
					number -= 40;
				}
				else if (number >= 10)
				{
					builder.Append("X");
					number -= 10;
				}
				else if (number >= 9)
				{
					builder.Append("IX");
					number -= 9;
				}
				else if (number >= 5)
				{
					builder.Append("V");
					number -= 5;
				}
				else if (number >= 4)
				{
					builder.Append("IV");
					number -= 4;
				}
				else if (number >= 1)
				{
					builder.Append("I");
					number -= 1;
				}
			}
			return builder.ToString();
		}
	
		public static int OfRoman(string roman)
		{
			int total = 0;
			int current = 0;
			int previous = 0;
			char currentRoman, previousRoman = '\0';
			for (int i = 0; i < roman.Length; i++)
			{
				currentRoman = roman[i];
				previous = previousRoman != '\0' ? RomanNumberOfChar(previousRoman) : '\0';
				current = RomanNumberOfChar(currentRoman);
				if (current == 0)
				{
					int.TryParse(roman, out total);
					return total;
				}
				if (previous != 0 && current > previous)
				{
					total = total - (2 * previous) + current;
				}
				else
				{
					total += current;
				}
				previousRoman = currentRoman;
			}
			return total;
		}
	}
}
