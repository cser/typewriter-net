using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MulticaretEditor
{
	public class FileNameFilter
	{
		private readonly string[] parts;
		private readonly FileNameFilter next;
		
		public FileNameFilter(string pattern)
		{
			int index = pattern.IndexOf(";");
			if (index != -1)
			{
				next = new FileNameFilter(pattern.Substring(index + 1));
				pattern = pattern.Substring(0, index);
			}
			else
			{
				next = null;
			}
			pattern = pattern.Trim();
			List<string> parts = new List<string>();
			StringBuilder builder = new StringBuilder();
			char prevC = '\0';
			for (int i = 0; i < pattern.Length; i++)
			{
				char c = pattern[i];
				if (c == '*')
				{
					if (prevC == '*')
					{
						continue;
					}
					if (builder.Length > 0)
					{
						parts.Add(builder.ToString());
						builder.Length = 0;
					}
					if (prevC != c)
						parts.Add("*");
				}
				else if (c == '?')
				{
					if (builder.Length > 0)
					{
						parts.Add(builder.ToString());
						builder.Length = 0;
					}
					parts.Add("?");
				}
				else
				{
					builder.Append(c);
				}
				prevC = c;
			}
			if (builder.Length > 0)
			{
				parts.Add(builder.ToString());
			}
			while (true)
			{
				bool changed = false;
				for (int i = parts.Count; i-- > 1;)
				{
					if (parts[i] == "?" && parts[i - 1] == "*")
					{
						parts[i] = "*";
						if (i + 1 < parts.Count && parts[i + 1] == "*")
							parts.RemoveAt(i + 1);
						parts[i - 1] = "?";
						changed = true;
					}
				}
				if (!changed)
				{
					break;
				}
			}
			this.parts = parts.ToArray();
		}
		
		public bool Match(string text)
		{
			return ProcessMatch(text) || next != null && next.Match(text);
		}
		
		private bool ProcessMatch(string text)
		{
			int length = text.Length;
			int partsLength = parts.Length;
			int offset = 0;
			for (int i = 0; i < partsLength; i++)
			{
				string part = parts[i];
				if (part == "?")
				{
					offset++;
				}
				else if (part == "*")
				{
					if (i == partsLength - 1)
					{
						return true;
					}
					++i;
					offset = text.IndexOf(parts[i], offset);
					if (offset == -1)
					{
						return false;
					}
					offset += parts[i].Length;
				}
				else
				{
					if (text.Substring(offset, part.Length) != part)
					{
						return false;
					}
					offset += part.Length;
				}
			}
			return offset == length;
		}
	}
}
