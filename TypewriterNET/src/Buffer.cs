using System;
using System.IO;
using MulticaretEditor;

public class Buffer
{
	public Buffer(string fullPath, string name)
	{
		SetFile(fullPath, name);
		controller = new Controller(new LineArray());
	}
	
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

	public static string StringOf(Buffer buffer)
	{
		return buffer.Name + (buffer.Changed ? "*" : "");
	}
}
