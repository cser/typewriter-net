using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;

public class Repl : Buffer
{
	private const int HistorySize = 50;
	
	private readonly Queue<string> textsToOutput = new Queue<string>();
	private readonly StringList history = new StringList();
	private readonly string arguments;
	private readonly string command;
	private readonly string invitation;
	private Process process;
	
	private static string RemoveParameters(string rawCommand)
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
		return command;
	}
	
	private static string GetShortName(string rawCommand)
	{
		string command = RemoveParameters(rawCommand);
		return command.Length <= 10 ? command : command.Substring(0, 10) + "…";
	}
	
	public Repl(string rawCommand, MainForm mainForm) :
		base(RemoveParameters(rawCommand), "REPL: " + GetShortName(rawCommand), SettingsMode.EditableNotFile)
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
			KeyAction action = new KeyAction("&Edit\\REPL\\Prev command", DoMoveUp, null, false);
			additionKeyMap.AddItem(new KeyItem(Keys.Up, null, action));
			additionKeyMap.AddItem(new KeyItem(Keys.Control | Keys.P, null, action));
			additionKeyMap.AddItem(new KeyItem(Keys.Control | Keys.K, null, action));
		}
		{
			KeyAction action = new KeyAction("&Edit\\REPL\\Next command", DoMoveDown, null, false);
			additionKeyMap.AddItem(new KeyItem(Keys.Control | Keys.N, null, action));
			additionKeyMap.AddItem(new KeyItem(Keys.Down, null, action));
			additionKeyMap.AddItem(new KeyItem(Keys.Control | Keys.J, null, action));
		}
		{
		    KeyAction action = new KeyAction("&Edit\\REPL\\Autocomplete path", DoAutocomplete, null, false);
            additionKeyMap.AddItem(new KeyItem(Keys.Control | Keys.Space, null, action));
            additionKeyMap.AddItem(new KeyItem(Keys.Tab, null, action));
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
	
	public override string ListShowingName { get { return FullPath; } }
	
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
			history.Add(command);
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
		return ProcessHome(false);
	}
	
	private bool OnHomeWithSelection(Controller controller)
	{
		return ProcessHome(true);
	}
	
	private bool ProcessHome(bool shift)
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
	
	private bool DoMoveUp(Controller controller)
	{
		return ProcessMove(true);
	}
	
	private bool DoMoveDown(Controller controller)
	{
		return ProcessMove(false);
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
	
	private bool ProcessMove(bool isUp)
	{
		if (Controller.LastSelection.caret < Controller.Lines.charsCount)
		{
			return false;
		}
		string command = GetCurrentLine();
		history.SetCurrent(command);
		history.Switch(isUp);
		if (history.Current != command)
		{
			SetCurrentLine(history.Current);
		}
		return true;
	}
	
	private bool DoAutocomplete(Controller controller)
	{
		string text = GetCurrentLine();
		int quotesCount = 0;
		int quotesIndex = 0;
		while (true)
		{
			quotesIndex = text.IndexOf('"', quotesIndex);
			if (quotesIndex == -1)
				break;
			quotesIndex++;
			if (quotesIndex >= text.Length)
				break;
			quotesCount++;
		}
		string path = "";
		int index = text.Length;
		while (true)
		{
			if (index <= 0)
			{
				path = text;
				break;
			}
			index--;
			if (quotesCount % 2 == 0 && (text[index] == ' ' || text[index] == '\t' || text[index] == '"') ||
				quotesCount % 2 == 1 && text[index] == '"')
			{
				path = text.Substring(index + 1);
				break;
			}
		}
		CommandDialog.AutocompletePath(Frame.TextBox, path, null, false);
		return true;
	}
}