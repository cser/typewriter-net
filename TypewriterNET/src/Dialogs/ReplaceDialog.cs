using System;
using System.Text.RegularExpressions;
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
	public class Data
	{
		public string oldText = "";
		public string oldReplaceText = "";
		public readonly StringList history;
		public readonly StringList replaceHistory;

		public Data(StringList history, StringList replaceHistory)
		{
			this.history = history;
			this.replaceHistory = replaceHistory;
		}
	}

	private Data data;
	private FindParams findParams;
	private Getter<string, bool> doFindText;
	private Getter<string, bool> doSelectAllFinded;
	private Getter<string, bool> doSelectNextFinded;
	private TabBar<string> tabBar;
	private SplitLine splitLine;
	private MulticaretTextBox textBox;
	private MulticaretTextBox replaceTextBox;
	private MonospaceLabel textLabel;
	private MonospaceLabel replaceTextLabel;

	public ReplaceDialog(Data data, FindParams findParams,
		Getter<string, bool> doFindText,
		Getter<string, bool> doSelectAllFinded,
		Getter<string, bool> doSelectNextFinded,
		string name)
	{
		this.data = data;
		this.findParams = findParams;
		this.doFindText = doFindText;
		this.doSelectAllFinded = doSelectAllFinded;
		this.doSelectNextFinded = doSelectNextFinded;
		Name = name;
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
		frameKeyMap.AddItem(new KeyItem(Keys.Escape, null, new KeyAction("F&ind\\Cancel find", DoCancel, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Tab, null, new KeyAction("F&ind\\Next field", DoNextField, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Enter, null, new KeyAction("F&ind\\Find next", DoFind, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.H, null, new KeyAction("F&ind\\Replace", DoReplace, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.Alt | Keys.Enter, null, new KeyAction("F&ind\\Replace all", DoReplaceAll, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Up, null, new KeyAction("F&ind\\Previous pattern", DoPrevPattern, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Down, null, new KeyAction("F&ind\\Next pattern", DoNextPattern, null, false)));
		
		KeyMap beforeKeyMap = new KeyMap();
		if (doSelectAllFinded != null)
		{
			beforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.D, null,
				new KeyAction("F&ind\\Select all finded", DoSelectAllFinded, null, false)));
			beforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.D, null,
				new KeyAction("F&ind\\Select all finded", DoSelectNextFinded, null, false)));
		}

		textBox = new MulticaretTextBox();
		textBox.ShowLineNumbers = false;
		textBox.HighlightCurrentLine = false;
		textBox.KeyMap.AddAfter(KeyMap);
		textBox.KeyMap.AddBefore(beforeKeyMap);
		textBox.KeyMap.AddAfter(frameKeyMap, 1);
		textBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
		textBox.FocusedChange += OnTextBoxFocusedChange;
		Controls.Add(textBox);

		replaceTextBox = new MulticaretTextBox();
		replaceTextBox.ShowLineNumbers = false;
		replaceTextBox.HighlightCurrentLine = false;
		replaceTextBox.KeyMap.AddAfter(frameKeyMap, 1);
		replaceTextBox.FocusedChange += OnTextBoxFocusedChange;
		Controls.Add(replaceTextBox);

		textLabel = new MonospaceLabel();
		textLabel.Text = "Text";
		Controls.Add(textLabel);

		replaceTextLabel = new MonospaceLabel();
		replaceTextLabel.Text = "Replace";
		Controls.Add(replaceTextLabel);

		tabBar.MouseDown += OnTabBarMouseDown;
		InitResizing(tabBar, splitLine);
		Height = MinSize.Height;
		UpdateFindParams();
	}

	private void UpdateFindParams()
	{
		tabBar.Text2 = findParams.GetIndicationTextWithEscape();
	}

	private void OnCloseClick()
	{
		DispatchNeedClose();
	}

	override protected void DoDestroy()
	{
		data.oldText = textBox.Text;
		data.oldReplaceText = replaceTextBox.Text;
	}

	override public void Focus()
	{
		textBox.Focus();

		Controller lastController = Nest.MainForm.LastFrame != null ? Nest.MainForm.LastFrame.Controller : null;

		textBox.Text = lastController == null || lastController.Lines.LastSelection.Empty ?
			data.oldText :
			lastController.Lines.GetText(lastController.Lines.LastSelection.Left, lastController.Lines.LastSelection.Count);
		textBox.Controller.SelectAllToEnd();

		replaceTextBox.Text = data.oldReplaceText;
		replaceTextBox.Controller.SelectAllToEnd();
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

	private bool DoFind(Controller controller)
	{
		string text = textBox.Text;
		doFindText(text);
		data.history.Add(text);
		return true;
	}

	private string GetReplaceText()
	{
		if (findParams.escape)
			return Regex.Unescape(replaceTextBox.Text);
		return replaceTextBox.Text;
	}

	private bool DoReplace(Controller controller)
	{
		if (Nest.MainForm.LastFrame != null)
		{
			Controller lastController = Nest.MainForm.LastFrame.Controller;
			if (!lastController.Lines.AllSelectionsEmpty)
				lastController.InsertText(GetReplaceText());
			string text = textBox.Text;
			doFindText(text);
			data.history.Add(text);
		}
		return true;
	}

	private bool DoReplaceAll(Controller controller)
	{
		if (Nest.MainForm.LastFrame != null)
		{
			Controller lastController = Nest.MainForm.LastFrame.Controller;
			lastController.ClearMinorSelections();
			lastController.LastSelection.anchor = lastController.LastSelection.caret = 0;
			string text = textBox.Text;
			while (true)
			{
				doFindText(text);
				if (!lastController.Lines.AllSelectionsEmpty)
					lastController.InsertText(GetReplaceText());
				else
					break;
			}
			data.history.Add(text);
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
		if (Destroyed)
			return;
		tabBar.Selected = textBox.Focused || replaceTextBox.Focused;
		if (textBox.Focused)
			Nest.MainForm.SetFocus(textBox, textBox.KeyMap, null);
		if (replaceTextBox.Focused)
			Nest.MainForm.SetFocus(replaceTextBox, textBox.KeyMap, null);
	}

	override public bool Focused { get { return textBox.Focused || replaceTextBox.Focused; } }

	override protected void OnResize(EventArgs e)
	{
		base.OnResize(e);
		int tabBarHeight = tabBar.Height;
		tabBar.Size = new Size(Width, tabBarHeight);
		splitLine.Location = new Point(Width - 10, tabBarHeight);
		splitLine.Size = new Size(10, Height - tabBarHeight);
		textLabel.Location = new Point(0, tabBarHeight);
		replaceTextLabel.Location = new Point(0, tabBarHeight + (Height - tabBarHeight) / 2 + 2);

		int left = Math.Max(textLabel.Width, replaceTextLabel.Width) + 10;
		textBox.Location = new Point(left, tabBarHeight);
		textBox.Size = new Size(Width - left - 10, (Height - tabBarHeight) / 2);
		replaceTextBox.Location = new Point(left, tabBarHeight + (Height - tabBarHeight) / 2 + 2);
		replaceTextBox.Size = new Size(Width - left - 10, (Height - tabBarHeight) / 2);
	}

	override protected void DoUpdateSettings(Settings settings, UpdatePhase phase)
	{
		if (phase == UpdatePhase.Raw)
		{
			settings.ApplySimpleParameters(textBox);
			settings.ApplyToLabel(textLabel);
			settings.ApplyToLabel(replaceTextLabel);
			tabBar.SetFont(settings.font.Value, settings.fontSize.Value);
		}
		else if (phase == UpdatePhase.Parsed)
		{
			BackColor = settings.ParsedScheme.tabsBgColor;
			textBox.Scheme = settings.ParsedScheme;
			replaceTextBox.Scheme = settings.ParsedScheme;
			tabBar.Scheme = settings.ParsedScheme;
			settings.ApplySchemeToLabel(textLabel);
			settings.ApplySchemeToLabel(replaceTextLabel);
		}
		else if (phase == UpdatePhase.FindParams)
		{
			UpdateFindParams();
		}
	}

	private bool DoPrevPattern(Controller controller)
	{
		return GetHistoryPattern(controller, true);
	}

	private bool DoNextPattern(Controller controller)
	{
		return GetHistoryPattern(controller, false);
	}

	private bool GetHistoryPattern(Controller controller, bool isPrev)
	{
		string text;
		string newText;
		MulticaretTextBox currentTextBox;
		if (controller == textBox.Controller)
		{
			currentTextBox = textBox;
			text = currentTextBox.Text;
			newText = data.history.Get(text, isPrev);
		}
		else
		{
			currentTextBox = replaceTextBox;
			text = currentTextBox.Text;
			newText = data.replaceHistory.Get(text, isPrev);
		}
		if (newText != text)
		{
			currentTextBox.Text = newText;
			currentTextBox.Controller.ClearMinorSelections();
			currentTextBox.Controller.LastSelection.anchor = currentTextBox.Controller.LastSelection.caret = newText.Length;
			return true;
		}
		return false;
	}
	
	private bool DoSelectAllFinded(Controller controller)
	{
		string text = textBox.Text;
		if (data.history != null)
			data.history.Add(text);
		if (doSelectAllFinded(text))
			DispatchNeedClose();
		return true;
	}
	
	private bool DoSelectNextFinded(Controller controller)
	{
		string text = textBox.Text;
		if (data.history != null)
			data.history.Add(text);
		doSelectAllFinded(text);
		return true;
	}
}
