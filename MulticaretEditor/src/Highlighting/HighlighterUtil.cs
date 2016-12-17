using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Globalization;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class HighlighterUtil
	{
		public static Color? ParseColor(string text)
		{
			if (!string.IsNullOrEmpty(text) && text[0] == '#')
			{
				if (text.Length == 4)
					text = new string(new char[] { text[0], text[1], text[1], text[2], text[2], text[3], text[3] });
				int value;
				if (int.TryParse(text.Substring(1, text.Length - 1), NumberStyles.HexNumber, CultureInfo.InstalledUICulture, out value))
					return Color.FromArgb(value | Color.Black.ToArgb());
			}
			return null;
		}
		
		private struct RegexChar
		{
			public char special;
			public string text;
			
			public RegexChar(char special, string text)
			{
				this.special = special;
				this.text = text;
			}
		}
		
		public static string LazyOfRegex(string regex)
		{
			string specials = @".$^{}[](|)*+?\";
			int length = regex.Length;
			List<RegexChar> chars = new List<RegexChar>();
			for (int i = 0; i < length; i++)
			{
				char c = regex[i];
				if (c == '\\')
				{
					i++;
					if (i >= length)
						break;
					c = regex[i];
					string text = "\\" + c;
					if (specials.IndexOf(c) == -1)
					{
						if (char.IsNumber(c) || c == 'x' || c == 'u')
						{
							while (true)
							{
								if (i + 1 >= length)
									break;
								c = regex[i + 1];
								if (!char.IsNumber(c))
									break;
								i++;
								text += c;
							}
						}
						else if (c == 'c')
						{
							i++;
							if (i < length)
							{
								c = regex[i];
								text += c;
							}
						}
					}
					chars.Add(new RegexChar('\0', text));
				}
				else
				{
					chars.Add(new RegexChar(specials.IndexOf(c) != -1 ? c : '\0', c + ""));
				}
			}
			int charsCount = chars.Count;
			char right = '\0';
			for (int i = charsCount; i-- > 0;)
			{
				char current = chars[i].special;
				switch (current)
				{
					case '*':
					case '+':
					case '}':
						if (right != '?')
							chars.Insert(i + 1, new RegexChar('?', "?"));
						break;
					case '?':
						if (right != '?' && (i < 1 || chars[i - 1].special == '\0'))
							chars.Insert(i + 1, new RegexChar('?', "?"));
						break;
					default:
						break;
				}
				right = current;
			}
			StringBuilder result = new StringBuilder();
			foreach (RegexChar c in chars)
			{
				result.Append(c.text);
			}
			return result.ToString();
		}
		
		public static string FixRegexUnicodeChars(string regex)
		{
			return new Regex(@"\\(\d\d\d\d)").Replace(regex, @"\u$1");
		}
		
		public static bool GetRGBForHighlight(List<Char> chars, int iChar, out int offset, out Color color)
		{
			offset = 0;
			int count = 0;
			for (int i = iChar; i-- > 0;)
			{
				char c = chars[i].c;
				if (count > 8 || !(c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F'))
					break;
				offset--;
				count++;
			}
			int charsCount = chars.Count;
			for (int i = iChar; i < charsCount; i++)
			{
				char c = chars[i].c;
				if (count > 8 || !(c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F'))
					break;
				count++;
			}
			
			if (count == 8)
			{
				offset += 2;
				count = 6;
			}
			if (count == 6)
			{
				int i = iChar + offset;
				int rgb = int.Parse(
					new string(new char[] { chars[i].c, chars[i + 1].c, chars[i + 2].c, chars[i + 3].c, chars[i + 4].c, chars[i + 5].c }),
				    NumberStyles.HexNumber, CultureInfo.InstalledUICulture);
				color = Color.FromArgb(Color.Black.ToArgb() | rgb);
				return true;
			}
			color = Color.Black;
			return false;
		}
		
		public static Regex GetFilenamePatternRegex(string pattern)
		{
			string s = pattern.Replace("+", "\\+").Replace(".", "\\.").Replace("?", ".").Replace("*", ".*");
			return new Regex("^" + s + "$");
		}
	}
}
