using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;
using MulticaretEditor;

public class SyntaxIncrementalSearch : IncrementalSearchBase
{
	public SyntaxIncrementalSearch(TempSettings tempSettings)
		: base(tempSettings, "Syntax selection", "Syntax selection")
	{
	}

	private List<string> items;
	private string currentItem;

	override protected bool Prebuild()
	{
		items = new List<string>();
		if (MainForm.LastFrame == null)
			return false;
		MulticaretTextBox textBox = MainForm.LastFrame.TextBox;
		if (textBox == null)
			return false;
		Highlighter highlighter = MainForm.LastFrame.TextBox.Highlighter;
		if (highlighter == null)
			return false;
		
		foreach (SyntaxFilesScanner.LanguageInfo info in MainForm.SyntaxFilesScanner.Infos)
		{
			items.Add(info.syntax);
		}
		currentItem = highlighter.type;
		return true;
	}
	
	private const string Reset = "[reset]";

	private List<string> sortedItems = new List<string>();

	override protected string GetVariantsText()
	{
		sortedItems.Clear();
		foreach (string item in items)
		{
			sortedItems.Add(item);
		}
		sortedItems.Sort(CompareItems);
		sortedItems.Insert(0, Reset);
		StringBuilder builder = new StringBuilder();
		bool first = true;
		foreach (string item in sortedItems)
		{
			if (GetIndex(item) != -1)
			{
				if (!first)
					builder.AppendLine();
				first = false;
				builder.Append(item);
			}
		}
		return builder.ToString();
	}

	private int CompareItems(string item0, string item1)
	{
		int equals0 = item0 == currentItem ? 1 : 0;
		int equals1 = item1 == currentItem ? 1 : 0;
		if (equals0 != equals1)
			return equals0 - equals1;
		int index0 = GetLastIndex(item0);
		int index1 = GetLastIndex(item1);
		int offset0 = item0.Length - index0;
		int offset1 = item1.Length - index1;
		if (offset0 != offset1)
			return offset1 - offset0;
		return item1.Length - item0.Length;
	}

	override protected void Execute(int line, string lineText)
	{
		if (MainForm.LastFrame == null)
			return;
		Buffer buffer = MainForm.LastFrame.SelectedBuffer;
		if (buffer == null)
			return;
		MulticaretTextBox textBox = MainForm.LastFrame.TextBox;
		if (textBox == null)
			return;
		buffer.customSyntax = lineText != Reset ? lineText : null;
		MainForm.UpdateHighlighter(textBox, buffer.Name, buffer);
		textBox.Controller.Lines.ResetHighlighting();
		DispatchNeedClose();
	}
}