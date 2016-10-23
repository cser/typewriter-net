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

public class RenameDialog : ADialog
{
	private Getter<string, bool> doInput;
	private TabBar<string> tabBar;
	private SplitLine splitLine;
	private MulticaretTextBox textBox;
	private string text;

	public RenameDialog(Getter<string, bool> doInput, string name, string text)
	{
		this.doInput = doInput;
		Name = name;
		this.text = text;
	}

	override protected void DoCreate()
	{
		tabBar = new TabBar<string>(new SwitchList<string>(), TabBar<string>.DefaultStringOf);
		tabBar.Text = Name;
		tabBar.CloseClick += OnCloseClick;
		Controls.Add(tabBar);

		splitLine = new SplitLine();
		Controls.Add(splitLine);

		KeyMap frameKeyMap = new KeyMap();
		KeyItem escape = new KeyItem(Keys.Escape, null,
			new KeyAction("&View\\File tree\\Cancel renaming", DoCancel, null, false));
		frameKeyMap.AddItem(escape);
		frameKeyMap.AddItem(new KeyItem(Keys.Enter, null,
			new KeyAction("&View\\File tree\\Complete renaming", DoComplete, null, false)));
		
		KeyMap beforeKeyMap = new KeyMap();
		beforeKeyMap.AddItem(escape);

		textBox = new MulticaretTextBox();
		textBox.KeyMap.AddBefore(beforeKeyMap);
		textBox.KeyMap.AddAfter(KeyMap);
		textBox.KeyMap.AddAfter(frameKeyMap, 1);
		textBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
		textBox.FocusedChange += OnTextBoxFocusedChange;
		Controls.Add(textBox);

		tabBar.MouseDown += OnTabBarMouseDown;
		InitResizing(tabBar, splitLine);
	}

	override public bool Focused { get { return textBox.Focused; } }

	private void OnCloseClick()
	{
		DispatchNeedClose();
	}

	override protected void DoDestroy()
	{
	}

	override public Size MinSize { get { return new Size(tabBar.Height * 3, tabBar.Height * 2); } }

	override public void Focus()
	{
		textBox.Focus();
		
		textBox.Text = text;
		textBox.Controller.ClearMinorSelections();
		for (int i = 0; i < textBox.Controller.Lines.LinesCount; i++)
		{
			Line line = textBox.Controller.Lines[i];
			if (i == 0)
			{
				textBox.Controller.PutCursor(new Place(0, i), false);
			}
			else
			{
				textBox.Controller.PutNewCursor(new Place(0, i));
			}
			textBox.Controller.PutCursor(new Place(line.chars.Count - line.GetRN().Length, i), true);
		}
		textBox.Invalidate();
		Nest.size = tabBar.Height + textBox.CharHeight * (
			!string.IsNullOrEmpty(text) && textBox.Controller != null ? textBox.GetScrollSizeY() : 1
		) + 4;
		SetNeedResize();
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
		splitLine.Location = new Point(Width - 10, tabBarHeight);
		splitLine.Size = new Size(10, Height - tabBarHeight);
		textBox.Location = new Point(0, tabBarHeight);
		textBox.Size = new Size(Width - 10, Height - tabBarHeight + 1);
	}

	override protected void DoUpdateSettings(Settings settings, UpdatePhase phase)
	{
		if (phase == UpdatePhase.Raw)
		{
			settings.ApplySimpleParameters(textBox, null);
			tabBar.SetFont(settings.font.Value, settings.fontSize.Value);
		}
		else if (phase == UpdatePhase.Parsed)
		{
			textBox.Scheme = settings.ParsedScheme;
			tabBar.Scheme = settings.ParsedScheme;
			splitLine.Scheme = settings.ParsedScheme;
		}
	}

	private bool DoCancel(Controller controller)
	{
		DispatchNeedClose();
		return true;
	}

	private bool DoComplete(Controller controller)
	{
		return doInput(textBox.Text);
	}
}
