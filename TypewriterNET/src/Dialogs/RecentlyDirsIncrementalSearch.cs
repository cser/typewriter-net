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
using MulticaretEditor;

public class RecentlyDirsIncrementalSearch : IncrementalSearchBase
{
	public RecentlyDirsIncrementalSearch(TempSettings tempSettings)
		: base(tempSettings, "Recently search", "Incremental recently search", null)
	{
	}
	
	override protected string GetSubname()
	{
		return Directory.GetCurrentDirectory();
	}
	
	private const string Dots = "…";

	private List<string> filesList;
	
	override protected bool Prebuild()
	{
		filesList = tempSettings.GetRecentlyDirs();
		string currentDir = Directory.GetCurrentDirectory().ToLowerInvariant();
		for (int i = filesList.Count; i-- > 0;)
		{
			if (filesList[i].ToLowerInvariant() == currentDir)
				filesList.RemoveAt(i);
		}
		return true;
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

	override protected void Execute(int line, string lineText)
	{
		string currentDir = null;
		if (!string.IsNullOrEmpty(lineText) && lineText != Dots)
		{
			currentDir = lineText;
		}
		else if (lineText != Dots && !string.IsNullOrEmpty(InputTextBox.Text))
		{
			currentDir = InputTextBox.Text;
		}
		if (currentDir != null)
		{
			string error;
			MainForm.SetCurrentDirectory(currentDir, out error);
			if (error != null)
			{
				MainForm.Dialogs.ShowInfo("Error", error);
				return;
			}
			DispatchNeedClose();
		}
	}
	
	protected override bool GetAllowAutocomplete()
	{
		return true;
	}
	
	protected override bool DoAutocomplete(Controller controller)
	{
		CommandDialog.AutocompletePath(InputTextBox, InputTextBox.Text, null, true);
		return true;
	}
}
