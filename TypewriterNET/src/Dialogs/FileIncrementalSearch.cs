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

public class FileIncrementalSearch : IncrementalSearchBase
{
	public FileIncrementalSearch(FindInFilesDialog.Data data, FindParams findParams, TempSettings tempSettings)
		: base(tempSettings, "File search", "Incremental file search")
	{
		this.data = data;
		this.findParams = findParams;
	}
	
	override protected string GetSubname()
	{
		return Directory.GetCurrentDirectory() + "\\" + MainForm.Settings.findInFilesFilter.Value;
	}
	
	private const string Dots = "...";

	private FindInFilesDialog.Data data;
	private FindParams findParams;
	private MulticaretTextBox filterTextBox;
	private char directorySeparator;
	private List<string> filesList = new List<string>();

	private Thread thread;
	
	protected override void DoInnerCreate(KeyMap textKeyMap)
	{
		textKeyMap.AddItem(new KeyItem(Keys.Control | Keys.E, null,
			new KeyAction("F&ind\\Temp filter", DoSwitchToTempFilter, null, false)));
		textKeyMap.AddItem(new KeyItem(Keys.Control | Keys.E, null,
			new KeyAction("F&ind\\Switch to input field", DoSwitchToInputField, null, false)));
		filterTextBox = new MulticaretTextBox(true);
		filterTextBox.FontFamily = FontFamily.GenericMonospace;
		filterTextBox.FontSize = 10.25f;
		filterTextBox.ShowLineNumbers = false;
		filterTextBox.HighlightCurrentLine = false;
		filterTextBox.KeyMap.AddAfter(KeyMap);
		filterTextBox.KeyMap.AddAfter(textKeyMap, 1);
		filterTextBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
		filterTextBox.FocusedChange += OnTextBoxFocusedChange;
		filterTextBox.Visible = false;
		filterTextBox.Text = data.currentFilter.value;
		filterTextBox.TextChange += OnFilterTextChange;
		Controls.Add(filterTextBox);
	}

	override protected bool Prebuild()
	{
		string directory = MainForm.Settings.findInFilesDir.Value;
		if (string.IsNullOrEmpty(directory))
		{
			directory = Directory.GetCurrentDirectory();
		}
		FileSystemScanner scanner = new FileSystemScanner(
			directory,
			MainForm.Settings.findInFilesFilter.Value,
			MainForm.Settings.findInFilesIgnoreDir.Value);
		thread = new Thread(new ThreadStart(scanner.Scan));
		thread.Start();
		thread.Join(new TimeSpan(0, 0, MainForm.Settings.fileIncrementalSearchTimeout.Value));
		if (scanner.done)
		{
			if (scanner.error != null)
			{
				MainForm.Dialogs.ShowInfo("Error", scanner.error);
			}
		}
		else
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
		foreach (string file in scanner.files)
		{
			filesList.Add(file.StartsWith(currentDirectory) ? file.Substring(currentDirectory.Length) : file);
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
			Buffer buffer = MainForm.LoadFile(lineText);
			if (buffer != null)
			{
				buffer.Controller.ViAddHistoryPosition(true);
			}
			DispatchNeedClose();
		}
	}
	
	private void OnFilterTextChange()
	{
		UpdateFilterText();
	}
	
	private void UpdateFilterText()
	{
		data.currentFilter.value = filterTextBox.Text;
		string filterText = GetFilterText();
		tabBar.Text = Name + " - " + (string.IsNullOrEmpty(filterTextBox.Text) ? filterText : "[" + filterText + "]");
	}
	
	private bool DoSwitchToInputField(Controller controller)
	{
		filterTextBox.Visible = false;
		textBox.Focus();
		return true;
	}
	
	private string GetFilterText()
	{
		return !string.IsNullOrEmpty(filterTextBox.Text) ? filterTextBox.Text : MainForm.Settings.findInFilesFilter.Value;
	}
	
	private void OnTextBoxFocusedChange()
	{
		if (Destroyed)
			return;
		tabBar.Selected = textBox.Focused || filterTextBox.Focused;
		if (textBox.Focused)
		{
			filterTextBox.Visible = false;
			Nest.MainForm.SetFocus(textBox, textBox.KeyMap, null);
		}
		else if (filterTextBox.Focused)
		{
			Nest.MainForm.SetFocus(filterTextBox, filterTextBox.KeyMap, null);
		}
		UpdateFindParams();
	}
	
	private void UpdateFindParams()
	{
		tabBar.Text2 = (string.IsNullOrEmpty(filterTextBox.Text) ? " filter  " : "[filter] ") +
			(findParams != null ? findParams.GetIndicationText() : "");
	}
	
	private bool DoNormalMode(Controller controller)
	{
		filterTextBox.SetViMode(true);
		filterTextBox.Controller.ViFixPositions(false);
		textBox.SetViMode(true);
		textBox.Controller.ViFixPositions(false);
		return true;
	}
	
	private bool DoFilterNextPattern(Controller controller)
	{
		return GetFilterHistoryPattern(false);
	}
	
	private bool GetFilterHistoryPattern(bool isPrev)
	{
		string text = filterTextBox.Text;
		string newText = data.filterHistory.GetOrEmpty(text, isPrev);
		if (newText != text)
		{
			filterTextBox.Text = newText;
			filterTextBox.Controller.ClearMinorSelections();
			filterTextBox.Controller.LastSelection.anchor = filterTextBox.Controller.LastSelection.caret = newText.Length;
			UpdateFilterText();
		}
		return true;
	}
	
	private bool DoFilterPrevPattern(Controller controller)
	{
		return GetFilterHistoryPattern(true);
	}
	
	private bool DoFindText(Controller controller)
	{
		string text = textBox.Text;
		if (data.history != null)
			data.history.Add(text);
		if (data.filterHistory != null && !string.IsNullOrEmpty(filterTextBox.Text))
			data.filterHistory.Add(filterTextBox.Text);
		return true;
	}
	
	private bool DoCancel(Controller controller)
	{
		DispatchNeedClose();
		return true;
	}
	
	private bool DoSwitchToTempFilter(Controller controller)
	{
		filterTextBox.Visible = true;
		filterTextBox.Focus();
		return true;
	}
}
