using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using MulticaretEditor;
using TinyJSON;

public class Repl : Buffer
{
	private readonly string arguments;
	private readonly string command;
	private Process process;
	
	public Repl(string rawCommand, MainForm mainForm) :
		base(null, "REPL: " + GetShortName(rawCommand), SettingsMode.EditableNotFile)
	{
		onAdd = OnAdd;
		onRemove = OnRemove;
		additionKeyMap = new KeyMap();
		additionKeyMap.AddItem(new KeyItem(Keys.Enter, null,
			new KeyAction("&Edit\\REPL\\Enter command", OnEnter, null, false)));
		additionKeyMap.AddItem(new KeyItem(Keys.Back, null,
			new KeyAction("&Edit\\REPL\\Backspace", OnBackspace, null, false)));
		additionBeforeKeyMap = new KeyMap();
		additionBeforeKeyMap.AddItem(new KeyItem(Keys.Home, null,
			new KeyAction("&Edit\\REPL\\Home", OnHome, null, false)));
		additionBeforeKeyMap.AddItem(new KeyItem(Keys.Shift | Keys.Home, null,
			new KeyAction("&Edit\\REPL\\Home with selection", OnHomeWithSelection, null, false)));
			
		string parameters = RunShellCommand.CutParametersFromLeft(ref rawCommand);
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
		arguments = index != -1 ? rawCommand.Substring(index + 1) : "";
		command = index != -1 ? rawCommand.Substring(0, index) : rawCommand;
		Encoding encoding = RunShellCommand.GetEncoding(mainForm, parameters);
		encodingPair = new EncodingPair(encoding, false);
		customSyntax = RunShellCommand.TryGetSyntax(parameters);
	}
	
	private static string GetShortName(string rawCommand)
	{
		string command = rawCommand.Trim();
		if (command.StartsWith("{"))
		{
			int index = command.IndexOf('}');
			if (index != -1)
			{
				command = command.Substring(index + 1);
			}
		}
		return command.Length <= 10 ? command : command.Substring(0, 10) + "…";
	}
	
	private void OnAdd(Buffer buffer)
	{
		Controller.InsertText(">> ");
		Controller.DocumentEnd(false);
		process = new Process();
		process.StartInfo.StandardOutputEncoding = encodingPair.encoding;
		process.StartInfo.StandardErrorEncoding = encodingPair.encoding;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.FileName = command;
		process.StartInfo.Arguments = arguments;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.RedirectStandardInput = true;
		process.OutputDataReceived += OnOutputDataReceived;
		process.ErrorDataReceived += OnErrorDataReceived;
		process.Disposed += OnDisposed;
		process.Exited += OnExited;
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
		InsertOutputLine(e.Data);
	}
	
	private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
	{
		InsertOutputLine(e.Data);
	}
	
	private void OnExited(object sender, EventArgs e)
	{
		InsertOutputLine("EXITED");
	}
	
	private void OnDisposed(object sender, EventArgs e)
	{
		InsertOutputLine("DISPOSED");
	}
	
	private void InsertOutputLine(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (text.Length == 1 && (int)text[0] == 12)
		{
			Controller.ClearMinorSelections();
			Controller.DocumentStart(false);
			Controller.DocumentEnd(true);
			Controller.ViMoveHome(true, false);
			Controller.EraseSelection();
			Controller.DocumentEnd(false);
		}
		else
		{
			Controller.ClearMinorSelections();
			Controller.DocumentEnd(false);
			Controller.ViMoveHome(false, false);
			Controller.InsertText(text);
			Controller.InsertText("\n");
			Controller.DocumentEnd(false);
			Controller.NeedScrollToCaret();
		}
	}
	
	private bool OnEnter(Controller controller)
	{
		if (process != null && Controller.LastSelection.caret >= Controller.Lines.charsCount)
		{
			Controller.ClearMinorSelections();
			Controller.DocumentEnd(false);
			Controller.ViMoveHome(true, false);
			string command = Controller.GetSelectedText();
			if (command.StartsWith(">>"))
			{
				command = command.Substring(2);
				if (command.StartsWith(" "))
				{
					command = command.Substring(1);
				}
			}
			Controller.EraseSelection();
			Controller.NeedScrollToCaret();
			Controller.InsertText(">> ");
			process.StandardInput.Write(command + "\n");
			return true;
		}
		return false;
	}
	
	private bool OnBackspace(Controller controller)
	{
		if (Controller.SelectionsCount == 1 && Controller.AllSelectionsEmpty)
		{
			Place place = Controller.Lines.PlaceOf(Controller.LastSelection.caret);
			if (place.iChar == 3 && place.iLine == Controller.Lines.LinesCount - 1)
			{
				return true;
			}
		}
		return false;
	}
	
	private bool OnHome(Controller controller)
	{
		return ProcessHome(controller, false);
	}
	
	private bool OnHomeWithSelection(Controller controller)
	{
		return ProcessHome(controller, true);
	}
	
	private bool ProcessHome(Controller controller, bool shift)
	{
		if (Controller.SelectionsCount == 1)
		{
			Place place = Controller.Lines.PlaceOf(Controller.LastSelection.caret);
			Place anchor = Controller.Lines.PlaceOf(Controller.LastSelection.anchor);
			if (place.iChar > 3 && anchor.iChar > 3 && place.iLine == Controller.Lines.LinesCount - 1)
			{
				place.iChar = 3;
				Controller.LastSelection.caret = Controller.Lines.IndexOf(place);
				Controller.LastSelection.SetEmptyIfNotShift(shift);
				return true;
			}
		}
		return false;
	}
}