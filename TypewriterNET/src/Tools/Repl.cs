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
	private const int HistorySize = 50;
	
	private readonly Queue<string> textsToOutput = new Queue<string>();
	private readonly List<string> history = new List<string>();
	private readonly string arguments;
	private readonly string command;
	private readonly string invitation;
	private Process process;
	
	public Repl(string rawCommand, MainForm mainForm) :
		base(null, "REPL: " + GetShortName(rawCommand), SettingsMode.EditableNotFile)
	{
		tags = BufferTag.NeedCorrectRemoving;
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
		{
			KeyAction action = new KeyAction("&View\\Autocomplete\\MoveUp", DoMoveUp, null, false);
			additionBeforeKeyMap.AddItem(new KeyItem(Keys.Up, null, action));
			additionBeforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.P, null, action));
			additionBeforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.K, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\Autocomplete\\MoveDown", DoMoveDown, null, false);
			additionBeforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.N, null, action));
			additionBeforeKeyMap.AddItem(new KeyItem(Keys.Down, null, action));
			additionBeforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.J, null, action));
		}
			
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
		string invitation = RunShellCommand.TryGetParameter(parameters, 'i');
		if (invitation == null)
		{
			invitation = "$";
		}
		if (invitation != "")
		{
			invitation += " ";
		}
		this.invitation = invitation;
		Controller.onBeforePaint = OnBeforePaint;
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
		Controller.InsertText(invitation);
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
		textsToOutput.Enqueue(text);
	}
	
	private void OnBeforePaint()
	{
		while (textsToOutput.Count > 0)
		{
			string text = textsToOutput.Dequeue();
			if (text.Length == 1 && (int)text[0] == 12)
			{
				Controller.ClearMinorSelections();
				Controller.LastSelection.anchor = 0;
				Controller.LastSelection.caret = 0;
				Controller.InitText(invitation);
				Controller.DocumentEnd(false);
			}
			else
			{
				Controller.ClearMinorSelections();
				Controller.DocumentEnd(false);
				Controller.ViMoveHome(false, false);
				Controller.InsertText(text + "\n");
				Controller.DocumentEnd(false);
				Controller.NeedScrollToCaret();
			}
		}
	}
	
	private bool OnEnter(Controller controller)
	{
		if (process != null && Controller.LastSelection.caret >= Controller.Lines.charsCount)
		{
			SelectCurrentLine();
			string command = Controller.GetSelectedText();
			if (command.StartsWith(invitation))
			{
				command = command.Substring(invitation.Length);
			}
			Controller.InsertText(invitation != "" ? invitation + command + "\n" + invitation : "");
			Controller.NeedScrollToCaret();
			AddHistory(command);
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
			if (place.iChar == invitation.Length && place.iLine == Controller.Lines.LinesCount - 1)
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
			if (place.iChar > invitation.Length &&
				anchor.iChar > invitation.Length &&
				place.iLine == Controller.Lines.LinesCount - 1)
			{
				place.iChar = invitation.Length;
				Controller.LastSelection.caret = Controller.Lines.IndexOf(place);
				Controller.LastSelection.SetEmptyIfNotShift(shift);
				return true;
			}
		}
		return false;
	}
	
	private void AddHistory(string text)
	{
		if (text != "")
		{
			history.Remove(text);
			history.Add(text);
			if (history.Count > HistorySize)
			{
				history.RemoveRange(0, history.Count - HistorySize);
			}
		}
	}
	
	private bool DoMoveUp(Controller controller)
	{
		return ProcessMove(controller, true);
	}
	
	private bool DoMoveDown(Controller controller)
	{
		return ProcessMove(controller, false);
	}
	
	private void SelectCurrentLine()
	{
		LineArray lines = Controller.Lines;
		Controller.ClearMinorSelections();
		Controller.LastSelection.anchor = lines.IndexOf(new Place(0, lines.LinesCount - 1));
		Controller.LastSelection.caret = lines.charsCount;
	}
	
	private string GetCurrentLine()
	{
		LineArray lines = Controller.Lines;
		string command = lines[lines.LinesCount - 1].Text;
		return command.StartsWith(invitation) ? command.Substring(invitation.Length) : command;
	}
	
	private void SetCurrentLine(string text)
	{
		SelectCurrentLine();
		Controller.InsertText(invitation + text);
		Controller.NeedScrollToCaret();
	}
	
	private bool ProcessMove(Controller controller, bool isUp)
	{
		if (Controller.LastSelection.caret < Controller.Lines.charsCount)
		{
			return false;
		}
		AddHistory(GetCurrentLine());
		if (history.Count > 0)
		{
			string current = GetCurrentLine();
			int index = history.IndexOf(current);
			if (current == null || index == -1)
			{
				if (isUp)
				{
					SetCurrentLine(history[history.Count - 1]);
				}
				else
				{
					SetCurrentLine("");
				}
				return true;
			}
			index += isUp ? -1 : 1;
			if (index >= history.Count)
			{
				SetCurrentLine("");
				return true;
			}
			if (index < 0)
			{
				index = 0;
			}
			SetCurrentLine(history[index]);
		}
		return true;
	}
}