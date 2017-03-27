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

public class ViFindDialog : ADialog
{
	private FindDialog.Data data;
	private FindParams findParams;
	private Getter<string, bool> doFind;
	private MulticaretTextBox textBox;
	private MonospaceLabel label;

	public ViFindDialog(FindDialog.Data data, FindParams findParams, Getter<string, bool> doFind)
	{
		this.data = data;
		this.findParams = findParams;
		this.doFind = doFind;
		Name = "Find";
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
		
		label = new MonospaceLabel();
		label.Text = "/";
		Controls.Add(label);
		
		KeyMap beforeKeyMap = new KeyMap();
		textBox = new MulticaretTextBox(true);
		textBox.KeyMap.AddBefore(beforeKeyMap);
		textBox.KeyMap.AddAfter(KeyMap);
		textBox.KeyMap.AddAfter(frameKeyMap, 1);
		textBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
		textBox.FocusedChange += OnTextBoxFocusedChange;
		textBox.TextChange += OnTextChange;
		Controls.Add(textBox);
		
		Height = MinSize.Height;
	}
	
	private void OnTextChange()
	{
		Nest.size = textBox.CharHeight * (textBox.Controller != null ? textBox.GetScrollSizeY() : 1);
		textBox.Controller.NeedScrollToCaret();
		SetNeedResize();
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

	override public Size MinSize { get { return new Size(textBox.CharHeight * 3, textBox.CharHeight); } }

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

	private void OnTextBoxFocusedChange()
	{
		if (Destroyed)
			return;
		if (textBox.Focused)
			Nest.MainForm.SetFocus(textBox, textBox.KeyMap, null);
	}

	override protected void OnResize(EventArgs e)
	{
		base.OnResize(e);
		label.Location = new Point(0, 0);
		textBox.Location = new Point(textBox.CharWidth, 0);
		textBox.Size = new Size(Width - textBox.CharWidth, Height + 1);
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
			label.TextColor = settings.ParsedScheme.fgColor;
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
}
