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
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;
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
				new KeyAction("F&ind|Cancel find", DoCancel, null, false)));
			frameKeyMap.AddItem(new KeyItem(Keys.Enter, null,
				new KeyAction("F&ind|Find text", DoFindText, null, false)));
			frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.E, null,
				new KeyAction("F&ind|Switch to input field", DoSwitchToInputField, null, false)));
			if (data.filterHistory != null)
			{
				frameKeyMap.AddItem(new KeyItem(Keys.Up, null,
					new KeyAction("F&ind|Previous filter", DoFilterPrevPattern, null, false)));
				frameKeyMap.AddItem(new KeyItem(Keys.Down, null,
					new KeyAction("F&ind|Next filter", DoFilterNextPattern, null, false)));
			}
			
			filterTextBox = new MulticaretTextBox();
			filterTextBox.FontFamily = FontFamily.GenericMonospace;
			filterTextBox.FontSize = 10.25f;
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
				new KeyAction("F&ind|Cancel find", DoCancel, null, false)));
			frameKeyMap.AddItem(new KeyItem(Keys.Enter, null,
				new KeyAction("F&ind|Find text", DoFindText, null, false)));
			frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.E, null,
				new KeyAction("F&ind|Temp filter", DoSwitchToTempFilter, null, false)));
			if (data.history != null)
			{
				frameKeyMap.AddItem(new KeyItem(Keys.Up, null,
					new KeyAction("F&ind|Previous pattern", DoPrevPattern, null, false)));
				frameKeyMap.AddItem(new KeyItem(Keys.Down, null,
					new KeyAction("F&ind|Next pattern", DoNextPattern, null, false)));
			}
	
			textBox = new MulticaretTextBox();
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
			settings.ApplySimpleParameters(filterTextBox, null, false);
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
		string text = textBox.Text;
		string newText = data.history.Get(text, isPrev);
		if (newText != text)
		{
			textBox.Text = newText;
			textBox.Controller.ClearMinorSelections();
			textBox.Controller.LastSelection.anchor = textBox.Controller.LastSelection.caret = newText.Length;
			return true;
		}
		return false;
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
			return true;
		}
		return false;
	}
}
