using System;
using System.Collections.Generic;
using System.Text;
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
			if (line.StartsWith("snippet ") || line.StartsWith("snippet\t"))
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
				int descIndex = -1;
				for (int i = 0; i < key.Length; ++i)
				{
					char c = key[i];
					if (c == ' ' || c == '\t')
					{
						descIndex = i;
						break;
					}
				}
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
	
	private readonly List<SnippetAtom> _atoms = new List<SnippetAtom>();
	public IEnumerable<SnippetAtom> Atoms { get { return _atoms; } }
}
