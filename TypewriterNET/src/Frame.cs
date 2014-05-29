using System;
using System.Collections;
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

public class Frame : AFrame, IEnumerable<Buffer>
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

	public Frame(BufferList buffers, KeyMap keyMap, KeyMap doNothingKeyMap)
	{
		this.buffers = buffers;

		buffers.frame = this;
		buffers.list.SelectedChange += OnTabSelected;

		tabBar = new TabBar<Buffer>(buffers.list, Buffer.StringOf);
		tabBar.CloseClick += OnCloseClick;
		tabBar.TabDoubleClick += OnTabDoubleClick;
		Controls.Add(tabBar);

		splitLine = new SplitLine();
		Controls.Add(splitLine);

		KeyMap frameKeyMap = new KeyMap();
		frameKeyMap.AddItem(new KeyItem(Keys.Tab, Keys.Control, new KeyAction("&View\\Switch tab", DoTabDown, DoTabModeChange, false)));
		frameKeyMap.AddItem(new KeyItem(Keys.Control | Keys.W, null, new KeyAction("&View\\Close tab", DoCloseTab, null, false)));

		textBox = new MulticaretTextBox();
		textBox.KeyMap.AddAfter(keyMap);
		textBox.KeyMap.AddAfter(frameKeyMap);
		textBox.KeyMap.AddAfter(doNothingKeyMap, -1);
		textBox.FocusedChange += OnTextBoxFocusedChange;
		textBox.Controller = GetEmptyController();
		Controls.Add(textBox);

		InitResizing(tabBar, splitLine);
		tabBar.MouseDown += OnTabBarMouseDown;
	}

	public void Destroy()
	{
		buffers.list.SelectedChange -= OnTabSelected;
		buffers.frame = null;
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

	override protected void DoUpdateSettings(Settings settings, UpdatePhase phase)
	{
		if (phase == UpdatePhase.Raw)
		{
			settings.ApplyParameters(textBox);
			tabBar.SetFont(settings.font.Value, settings.fontSize.Value);
		}
		else if (phase == UpdatePhase.Parsed)
		{
			textBox.Scheme = settings.ParsedScheme;
			tabBar.Scheme = settings.ParsedScheme;
		}
		else if (phase == UpdatePhase.HighlighterChange)
		{
			UpdateHighlighter();
		}
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
				buffer.Frame.RemoveBuffer(buffer);
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
		if (buffer == null)
			return;
		if (buffer.onRemove != null && !buffer.onRemove(buffer))
			return;
		buffer.Controller.history.ChangedChange -= OnChangedChange;
		buffer.owner = null;
		buffers.list.Remove(buffer);
		if (buffers.list.Count == 0)
			Close();
	}

	private void OnChangedChange()
	{
		tabBar.Invalidate();
	}

	private KeyMap additionKeyMap;

	private void OnTabSelected()
	{
		Buffer buffer = buffers.list.Selected;
		if (additionKeyMap != null)
			textBox.KeyMap.RemoveAfter(additionKeyMap);
		additionKeyMap = buffer != null ? buffer.additionKeyMap : null;
		textBox.Controller = buffer != null ? buffer.Controller : GetEmptyController();
		if (additionKeyMap != null)
			textBox.KeyMap.AddAfter(additionKeyMap, 1);
		UpdateHighlighter();
	}

	public void UpdateHighlighter()
	{
		Nest.MainForm.UpdateHighlighter(textBox, buffers.list.Selected != null ? buffers.list.Selected.Name : null);
	}

	private void OnCloseClick()
	{
		RemoveBuffer(buffers.list.Selected);
		if (buffers.list.Count == 0)
			Close();
	}

	private void OnTabDoubleClick(Buffer buffer)
	{
		RemoveBuffer(buffer);
	}

	private void Close()
	{
		if (Nest == null)
			return;
		MainForm mainForm = Nest.MainForm;
		Nest.AFrame = null;
		mainForm.DoResize();
	}

	public int BuffersCount { get { return buffers.list.Count; } }

	public Buffer this[int index]
	{
		get { return buffers.list[index]; }
	}

	public IEnumerator<Buffer> GetEnumerator()
	{
		return buffers.list.GetEnumerator();
	}
	
	IEnumerator IEnumerable.GetEnumerator()
	{
		return buffers.list.GetEnumerator();
	}

	public Buffer GetByFullPath(BufferTag tags, string fullPath)
	{
		foreach (Buffer buffer in buffers.list)
		{
			if (buffer.FullPath == fullPath && (buffer.tags & tags) == tags)
				return buffer;
		}
		return null;
	}
}
