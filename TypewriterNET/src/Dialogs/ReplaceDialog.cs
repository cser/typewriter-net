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

public class ReplaceDialog : ADialog
{
	private TabBar<string> tabBar;
	private SplitLine splitLine;
	private MulticaretTextBox textBox;
	private MulticaretTextBox replaceTextBox;

	public ReplaceDialog(string name, KeyMap keyMap, KeyMap doNothingKeyMap)
	{
		Name = name;

		tabBar = new TabBar<string>(new SwitchList<string>(), TabBar<string>.DefaultStringOf);
		tabBar.Text = name;
		Controls.Add(tabBar);

		splitLine = new SplitLine();
		Controls.Add(splitLine);

		KeyMap frameKeyMap = new KeyMap();
		frameKeyMap.AddItem(new KeyItem(Keys.Escape, null, new KeyAction("&View\\Cancel find", DoCancel, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Tab, null, new KeyAction("&View\\Next field", DoNextField, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Enter, null, new KeyAction("&View\\Find next", DoFindNext, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.H, null, new KeyAction("&View\\Replace", DoReplace, null, false)));

		textBox = new MulticaretTextBox();
		textBox.ShowLineNumbers = false;
		textBox.HighlightCurrentLine = false;
		textBox.KeyMap.AddAfter(keyMap);
		textBox.KeyMap.AddAfter(frameKeyMap, 1);
		textBox.KeyMap.AddAfter(doNothingKeyMap, -1);
		textBox.FocusedChange += OnTextBoxFocusedChange;
		Controls.Add(textBox);

		replaceTextBox = new MulticaretTextBox();
		replaceTextBox.ShowLineNumbers = false;
		replaceTextBox.HighlightCurrentLine = false;
		replaceTextBox.KeyMap.AddAfter(frameKeyMap, 1);
		replaceTextBox.FocusedChange += OnTextBoxFocusedChange;
		Controls.Add(replaceTextBox);

		tabBar.MouseDown += OnTabBarMouseDown;
		InitResizing(tabBar, splitLine);
		Height = MinSize.Height;
	}

	override public void Focus()
	{
		textBox.Focus();
	}

	private bool DoCancel(Controller controller)
	{
		DispatchNeedClose();
		return true;
	}

	private bool DoNextField(Controller controller)
	{
		if (controller == textBox.Controller)
			replaceTextBox.Focus();
		else
			textBox.Focus();
		return true;
	}

	private bool DoFindNext(Controller controller)
	{
		if (Nest.MainForm.LastFrame != null)
		{
			Controller lastController = Nest.MainForm.LastFrame.Controller;
			string text = textBox.Text;
			int index = lastController.Lines.IndexOf(text, lastController.Lines.LastSelection.Right);
			if (index == -1)
				index = lastController.Lines.IndexOf(text, 0);
			if (index != -1)
			{
				lastController.PutCursor(lastController.Lines.PlaceOf(index), false);
				lastController.PutCursor(lastController.Lines.PlaceOf(index + text.Length), true);
				Nest.MainForm.LastFrame.TextBox.MoveToCaret();
			}
		}
		return true;
	}

	private bool DoReplace(Controller controller)
	{
		if (Nest.MainForm.LastFrame != null)
		{
			Controller lastController = Nest.MainForm.LastFrame.Controller;
			if (!lastController.Lines.AllSelectionsEmpty)
				lastController.InsertText(replaceTextBox.Text);
			DoFindNext(controller);
		}
		return true;
	}

	override public Size MinSize { get { return new Size(tabBar.Height * 3, tabBar.Height * 3 + 2); } }

	private void OnTabBarMouseDown(object sender, EventArgs e)
	{
		textBox.Focus();
	}

	private void OnTextBoxFocusedChange()
	{
		tabBar.Selected = textBox.Focused;
		if (textBox.Focused)
			Nest.MainForm.SetFocus(textBox, textBox.KeyMap, null);
		if (replaceTextBox.Focused)
			Nest.MainForm.SetFocus(replaceTextBox, textBox.KeyMap, null);
		if (!textBox.Focused && !replaceTextBox.Focused)
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
		textBox.Size = new Size(Width - 10, (Height - tabBarHeight) / 2);
		replaceTextBox.Location = new Point(0, tabBarHeight + (Height - tabBarHeight) / 2 + 2);
		replaceTextBox.Size = new Size(Width - 10, (Height - tabBarHeight) / 2);
	}
}
