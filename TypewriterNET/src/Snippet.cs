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
		rawText = new Regex(@"\$\{(\d):\$\{VISUAL\}\}").Replace(rawText, @"${$1}");
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
		StringBuilder builder = new StringBuilder();
		foreach (Part part in parts)
		{
			if (part.isEntry)
			{
				int index = part.entry_value.IndexOf("${");
				bool needParse =
					index != -1 &&
					index + 2 < part.entry_value.Length &&
					char.IsDigit(part.entry_value[index + 2]);
				if (!needParse)
				{
					for (int i = part.entry_value.Length - 1; i-- > 0;)
					{
						if (part.entry_value[i] == '$' && char.IsDigit(part.entry_value[i + 1]))
						{
							needParse = true;
							break;
						}
					}
				}
				if (needParse)
				{	
					List<Part> nested = ParseText(part.entry_value);
					SnippetRange current = part.entry_range;
					SnippetRange subrange = part.entry_range;
					StringBuilder nestedBuilder = new StringBuilder();
					foreach (Part partI in nested)
					{
						if (partI.isEntry)
						{
							if (partI.entry_secondary)
							{
								subrange.subrange = new SnippetRange(partI.entry_order);
								subrange = subrange.subrange;
								subrange.index = nestedBuilder.Length;
								foreach (Part partJ in parts)
								{
									if (partJ.isEntry && !partJ.entry_secondary && partJ.entry_order == subrange.order)
									{
										subrange.defaultValue = partJ.entry_value;
									}
								}
								subrange.count = subrange.defaultValue.Length;
								nestedBuilder.Append(subrange.defaultValue);
							}
							else
							{
								current.nested = new SnippetRange(partI.entry_order);
								current = current.nested;
								current.index = nestedBuilder.Length;
								current.count = partI.entry_value.Length;
								current.defaultValue = partI.entry_value;
								nestedBuilder.Append(current.defaultValue);
							}
						}
						else
						{
							nestedBuilder.Append(partI.text_value);
						}
					}
					part.entry_range.defaultValue = nestedBuilder.ToString();
					part.entry_range.count = part.entry_range.defaultValue.Length;
				}
				part.entry_range.index = builder.Length;
				part.entry_range.count = part.entry_range.defaultValue.Length;
				builder.Append(part.entry_range.defaultValue);
			}
			else
			{
				builder.Append(part.text_value);
			}
		}
		int builderLength = builder.Length;
		if (builderLength >= 2 && builder[builderLength - 2] == '\r' && builder[builderLength - 1] == '\n')
		{
			builder.Length -= 2;
		}
		else if (builderLength >= 1 && (builder[builderLength - 1] == '\n' || builder[builderLength - 1] == '\r'))
		{
			--builder.Length;
		}
		_startText = builder.ToString();
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
					if (i - prevI > 0)
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
		if (prevI < rawText.Length)
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
	
	protected virtual DateTime GetNow()
	{
		return DateTime.Now;
	}
	
	protected string ReplaceTime(string text)
	{
		string bra = "`strftime(\"";
		string ket = "\")`";
		if (text.IndexOf(bra) == -1)
		{
			return text;
		}
		DateTime time = GetNow();
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
			builder.Append(text, prevIndex, braIndex - prevIndex);
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
