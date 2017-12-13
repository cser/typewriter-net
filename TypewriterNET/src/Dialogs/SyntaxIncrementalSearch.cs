using System;
using System.Collections.Generic;
using System.Text;
using MulticaretEditor;

public class SyntaxIncrementalSearch : IncrementalSearchBase
{
	public SyntaxIncrementalSearch(TempSettings tempSettings)
		: base(tempSettings, "Syntax selection", "Syntax selection", null)
	{
	}

	private List<string> items = new List<string>();
	private List<string> sortedItems = new List<string>();
	private string currentItem;
	private const string Reset = "[reset]";

	override protected bool Prebuild()
	{
		if (MainForm.LastFrame == null)
			return false;
		MulticaretTextBox textBox = MainForm.LastFrame.TextBox;
		if (textBox == null)
			return false;

		items.Clear();		
		foreach (SyntaxFilesScanner.LanguageInfo info in MainForm.SyntaxFilesScanner.Infos)
		{
			items.Add(info.syntax);
		}
		
		Highlighter highlighter = MainForm.LastFrame.TextBox.Highlighter;
		currentItem = highlighter != null ? highlighter.type : Reset;
		return true;
	}
	
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
		//textBox.Controller.Lines.ResetHighlighting();
		buffer.customSyntax = lineText != Reset ? lineText : null;
		MainForm.UpdateHighlighter(textBox, buffer.Name, buffer);
		DispatchNeedClose();
	}
}