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
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;
using MulticaretEditor;
using System.Text.RegularExpressions;

public class CommandDialog : ADialog
{
	public class Data
	{
		public string oldText = "";
	}

	private TabBar<string> tabBar;
	private MulticaretTextBox textBox;
	private Data data;
	private string text;

	public CommandDialog(Data data, string name, string text)
	{
		this.data = data;
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
			new KeyAction("&View|Cancel command", DoCancel, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Enter, null,
			new KeyAction("&View|Run command", DoRunCommand, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Up, null,
			new KeyAction("&View|Previous command", DoPrevCommand, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Down, null,
			new KeyAction("&View|Next command", DoNextCommand, null, false)));
		{
			KeyAction action = new KeyAction("&View|Autocomplete", DoAutocomplete, null, false);
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
		DispatchNeedClose();
		return true;
	}

	private bool DoRunCommand(Controller controller)
	{
		Commander commander = MainForm.commander;
		DispatchNeedClose();
		commander.Execute(textBox.Text, false, false);
		return true;
	}

	private bool DoPrevCommand(Controller controller)
	{
		return GetHistoryCommand(true);
	}

	private bool DoNextCommand(Controller controller)
	{
		return GetHistoryCommand(false);
	}

	private bool GetHistoryCommand(bool isPrev)
	{
		string text = textBox.Text;
		string newText = MainForm.commander.History.Get(text, isPrev);
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
		if (!text.StartsWith("!"))
		{
			if (text.IndexOf(' ') == -1 && text.IndexOf('\t') == -1)
			{
				AutocompleteCommand(text);
				return true;
			}
			int index0 = text.IndexOf(' ');
			int index1 = text.IndexOf('\t');
			if (index0 != -1 || index1 != -1)
			{
				int spaceIndex;
				if (index0 == -1)
					spaceIndex = index1;
				else if (index1 == -1)
					spaceIndex = index0;
				else
					spaceIndex = Math.Min(index0, index1);
				if (text.IndexOf(' ', spaceIndex + 1) == -1 && text.IndexOf('\t', spaceIndex + 1) == -1)
				{
					string command = text.Substring(0, spaceIndex);
					string word = text.Substring(spaceIndex + 1);
					if (command == "reset")
					{
						AutocompleteProperty(word);
						return true;
					}
					if (MainForm.Settings != null)
					{
						Properties.Property property = MainForm.Settings[command];
						if (property != null)
						{
							List<Variant> variants = property.GetAutocompleteVariants();
							if (variants != null)
							{
								AutocompleteMode autocomplete = new AutocompleteMode(textBox, true);
								autocomplete.Show(variants, word);
								return true;
							}
						}
					}
				}
			}
		}
		else
		{
			if ((text.StartsWith("!{") || text.StartsWith("!^{")) && !text.Contains("}"))
			{
				int prefixIndex;
				prefixIndex = text.LastIndexOf("s:");
				if (prefixIndex != -1 && text.IndexOf(";", prefixIndex) == -1)
				{
					List<Variant> variants = new List<Variant>();
					foreach (SyntaxFilesScanner.LanguageInfo info in MainForm.SyntaxFilesScanner.Infos)
					{
						Variant variant = new Variant();
						variant.CompletionText = info.syntax;
						variant.DisplayText = info.syntax;
						variants.Add(variant);
					}
					if (variants.Count > 0)
					{
						AutocompleteMode autocomplete = new AutocompleteMode(textBox, true);
						autocomplete.Show(variants, text.Substring(prefixIndex + 2));
						return true;
					}
				}
				prefixIndex = text.LastIndexOf("e:");
				if (prefixIndex != -1 && text.IndexOf(";", prefixIndex) == -1)
				{
					Properties.Property property = new Properties.EncodingProperty("", new EncodingPair(Encoding.UTF8, false));
					List<Variant> variants = property.GetAutocompleteVariants();
					if (variants != null)
					{
						AutocompleteMode autocomplete = new AutocompleteMode(textBox, true);
						autocomplete.Show(variants, text.Substring(prefixIndex + 2));
						return true;
					}
				}
			}
		}
		if (text.StartsWith("!!!!"))
			text = text.Substring(4);
		else if (text.StartsWith("!!!"))
			text = text.Substring(3);
		else if (text.StartsWith("!!"))
			text = text.Substring(2);
		else if (text.StartsWith("!^"))
			text = text.Substring(2);
		else if (text.StartsWith("!"))
			text = text.Substring(1);
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
	
	private void AutocompleteCommand(string text)
	{
		AutocompleteMode autocomplete = new AutocompleteMode(textBox, true);
		List<Variant> variants = new List<Variant>();
		foreach (Commander.Command command in MainForm.commander.Commands)
		{
			Variant variant = new Variant();
			variant.CompletionText = command.name;
			variant.DisplayText = command.name + (!string.IsNullOrEmpty(command.argNames) ? " <" + command.argNames + ">" : "");
			variants.Add(variant);
		}
		if (MainForm.Settings != null)
		{
			foreach (Properties.Property property in MainForm.Settings.GetProperties())
			{
				variants.Add(GetPropertyVariant(property));
			}
		}
		autocomplete.Show(variants, text);
	}
	
	private void AutocompleteProperty(string text)
	{
		AutocompleteMode autocomplete = new AutocompleteMode(textBox, true);
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
	
	private string GetFile()
	{
		Buffer lastBuffer = MainForm.LastBuffer;
		if (lastBuffer == null || string.IsNullOrEmpty(lastBuffer.FullPath))
		{
			if (MainForm.LeftNest.AFrame != null && MainForm.LeftNest.buffers.list.Selected == MainForm.FileTree.Buffer)
			{
				return MainForm.FileTree.GetCurrentFile();
			}
			return null;
		}
		return lastBuffer.FullPath;
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
		if (dir.Contains(RunShellCommand.FileDirVar))
		{
			string file = GetFile();
			if (file != null)
			{
				dir = dir.Replace(RunShellCommand.FileDirVar, Path.GetDirectoryName(file));
			}
		}
		string[] dirs = null;
		string[] files = null;
		try
		{
			dirs = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
		}
		catch
		{
		}
		try
		{
			files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
		}
		catch
		{
		}
		if ((files == null || files.Length == 0) && (dirs == null || dirs.Length == 0))
			return;
		AutocompleteMode autocomplete = new AutocompleteMode(textBox, true);
		List<Variant> variants = new List<Variant>();
		if (dirs != null)
		{
			foreach (string file in dirs)
			{
				string fileName = Path.GetFileName(file);
				Variant variant = new Variant();
				variant.CompletionText = fileName + "\\";
				variant.DisplayText = fileName + "\\";
				variants.Add(variant);
			}
		}
		if (files != null)
		{
			foreach (string file in files)
			{
				string fileName = Path.GetFileName(file);
				Variant variant = new Variant();
				variant.CompletionText = fileName;
				variant.DisplayText = fileName;
				variants.Add(variant);
			}
		}
		autocomplete.Show(variants, name);
	}
}
