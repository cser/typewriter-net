using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;
using System.Threading;

public class TabList : Buffer
{
	public class Node
	{
		public Node(string name, string fullPath)
		{
			this.name = name;
			this.fullPath = fullPath;
			hash = fullPath.GetHashCode();
		}

		public readonly string name;
		public readonly string fullPath;
		public readonly int hash;
		public readonly List<Node> childs = new List<Node>();
		public bool expanded = false;
		public int line = -1;
	}

	private readonly Buffer buffer;
	private readonly MainForm mainForm;
	private Dictionary<int, bool> expanded;

	public TabList(Buffer buffer, MainForm mainForm) : base(null, "Tab list", SettingsMode.TabList)
	{
		this.buffer = buffer;
		this.mainForm = mainForm;
        
		expanded = new Dictionary<int, bool>();
		showEncoding = false;
		Controller.isReadonly = true;
		onSelected = OnBufferSelected;
		additionKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("&View\\Tab list\\Close tab list", DoCloseBuffer, null, false);
			additionKeyMap.AddItem(new KeyItem(Keys.Escape, null, action));
			additionKeyMap.AddItem(new KeyItem(Keys.Control | Keys.OemOpenBrackets, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\Tab list\\Select tab", DoOpenTab, null, false);
			additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
		}
	}
	
	public bool DoOnViShortcut(Controller controller, string shortcut)
	{
		if (shortcut == "dd")
		{
			Selection selection = buffer.Controller.LastSelection;
			int index = buffer.Controller.Lines.PlaceOf(selection.caret).iLine;
			if (index >= 0 && index < buffers.Count)
			{
				mainForm.CloseIfExists(buffers[index]);
				Rebuild();
				return true;
			}
		}
		return false;
	}

	private void OnBufferSelected(Buffer buffer)
	{
		Rebuild();
	}
	
	private List<Buffer> buffers = new List<Buffer>();

	private void Rebuild()
	{
		buffers.Clear();
		StringBuilder builder = new StringBuilder();
		foreach (Buffer bufferI in mainForm.GetFileBuffers())
		{
			if (bufferI.settingsMode != SettingsMode.TabList)
			{
				if (builder.Length > 0)
				{
					builder.Append("\n");
				}
				builder.Append(bufferI.Name + (bufferI.Changed ? "*" : ""));
				buffers.Add(bufferI);
			}
		}
		Controller.InitText(builder.ToString());
	
		int charsCount = Controller.Lines.charsCount;
		foreach (Selection selection in Controller.Selections)
		{
			if (selection.anchor > charsCount)
				selection.anchor = charsCount;
			if (selection.caret > charsCount)
				selection.caret = charsCount;
		}
		Controller.JoinSelections();
	}
	
	public void Open()
	{
		Frame frame = mainForm.GetMainNest().Frame;
		if (frame != null)
		{
			frame.AddBuffer(buffer);
			frame.Focus();
		}
	}

	private bool DoCloseBuffer(Controller controller)
	{
		Close();
		return true;
	}
	
	private bool DoOpenTab(Controller controller)
	{
		if (buffer != null)
		{
			Selection selection = Controller.LastSelection;
			int index = Controller.Lines.PlaceOf(selection.caret).iLine;
			if (index >= 0 && index < buffers.Count)
			{
				mainForm.SelectIfExists(buffers[index]);
				CloseSilent();
			}
		}
		return true;
	}
	
	public void Close()
	{
		Frame frame = Frame;
		if (frame != null)
		{
			if (frame.ContainsBuffer(buffer))
			{
				frame.SelectedBuffer = buffer;
			}
			frame.RemoveBuffer(this);
		}
	}
	
	public void CloseSilent()
	{
		if (Frame != null)
		{
			Frame.RemoveBuffer(this);
		}
	}
}
