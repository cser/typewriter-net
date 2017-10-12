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
	private string current;
	private string last;
	private bool changed;
	
	public string Current { get { return current ?? ""; } }
	
	public void SetCurrent(string value)
	{
		string newCurrent = value ?? "";
		if (current != newCurrent)
		{
			current = newCurrent;
			last = newCurrent;
			changed = true;
		}
	}
	
	public void Switch(bool isPrev)
	{
		if (list.Count == 0)
		{
			current = last ?? current;
		}
		else if (changed)
		{
			current = list[list.Count - 1];
			if (current == last && list.Count > 1)
			{
				current = list[list.Count - 2];
			}
		}
		else
		{
			int index = list.IndexOf(current);
			if (isPrev)
			{
				if (index == -1 || current == last)
				{
					index = list.Count - 1;
				}
				else
				{
					--index;
				}
				if (index > 0 && last == list[index])
				{
					--index;
				}
				else if (index == 0 && last == list[index] && list.Count > 1)
				{
					index++;
				}
				current = index >= 0 ? list[index] : list[0];
			}
			else if (index != -1 && current != last)
			{
				++index;
				if (index < list.Count && last == list[index])
				{
					++index;
				}
				current = index < list.Count ? list[index] : last;
			}
		}
		changed = false;
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
	
	public bool Switch(MulticaretTextBox textBox, bool isPrev)
	{
		SetCurrent(textBox.Text);
		Switch(isPrev);
		if (textBox.Text != Current)
		{
			textBox.Controller.ClearMinorSelections();
			textBox.Controller.SelectAll();
			textBox.Controller.InsertText(Current);
			return true;
		}
		return false;
	}

	public SValue Serialize()
	{
		SValue value = SValue.NewList();
		foreach (string text in list)
		{
			if (!string.IsNullOrEmpty(text))
			{
				value.Add(SValue.NewString(text));
			}
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
