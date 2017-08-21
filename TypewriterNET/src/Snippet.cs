using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Diagnostics;
using MulticaretEditor;

public class Snippet
{
	public class Part
	{
		public readonly bool isEntry;
		
		public string entry_order;
		public string entry_value;
		public bool entry_secondary;
		public SnippetRange entry_range;
		
		public string text_value;
		
		public Part(bool isEntry)
		{
			this.isEntry = isEntry;
		}
	}
	
	private string _startText;
	public string StartText { get { return _startText; } }
	
	public readonly List<SnippetRange> ranges = new List<SnippetRange>();
	
	public Snippet(string rawText,
		Settings settings,	
		Getter<string, string> replaceDefaultValue)
	{
		rawText = rawText.Replace("${0:${VISUAL}}", "${0}");
		rawText = rawText.Replace("`g:snips_author`", settings.snipsAuthor.Value);
		rawText = ReplaceTime(rawText);
		List<Part> parts = ParseText(rawText);
		List<SnippetRange> secondaryRanges = new List<SnippetRange>();
		foreach (Part part in parts)
		{
			if (part.isEntry)
			{
				if (replaceDefaultValue != null)
				{
					part.entry_value = replaceDefaultValue(part.entry_value);
				}
				part.entry_range = new SnippetRange(part.entry_order);
				part.entry_range.defaultValue = part.entry_value;
				if (part.entry_secondary)
				{
					secondaryRanges.Add(part.entry_range);
				}
				else
				{
					ranges.Add(part.entry_range);
				}
			}
		}
		for (int i = secondaryRanges.Count; i-- > 0;)
		{
			SnippetRange secondaryRange = secondaryRanges[i];
			foreach (SnippetRange range in ranges)
			{
				if (range.order == secondaryRange.order)
				{
					secondaryRange.next = range.next;
					secondaryRange.defaultValue = range.defaultValue;
					secondaryRange.count = range.defaultValue.Length;
					range.next = secondaryRange;
				}
			}
		}
		foreach (SnippetRange rangeI in ranges)
		{
			if (rangeI.defaultValue.StartsWith("$"))
			{
				string order = rangeI.defaultValue.Substring(1);
				foreach (SnippetRange rangeJ in ranges)
				{
					if (rangeJ.order == order)
					{
						rangeJ.subrange = rangeI;
						rangeI.defaultValue = rangeJ.defaultValue;
						break;
					}
				}
			}
		}
		StringBuilder builder = new StringBuilder();
		foreach (Part part in parts)
		{
			if (part.isEntry)
			{
				part.entry_range.index = builder.Length;
				part.entry_range.count = part.entry_range.defaultValue.Length;
				builder.Append(part.entry_range.defaultValue);
			}
			else
			{
				builder.Append(part.text_value);
			}
		}
		_startText = builder.ToString();
		if (_startText.EndsWith("\r\n"))
		{
			_startText = _startText.Substring(0, _startText.Length - 2);
		}
		else if (_startText.EndsWith("\n") || _startText.EndsWith("\r"))
		{
			_startText = _startText.Substring(0, _startText.Length - 1);
		}
		ranges.Sort(SnippetRange.Compare);
	}
	
	protected Snippet()
	{
	}
	
	protected List<Part> ParseText(string rawText)
	{
		List<Part> parts = new List<Part>();
		int prevI = 0;
		for (int i = 0; i < rawText.Length;)
		{
			char c = rawText[i];
			if (c == '$')
			{
				string order;
				string defaultValue;
				bool secondary;
				string entry = ParseEntry(rawText, i, out order, out defaultValue, out secondary);
				if (entry != null)
				{
					{
						Part part = new Part(false);
						part.text_value = rawText.Substring(prevI, i - prevI);
						parts.Add(part);
					}
					i += entry.Length;
					prevI = i;
					{
						Part part = new Part(true);
						part.entry_order = order;
						part.entry_value = defaultValue;
						part.entry_secondary = secondary;
						parts.Add(part);
					}
				}
				else
				{
					++i;
				}
			}
			else
			{
				++i;
			}
		}
		{
			Part part = new Part(false);
			part.text_value = rawText.Substring(prevI);
			parts.Add(part);
		}
		return parts;
	}
	
	protected string ParseEntry(string rawText, int i, out string order, out string defaultValue, out bool secondary)
	{
		order = "0";
		defaultValue = "";
		secondary = false;
		StringBuilder builder = new StringBuilder();
		int length = rawText.Length;
		int index = i + 1;
		if (index >= length || rawText[index] != '{')
		{
			if (!char.IsDigit(rawText[index]))
			{
				return null;
			}
			while (index < length && char.IsDigit(rawText[index]))
			{
				builder.Append(rawText[index]);
				++index;
			}
			order = builder.ToString();
			secondary = true;
			return rawText.Substring(i, index - i);
		}
		++index;
		if (index >= length || !char.IsDigit(rawText[index]))
		{
			return null;
		}
		builder.Length = 0;
		while (index < length && char.IsDigit(rawText[index]))
		{
			builder.Append(rawText[index]);
			++index;
		}
		order = builder.ToString();
		if (index >= length)
		{
			return null;
		}
		if (rawText[index] == ':')
		{
			++index;
			builder.Length = 0;
			if (index < length && rawText[index] == '`')
			{
				++index;
				while (index < length)
				{
					char c = rawText[index];
					if (c == '`')
					{
						++index;
						break;
					}
					builder.Append(c);
					++index;
				}
			}
			else
			{
				int depth = 0;
				while (index < length)
				{
					char c = rawText[index];
					if (c == '$')
					{
						++index;
						if (index < length)
						{
							builder.Append(c);
							c = rawText[index];
							if (c == '{')
							{
								++depth;
							}
						}
					}
					if (c == '}')
					{
						if (depth <= 0)
						{
							break;
						}
						--depth;
					}
					builder.Append(c);
					++index;
				}
			}
			defaultValue = builder.ToString();
		}
		if (rawText[index] != '}')
		{
			return null;
		}
		++index;
		return rawText.Substring(i, index - i);
	}
	
	protected string ReplaceTime(string text)
	{
		string bra = "`strftime(\"";
		string ket = "\")`";
		if (text.IndexOf(bra) == -1)
		{
			return text;
		}
		DateTime time = DateTime.Now;
		StringBuilder builder = new StringBuilder();
		int prevIndex = 0;
		while (true)
		{
			int braIndex = text.IndexOf(bra, prevIndex);
			if (braIndex == -1)
			{
				builder.Append(text, prevIndex, text.Length - prevIndex);
				break;
			}
			builder.Append(text, prevIndex, braIndex);
			int ketIndex = text.IndexOf(ket, braIndex + bra.Length);
			if (ketIndex == -1)
			{
				builder.Append(text, prevIndex, text.Length - prevIndex);
				break;
			}
			string timeText = text.Substring(braIndex + bra.Length, ketIndex - braIndex - bra.Length);
			builder.Append(time.ToString(timeText
				.Replace("%Y", "yyyy")
				.Replace("%m", "MM")
				.Replace("%d", "dd")
				.Replace("%H", "HH")
				.Replace("%M", "mm")
				.Replace("%S", "ss")
				.Replace("%a", "ddd")
				.Replace("%A", "dddd")
				.Replace("%b", "MMM")
				.Replace("%B", "MMMM")
			, CultureInfo.InvariantCulture));
			prevIndex = ketIndex + ket.Length;
		}
		return builder.ToString();
	}
}
