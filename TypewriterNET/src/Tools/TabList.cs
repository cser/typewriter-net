using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;
using System.Threading;

public class TabList
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

	private MainForm mainForm;
	private Dictionary<int, bool> expanded;

	private Buffer buffer;
	public Buffer Buffer { get { return buffer; } }

	public TabList(MainForm mainForm)
	{
		this.mainForm = mainForm;
        
		expanded = new Dictionary<int, bool>();
		buffer = new Buffer(null, "Tab list", SettingsMode.TabList);
		buffer.showEncoding = false;
		buffer.Controller.isReadonly = true;
		buffer.onSelected = OnBufferSelected;
		buffer.onRemove = OnBufferRemove;
		buffer.additionKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("&View\\Tab list\\Close tab list", DoCloseBuffer, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Escape, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Control | Keys.OemOpenBrackets, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\Tab list\\Select tab", DoOpenTab, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
		}
	}
	
	public void DoOnViShortcut(Controller controller, string shortcut)
	{
		if (buffer == null)
		{
			return;
		}
		if (shortcut == "dd")
		{
			Selection selection = buffer.Controller.LastSelection;
			int index = buffer.Controller.Lines.PlaceOf(selection.caret).iLine;
			if (index >= 0 && index < buffers.Count)
			{
				mainForm.CloseIfExists(buffers[index]);
				Rebuild();
			}
		}
	}

	private void OnBufferSelected(Buffer buffer)
	{
		Rebuild();
	}
	
	private bool OnBufferRemove(Buffer buffer)
	{
		buffers.Clear();
		buffer = null;
		return true;
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
				builder.Append(bufferI.Name);
				buffers.Add(bufferI);
			}
		}
		buffer.Controller.InitText(builder.ToString());
	
		int charsCount = buffer.Controller.Lines.charsCount;
		foreach (Selection selection in buffer.Controller.Selections)
		{
			if (selection.anchor > charsCount)
				selection.anchor = charsCount;
			if (selection.caret > charsCount)
				selection.caret = charsCount;
		}
		buffer.Controller.JoinSelections();
	}
	
	public void Open()
	{
		Frame frame = mainForm.MainNest.Frame;
		if (frame != null)
		{
			frame.AddBuffer(buffer);
			frame.Focus();
		}
	}

	private bool DoCloseBuffer(Controller controller)
	{
		CloseBuffer();
		return true;
	}
	
	private bool DoOpenTab(Controller controller)
	{
		if (buffer != null)
		{
			Selection selection = buffer.Controller.LastSelection;
			int index = buffer.Controller.Lines.PlaceOf(selection.caret).iLine;
			if (index >= 0 && index < buffers.Count)
			{
				mainForm.SelectIfExists(buffers[index]);
				CloseBuffer();
			}
		}
		return true;
	}
	
	private void CloseBuffer()
	{
		if (buffer != null && buffer.Frame != null)
		{
			buffer.Frame.RemoveBuffer(buffer);
		}
	}
}
