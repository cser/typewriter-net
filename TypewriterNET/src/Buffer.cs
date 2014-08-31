using System;
using System.IO;
using MulticaretEditor;
using MulticaretEditor.Highlighting;
using MulticaretEditor.KeyMapping;

public class Buffer
{
	public Buffer(string fullPath, string name)
	{
		SetFile(fullPath, name);
		controller = new Controller(new LineArray());
	}

	public BufferList owner;

	public Frame Frame { get { return owner != null ? owner.frame : null; } }

	private Controller controller;
	public Controller Controller { get { return controller; } }

	public bool HasHistory { get { return controller.history.CanUndo || controller.history.CanRedo; } }
	public bool Changed { get { return controller.history.Changed; } }
	public bool IsEmpty { get { return controller.Lines.IsEmpty; } }
	
	private string fullPath;
	public string FullPath { get { return fullPath; } }
	
	private string name;
	public string Name { get { return name; } }

	public string httpServer;
	
	public void SetFile(string fullPath, string name)
	{
		this.fullPath = fullPath;
		this.name = name;
	}
	
	public bool needSaveAs;
	public FileInfo fileInfo;
	public DateTime lastWriteTimeUtc;
	public BufferTag tags = BufferTag.None;
	public Getter<Buffer, bool> onRemove;
	public Setter<Buffer> onAdd;
	public Setter<Buffer> onSelected;
	public Setter<Buffer, UpdatePhase> onUpdateSettings;

	public KeyMap additionKeyMap;

	public static string StringOf(Buffer buffer)
	{
		return buffer.Name + (buffer.Changed ? "*" : "");
	}

	private bool? overrideWordWrap = null;
	public bool? OverrideWordWrap
	{
		get { return overrideWordWrap; }
		set
		{
			if (overrideWordWrap != value)
			{
				overrideWordWrap = value;
				if (Frame != null)
					Frame.UpdateOverrides();
			}
		}
	}

	//--------------------------------------------------------------------------
	// Helped
	//--------------------------------------------------------------------------

	public void Write(string text)
	{
		Write(text, null);
	}
	
	public void Write(string text, Ds ds)
	{
		int index = controller.Lines.charsCount;
		controller.ClearMinorSelections();
		controller.PutCursor(controller.Lines.PlaceOf(controller.Lines.charsCount), false);
		controller.Lines.InsertText(index, text);
		if (ds != null)
			controller.SetStyleRange(new StyleRange(index, text.Length, ds.index));
		controller.NeedScrollToCaret();
	}
	
	public void WriteLine(string text)
	{
		WriteLine(text, null);
	}
	
	public void WriteLine(string text, Ds ds)
	{
		Write(text + "\n", ds);
	}

	public void InitText(string text)
	{
		controller.InitText(text);
	}
}
