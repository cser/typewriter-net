using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;

public class RunShellCommand
{
	private MainForm mainForm;

	public RunShellCommand(MainForm mainForm)
	{
		this.mainForm = mainForm;
	}

	private Buffer buffer;

	public string Execute(string commandText)
	{
		Process p = new Process();
		p.StartInfo.UseShellExecute = false;
		p.StartInfo.RedirectStandardOutput = true;
		p.StartInfo.FileName = "cmd.exe";
		p.StartInfo.Arguments = "/C " + commandText;
		p.Start();
		string output = p.StandardOutput.ReadToEnd();
		p.WaitForExit();

		buffer = new Buffer(null, "Shell command results");
		buffer.Controller.isReadonly = true;
		buffer.Controller.InitText(output);
		buffer.additionKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("F&ind\\Navigate to position", ExecuteEnter, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.None, null, action).SetDoubleClick(true));
		}
		{
			KeyAction action = new KeyAction("F&ind\\Close execution", CloseBuffer, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Escape, null, action));
		}
		mainForm.ShowBuffer(mainForm.ConsoleNest, buffer);
		if (mainForm.ConsoleNest.Frame != null)
			mainForm.ConsoleNest.Frame.Focus();
		return null;
	}

	public bool ExecuteEnter(Controller controller)
	{
		mainForm.Dialogs.ShowInfo("Error", "Not realized");
		// TODO
		return true;
	}

	private bool CloseBuffer(Controller controller)
	{
		if (buffer != null && buffer.Frame != null)
			buffer.Frame.RemoveBuffer(buffer);
		return true;
	}
}
