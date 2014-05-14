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

public class InfoDialog : ADialog
{
	private TabBar<string> tabBar;
	private SplitLine splitLine;
	private MulticaretTextBox textBox;

	public InfoDialog(string name, KeyMap keyMap, KeyMap doNothingKeyMap)
	{
		tabBar = new TabBar<string>(new SwitchList<string>(), TabBar<string>.DefaultStringOf);
		Controls.Add(tabBar);

		Name = name;

		splitLine = new SplitLine();
		Controls.Add(splitLine);

		KeyMap frameKeyMap = new KeyMap();
		frameKeyMap.AddItem(new KeyItem(Keys.Escape, null, new KeyAction("&View\\Close info", DoClose, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Enter, null, new KeyAction("&View\\Close info", DoClose, null, false)));

		textBox = new MulticaretTextBox();
		textBox.ShowLineNumbers = false;
		textBox.HighlightCurrentLine = false;
		textBox.KeyMap.AddAfter(keyMap);
		textBox.KeyMap.AddAfter(frameKeyMap, 1);
		textBox.KeyMap.AddAfter(doNothingKeyMap, -1);
		textBox.FocusedChange += OnTextBoxFocusedChange;
		textBox.Controller.isReadonly = true;
		Controls.Add(textBox);

		tabBar.MouseDown += OnTabBarMouseDown;
		InitResizing(tabBar, splitLine);
		Height = MinSize.Height;
	}

	new public string Name
	{
		get { return tabBar.Text; }
		set { tabBar.Text = value; }
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
		tabBar.Selected = textBox.Focused;
		if (textBox.Focused)
			Nest.MainForm.SetFocus(textBox, textBox.KeyMap);
		if (!textBox.Focused)
			DoOnLostFocus();
	}

	private void DoOnLostFocus()
	{
		DispatchNeedClose();
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

	private bool DoClose(Controller controller)
	{
		DispatchNeedClose();
		return true;
	}

	public void InitText(string text)
	{
		textBox.Controller.InitText(text);
	}
}
