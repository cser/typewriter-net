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

public class FileIncrementalSearch : ADialog
{
	private SwitchList<string> list;
	private TabBar<string> tabBar;
	private SplitLine splitLine;
	private MulticaretTextBox variantsTextBox;
	private MulticaretTextBox textBox;

	public FileIncrementalSearch()
	{
	}

	override protected void DoCreate()
	{
		list = new SwitchList<string>();
		tabBar = new TabBar<string>(list, TabBar<string>.DefaultStringOf);
		tabBar.Text = "Search";
		tabBar.CloseClick += OnCloseClick;
		Controls.Add(tabBar);

		splitLine = new SplitLine();
		Controls.Add(splitLine);

		KeyMap frameKeyMap = new KeyMap();
		frameKeyMap.AddItem(new KeyItem(Keys.Escape, null, new KeyAction("&View\\Close search", DoClose, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Tab, null, new KeyAction("V&iew\\Next field", DoNextField, null, false)));
		{
			KeyAction action = new KeyAction("&View\\Open searching file", DoOpen, null, false);
			frameKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
			frameKeyMap.AddItem(new KeyItem(Keys.None, null, action).SetDoubleClick(true));
		}

		variantsTextBox = new MulticaretTextBox();
		variantsTextBox.KeyMap.AddAfter(KeyMap);
		variantsTextBox.KeyMap.AddAfter(frameKeyMap, 1);
		variantsTextBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
		variantsTextBox.FocusedChange += OnTextBoxFocusedChange;
		variantsTextBox.Controller.isReadonly = true;
		Controls.Add(variantsTextBox);

		textBox = new MulticaretTextBox();
		textBox.KeyMap.AddAfter(KeyMap);
		textBox.KeyMap.AddAfter(frameKeyMap, 1);
		textBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
		textBox.FocusedChange += OnTextBoxFocusedChange;
		textBox.TextChange += OnTextBoxTextChange;
		Controls.Add(textBox);

		SetTextBoxParameters();

		tabBar.MouseDown += OnTabBarMouseDown;
		InitResizing(tabBar, splitLine);
		Height = MinSize.Height;

		Name = Directory.GetCurrentDirectory();
		BuildFilesList();
		InitVariantsText("");
	}

	private void OnCloseClick()
	{
		DispatchNeedClose();
	}

	override protected void DoDestroy()
	{
	}

	new public string Name
	{
		get { return list.Count > 0 ? list[0] : ""; }
		set
		{
			list.Clear();
			list.Add(value);
		}
	}

	override public Size MinSize { get { return new Size(tabBar.Height * 3, tabBar.Height * 2); } }

	override public void Focus()
	{
		textBox.Focus();
	}

	private void OnTabBarMouseDown(object sender, EventArgs e)
	{
		textBox.Focus();
	}

	private void OnTextBoxFocusedChange()
	{
		if (Destroyed)
			return;
		tabBar.Selected = textBox.Focused;
		if (textBox.Focused)
			Nest.MainForm.SetFocus(textBox, textBox.KeyMap, null);
	}

	private void OnTextBoxTextChange()
	{
		InitVariantsText(GetVariantsText(textBox.Text));
	}

	override public bool Focused { get { return textBox.Focused; } }

	override protected void OnResize(EventArgs e)
	{
		base.OnResize(e);
		int tabBarHeight = tabBar.Height;
		tabBar.Size = new Size(Width, tabBarHeight);
		splitLine.Location = new Point(Width - 10, tabBarHeight);
		splitLine.Size = new Size(10, Height - tabBarHeight);
		variantsTextBox.Location = new Point(0, tabBarHeight);
		variantsTextBox.Size = new Size(Width - 10, Height - tabBarHeight - variantsTextBox.CharHeight - 2);
		variantsTextBox.Controller.NeedScrollToCaret();
		textBox.Location = new Point(0, Height - variantsTextBox.CharHeight);
		textBox.Size = new Size(Width - 10, variantsTextBox.CharHeight);
	}

	override protected void DoUpdateSettings(Settings settings, UpdatePhase phase)
	{
		if (phase == UpdatePhase.Raw)
		{
			settings.ApplySimpleParameters(variantsTextBox);
			settings.ApplySimpleParameters(textBox);
			SetTextBoxParameters();
			tabBar.SetFont(settings.font.Value, settings.fontSize.Value);
		}
		else if (phase == UpdatePhase.Parsed)
		{
			BackColor = settings.ParsedScheme.tabsBgColor;
			variantsTextBox.Scheme = settings.ParsedScheme;
			textBox.Scheme = settings.ParsedScheme;
			tabBar.Scheme = settings.ParsedScheme;
		}
	}

	private void SetTextBoxParameters()
	{
		variantsTextBox.ShowLineNumbers = false;
		variantsTextBox.HighlightCurrentLine = true;
		variantsTextBox.WordWrap = true;

		textBox.ShowLineNumbers = false;
		textBox.HighlightCurrentLine = false;
	}

	private bool DoClose(Controller controller)
	{
		DispatchNeedClose();
		return true;
	}

	private bool DoNextField(Controller controller)
	{
		if (controller == textBox.Controller)
			variantsTextBox.Focus();
		else
			textBox.Focus();
		return true;
	}

	private void InitVariantsText(string text)
	{
		variantsTextBox.Controller.InitText(text);
		variantsTextBox.Controller.ClearMinorSelections();
		Selection selection = variantsTextBox.Controller.LastSelection;
		Place place = new Place(0, variantsTextBox.Controller.Lines.LinesCount - 1);
		selection.anchor = selection.caret = variantsTextBox.Controller.Lines.IndexOf(place);
		variantsTextBox.Invalidate();
		Nest.size = tabBar.Height + variantsTextBox.CharHeight *
			(!string.IsNullOrEmpty(text) && variantsTextBox.Controller != null ? variantsTextBox.GetScrollSizeY() + 1 : 1) + 4;
		SetNeedResize();
	}

	private List<string> filesList;

	private void BuildFilesList()
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

	private string compareText;

	private string GetVariantsText(string text)
	{
		if (string.IsNullOrEmpty(text))
			return "";
		List<string> files = new List<string>();
		foreach (string file in filesList)
		{
			if (file.Contains(text))
				files.Add(file);
		}
		compareText = text;
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
		int offset0 = file0.Length - file0.IndexOf(compareText);
		int offset1 = file1.Length - file1.IndexOf(compareText);
		if (offset0 != offset1)
			return offset1 - offset0;
		return file1.Length - file0.Length;
	}

	private bool DoOpen(Controller controller)
	{
		Place place = variantsTextBox.Controller.Lines.PlaceOf(variantsTextBox.Controller.LastSelection.caret);
		string file = variantsTextBox.Controller.Lines[place.iLine].Text.Trim();
		if (!string.IsNullOrEmpty(file))
		{
			MainForm.LoadFile(file);
			DispatchNeedClose();
		}
		return true;
	}
}
