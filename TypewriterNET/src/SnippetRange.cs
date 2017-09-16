using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Diagnostics;
using MulticaretEditor;

public class SnippetRange
{
	public readonly string order;
	
	public SnippetRange next;
	public SnippetRange subrange;
	public SnippetRange nested;
	public int index;
	public int count;
	public string defaultValue;
	
	public SnippetRange(string order)
	{
		this.order = order;
	}
	
	public static int Compare(SnippetRange a, SnippetRange b)
	{
		if (a.order != b.order)
		{
			if (a.order == "0")
			{
				return 1;
			}
			if (b.order == "0")
			{
				return -1;
			}
			return string.Compare(a.order, b.order);
		}
		if (a.index != b.index)
		{
			return a.index - b.index;
		}
		return a.count - b.count;
	}
}
