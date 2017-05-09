using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;
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
	private BufferList buffers;

	override protected void DoCreate()
	{
		if (Nest.buffers == null)
			throw new Exception("buffers == null");
		this.buffers = Nest.buffers;

		buffers.frame = this;
		buffers.list.SelectedChange += OnTabSelected;

		tabBar = new TabBar<Buffer>(buffers.list, Buffer.StringOf);
		tabBar.CloseClick += OnCloseClick;
		tabBar.TabDoubleClick += OnTabDoubleClick;
		tabBar.NewTabDoubleClick += OnNewTabDoubleClick;
		Controls.Add(tabBar);

		splitLine = new SplitLine();
		Controls.Add(splitLine);

		KeyMap frameKeyMap = new KeyMap();
		frameKeyMap.AddItem(new KeyItem(Keys.Tab, Keys.Control, new KeyAction("&View\\Switch tab", DoTabDown, DoTabModeChange, false)));
		{
			KeyAction action = new KeyAction("&View\\Close tab", DoCloseTab, null, false);
			frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.W, null, action));
			frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.F4, null, action));
		}
		frameKeyMap.AddItem(new KeyItem(Keys.Tab, null,
			new KeyAction("&Edit\\Snippets\\Apply snippet", Snippets_DoApply, null, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.Tab, null,
			new KeyAction("&Edit\\Snippets\\Autocomplete snippet", Snippets_DoAutocomplete, null, false)));

		textBox = new MulticaretTextBox();
		textBox.ViShortcut += OnViShortcut;
		textBox.KeyMap.AddAfter(KeyMap);
		textBox.KeyMap.AddAfter(frameKeyMap);
		textBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
		textBox.FocusedChange += OnTextBoxFocusedChange;
		textBox.Controller = GetEmptyController();
		Controls.Add(textBox);

		InitResizing(tabBar, splitLine);
		tabBar.MouseDown += OnTabBarMouseDown;
		OnTabSelected();
	}

	override protected void DoDestroy()
	{
		tabBar.List = null;
		buffers.list.SelectedChange -= OnTabSelected;
		buffers.frame = null;
	}
	
	private void OnViShortcut(string shortcut)
	{
		MainForm.ProcessViShortcut(textBox.Controller, shortcut);
	}

	public MulticaretTextBox TextBox { get { return textBox; } }
	public Controller Controller { get { return textBox.Controller; } }

	private bool DoTabDown(Controller controller)
	{
		buffers.list.Down();
		return true;
	}

	private void DoTabModeChange(Controller controller, bool mode)
	{
		if (mode)
			buffers.list.ModeOn();
		else
			buffers.list.ModeOff();
	}
    
	private bool DoCloseTab(Controller controller)
	{
		CloseAutocomplete();
		Snippets_CloseAutocomplete();
		Snippets_CloseMode();
		RemoveBuffer(buffers.list.Selected);
		return true;
	}
	
	override public Size MinSize { get { return new Size(tabBar.Height * 3, tabBar.Height); } }
	override public Frame AsFrame { get { return this; } }

	public Buffer SelectedBuffer
	{
		get { return buffers.list.Selected; }
		set
		{
			if (value.Frame == this)
				buffers.list.Selected = value;
		}
	}

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
		if (Nest != null)
		{
			tabBar.Selected = textBox.Focused;
			Nest.MainForm.SetFocus(textBox, textBox.KeyMap, this);
		}
	}

	public string Title
	{
		get { return tabBar.Text; }
		set { tabBar.Text = value; }
	}
	
	private bool splitLineVisible = true;

	override protected void OnResize(EventArgs e)
	{
		base.OnResize(e);
		int tabBarHeight = tabBar.Height;
		tabBar.Size = new Size(Width, tabBarHeight);
		splitLine.Location = new Point(Width - 8, tabBarHeight);
		splitLine.Size = new Size(8, Height - tabBarHeight);
		textBox.Location = new Point(0, tabBarHeight);
		bool splitLineVisible = Nest.HasRight();
		if (this.splitLineVisible != splitLineVisible)
		{
			this.splitLineVisible = splitLineVisible;
			splitLine.Visible = splitLineVisible;
		}
		textBox.Size = new Size(Width - (splitLineVisible ? 8 : 0), Height - tabBarHeight);
	}

	private Settings settings;

	override protected void DoUpdateSettings(Settings settings, UpdatePhase phase)
	{
		this.settings = settings;
		Buffer buffer = buffers.list.Selected;

		if (phase == UpdatePhase.Raw)
		{
			settings.ApplyParameters(textBox, buffer != null ? buffer.settingsMode : SettingsMode.None, buffer);
		}
		else if (phase == UpdatePhase.Parsed)
		{
			textBox.Scheme = settings.ParsedScheme;
			tabBar.Scheme = settings.ParsedScheme;
			if (settings.showEncoding.Value)
				tabBar.Text2Of = Buffer.EncodeOf;
			else
				tabBar.Text2Of = null;
			splitLine.Scheme = settings.ParsedScheme;
		}
		else if (phase == UpdatePhase.HighlighterChange)
		{
			UpdateHighlighter();
		}
		else if (phase == UpdatePhase.FileSaved)
		{
			tabBar.Invalidate();
		    settings.ApplyOnlyFileParameters(textBox, buffer);
		}

		if (buffer != null && buffer.onUpdateSettings != null)
			buffer.onUpdateSettings(buffer, phase);
	}

	public bool ContainsBuffer(Buffer buffer)
	{
		return buffers.list.Contains(buffer);
	}

	public Buffer GetBuffer(string fullPath, string name)
	{
		return buffers.GetBuffer(fullPath, name);
	}

	public void AddBuffer(Buffer buffer)
	{
		if (buffer.Frame != this)
		{
			if (buffer.Frame != null)
			{
				buffer.softRemove = true;
				buffer.Frame.RemoveBuffer(buffer);
				buffer.softRemove = false;
			}
			buffer.owner = buffers;
			buffer.Controller.history.ChangedChange += OnChangedChange;
			buffers.list.Add(buffer);
			if (buffer.onAdd != null)
				buffer.onAdd(buffer);
		}
		else
		{
			buffers.list.Add(buffer);
		}
	}

	public void RemoveBuffer(Buffer buffer)
	{
		CloseAutocomplete();
		Snippets_CloseMode();
		if (buffer == null)
			return;
		if (!buffer.softRemove && buffer.onRemove != null && !buffer.onRemove(buffer))
			return;
		buffer.Controller.history.ChangedChange -= OnChangedChange;
		buffer.owner = null;
		buffers.list.Remove(buffer);
		if (buffers.list.Count == 0)
			Destroy();
	}

	private void OnChangedChange()
	{
		tabBar.Invalidate();
	}

	private KeyMap additionKeyMap;
	private KeyMap additionBeforeKeyMap;

	private void OnTabSelected()
	{
		CloseAutocomplete();
		Snippets_CloseMode();
		Buffer buffer = buffers.list.Selected;
		if (additionKeyMap != null)
			textBox.KeyMap.RemoveAfter(additionKeyMap);
		if (additionBeforeKeyMap != null)
			textBox.KeyMap.RemoveBefore(additionBeforeKeyMap);
		additionKeyMap = buffer != null ? buffer.additionKeyMap : null;
		additionBeforeKeyMap = buffer != null ? buffer.additionBeforeKeyMap : null;
		textBox.Controller = buffer != null ? buffer.Controller : GetEmptyController();
		if (additionKeyMap != null)
			textBox.KeyMap.AddAfter(additionKeyMap, 1);
		if (additionBeforeKeyMap != null)
			textBox.KeyMap.AddBefore(additionBeforeKeyMap);
		if (settings != null && buffer != null)
			settings.ApplyParameters(textBox, buffer.settingsMode, buffer);
		UpdateHighlighter();
		if (buffer != null && buffer.onSelected != null)
			buffer.onSelected(buffer);
		if (Nest != null)
		{
			Nest.MainForm.UpdateTitle();
			if (buffer != null && buffer.FullPath != null)
				Nest.MainForm.MarkShowed(buffer);
		}
	}

	public void UpdateHighlighter()
	{
		Buffer buffer = buffers.list.Selected != null ? buffers.list.Selected : null;
		Nest.MainForm.UpdateHighlighter(textBox, buffer != null ? buffer.Name : null, buffer);
	}

	private void OnCloseClick()
	{
		RemoveBuffer(buffers.list.Selected);
	}

	private void OnTabDoubleClick(Buffer buffer)
	{
		RemoveBuffer(buffer);
	}

	private void OnNewTabDoubleClick()
	{
		Nest.MainForm.ProcessTabBarDoubleClick(Nest);
	}

	private AutocompleteMode autocomplete;
	
	private void CloseAutocomplete()
	{
		if (autocomplete != null)
		{
			autocomplete.Close(false);
			autocomplete = null;
		}
	}
	
	public void ShowAutocomplete(List<Variant> variants, string leftWord)
	{
		CloseAutocomplete();
		Buffer buffer = buffers.list.Selected != null ? buffers.list.Selected : null;
		if (buffer == null)
			return;
		autocomplete = new AutocompleteMode(textBox, AutocompleteMode.Mode.Normal);
		autocomplete.Show(variants, leftWord);
	}
	
	//--------------------------------------------------------------------------
	// Snippets
	//--------------------------------------------------------------------------
	
	private SnippetMode snippetsMode;
	private AutocompleteMode snippetsAutocomplete;
	
	private void Snippets_CloseMode()
	{
		if (snippetsMode != null)
		{
			snippetsMode.Close();
			snippetsMode = null;
		}
	}
	
	private void Snippets_CloseAutocomplete()
	{
		if (snippetsAutocomplete != null)
		{
			snippetsAutocomplete.Close(false);
			snippetsAutocomplete = null;
		}
	}
	
	private bool Snippets_DoApply(Controller controller)
	{
		return Snippets_Apply(controller, null);
	}
	
	private bool Snippets_Apply(Controller controller, Variant variant)
	{
		if (snippetsMode != null)
		{
			snippetsMode.NextEntry();
			return true;
		}
		if (!controller.AllSelectionsEmpty)
		{
			return false;
		}
		Buffer buffer = buffers.list.Selected;
		if (buffer != null && (buffer.settingsMode | SettingsMode.Normal) != 0)
		{
			Selection selection = buffer.Controller.LastSelection;
			Place place = controller.Lines.PlaceOf(selection.anchor);
			SnippetFilesScanner scanner = Nest.MainForm.SnippetFilesScanner;
			scanner.TryRescan();
			Line line = controller.Lines[place.iLine];
			string indent;
			int tabsCount;
			line.GetFirstIntegerTabs(out indent, out tabsCount);
			List<SnippetInfo> infos = scanner.GetInfos(buffer.Name);
			List<SnippetAtom> sortedAtoms = new List<SnippetAtom>();
			List<SnippetAtom> atoms = new List<SnippetAtom>();
			scanner.LoadFiles(infos);
			foreach (SnippetInfo info in infos)
			{
				SnippetFile snippetFile = scanner.GetFile(info.path);
				if (snippetFile != null)
				{
					sortedAtoms.AddRange(snippetFile.Atoms);
				}
			}
			sortedAtoms.Sort(SnippetAtom.Compare);
			int lastCount = -1;
			foreach (SnippetAtom atom in sortedAtoms)
			{
				bool matched = true;
				for (int i = 0; i < atom.key.Length; ++i)
				{
					int iChar = place.iChar - atom.key.Length + i;
					if (iChar < 0)
					{
						matched = false;
						break;
					}
					if (atom.key[i] != line.chars[iChar].c)
					{
						matched = false;
					}
				}
				if (matched)
				{
					int iChar0 = place.iChar - atom.key.Length;
					int iChar1 = iChar0 - 1;
					if (iChar0 >= 0 && iChar1 >= 0)
					{
						char c0 = line.chars[iChar0].c;
						char c1 = line.chars[iChar1].c;
						if ((char.IsLetterOrDigit(c0) || c0 == '_') &&
							(char.IsLetterOrDigit(c1) || c1 == '_'))
						{
							matched = false;
						}
					}
				}
				if (matched && (variant == null ||
					atom.index == variant.Index && atom.GetCompletionText() == variant.DisplayText) &&
					(lastCount == -1 || atom.key.Length == lastCount))
				{
					atoms.Add(atom);
					lastCount = atom.key.Length;
				}
			}
			if (atoms.Count > 1 && variant != null)
			{
				for (int i = atoms.Count; i-- > 0;)
				{
					SnippetAtom atom = atoms[i];
					if (i == variant.Index)
					{
						atoms.RemoveAt(i);
					}
				}
			}
			if (atoms.Count == 1)
			{
				SnippetAtom atom = atoms[0];
				Snippet snippet = new Snippet(atom.GetIndentedText(indent, controller.Lines.TabSettings), Snippets_ReplaceValue);
				controller.ClearMinorSelections();
				
				int position = selection.anchor;
				controller.LastSelection.anchor = position - atom.key.Length;
				controller.LastSelection.caret = position;
				controller.InsertText(snippet.StartText);
				
				snippetsMode = new SnippetMode(
					textBox, controller, snippet, position - atom.key.Length, Snippets_OnAutocompleteClose);
				snippetsMode.Show();
				return true;
			}
			if (atoms.Count > 1)
			{
				Snippets_ShowAutocomplete(atoms, true);
				return true;
			}
		}
		return false;
	}
	
	private string Snippets_ReplaceValue(string value)
	{
		string error;
		Commander.ReplaceVars(Nest.MainForm, Snippets_GetFile, settings, ref value, out error);
		return value;
	}
	
	private string Snippets_GetFile()
	{
		Buffer lastBuffer = Nest.MainForm.LastBuffer;
		return lastBuffer != null ? lastBuffer.FullPath : null;
	}
	
	private void Snippets_OnAutocompleteClose()
	{
		snippetsMode = null;
	}
	
	private bool Snippets_DoAutocomplete(Controller controller)
	{
		Buffer buffer = buffers.list.Selected;
		if (buffer != null && (buffer.settingsMode | SettingsMode.Normal) != 0)
		{
			SnippetFilesScanner scanner = MainForm.SnippetFilesScanner;
			scanner.TryRescan();
			List<SnippetInfo> infos = scanner.GetInfos(buffer.Name);
			List<SnippetAtom> atoms = new List<SnippetAtom>();
			if (infos != null && infos.Count > 0)
			{
				scanner.LoadFiles(infos);
				foreach (SnippetInfo info in infos) 
				{
					SnippetFile file = scanner.GetFile(info.path);
					if (file != null)
					{
						foreach (SnippetAtom atom in file.Atoms)
						{
							atoms.Add(atom);
						}
					}
				}
			}
			if (atoms.Count > 0)
			{
				Snippets_ShowAutocomplete(atoms, false);
				return true;
			}
		}
		return false;
	}
	
	private void Snippets_ShowAutocomplete(List<SnippetAtom> atoms, bool erase)
	{
		Snippets_CloseAutocomplete();
		snippetsAutocomplete = new AutocompleteMode(textBox, AutocompleteMode.Mode.Raw);
		List<Variant> variants = new List<Variant>();
		foreach (SnippetAtom atom in atoms)
		{
			Variant variant = new Variant();
			variant.Index = atom.index;
			variant.CompletionText = atom.key;
			variant.DisplayText = atom.GetCompletionText();
			variants.Add(variant);
		}
		if (erase)
		{
			snippetsAutocomplete.onDone = Snippets_OnAutocompleteDone_Erase;
		}
		else
		{
			snippetsAutocomplete.onDone = Snippets_OnAutocompleteDone;
		}
		snippetsAutocomplete.Show(variants, "");
	}
	
	private void Snippets_OnAutocompleteDone(Controller controller, Variant variant)
	{
		Snippets_Apply(controller, variant);
	}
	
	private void Snippets_OnAutocompleteDone_Erase(Controller controller, Variant variant)
	{
		controller.ClearMinorSelections();
		int position = controller.LastSelection.anchor;
		controller.LastSelection.anchor = position - variant.CompletionText.Length;
		controller.LastSelection.caret = position;
		controller.EraseSelection();
		Snippets_Apply(controller, variant);
	}
}
