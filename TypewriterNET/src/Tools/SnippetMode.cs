using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MulticaretEditor;

public class SnippetMode : TextChangeHook
{
	private MulticaretTextBox textBox;
	private Controller controller;
	
	private KeyMap keyMap;
	private Snippet snippet;
	private int position;
	private Setter onClose;
	
	public SnippetMode(MulticaretTextBox textBox, Controller controller, Snippet snippet, int position,
		Setter onClose)
	{
		this.textBox = textBox;
		this.snippet = snippet;
		this.controller = controller;
		this.position = position;
		this.onClose = onClose;
		
		keyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("&Edit\\Snippets\\Exit", DoExit, null, false);
			keyMap.AddItem(new KeyItem(Keys.Up, null, action));
			keyMap.AddItem(new KeyItem(Keys.Down, null, action));
			keyMap.AddItem(new KeyItem(Keys.PageUp, null, action));
			keyMap.AddItem(new KeyItem(Keys.PageDown, null, action));
			keyMap.AddItem(new KeyItem(Keys.Control | Keys.Home, null, action));
			keyMap.AddItem(new KeyItem(Keys.Control | Keys.End, null, action));
		}
		{
			KeyAction action = new KeyAction("&Edit\\Snippets\\Exit", DoExitWithConsume, null, false);
			keyMap.AddItem(new KeyItem(Keys.Escape, null, action));
		}
	}
	
	public void Show()
	{
		textBox.KeyMap.AddBefore(keyMap);
		textBox.FocusedChange += OnFocusedChange;
		textBox.AfterKeyPress += OnAfterKeyPress;
		controller.Lines.hook = this;
		NextEntry();
	}
	
	public void Close()
	{
		controller.Lines.hook = null;
		textBox.FocusedChange -= OnFocusedChange;
		textBox.AfterKeyPress -= OnAfterKeyPress;
		textBox.KeyMap.RemoveBefore(keyMap);
		if (onClose != null)
			onClose();
	}
	
	private void OnFocusedChange()
	{
		Close();
	}
	
	private bool DoExit(Controller controller)
	{
		Close();
		return false;
	}
	
	private bool DoExitWithConsume(Controller controller)
	{
		Close();
		return true;
	}
	
	private bool DoNextEntry(Controller controller)
	{
		NextEntry();
		return true;
	}
	
	private int state = 0;
	private bool allowNested;
	private bool needNext;
	
	public void NextEntry()
	{
		if (needNext)
		{
			needNext = false;
			if (allowNested)
			{
				allowNested = false;
				SnippetRange prev = snippet.ranges[state];
				SnippetRange range = prev.nested;
				++state;
				int index = state;
				for (; range != null; range = range.nested)
				{
					range.index += prev.index;
					snippet.ranges.Insert(index, range);
					++index;
				}
			}
			else
			{
				++state;
				if (state >= snippet.ranges.Count)
				{
					Close();
				}
			}
		}
		if (state < snippet.ranges.Count)
		{
			SnippetRange range = snippet.ranges[state];
			controller.ClearMinorSelections();
			controller.LastSelection.anchor = position + range.index;
			controller.LastSelection.caret = position + range.index + range.count;
			for (SnippetRange rangeI = range.next; rangeI != null; rangeI = rangeI.next)
			{
				controller.PutNewCursor(controller.Lines.PlaceOf(position + rangeI.index));
				controller.LastSelection.caret = controller.LastSelection.anchor + rangeI.count;
			}
			foreach (SnippetRange rangeI in snippet.ranges)
			{
				for (SnippetRange rangeJ = rangeI.subrange; rangeJ != null; rangeJ = rangeJ.subrange)
				{
					if (rangeJ.order == range.order)
					{
						controller.PutNewCursor(controller.Lines.PlaceOf(position + rangeI.index + rangeJ.index));
						controller.LastSelection.caret = controller.LastSelection.anchor + rangeJ.count;
					}
				}
			}
			controller.NeedScrollToCaret();
		}
		if (state >= snippet.ranges.Count)
		{
			Close();
			return;
		}
		if (snippet.ranges[state].nested == null)
		{
			++state;
			if (state >= snippet.ranges.Count)
			{
				Close();
			}
		}
		else
		{
			needNext = true;
			allowNested = true;
		}
	}
	
	public override void InsertText(int index, string text)
	{
		if (state <= 0 && state >= snippet.ranges.Count)
		{
			return;
		}
		int offset = text.Length;
		foreach (SnippetRange range in snippet.ranges)
		{
			for (SnippetRange rangeI = range; rangeI != null; rangeI = rangeI.next)
			{
				if (position + rangeI.index >= index)
				{
					rangeI.index += offset;
				}
				else if (position + rangeI.index + rangeI.count >= index)
				{
					rangeI.count += offset;
				}
			}
		}
		allowNested = false;
	}
	
	public override void RemoveText(int index, int count)
	{
		if (state <= 0 && state >= snippet.ranges.Count)
		{
			return;
		}
		foreach (SnippetRange range in snippet.ranges)
		{
			for (SnippetRange rangeI = range; rangeI != null; rangeI = rangeI.next)
			{
				if (position + rangeI.index >= index)
				{
					rangeI.index -= count;
				}
				else if (position + rangeI.index + rangeI.count >= index)
				{
					rangeI.count -= count;
				}
			}
		}
		allowNested = false;
	}
	
	private void OnAfterKeyPress()
	{
		if (needNext)
		{
			return;
		}
		SnippetRange current = snippet.ranges[state - 1];
		{
			for (SnippetRange rangeI = current; rangeI != null; rangeI = rangeI.next)
			{
				int position0 = position + rangeI.index;
				int position1 = position + rangeI.index + rangeI.count;
				bool isInside = false;
				foreach (Selection selection in controller.Selections)
				{
					if (selection.caret >= position0 && selection.caret <= position1)
					{
						isInside = true;
					}
				}
				if (!isInside)
				{
					Close();
				}
			}
		}
	}
}