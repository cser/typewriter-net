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

public class FileIncrementalSearch : IncrementalSearchBase
{
	public FileIncrementalSearch() : base("Incremental file search")
	{
	}

	private string compareText;
	private char directorySeparator;
	private List<string> filesList;

	override protected void Prebuild()
	{
		filesList = new List<string>();
		string filter = MainForm.Settings.findInFilesFilter.Value;
		if (string.IsNullOrEmpty(filter))
			filter = "*";
		string[] files = null;
		try
		{
			files = Directory.GetFiles(Directory.GetCurrentDirectory(), filter, SearchOption.AllDirectories);
		}
		catch (Exception e)
		{
			MainForm.Dialogs.ShowInfo("Error", "File list reading error: " + e.Message);
			DispatchNeedClose();
			return;
		}
		string currentDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;
		foreach (string file in files)
		{
			string path = file;
			if (path.StartsWith(currentDirectory))
				path = file.Substring(currentDirectory.Length);
			filesList.Add(path);
		}
	}

	override protected string GetVariantsText(string text)
	{
		List<string> files = new List<string>();
		foreach (string file in filesList)
		{
			if (file.Contains(text))
				files.Add(file);
		}
		compareText = text;
		directorySeparator = Path.DirectorySeparatorChar;
		files.Sort(CompareFiles);
		StringBuilder builder = new StringBuilder();
		bool first = true;
		foreach (string file in files)
		{
			if (!first)
				builder.AppendLine();
			first = false;
			builder.Append(file);
		}
		return builder.ToString();
	}

	private int CompareFiles(string file0, string file1)
	{
		int index0 = file0.LastIndexOf(compareText);
		int index1 = file1.LastIndexOf(compareText);
		int separatorCriterion0 = index0 == file0.LastIndexOf(directorySeparator) + 1 ? 1 : 0;
		int separatorCriterion1 = index1 == file1.LastIndexOf(directorySeparator) + 1 ? 1 : 0;
		if (separatorCriterion0 != separatorCriterion1)
			return separatorCriterion0 - separatorCriterion1;
		int offset0 = file0.Length - index0;
		int offset1 = file1.Length - index1;
		if (offset0 != offset1)
			return offset1 - offset0;
		return file1.Length - file0.Length;
	}

	override protected void Execute(int line, string lineText)
	{
		if (!string.IsNullOrEmpty(lineText))
		{
			MainForm.LoadFile(lineText);
			DispatchNeedClose();
		}
	}
}
