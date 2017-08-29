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

public class RecentlyIncrementalSearch : IncrementalSearchBase
{
	public RecentlyIncrementalSearch(TempSettings tempSettings)
		: base(tempSettings, "Recently search", "Incremental recently search", null)
	{
	}
	
	override protected string GetSubname()
	{
		return Directory.GetCurrentDirectory();
	}
	
	private const string Dots = "...";

	private char directorySeparator;
	private List<string> filesList;
	
	override protected bool Prebuild()
	{
		filesList = tempSettings.GetRecentlyFiles();
		
		Buffer lastBuffer = MainForm.LastBuffer;
		string currentFile = lastBuffer != null ? lastBuffer.FullPath : null;
		if (currentFile != null)
		{
			currentFile = currentFile.ToLowerInvariant();
			for (int i = filesList.Count; i-- > 0;)
			{
				if (filesList[i].ToLowerInvariant() == currentFile)
					filesList.RemoveAt(i);
			}
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
		directorySeparator = Path.DirectorySeparatorChar;
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
		if (!string.IsNullOrEmpty(lineText) && lineText != Dots)
		{
			Buffer buffer = MainForm.LoadFile(lineText);
			if (buffer != null)
			{
				buffer.Controller.ViAddHistoryPosition(true);
			}
			DispatchNeedClose();
		}
	}
}
