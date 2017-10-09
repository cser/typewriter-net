using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using MulticaretEditor;

public class FindInFilesDialog : ADialog
{
	public class Data
	{
		public string oldText = "";
		public readonly StringList history;
		public readonly StringList filterHistory;
		public readonly StringValue currentFilter;

		public Data(StringList history, StringList filterHistory, StringValue currentFilter)
		{
			this.history = history;
			this.filterHistory = filterHistory;
			this.currentFilter = currentFilter;
		}
	}

	private Data data;
	private FindParams findParams;
	private Getter<string, string, bool> doFind;
	private TabBar<string> tabBar;
	private MulticaretTextBox textBox;
	private MulticaretTextBox filterTextBox;

	public FindInFilesDialog(Data data, FindParams findParams, Getter<string, string, bool> doFind, string name)
	{
		this.data = data;
		this.findParams = findParams;
		this.doFind = doFind;
		Name = name;
	}

	override protected void DoCreate()
	{
		{
			KeyMap frameKeyMap = new KeyMap();
			frameKeyMap.AddItem(new KeyItem(Keys.Escape, null,
				new KeyAction("F&ind\\Cancel find", DoCancel, null, false)));
			frameKeyMap.AddItem(new KeyItem(Keys.Enter, null,
				new KeyAction("F&ind\\Find text", DoFindText, null, false)));
			frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.E, null,
				new KeyAction("F&ind\\Switch to input field", DoSwitchToInputField, null, false)));
			if (data.filterHistory != null)
			{
				KeyAction prevAction = new KeyAction("F&ind\\Previous filter", DoFilterPrevPattern, null, false);
				KeyAction nextAction = new KeyAction("F&ind\\Next filter", DoFilterNextPattern, null, false);
				frameKeyMap.AddItem(new KeyItem(Keys.Up, null, prevAction));
				frameKeyMap.AddItem(new KeyItem(Keys.Down, null, nextAction));
				frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.P, null, prevAction));
				frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.N, null, nextAction));
			}
			frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.F, null,
				new KeyAction("F&ind\\Vi normal mode", DoNormalMode, null, true)));
			
			filterTextBox = new MulticaretTextBox(true);
			filterTextBox.FontFamily = FontFamily.GenericMonospace;
			filterTextBox.SetFontSize(10.25f, 0);
			filterTextBox.ShowLineNumbers = false;
			filterTextBox.HighlightCurrentLine = false;
			filterTextBox.KeyMap.AddAfter(KeyMap);
			filterTextBox.KeyMap.AddAfter(frameKeyMap, 1);
			filterTextBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
			filterTextBox.FocusedChange += OnTextBoxFocusedChange;
			filterTextBox.Visible = false;
			filterTextBox.Text = data.currentFilter.value;
			filterTextBox.TextChange += OnFilterTextChange;
			Controls.Add(filterTextBox);
		}
		
		tabBar = new TabBar<string>(new SwitchList<string>(), TabBar<string>.DefaultStringOf);
		tabBar.CloseClick += OnCloseClick;
		Controls.Add(tabBar);

		{
			KeyMap frameKeyMap = new KeyMap();
			frameKeyMap.AddItem(new KeyItem(Keys.Escape, null,
				new KeyAction("F&ind\\Cancel find", DoCancel, null, false)));
			frameKeyMap.AddItem(new KeyItem(Keys.Enter, null,
				new KeyAction("F&ind\\Find text", DoFindText, null, false)));
			frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.E, null,
				new KeyAction("F&ind\\Temp filter", DoSwitchToTempFilter, null, false)));
			frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.F, null,
				new KeyAction("&View\\Vi normal mode", DoNormalMode, null, false)));
			if (data.history != null)
			{
				KeyAction prevAction = new KeyAction("F&ind\\Previous pattern", DoPrevPattern, null, false);
				KeyAction nextAction = new KeyAction("F&ind\\Next pattern", DoNextPattern, null, false);
				frameKeyMap.AddItem(new KeyItem(Keys.Up, null, prevAction));
				frameKeyMap.AddItem(new KeyItem(Keys.Down, null, nextAction));
				frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.P, null, prevAction));
				frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.N, null, nextAction));
			}
	
			textBox = new MulticaretTextBox(true);
			textBox.KeyMap.AddAfter(KeyMap);
			textBox.KeyMap.AddAfter(frameKeyMap, 1);
			textBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
			textBox.FocusedChange += OnFiltersTextBoxFocusedChange;
			Controls.Add(textBox);		
		}

		tabBar.RightHint = "Ctrl+E(enter/exit custom filter)" +
			(findParams != null ? "/" + findParams.GetIndicationHint() : "");
		tabBar.MouseDown += OnTabBarMouseDown;
		InitResizing(tabBar, null);
		Height = MinSize.Height;
		UpdateFindParams();
	}

	override public bool Focused { get { return textBox.Focused; } }

	private void OnCloseClick()
	{
		DispatchNeedClose();
	}

	override protected void DoDestroy()
	{
		data.oldText = textBox.Text;
	}

	override public Size MinSize { get { return new Size(tabBar.Height * 3, tabBar.Height + textBox.CharHeight); } }
	
	override protected void OnResize(EventArgs e)
	{
		base.OnResize(e);
		int tabBarHeight = tabBar.Height;
		tabBar.Size = new Size(Width, tabBarHeight);
		textBox.Location = new Point(0, tabBarHeight);
		textBox.Size = new Size(Width, Height - tabBarHeight + 1);
		int size = 20;
		filterTextBox.Location = new Point(Width - 9 * filterTextBox.CharWidth - size * filterTextBox.CharWidth, 2);
		filterTextBox.Size = new Size(size * filterTextBox.CharWidth, filterTextBox.CharHeight + 1);
	}

	override public void Focus()
	{
		textBox.Focus();

		Frame lastFrame = Nest.MainForm.LastFrame;
		Controller lastController = lastFrame != null ? lastFrame.Controller : null;
		if (lastController != null)
		{
			textBox.Text = lastController.Lines.LastSelection.Empty ?
				data.oldText :
				lastController.Lines.GetText(lastController.Lines.LastSelection.Left, lastController.Lines.LastSelection.Count);
			textBox.Controller.SelectAllToEnd();
		}
	}

	private void OnTabBarMouseDown(object sender, EventArgs e)
	{
		textBox.Focus();
	}
	
	private void OnFilterTextChange()
	{
		UpdateFilterText();
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
		if (filterTextBox.Focused)
		{
			int position = filterTextBox.Text.Length;
			filterTextBox.Controller.ClearMinorSelections();
			filterTextBox.Controller.LastSelection.anchor = position;
			filterTextBox.Controller.LastSelection.caret = position;
			filterTextBox.Controller.NeedScrollToCaret();
		}
	}
	
	private void OnFiltersTextBoxFocusedChange()
	{
		OnTextBoxFocusedChange();
	}

	override protected void DoUpdateSettings(Settings settings, UpdatePhase phase)
	{
		if (phase == UpdatePhase.Raw) 
		{
			settings.ApplySimpleParameters(textBox, null);
			textBox.SetViMap(settings.viMapSource.Value, settings.viMapResult.Value);
			settings.ApplySimpleParameters(filterTextBox, null, false);
			filterTextBox.SetViMap(settings.viMapSource.Value, settings.viMapResult.Value);
		}
		else if (phase == UpdatePhase.Parsed)
		{
			textBox.Scheme = settings.ParsedScheme;
			filterTextBox.Scheme = settings.ParsedScheme;
			tabBar.Scheme = settings.ParsedScheme;
			UpdateFilterText();
		}
		else if (phase == UpdatePhase.FindParams)
		{
			UpdateFindParams();
		}
	}
	
	private void UpdateFindParams()
	{
		tabBar.Text2 = (string.IsNullOrEmpty(filterTextBox.Text) ? " filter  " : "[filter] ") +
			(findParams != null ? findParams.GetIndicationText() : "");
	}
	
	private void UpdateFilterText()
	{
		data.currentFilter.value = filterTextBox.Text;
		string filterText = GetFilterText();
		tabBar.Text = Name + " - " + (string.IsNullOrEmpty(filterTextBox.Text) ? filterText : "[" + filterText + "]");
	}
	
	private string GetFilterText()
	{
		return !string.IsNullOrEmpty(filterTextBox.Text) ? filterTextBox.Text : MainForm.Settings.findInFilesFilter.Value;
	}

	private bool DoCancel(Controller controller)
	{
		DispatchNeedClose();
		return true;
	}

	private bool DoFindText(Controller controller)
	{
		string text = textBox.Text;
		if (data.history != null)
			data.history.Add(text);
		if (data.filterHistory != null && !string.IsNullOrEmpty(filterTextBox.Text))
			data.filterHistory.Add(filterTextBox.Text);
		data.currentFilter.value = filterTextBox.Text;
		return doFind(text, GetFilterText());
	}
	
	private bool DoSwitchToTempFilter(Controller controller)
	{
		filterTextBox.Visible = true;
		filterTextBox.Focus();
		return true;
	}
	
	private bool DoSwitchToInputField(Controller controller)
	{
		filterTextBox.Visible = false;
		textBox.Focus();
		return true;
	}
	
	private bool DoPrevPattern(Controller controller)
	{
		return GetHistoryPattern(true);
	}

	private bool DoNextPattern(Controller controller)
	{
		return GetHistoryPattern(false);
	}
	
	private bool DoFilterPrevPattern(Controller controller)
	{
		return GetFilterHistoryPattern(true);
	}
	
	private bool DoFilterNextPattern(Controller controller)
	{
		return GetFilterHistoryPattern(false);
	}

	private bool GetHistoryPattern(bool isPrev)
	{
		data.history.Switch(textBox, isPrev);
		return true;
	}
	
	private bool GetFilterHistoryPattern(bool isPrev)
	{
		if (data.filterHistory.Switch(filterTextBox, isPrev))
		{
			UpdateFilterText();
		}
		return true;
	}
	
	private bool DoNormalMode(Controller controller)
	{
		filterTextBox.SetViMode(true);
		filterTextBox.Controller.ViFixPositions(false);
		textBox.SetViMode(true);
		textBox.Controller.ViFixPositions(false);
		return true;
	}
}
