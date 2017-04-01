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

public class FindDialog : ADialog
{
	public class Data
	{
		public string oldText = "";
		public readonly StringList history;

		public Data(StringList history)
		{
			this.history = history;
		}
	}

	private Data data;
	private FindParams findParams;
	private Getter<string, bool> doFind;
	private Getter<string, bool> doSelectAllFound;
	private Getter<string, bool> doSelectNextFound;
	private Getter<string> getFilterText;
	private TabBar<NamedAction> tabBar;
	private MulticaretTextBox textBox;

	public FindDialog(
		Data data,
		FindParams findParams,
		Getter<string, bool> doFind,
		Getter<string, bool> doSelectAllFound,
		Getter<string, bool> doSelectNextFound,
		string name,
		Getter<string> getFilterText)
	{
		this.data = data;
		this.findParams = findParams;
		this.doFind = doFind;
		this.doSelectAllFound = doSelectAllFound;
		this.doSelectNextFound = doSelectNextFound;
		this.getFilterText = getFilterText;
		Name = name;
	}

	override protected void DoCreate()
	{
		KeyMap frameKeyMap = new KeyMap();
		frameKeyMap.AddItem(new KeyItem(Keys.Escape, null,
			new KeyAction("F&ind\\Cancel find", DoCancel, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Enter, null,
			new KeyAction("F&ind\\Find next", DoFindNext, null, false)));
		if (data.history != null)
		{
			frameKeyMap.AddItem(new KeyItem(Keys.Up, null,
				new KeyAction("F&ind\\Previous pattern", DoPrevPattern, null, false)));
			frameKeyMap.AddItem(new KeyItem(Keys.Down, null,
				new KeyAction("F&ind\\Next pattern", DoNextPattern, null, false)));
		}
		frameKeyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("F&ind\\-", null, null, false)));
		
		SwitchList<NamedAction> list = new SwitchList<NamedAction>();
		list.Add(new NamedAction("FIND NEXT", DoFindNext));

		KeyMap beforeKeyMap = new KeyMap();
		if (doSelectAllFound != null)
		{
			beforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.D, null,
				new KeyAction("F&ind\\Select next found", DoSelectNextFound, null, false)));
			beforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.D, null,
				new KeyAction("F&ind\\Select all found", DoSelectAllFound, null, false)));
			
			list.Add(new NamedAction("SELECT NEXT FOUND", DoSelectNextFound));
			list.Add(new NamedAction("SELECT ALL FOUND", DoSelectAllFound));
		}

		textBox = new MulticaretTextBox();
		textBox.KeyMap.AddBefore(beforeKeyMap);
		textBox.KeyMap.AddAfter(KeyMap);
		textBox.KeyMap.AddAfter(frameKeyMap, 1);
		textBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
		textBox.FocusedChange += OnTextBoxFocusedChange;
		Controls.Add(textBox);

		tabBar = new TabBar<NamedAction>(list, TabBar<NamedAction>.DefaultStringOf);
		tabBar.ButtonMode = true;
		tabBar.TabClick += OnTabClick;
		tabBar.CloseClick += OnCloseClick;
		tabBar.MouseDown += OnTabBarMouseDown;
		Controls.Add(tabBar);
		
		InitResizing(tabBar, null);
		Height = MinSize.Height;
		UpdateFindParams();
	}

	private void UpdateFindParams()
	{
		tabBar.Text2 = findParams != null ? findParams.GetIndicationText() : "";
	}

	override public bool Focused { get { return textBox.Focused; } }

	private void OnCloseClick()
	{
		DispatchNeedClose();
	}
	
	private void OnTabClick(NamedAction action)
	{
		action.Execute(textBox.Controller);
	}

	override protected void DoDestroy()
	{
		data.oldText = textBox.Text;
	}

	override public Size MinSize { get { return new Size(tabBar.Height * 3, tabBar.Height + textBox.CharHeight); } }

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

	private void OnTextBoxFocusedChange()
	{
		if (Destroyed)
			return;
		tabBar.Selected = textBox.Focused;
		if (textBox.Focused)
			Nest.MainForm.SetFocus(textBox, textBox.KeyMap, null);
	}

	override protected void OnResize(EventArgs e)
	{
		base.OnResize(e);
		int tabBarHeight = tabBar.Height;
		tabBar.Size = new Size(Width, tabBarHeight);
		textBox.Location = new Point(0, tabBarHeight);
		textBox.Size = new Size(Width, Height - tabBarHeight + 1);
	}

	override protected void DoUpdateSettings(Settings settings, UpdatePhase phase)
	{
		if (phase == UpdatePhase.Raw)
		{
			settings.ApplySimpleParameters(textBox, null);
		}
		else if (phase == UpdatePhase.Parsed)
		{
			textBox.Scheme = settings.ParsedScheme;
			tabBar.Scheme = settings.ParsedScheme;
			tabBar.Text = Name + (getFilterText != null ? " - " + getFilterText() : "");
		}
		else if (phase == UpdatePhase.FindParams)
		{
			UpdateFindParams();
		}
	}

	private bool DoCancel(Controller controller)
	{
		DispatchNeedClose();
		return true;
	}

	private bool DoFindNext(Controller controller)
	{
		string text = textBox.Text;
		if (data.history != null)
			data.history.Add(text);
		return doFind(text);
	}
	
	private bool DoPrevPattern(Controller controller)
	{
		return GetHistoryPattern(true);
	}

	private bool DoNextPattern(Controller controller)
	{
		return GetHistoryPattern(false);
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
	
	private bool DoSelectAllFound(Controller controller)
	{
		string text = textBox.Text;
		if (data.history != null)
			data.history.Add(text);
		if (doSelectAllFound(text))
			DispatchNeedClose();
		return true;
	}
	
	private bool DoSelectNextFound(Controller controller)
	{
		string text = textBox.Text;
		if (data.history != null)
			data.history.Add(text);
		doSelectNextFound(text);
		return true;
	}
}
