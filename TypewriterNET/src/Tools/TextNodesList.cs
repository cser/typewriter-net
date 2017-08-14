using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;
using TinyJSON;

public class TextNodesList : Buffer
{
	private Buffer buffer;
	
	public TextNodesList() : base(null, "Nodes list", SettingsMode.TabList)
	{
	}
	
	public void Build(Buffer buffer, Properties.CommandInfo commandInfo, Encoding encoding, out string error)
	{
		this.buffer = buffer;
		
		Process p = new Process();
		p.StartInfo.RedirectStandardOutput = true;
		p.StartInfo.RedirectStandardError = true;
		p.StartInfo.RedirectStandardInput = true;
		p.StartInfo.StandardOutputEncoding = encoding;
		p.StartInfo.StandardErrorEncoding = encoding;
		p.StartInfo.UseShellExecute = false;
		p.StartInfo.FileName = "cmd.exe";
		p.StartInfo.Arguments = "/C " + commandInfo.command;
		p.Start();
		p.StandardInput.Write(buffer.Controller.Lines.GetText());
		p.StandardInput.Close();
		string output = p.StandardOutput.ReadToEnd();
		string errors = p.StandardError.ReadToEnd();
		p.WaitForExit();
		
		error = null;
		try
		{
			Node node = new Parser().Load(output);
		}
		catch (Exception e)
		{
			error = "Parsing error: " + e.Message;
		}
		
		showEncoding = false;
		Controller.isReadonly = true;
		additionKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("&View\\Nodes list\\Close nodes list", DoCloseBuffer, null, false);
			additionKeyMap.AddItem(new KeyItem(Keys.Escape, null, action));
			additionKeyMap.AddItem(new KeyItem(Keys.Control | Keys.OemOpenBrackets, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\Nodes list\\Jump to node", DoOpenTab, null, false);
			additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
		}
	}
	
	private bool DoCloseBuffer(Controller controller)
	{
		Close();
		return true;
	}
	
	private bool DoOpenTab(Controller controller)
	{
		return true;
	}
	
	public void Close()
	{
		if (Frame != null)
		{
			Frame.RemoveBuffer(this);
		}
	}
}