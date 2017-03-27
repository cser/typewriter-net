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
		Nest.MainForm.OpenNew();
	}

	private AutocompleteMode autocomplete;
	
	private void CloseAutocomplete()
	{
		if (autocomplete != null)
		{
			autocomplete.Close();
			autocomplete = null;
		}
	}
	
	public void ShowAutocomplete(List<Variant> variants, string leftWord)
	{
		CloseAutocomplete();
		Buffer buffer = buffers.list.Selected != null ? buffers.list.Selected : null;
		if (buffer == null)
			return;
		autocomplete = new AutocompleteMode(textBox, false);
		autocomplete.Show(variants, leftWord);
	}
}
