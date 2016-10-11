using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Forms;
using MulticaretEditor;
using MulticaretEditor.Highlighting;
using TinyJSON;

public class Commander
{
	public class Command
	{
		public readonly string name;
		public readonly string argNames;
		public readonly string desc;
		public readonly Setter<string> execute;

		public Command(string name, string argNames, string desc, Setter<string> execute)
		{
			this.name = name;
			this.argNames = argNames;
			this.desc = desc;
			this.execute = execute;
		}
	}

	private MainForm mainForm;
	private TempSettings tempSettings;
	private Settings settings;

	private readonly List<Command> commands = new List<Command>();

	private StringList history;
	public StringList History { get { return history; } }

	private string FirstWord(string text, out string tail)
	{
		string first;
		int index = text.IndexOf(' ');
		if (index != -1)
		{
			first = text.Substring(0, index);
			tail = text.Substring(index + 1);
		}
		else
		{
			first = text;
			tail = "";
		}
		return first;
	}

	public void Execute(string text)
	{
		if (string.IsNullOrEmpty(text))
			return;
		string args;
		string name = FirstWord(text, out args);
		if (name == "")
			return;
		history.Add(text);
		Command command = null;
		foreach (Command commandI in commands)
		{
			if (commandI.name == name)
			{
				command = commandI;
				break;
			}
		}
		if (command != null)
		{
			command.execute(args);
		}
		else if (!string.IsNullOrEmpty(Properties.NameOfName(name)) && settings[Properties.NameOfName(name)] != null)
		{
			if (args != "")
			{
				string errors = settings[Properties.NameOfName(name)].SetText(args, Properties.SubvalueOfName(name));
				settings.DispatchChange();
				if (!string.IsNullOrEmpty(errors))
					mainForm.Dialogs.ShowInfo("Error assign of \"" + name + "\"", errors);
			}
			else
			{
				mainForm.Dialogs.ShowInfo("Value of \"" + Properties.NameOfName(name) + "\"", settings[name].Text);
			}
		}
		else if (name.StartsWith("!!!"))
		{
			string commandText = text.Substring(3);
			if (ReplaceVars(ref commandText))
			{
				Encoding encoding = mainForm.Settings.shellEncoding.Value.encoding ?? Encoding.UTF8;
				Process p = new Process();
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.RedirectStandardError = true;
				p.StartInfo.StandardOutputEncoding = encoding;
				p.StartInfo.StandardErrorEncoding = encoding;
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.FileName = "cmd.exe";
				p.StartInfo.Arguments = "/C " + commandText;
				p.Start();
				string output = p.StandardOutput.ReadToEnd();
				string errors = p.StandardError.ReadToEnd();
				p.WaitForExit();
				
				string infoText = "";
				if (!string.IsNullOrEmpty(errors))
				{
					infoText += "ERRORS:\n" + errors;
				}
				if (!string.IsNullOrEmpty(output))
				{
					if (infoText != "")
						infoText += "\n";
					infoText += output;
				}
				mainForm.Dialogs.ShowInfo(commandText, infoText);
			}
		}
		else if (name.StartsWith("!!"))
		{
			string commandText = text.Substring(2);
			if (ReplaceVars(ref commandText))
			{
				Process p = new Process();
				p.StartInfo.UseShellExecute = true;
				p.StartInfo.FileName = "cmd.exe";
				p.StartInfo.Arguments = "/C " + commandText;
				p.Start();
			}
		}
		else if (name.StartsWith("!"))
		{
			string commandText = text.Substring(1).Trim();
			if (ReplaceVars(ref commandText))
				ExecuteShellCommand(commandText);
		}
		else
		{
			mainForm.Dialogs.ShowInfo("Error", "Unknown command/property \"" + name + "\"");
		}
	}

	private bool ReplaceVars(ref string commandText)
	{
		if (commandText.Contains(RunShellCommand.FileVar))
		{
			Buffer lastBuffer = mainForm.LastBuffer;
			if (lastBuffer == null || string.IsNullOrEmpty(lastBuffer.FullPath))
			{
				mainForm.Dialogs.ShowInfo(
					"Error", "No opened file in current frame for replace " + RunShellCommand.FileVar);
				return false;
			}
			commandText = commandText.Replace(RunShellCommand.FileVar, lastBuffer.FullPath);
		}
		if (commandText.Contains(RunShellCommand.FileVarSoftly))
		{
			Buffer lastBuffer = mainForm.LastBuffer;
			if (lastBuffer == null)
			{
				mainForm.Dialogs.ShowInfo(
					"Error", "No last selected buffer for " + RunShellCommand.FileVarSoftly);
				return false;
			}
			commandText = commandText.Replace(RunShellCommand.FileVar, lastBuffer.FullPath ?? "");
		}
		if (commandText.Contains(RunShellCommand.FileDirVar))
		{
			Buffer lastBuffer = mainForm.LastBuffer;
			if (lastBuffer == null || string.IsNullOrEmpty(lastBuffer.FullPath))
			{
				mainForm.Dialogs.ShowInfo(
					"Error", "No opened file in current frame for replace " + RunShellCommand.FileDirVar);
				return false;
			}
			string dir = Path.GetDirectoryName(lastBuffer.FullPath);
			commandText = commandText.Replace(RunShellCommand.FileDirVar, dir);
		}
		if (commandText.Contains(RunShellCommand.LineVar))
		{
			Buffer lastBuffer = mainForm.LastBuffer;
			if (lastBuffer == null)
			{
				mainForm.Dialogs.ShowInfo("Error", "No last selected buffer for " + RunShellCommand.LineVar);
				return false;
			}
			commandText = commandText.Replace(
				RunShellCommand.LineVar,
				(lastBuffer.Controller.Lines.PlaceOf(lastBuffer.Controller.LastSelection.caret).iLine + 1) + ""
			);
		}
		if (commandText.Contains(RunShellCommand.CharVar))
		{
			Buffer lastBuffer = mainForm.LastBuffer;
			if (lastBuffer == null)
			{
				mainForm.Dialogs.ShowInfo("Error", "No last selected buffer for " + RunShellCommand.CharVar);
				return false;
			}
			commandText = commandText.Replace(
				RunShellCommand.CharVar,
				lastBuffer.Controller.Lines.PlaceOf(lastBuffer.Controller.LastSelection.caret).iChar + ""
			);
		}
		if (commandText.Contains(RunShellCommand.SelectedVar))
		{
			Buffer lastBuffer = mainForm.LastBuffer;
			if (lastBuffer == null)
			{
				mainForm.Dialogs.ShowInfo(
					"Error", "No buffer with selection for replace " + RunShellCommand.SelectedVar);
				return false;
			}
			StringBuilder builder = new StringBuilder();
			bool hasNotEmpty = false;
			foreach (Selection selection in lastBuffer.Controller.Selections)
			{
				if (!selection.Empty)
				{
					hasNotEmpty = true;
					break;
				}
			}
			foreach (Selection selection in lastBuffer.Controller.Selections)
			{
				if (selection.Empty && hasNotEmpty)
					continue;
				if (builder.Length > 0)
					builder.Append(settings.lineBreak.Value);
				if (selection.Empty)
				{
					Place place = lastBuffer.Controller.Lines.PlaceOf(selection.Left);
					builder.Append(lastBuffer.Controller.Lines[place.iLine].Text.Replace("\n", "").Replace("\r", ""));
				}
				else
				{
					builder.Append(lastBuffer.Controller.Lines.GetText(selection.Left, selection.Count));
				}	
			}
			commandText = commandText.Replace(RunShellCommand.SelectedVar, EscapeForCommandLine(builder.ToString()));
		}
		if (commandText.Contains(RunShellCommand.WordVar))
		{
			Buffer lastBuffer = mainForm.LastBuffer;
			if (lastBuffer == null)
			{
				mainForm.Dialogs.ShowInfo(
					"Error", "No buffer with selection for replace " + RunShellCommand.WordVar);
				return false;
			}
			bool hasNotEmpty = false;
			foreach (Selection selection in lastBuffer.Controller.Selections)
			{
				if (!selection.Empty)
				{
					hasNotEmpty = true;
					break;
				}
			}
			string varValue;
			if (hasNotEmpty)
			{
				StringBuilder builder = new StringBuilder();
				foreach (Selection selection in lastBuffer.Controller.Selections)
				{
					if (selection.Empty)
						continue;
					if (builder.Length > 0)
						builder.Append(settings.lineBreak.Value);
					builder.Append(lastBuffer.Controller.Lines.GetText(selection.Left, selection.Count));
				}
				varValue = builder.ToString();
			}
			else
			{
				Place place = lastBuffer.Controller.Lines.PlaceOf(lastBuffer.Controller.LastSelection.caret);
				varValue = lastBuffer.Controller.GetWord(place);
			}
			commandText = commandText.Replace(RunShellCommand.WordVar, EscapeForCommandLine(varValue));
		}
		return true;
	}

	public string GetHelpText()
	{
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("# Commands");
		builder.AppendLine();

		TextTable table = new TextTable().SetMaxColWidth(40);
		table.Add("Command").Add("Arguments").Add("Description");
		table.AddLine();
		table.Add("!command").Add("*").Add("Run shell command");
		table.NewRow();
		table.Add("!!command").Add("*").Add("Execute without output capture");
		table.NewRow();
		table.Add("!!!command").Add("*").Add("Execute with output into info panel");
		table.NewRow();
		table.Add("").Add("").Add("Variables: ");
		table.NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.FileVar + " - current file dir path");
		table.NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.FileVarSoftly + " - current file full path, and use empty if no saved file");
		table.NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.FileDirVar + " - current file directory path");
		table.NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.LineVar + " - current file line at cursor");
		table.NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.CharVar + " - current file char at cursor");
		table.NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.SelectedVar + " - current selected text or line");
		table.NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.WordVar + " - current selected text or word");
		foreach (Command command in commands)
		{
			table.NewRow();
			table.Add(command.name)
				.Add(!string.IsNullOrEmpty(command.desc) ? command.argNames : "")
				.Add(!string.IsNullOrEmpty(command.desc) ? command.desc : "");
		}
		builder.Append(table);

		return builder.ToString();
	}

	public void Init(MainForm mainForm, Settings settings, TempSettings tempSettings)
	{
		this.mainForm = mainForm;
		this.settings = settings;
		this.tempSettings = tempSettings;

		history = tempSettings.CommandHistory;
		commands.Add(new Command("help", "", "Open/close tab with help text", DoHelp));
		commands.Add(new Command("cd", "path", "Change/show current directory", DoChangeCurrentDirectory));
		commands.Add(new Command("exit", "", "Close window", DoExit));
		commands.Add(new Command("lclear", "", "Clear editor log", DoClearLog));
		commands.Add(new Command("reset", "name", "Reset property", DoResetProperty));
		commands.Add(new Command("edit", "file", "Edit file/new file", DoEditFile));
		commands.Add(new Command("open", "file", "Open file", DoOpenFile));
		commands.Add(new Command("md", "directory", "Create directory", DoCreateDirectory));
		commands.Add(new Command(
			"shortcut", "text", "Just reopen dialog with text - for config shorcuts", DoShortcut));
		commands.Add(new Command("omnisharp-autocomplete", "", "autocomplete by omnisharp server", DoOmnisharpAutocomplete));
		commands.Add(new Command("omnisharp-findUsages", "", "find usages by omnisharp server", DoOmnisharpFindUsages));
		commands.Add(new Command("omnisharp-goToDefinition", "", "go to definition by omnisharp server", DoGoToDefinition));
		commands.Add(new Command("omnisharp-codecheck", "", "check code", DoCodeckeck));
		commands.Add(new Command("omnisharp-syntaxerrors", "", "show syntax errors", DoSyntaxErrors));
	}

	private void DoHelp(string args)
	{
		mainForm.ProcessHelp();
	}

	private void DoExit(string args)
	{
		mainForm.Close();
	}

	private void DoClearLog(string args)
	{
		mainForm.Log.Clear();
	}

	private void DoResetProperty(string args)
	{
		if (args == "")
		{
			settings.Reset();
			settings.DispatchChange();
		}
		else if (settings[args] != null)
		{
			settings[args].Reset();
			settings.DispatchChange();
			mainForm.Dialogs.ShowInfo("Value of \"" + args + "\"", settings[args].Text);
		}
		else
		{
			mainForm.Dialogs.ShowInfo("Error", "Unknown property \"" + args + "\"");
		}
	}

	private void DoChangeCurrentDirectory(string path)
	{
		string error = "";
		if (string.IsNullOrEmpty(path) || mainForm.SetCurrentDirectory(path, out error))
			mainForm.Dialogs.ShowInfo("Current directory", Directory.GetCurrentDirectory());
		else
			mainForm.Dialogs.ShowInfo("Error", error);
	}

	private void ExecuteShellCommand(string commandText)
	{
		new RunShellCommand(mainForm).Execute(commandText, settings.shellRegexList.Value);
	}

	private void DoEditFile(string file)
	{
		if (ReplaceVars(ref file))
		{
			Buffer buffer = mainForm.ForcedLoadFile(file);
			buffer.needSaveAs = false;
		}
	}

	private void DoOpenFile(string file)
	{
		mainForm.LoadFile(file);
	}

	private void DoCreateDirectory(string dir)
	{
		try
		{
			DirectoryInfo info = Directory.CreateDirectory(dir);
			mainForm.Dialogs.ShowInfo("Created directory", info.FullName);
		}
		catch (Exception e)
		{
			mainForm.Dialogs.ShowInfo("Error", e.Message);
		}
	}
	
	private void DoShortcut(string text)
	{
		mainForm.Dialogs.ShowInputCommand(text);
	}
	
	public static string EscapeForCommandLine(string text)
	{
		return text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\\n")
			.Replace("\\", "\\\\").Replace("\"", "\\\"");
	}
	
	public void DoOmnisharpAutocomplete(string text)
	{
		if (!mainForm.SharpManager.Started)
		{
			mainForm.Dialogs.ShowInfo("Error", "OmniSharp server is not started");
			return;
		}
		
		Buffer lastBuffer = mainForm.LastBuffer;
		if (lastBuffer == null)
		{
			mainForm.Dialogs.ShowInfo("Error", "No last selected buffer for omnisharp autocomplete");
			return;
		}

		Selection selection = lastBuffer.Controller.LastSelection;
		Place place = lastBuffer.Controller.Lines.PlaceOf(selection.anchor);
		string editorText = lastBuffer.Controller.Lines.GetText();
		string word = lastBuffer.Controller.GetLeftWord(place);
		
		Node node = new SharpRequest(mainForm)
			.Add("FileName", lastBuffer.FullPath)
			.Add("WordToComplete", "")
			.Add("Buffer", editorText)
			.Add("Line", (place.iLine + 1) + "")
			.Add("Column", (place.iChar + 1) + "")
			.Send(mainForm.SharpManager.Url + "/autocomplete", false);
		if (node != null)
		{
			if (!node.IsArray())
			{
				mainForm.Dialogs.ShowInfo("OmniSharp", "Response parsing error: Array expected, but was:" + node.TypeOf());
				return;
			}
			List<Variant> variants = new List<Variant>();
			for (int i = 0; i < node.Count; i++)
			{
				try
				{
					Variant variant = new Variant();
					variant.CompletionText = (string)node[i]["CompletionText"];
					variant.DisplayText = (string)node[i]["DisplayText"];
					variants.Add(variant);
				}
				catch (Exception)
				{
				}
			}
			if (mainForm.LastFrame.AsFrame != null)
				mainForm.LastFrame.AsFrame.ShowAutocomplete(variants, word);
		}
	}
	
	public void DoOmnisharpFindUsages(string text)
	{
		if (!mainForm.SharpManager.Started)
		{
			mainForm.Dialogs.ShowInfo("Error", "OmniSharp server is not started");
			return;
		}
		
		Buffer lastBuffer = mainForm.LastBuffer;
		if (lastBuffer == null)
		{
			mainForm.Dialogs.ShowInfo("Error", "No last selected buffer for omnisharp autocomplete");
			return;
		}

		Selection selection = lastBuffer.Controller.LastSelection;
		Place place = lastBuffer.Controller.Lines.PlaceOf(selection.anchor);
		string editorText = lastBuffer.Controller.Lines.GetText();
		string word = lastBuffer.Controller.GetWord(place);
		
		Node node = new SharpRequest(mainForm)
			.Add("FileName", lastBuffer.FullPath)
			.Add("WordToComplete", word)
			.Add("Buffer", editorText)
			.Add("Line", (place.iLine + 1) + "")
			.Add("Column", (place.iChar + 1) + "")
			.Send(mainForm.SharpManager.Url + "/findusages", false);
		if (node != null)
		{
			if (!node.IsTable())
			{
				mainForm.Dialogs.ShowInfo("OmniSharp", "Response parsing error: Table expected, but was:" + node.TypeOf());
				return;
			}
			node = node["QuickFixes"];
			if (!node.IsArray())
			{
				mainForm.Dialogs.ShowInfo("OmniSharp", "Response parsing error: Array expected, but was:" + node.TypeOf());
				return;
			}
			List<Usage> usages = new List<Usage>();
			for (int i = 0; i < node.Count; i++)
			{
				try
				{
					Usage usage = new Usage();
					usage.FileName = (string)node[i]["FileName"];
					usage.Line = (int)node[i]["Line"];
					usage.Column = (int)node[i]["Column"];
					usage.Text = (string)node[i]["Text"];
					usages.Add(usage);
				}
				catch (Exception)
				{
				}
			}
			string errors = new ShowUsages(mainForm).Execute(usages, word);
			if (errors != null)
				mainForm.Dialogs.ShowInfo("Usages", errors);
		}
	}
	
	public void DoGoToDefinition(string text)
	{
		if (!mainForm.SharpManager.Started)
		{
			mainForm.Dialogs.ShowInfo("Error", "OmniSharp server is not started");
			return;
		}
		
		Buffer lastBuffer = mainForm.LastBuffer;
		if (lastBuffer == null)
		{
			mainForm.Dialogs.ShowInfo("Error", "No last selected buffer for omnisharp autocomplete");
			return;
		}

		Selection selection = lastBuffer.Controller.LastSelection;
		Place place = lastBuffer.Controller.Lines.PlaceOf(selection.anchor);
		string editorText = lastBuffer.Controller.Lines.GetText();
		string word = lastBuffer.Controller.GetWord(place);
		
		Node node = new SharpRequest(mainForm)
			.Add("FileName", lastBuffer.FullPath)
			.Add("WordToComplete", "")
			.Add("Buffer", editorText)
			.Add("Line", (place.iLine + 1) + "")
			.Add("Column", (place.iChar + 1) + "")
			.Send(mainForm.SharpManager.Url + "/gotodefinition", false);
		if (node != null)
		{
			if (!node.IsTable())
			{
				mainForm.Dialogs.ShowInfo("OmniSharp", "Response parsing error: Array expected, but was:" + node.TypeOf());
				return;
			}
			string fullPath = null;
			Place navigationPlace = new Place();
			try
			{
				Variant variant = new Variant();
				fullPath = !node["FileName"].IsNull() ? (string)node["FileName"] : null;
				int line = (int)node["Line"];
				int column = (int)node["Column"];
				navigationPlace = new Place(column - 1, line - 1);
			}
			catch (Exception)
			{
				mainForm.Dialogs.ShowInfo("OmniSharp", "Error: incorrect format");
			}
			if (fullPath != null)
			{
				mainForm.NavigateTo(fullPath, navigationPlace, navigationPlace);
			}
			else
			{
				mainForm.Dialogs.ShowInfo("OmniSharp", "Can't navigate to: " + word);
			}
		}
	}
	
	public void DoCodeckeck(string text)
	{
		ProcessCodeckeck(text, "Code check results", "/codecheck", "QuickFixes");
	}
	
	public void DoSyntaxErrors(string text)
	{
		ProcessCodeckeck(text, "Syntax errors", "/syntaxerrors", "Errors");
	}
	
	public void ProcessCodeckeck(string text, string name, string uri, string fieldName)
	{
		if (!mainForm.SharpManager.Started)
		{
			mainForm.Dialogs.ShowInfo("Error", "OmniSharp server is not started");
			return;
		}
		
		Buffer lastBuffer = mainForm.LastBuffer;
		if (lastBuffer == null)
		{
			mainForm.Dialogs.ShowInfo("Error", "No last selected buffer for omnisharp autocomplete");
			return;
		}

		Selection selection = lastBuffer.Controller.LastSelection;
		Place place = lastBuffer.Controller.Lines.PlaceOf(selection.anchor);
		string editorText = lastBuffer.Controller.Lines.GetText();
		string word = lastBuffer.Controller.GetWord(place);
		
		Node node = new SharpRequest(mainForm)
			.Add("FileName", lastBuffer.FullPath)
			.Add("WordToComplete", word)
			.Add("Buffer", editorText)
			.Add("Line", (place.iLine + 1) + "")
			.Add("Column", (place.iChar + 1) + "")
			.Send(mainForm.SharpManager.Url + uri, false);
		if (node != null)
		{
			if (!node.IsTable())
			{
				mainForm.Dialogs.ShowInfo("OmniSharp", "Response parsing error: Table expected, but was:" + node.TypeOf());
				return;
			}
			node = node[fieldName];
			if (!node.IsArray())
			{
				mainForm.Dialogs.ShowInfo("OmniSharp", "Response parsing error: Array expected, but was:" + node.TypeOf());
				return;
			}
			List<Codecheck> codechecks = new List<Codecheck>();
			for (int i = 0; i < node.Count; i++)
			{
				try
				{
					if (uri == "/codecheck")
					{
						Codecheck codecheck = new Codecheck();
						codecheck.LogLevel = (string)node[i]["LogLevel"];
						codecheck.FileName = (string)node[i]["FileName"];
						codecheck.Line = (int)node[i]["Line"];
						codecheck.Column = (int)node[i]["Column"];
						codecheck.Text = (string)node[i]["Text"];
						codechecks.Add(codecheck);
					}
					else if (uri == "/syntaxerrors")
					{
						Codecheck codecheck = new Codecheck();
						codecheck.LogLevel = "";
						codecheck.FileName = (string)node[i]["FileName"];
						codecheck.Line = (int)node[i]["Line"];
						codecheck.Column = (int)node[i]["Column"];
						codecheck.Text = (string)node[i]["Message"];
						codechecks.Add(codecheck);
					}
				}
				catch (Exception)
				{
				}
			}
			if (uri == "/codecheck" && node.Count == 0)
			{
				mainForm.Dialogs.ShowInfo(name, "No tips");
				return;
			}
			else if (uri == "/syntaxerrors" && node.Count == 0)
			{
				mainForm.Dialogs.ShowInfo(name, "No errors");
				return;
			}
			string errors = new ShowCodecheck(mainForm, name).Execute(codechecks, word);
			if (errors != null)
				mainForm.Dialogs.ShowInfo(name, errors);
		}
	}
}
