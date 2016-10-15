using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;
using MulticaretEditor;

public class FileIncrementalSearch : IncrementalSearchBase
{
	public FileIncrementalSearch(TempSettings tempSettings)
		: base(tempSettings, "File search", "Incremental file search")
	{
	}
	
	override protected string GetSubname()
	{
		return Directory.GetCurrentDirectory();
	}
	
	private const string Dots = "...";

	private char directorySeparator;
	private List<string> filesList = new List<string>();

	private string filter;
	private Thread thread;
	private string[] files;
	private string error;
	
	override protected bool Prebuild()
	{
		files = null;
		error = null;
		filter = MainForm.Settings.findInFilesFilter.Value;
		FileNameFilter hardFilter = null;
		if (string.IsNullOrEmpty(filter))
		{
			filter = "*";
		}
		else if (filter.Contains(";"))
		{
			hardFilter = new FileNameFilter(filter);
			filter = "*";
		}
		thread = new Thread(new ThreadStart(ScanFiles));
		thread.Start();
		thread.Join(new TimeSpan(0, 0, MainForm.Settings.fileIncrementalSearchTimeout.Value));
		if (error != null)
		{
			MainForm.Dialogs.ShowInfo("Error", error);
			return false;
		}
		else if (files == null)
		{
			MainForm.Dialogs.ShowInfo(
				"Error",
				"File system scanning timeout (" +
				MainForm.Settings.fileIncrementalSearchTimeout.name + "=" + MainForm.Settings.fileIncrementalSearchTimeout.Value +
				")"
			);
			return false;
		}
		filesList.Clear();
		string currentDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;
		for (int i = 0; i < files.Length; ++i)
		{
			string file = files[i];
			if (file.StartsWith(currentDirectory))
				file = file.Substring(currentDirectory.Length);
			if (hardFilter != null)
			{
				string name = Path.GetFileName(file);
				if (!hardFilter.Match(name))
					continue;
			}
			filesList.Add(file);
		}
		return true;
	}
	
	private void ScanFiles()
	{
		try
		{
			files = Directory.GetFiles(Directory.GetCurrentDirectory(), filter, SearchOption.AllDirectories);
		}
		catch (Exception e)
		{
			error = "File list reading error: " + e.Message;
		}
	}

	override protected string GetVariantsText()
	{
		List<string> files = new List<string>();
		int count = 0;
		foreach (string file in filesList)
		{
			if (GetIndex(file) != -1)
			{
				files.Add(file);
				count++;
				if (count > 500)
				{
					files.Add(Dots);
					break;
				}
			}
		}
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
		int index0 = GetLastIndex(file0);
		int index1 = GetLastIndex(file1);
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
		if (!string.IsNullOrEmpty(lineText) && lineText != Dots)
		{
			MainForm.LoadFile(lineText);
			DispatchNeedClose();
		}
	}
}
