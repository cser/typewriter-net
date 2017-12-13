using MulticaretEditor;

public class Log
{
	public static Log DebugLogger;
	
	private readonly MainForm mainForm;
	private readonly Buffer buffer;
	private readonly Nest consoleNest;

	public Log(MainForm mainForm, Nest consoleNest)
	{
		DebugLogger = this;
		
		this.mainForm = mainForm;
		this.consoleNest = consoleNest;

		buffer = new Buffer(null, "Log", SettingsMode.Normal);
		buffer.tags = BufferTag.Console;
		buffer.needSaveAs = false;
		buffer.Controller.isReadonly = true;
		buffer.onAdd = OnLogAdd;
		mainForm.RegisterConsoleBuffer(MainForm.LogId, buffer);
	}

	private void OnLogAdd(Buffer buffer)
	{
		buffer.Frame.Focus();
	}

	public void Open()
	{
		mainForm.ShowConsoleBuffer(MainForm.LogId, buffer);
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
