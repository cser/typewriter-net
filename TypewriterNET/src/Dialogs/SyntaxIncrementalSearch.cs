using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;
using MulticaretEditor;

public class SyntaxIncrementalSearch : IncrementalSearchBase
{
	public SyntaxIncrementalSearch() : base("Menu item incremental search")
	{
	}

	private List<string> items;
	private string currentItem;

	override protected void Prebuild()
	{
		items = new List<string>();
		if (MainForm.LastFrame == null)
			return;
		MulticaretTextBox textBox = MainForm.LastFrame.TextBox;
		if (textBox == null)
			return;
		Highlighter highlighter = MainForm.LastFrame.TextBox.Highlighter;
		if (highlighter == null)
			return;
		
		foreach (SyntaxFilesScanner.LanguageInfo info in MainForm.SyntaxFilesScanner.Infos)
		{
			items.Add(info.syntax);
		}
		currentItem = highlighter.type;
	}

	private string compareText;
	private List<string> sortedItems = new List<string>();

	override protected string GetVariantsText(string text)
	{
		compareText = text;
		sortedItems.Clear();
		foreach (string item in items)
		{
			sortedItems.Add(item);
		}
		sortedItems.Sort(CompareItems);
		compareText = text;
		StringBuilder builder = new StringBuilder();
		bool first = true;
		foreach (string item in sortedItems)
		{
			if (item.Contains(text))
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
		int index0 = item0.LastIndexOf(compareText);
		int index1 = item1.LastIndexOf(compareText);
		int offset0 = item0.Length - index0;
		int offset1 = item1.Length - index1;
		if (offset0 != offset1)
			return offset1 - offset0;
		return item1.Length - item0.Length;
	}

	override protected void Execute(int line, string lineText)
	{
		Highlighter highlighter = MainForm.HighlightingSet.GetHighlighter(lineText);
		if (highlighter == null)
			return;
		if (MainForm.LastFrame == null)
			return;
		MulticaretTextBox textBox = MainForm.LastFrame.TextBox;
		if (textBox == null)
			return;
		textBox.Highlighter = highlighter;
		textBox.Controller.Lines.ResetHighlighting();
		DispatchNeedClose();
	}
}