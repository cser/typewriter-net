using System;
using System.IO;
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
using System.Text.RegularExpressions;

public class MoveDialog : ADialog
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

	private TabBar<string> tabBar;
	private MulticaretTextBox textBox;
	private Data data;
	private string text;
	private Getter<string, bool> onInput;
	private bool startViMode;

	public MoveDialog(Data data, Getter<string, bool> onInput, string name, string text)
	{
		this.data = data;
		this.onInput = onInput;
		Name = name;
		this.text = text;
	}

	override protected void DoCreate()
	{
		tabBar = new TabBar<string>(null, TabBar<string>.DefaultStringOf);
		tabBar.CloseClick += OnCloseClick;
		tabBar.Text = Name;
		Controls.Add(tabBar);

		KeyMap frameKeyMap = new KeyMap();
		frameKeyMap.AddItem(new KeyItem(Keys.Escape, null,
			new KeyAction("&View\\File tree\\Cancel path", DoCancel, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Enter, null,
			new KeyAction("&View\\File tree\\Run path", DoInput, null, false)));
		if (data.history != null)
		{
			frameKeyMap.AddItem(new KeyItem(Keys.Up, null,
				new KeyAction("&View\\File tree\\Previous path", DoPrevPath, null, false)));
			frameKeyMap.AddItem(new KeyItem(Keys.Down, null,
				new KeyAction("&View\\File tree\\Next path", DoNextPath, null, false)));
		}
		{
		    KeyAction action = new KeyAction("&View\\File tree\\Autocomplete", DoAutocomplete, null, false);
            frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.Space, null, action));
            frameKeyMap.AddItem(new KeyItem(Keys.Tab, null, action));
		}

		textBox = new MulticaretTextBox();
		textBox.KeyMap.AddAfter(KeyMap);
		textBox.KeyMap.AddAfter(frameKeyMap, 1);
		textBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
		textBox.FocusedChange += OnTextBoxFocusedChange;
		Controls.Add(textBox);

		tabBar.MouseDown += OnTabBarMouseDown;
		InitResizing(tabBar, null);
		Height = MinSize.Height;
	}

	override public bool Focused { get { return textBox.Focused; } }

	private void OnCloseClick()
	{
		if (startViMode)
		{
			textBox.SetViMode(true);
		}
		DispatchNeedClose();
	}

	override protected void DoDestroy()
	{
		data.oldText = textBox.Text;
	}

	override public Size MinSize { get { return new Size(tabBar.Height * 3, tabBar.Height + textBox.CharHeight); } }

	override public void Focus()
	{
		textBox.Focus();
		
		startViMode = MulticaretTextBox.initMacrosExecutor != null &&
			MulticaretTextBox.initMacrosExecutor.viMode != ViMode.Insert;
		Frame lastFrame = Nest.MainForm.LastFrame;
		Controller lastController = lastFrame != null ? lastFrame.Controller : null;
		if (lastController != null)
		{
			if (text != null)
			{
				textBox.Text = text;
				textBox.Controller.DocumentEnd(false);
			}
			else
			{
				textBox.Text = data.oldText;
				textBox.Controller.SelectAllToEnd();
			}
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
		}
	}

	private bool DoCancel(Controller controller)
	{
		if (startViMode)
		{
			textBox.SetViMode(true);
		}
		DispatchNeedClose();
		return true;
	}

	private bool DoInput(Controller controller)
	{
		string text = textBox.Text;
		if (data.history != null)
			data.history.Add(text);
		if (onInput(textBox.Controller.Lines.GetText()))
		{
			if (startViMode)
			{
				textBox.SetViMode(true);
			}
			DispatchNeedClose();
		}
		return true;
	}

	private bool DoPrevPath(Controller controller)
	{
		return GetHistoryPath(true);
	}

	private bool DoNextPath(Controller controller)
	{
		return GetHistoryPath(false);
	}

	private bool GetHistoryPath(bool isPrev)
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
	
	private bool DoAutocomplete(Controller controller)
	{
		string text = textBox.Controller.Lines[0].Text;
		Place place = textBox.Controller.Lines.PlaceOf(textBox.Controller.LastSelection.caret);
		if (place.iChar < text.Length)
		{
			text = text.Substring(0, place.iChar);
		}
		int quotesCount = 0;
		int quotesIndex = 0;
		while (true)
		{
			quotesIndex = text.IndexOf('"', quotesIndex);
			if (quotesIndex == -1)
				break;
			quotesIndex++;
			if (quotesIndex >= text.Length)
				break;
			quotesCount++;
		}
		string path = "";
		int index = text.Length;
		while (true)
		{
			if (index <= 0)
			{
				path = text;
				break;
			}
			index--;
			if (quotesCount % 2 == 0 && (text[index] == ' ' || text[index] == '\t' || text[index] == '"') ||
				quotesCount % 2 == 1 && text[index] == '"')
			{
				path = text.Substring(index + 1);
				break;
			}
		}
		AutocompletePath(path);
		return true;
	}
	
	private void AutocompleteProperty(string text)
	{
		AutocompleteMode autocomplete = new AutocompleteMode(textBox, AutocompleteMode.Mode.Raw);
		List<Variant> variants = new List<Variant>();
		if (MainForm.Settings != null)
		{
			foreach (Properties.Property property in MainForm.Settings.GetProperties())
			{
				variants.Add(GetPropertyVariant(property));
			}
			autocomplete.Show(variants, text);
		}
	}
	
	private Variant GetPropertyVariant(Properties.Property property)
	{
		Variant variant = new Variant();
		variant.CompletionText = property.name;
		variant.DisplayText = property.name + " <new value>";
		return variant;
	}
	
	private void AutocompletePath(string path)
	{
		if (path == null)
			return;
		path = path.Replace("/", "\\").Replace("\\\\", "\\");
		string dir = ".";
		string name = path;
		int index = path.LastIndexOf("\\");
		if (index != -1)
		{
			dir = path.Substring(0, index + 1);
			name = path.Substring(index + 1);
		}
		string[] dirs = null;
		try
		{
			dirs = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
		}
		catch
		{
		}
		if (dirs == null || dirs.Length == 0)
			return;
		AutocompleteMode autocomplete = new AutocompleteMode(textBox, AutocompleteMode.Mode.Raw);
		List<Variant> variants = new List<Variant>();
		foreach (string file in dirs)
		{
			string fileName = Path.GetFileName(file);
			Variant variant = new Variant();
			variant.CompletionText = fileName + "\\";
			variant.DisplayText = fileName + "\\";
			variants.Add(variant);
		}
		autocomplete.Show(variants, name);
	}
}
