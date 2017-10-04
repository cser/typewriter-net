using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;
using TinyJSON;

public class Repl : Buffer
{
	private Process process;
	private string rawCommand;
	
	public Repl(string rawCommand) : base(
		null,
		"REPL: " + (rawCommand.Length <= 10 ? rawCommand : rawCommand.Substring(0, 10) + "…"),
		SettingsMode.TabList)
	{
		this.rawCommand = rawCommand;
		onAdd = OnAdd;
		onRemove = OnRemove;
		additionKeyMap = new KeyMap();
		additionKeyMap.AddItem(new KeyItem(Keys.Enter, null,
			new KeyAction("&Edit\\Enter command", OnEnter, null, false)));
	}
	
	private void OnAdd(Buffer buffer)
	{
		int index = -1;
		for (int i = 0; i < rawCommand.Length; ++i)
		{
			if (rawCommand[i] == '"')
			{
				for (++i; i < rawCommand.Length; ++i)
				{
					if (rawCommand[i] == '"')
					{
						break;
					}
				}
			}
			else if (rawCommand[i] == ' ')
			{
				index = i;
				break;
			}
		}
		string arguments = index != -1 ? rawCommand.Substring(index + 1) : "";
		string command = index != -1 ? rawCommand.Substring(0, index) : rawCommand;
		process = new Process();
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.FileName = command;
		process.StartInfo.Arguments = arguments;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.RedirectStandardInput = true;
		process.OutputDataReceived += OnOutputDataReceived;
		process.ErrorDataReceived += OnErrorDataReceived;
		try
		{
			process.Start();
			process.BeginErrorReadLine();
			process.BeginOutputReadLine();
		}
		catch (Exception e)
		{
			Controller.InsertText(e.Message);
			Controller.InsertText(Controller.Lines.lineBreak);
		}
	}
	
	private bool OnRemove(Buffer buffer)
	{
		if (process != null)
		{
			try
			{
				process.Kill();
			}
			catch
			{
			}
		}
		return true;
	}
	
	private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
	{
		Controller.InsertText(e.Data);
		Controller.InsertText("\n");
		Controller.NeedScrollToCaret();
	}
	
	private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
	{
		Controller.InsertText(e.Data);
		Controller.InsertText("\n");
		Controller.NeedScrollToCaret();
	}
	
	private bool OnEnter(Controller controller)
	{
		if (process != null && Controller.LastSelection.caret >= Controller.Lines.charsCount)
		{
			Controller.ClearMinorSelections();
			Controller.DocumentEnd(false);
			Controller.ViMoveHome(true, false);
			string command = Controller.GetSelectedText();
			Controller.EraseSelection();
			Controller.NeedScrollToCaret();
			Controller.InsertText(">> " + command + "\n");
			process.StandardInput.WriteLine(command);
			return true;
		}
		return false;
	}
}