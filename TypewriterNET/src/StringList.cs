using System;
using System.Collections.Generic;
using MulticaretEditor;

public class StringList
{
	private int maxCount = 20;
	public int MaxCount
	{
		get { return maxCount; }
		set { maxCount = value; }
	}

	private List<string> list = new List<string>();
	
	public string GetOrEmpty(string text, bool isPrev)
	{
		int index = list.IndexOf(text);
		if (index == -1)
		{
			index = list.Count;
		}
		else if (string.IsNullOrEmpty(text))
		{
			index = list.Count;
		}
		index += isPrev ? -1 : 1;
		if (index >= list.Count)
			return "";
		return index >= 0 && index < list.Count ? list[index] : text;
	}

	public string Get(string text, bool isPrev)
	{
		int index = list.IndexOf(text);
		if (index == -1)
			index = list.Count;
		index += isPrev ? -1 : 1;
		return index >= 0 && index < list.Count ? list[index] : text;
	}

	public void Add(string text)
	{
		if (string.IsNullOrEmpty(text))
			return;
		list.Remove(text);
		list.Add(text);
		if (list.Count > maxCount)
			list.RemoveRange(0, list.Count - maxCount);
	}

	public SValue Serialize()
	{
		SValue value = SValue.NewList();
		foreach (string text in list)
		{
			value.Add(SValue.NewString(text));
		}
		return value;
	}

	public void Unserialize(SValue value)
	{
		list.Clear();
		foreach (SValue valueI in value.List)
		{
			list.Add(valueI.String);
		}
	}
}
