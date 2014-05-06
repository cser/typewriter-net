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
			buffer.Frame.RemoveBuffer(buffer);
	}

	public bool Opened
	{
		get
		{
			return buffer.Frame != null;
		}
	}

	public void Write(string text)
	{
		buffer.Write(text);
	}

	public void Write(string text, Ds ds)
	{
		buffer.Write(text, ds);
	}

	public void WriteLine(string text)
	{
		buffer.WriteLine(text);
	}

	public void WriteLine(string text, Ds ds)
	{
		buffer.WriteLine(text, ds);
	}
}
