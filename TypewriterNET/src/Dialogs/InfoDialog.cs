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

public class InfoDialog : ADialog
{
	private SwitchList<string> list;
	private TabBar<string> tabBar;
	private MulticaretTextBox textBox;

	public InfoDialog()
	{
	}

	override protected void DoCreate()
	{
		list = new SwitchList<string>();
		tabBar = new TabBar<string>(list, TabBar<string>.DefaultStringOf);
		tabBar.Text = "Info";
		tabBar.CloseClick += OnCloseClick;
		Controls.Add(tabBar);

		KeyMap frameKeyMap = new KeyMap();
		frameKeyMap.AddItem(new KeyItem(Keys.Escape, null, new KeyAction("&View\\Close info", DoClose, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Enter, null, new KeyAction("&View\\Close info", DoClose, null, false)));

		textBox = new MulticaretTextBox();
		textBox.KeyMap.AddAfter(KeyMap);
		textBox.KeyMap.AddAfter(frameKeyMap, 1);
		textBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
		textBox.FocusedChange += OnTextBoxFocusedChange;
		textBox.Controller.isReadonly = true;
		SetTextBoxParameters();
		Controls.Add(textBox);

		tabBar.MouseDown += OnTabBarMouseDown;
		InitResizing(tabBar, null);
		Height = MinSize.Height;
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

	override public bool Focused { get { return textBox.Focused; } }

	override protected void OnResize(EventArgs e)
	{
		base.OnResize(e);
		int tabBarHeight = tabBar.Height;
		tabBar.Size = new Size(Width, tabBarHeight);
		textBox.Location = new Point(0, tabBarHeight);
		textBox.Size = new Size(Width, Height - tabBarHeight);
	}

	override protected void DoUpdateSettings(Settings settings, UpdatePhase phase)
	{
		if (phase == UpdatePhase.Raw)
		{
			settings.ApplySimpleParameters(textBox, null);
			SetTextBoxParameters();
			tabBar.SetFont(settings.font.Value, settings.fontSize.Value);
		}
		else if (phase == UpdatePhase.Parsed)
		{
			textBox.Scheme = settings.ParsedScheme;
			tabBar.Scheme = settings.ParsedScheme;
		}
	}

	private void SetTextBoxParameters()
	{
		textBox.ShowLineNumbers = false;
		textBox.HighlightCurrentLine = false;
		textBox.WordWrap = true;
	}

	private bool DoClose(Controller controller)
	{
		DispatchNeedClose();
		return true;
	}
	
	private string text;
	public string SettedText { get { return text; } }

	public void InitText(string text)
	{
	    text = text ?? "";
		this.text = text;
		textBox.Controller.InitText(text);
		Nest.size = tabBar.Height + textBox.CharHeight * (textBox.Controller != null ? textBox.GetScrollSizeY() : 1);
		SetNeedResize();
	}
}
