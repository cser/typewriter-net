using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Diagnostics;
using MulticaretEditor;

public class SnippetAtom
{
	public readonly int index;
	public readonly string key;
	public readonly string desc;
	public readonly string text;
	public readonly string fileName;
	
	public SnippetAtom(int index, string key, string desc, string text, string fileName)
	{
		this.index = index;
		this.key = key ?? "";
		this.desc = desc ?? "";
		this.text = text ?? "";
		this.fileName = fileName;
	}
	
	public string GetCompletionText()
	{
		string prefix = "action:";
		return key + " [" + fileName + "] " +
			(desc.StartsWith(prefix) ? desc.Substring(prefix.Length).Trim() : desc).Trim();
	}
	
	public string GetIndentedText(string indent, TabSettings tabSettings)
	{
		LineSubdivider subdivider = new LineSubdivider(text, true);
		StringBuilder builder = new StringBuilder();
		bool first = true;
		foreach (string line in subdivider.GetLines())
		{
			if (!first)
			{
				builder.Append(indent);
			}
			first = false;
			if (tabSettings.useSpaces && line.StartsWith("\t"))
			{
				string tab = tabSettings.Tab;
				int count = 0;
				while (count < line.Length && line[count] == '\t')
				{
					++count;
					builder.Append(tab);
				}
				builder.Append(line.Substring(count));
			}
			else
			{
				builder.Append(line);
			}
		}
		return builder.ToString();
	}
	
	public override string ToString()
	{
		return "(" + key + ":" + desc + " text=" + text + ")";
	}
	
	public static int Compare(SnippetAtom a, SnippetAtom b)
	{
		if (a.key.Length != b.key.Length)
		{
			return b.key.Length - a.key.Length;
		}
		return a.index - b.index;
	}
}
