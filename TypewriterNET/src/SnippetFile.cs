using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Diagnostics;
using MulticaretEditor;

public class SnippetFile
{
	public SnippetFile(string rawText, string fileName)
	{
		LineSubdivider subdivider = new LineSubdivider(rawText, true);
		StringBuilder builder = new StringBuilder();
		string lastKey = null;
		string lastDesc = null;
		int index = 0;
		foreach (string line in subdivider.GetLines())
		{
			if (line.StartsWith("#") || line.StartsWith("extensions"))
			{
				continue;
			}
			if (line.StartsWith("snippet "))
			{
				if (lastKey != null)
				{
					_atoms.Add(new SnippetAtom(index++, lastKey, lastDesc, builder.ToString(), fileName));
					builder.Length = 0;
					lastKey = null;
					lastDesc = null;
				}
				string key = line.Substring("snippet ".Length).Trim();
				string desc = "";
				int descIndex = key.IndexOf(' ');
				if (descIndex != -1)
				{
					desc = key.Substring(descIndex + 1);
					key = key.Substring(0, descIndex);
				}
				builder.Length = 0;
				if (!string.IsNullOrEmpty(key))
				{
					lastKey = key;
					lastDesc = desc;
					builder.Length = 0;
				}
			}
			else
			{
				if (lastKey != null && line.StartsWith("\t"))
				{
					builder.Append(line.Substring(1));
				}
			}
		}
		if (lastKey != null)
		{
			_atoms.Add(new SnippetAtom(index++, lastKey, lastDesc, builder.ToString(), fileName));
		}
	}
	
	private List<SnippetAtom> _atoms = new List<SnippetAtom>();
	public IEnumerable<SnippetAtom> Atoms { get { return _atoms; } }
}
