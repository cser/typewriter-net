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

public class Frame : AFrame
{
	private static Controller _emptyController;

	private static Controller GetEmptyController()
	{
		if (_emptyController == null)
		{
			_emptyController = new Controller(new LineArray());
			_emptyController.InitText("[Empty]");
			_emptyController.isReadonly = true;
		}
		return _emptyController;
	}

	private TabBar<Buffer> tabBar;
	private SplitLine splitLine;
	private MulticaretTextBox textBox;
	private SwitchList<Buffer> list;

	public Frame(string name, KeyMap keyMap, KeyMap doNothingKeyMap)
	{
		Name = name;

		list = new SwitchList<Buffer>();
		list.SelectedChange += OnTabSelected;

		tabBar = new TabBar<Buffer>(list, Buffer.StringOf);
		tabBar.Text = name;
		tabBar.CloseClick += OnCloseClick;
		tabBar.TabDoubleClick += OnTabDoubleClick;
		Controls.Add(tabBar);

		splitLine = new SplitLine();
		Controls.Add(splitLine);

		KeyMap frameKeyMap = new KeyMap();
		frameKeyMap.AddItem(new KeyItem(Keys.Tab, Keys.Control, new KeyAction("&View\\Switch tab", DoTabDown, DoTabModeChange, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.W, null, new KeyAction("&View\\Close tab", DoCloseTab, null, false)));

		textBox = new MulticaretTextBox();
		textBox.KeyMap.AddAfter(keyMap);
		textBox.KeyMap.AddAfter(frameKeyMap);
		textBox.KeyMap.AddAfter(doNothingKeyMap, -1);
		textBox.FocusedChange += OnTextBoxFocusedChange;
		textBox.Controller = GetEmptyController();
		Controls.Add(textBox);

		InitResizing(tabBar, splitLine);
		tabBar.MouseDown += OnTabBarMouseDown;
	}

	private bool DoTabDown(Controller controller)
	{
		list.Down();
		return true;
	}
	
	private void DoTabModeChange(Controller controller, bool mode)
	{
		if (mode)
			list.ModeOn();
		else
			list.ModeOff();
	}

	private bool DoCloseTab(Controller controller)
	{
		RemoveBuffer(list.Selected);
		return true;
	}

	override public Size MinSize { get { return new Size(tabBar.Height * 3, tabBar.Height); } }
	override public Frame AsFrame { get { return this; } }

	public Buffer SelectedBuffer { get { return list.Selected; } }

	override public bool Focused { get { return textBox.Focused; } }

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
		tabBar.Selected = textBox.Focused;
		Nest.MainForm.MenuNode = textBox.KeyMap;
	}

	public string Title
	{
		get { return tabBar.Text; }
		set { tabBar.Text = value; }
	}

	override protected void OnResize(EventArgs e)
	{
		base.OnResize(e);
		int tabBarHeight = tabBar.Height;
		tabBar.Size = new Size(Width, tabBarHeight);
		splitLine.Location = new Point(Width - 10, tabBarHeight);
		splitLine.Size = new Size(10, Height - tabBarHeight);
		textBox.Location = new Point(0, tabBarHeight);
		textBox.Size = new Size(Width - 10, Height - tabBarHeight);
	}

	public void AddBuffer(Buffer buffer)
	{
		list.Add(buffer);
		buffer.Controller.history.ChangedChange += OnChangedChange;
	}

	public void RemoveBuffer(Buffer buffer)
	{
		buffer.Controller.history.ChangedChange -= OnChangedChange;
		list.Remove(buffer);
	}

	private void OnChangedChange()
	{
		tabBar.Invalidate();
	}

	private void OnTabSelected()
	{
		Buffer buffer = list.Selected;
		textBox.Controller = buffer != null ? buffer.Controller : GetEmptyController();
	}

	private void OnCloseClick()
	{
		RemoveBuffer(list.Selected);
	}

	private void OnTabDoubleClick(Buffer buffer)
	{
		RemoveBuffer(buffer);
	}
}
