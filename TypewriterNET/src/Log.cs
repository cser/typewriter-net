using MulticaretEditor;
using MulticaretEditor.Highlighting;

public class Log
{
	private MainForm mainForm;
	private Buffer buffer;
	private Nest consoleNest;

	public Log(MainForm mainForm, Nest consoleNest)
	{
		this.mainForm = mainForm;
		this.consoleNest = consoleNest;

		buffer = new Buffer(null, "Log");
		buffer.tags = BufferTag.Console;
		buffer.needSaveAs = false;
		buffer.Controller.isReadonly = true;
		buffer.onAdd = OnLogAdd;
	}

	private void OnLogAdd(Buffer buffer)
	{
		buffer.Frame.Focus();
	}

	public void Open()
	{
		mainForm.ShowBuffer(consoleNest, buffer);
	}

	public void Close()
	{
		if (buffer.Frame != null)
			buffer.Frame.Destroy();
	}

	public void Focus()
	{
		buffer.Frame.Focus();
	}

	public bool Opened { get { return buffer.Frame != null; } }

	public void WriteInfo(string type, string desc)
	{
		buffer.Write(type + ":", Ds.Comment);
		buffer.WriteLine(" " + desc);
	}

	public void WriteError(string type, string desc)
	{
		buffer.Write(type + ":", Ds.Error);
		buffer.WriteLine(" " + desc);
	}

	public void WriteWarning(string type, string desc)
	{
		buffer.Write(type + ":", Ds.Others);
		buffer.WriteLine(" " + desc);
	}

	public void Clear()
	{
		buffer.InitText("");
	}
}
