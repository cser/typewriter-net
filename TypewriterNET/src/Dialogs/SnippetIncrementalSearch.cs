using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using MulticaretEditor;

public class SnippetIncrementalSearch : IncrementalSearchBase
{
	public SnippetIncrementalSearch(TempSettings tempSettings)
		: base(tempSettings, "Snippet selection", "Snippet selection", null)
	{
	}
	
	private List<string> items = new List<string>();
	private List<string> sortedItems = new List<string>();
	private string currentItem;

	override protected bool Prebuild()
	{
		if (MainForm.LastFrame == null)
			return false;
		MulticaretTextBox textBox = MainForm.LastFrame.TextBox;
		if (textBox == null)
			return false;

		items.Clear();
		SnippetFilesScanner scanner = MainForm.SnippetFilesScanner;
		scanner.TryRescan();
		foreach (SnippetInfo info in scanner.Infos)
		{
			items.Add(Path.GetFileName(info.path));
		}
		
		currentItem = null;
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
		if (string.IsNullOrEmpty(lineText))
		{
			DispatchNeedClose();
			return;
		}
		MainForm.CreateAppDataFolders();
		{
			string path = Path.Combine(AppPath.SnippetsDir.appDataPath, lineText);
			if (File.Exists(path))
			{
				MainForm.LoadFile(path);
				DispatchNeedClose();
				return;
			}
		}
		{
			string path = Path.Combine(AppPath.SnippetsDir.startupPath, lineText);
			if (File.Exists(path))
			{
				string newPath = Path.Combine(AppPath.SnippetsDir.appDataPath, lineText);
				string text = "";
				try
				{
					text = File.ReadAllText(path);
				}
				catch
				{
				}
				MainForm.OpenNewAsFile(newPath, text, false);
				DispatchNeedClose();
				return;
			}
		}
		DispatchNeedClose();
	}
}