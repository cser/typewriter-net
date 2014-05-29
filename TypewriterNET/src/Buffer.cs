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

	public bool Changed { get { return controller.history.Changed; } }
	
	private string fullPath;
	public string FullPath { get { return fullPath; } }
	
	private string name;
	public string Name { get { return name; } }
	
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

	public KeyMap additionKeyMap;

	public static string StringOf(Buffer buffer)
	{
		return buffer.Name + (buffer.Changed ? "*" : "");
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
			controller.Lines.SetRangeStyle(index, text.Length, ds.index);
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
