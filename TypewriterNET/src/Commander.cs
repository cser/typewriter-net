using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
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

	public void Execute(string text, bool dontPutInHistory, bool showCommandInOutput)
	{
		if (string.IsNullOrEmpty(text))
			return;
		bool needFileTreeReload = mainForm.FileTreeFocused;
		string args;
		string name = FirstWord(text, out args);
		if (name == "")
			return;
		if (!dontPutInHistory)
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
			bool dontChangeFocus = false;
			if (commandText.StartsWith("!"))
			{
				commandText = commandText.Substring(1);
				dontChangeFocus = true;
			}
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
				if (dontChangeFocus && mainForm.LastFrame != null)
					mainForm.LastFrame.Focus();
				if (needFileTreeReload)
				    mainForm.FileTreeReload();
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
			{
				ExecuteShellCommand(commandText, showCommandInOutput);
				if (needFileTreeReload)
				    mainForm.FileTreeReload();
		    }
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
			string file = null;
			if (lastBuffer == null || string.IsNullOrEmpty(lastBuffer.FullPath))
			{
			    if (mainForm.LeftNest.AFrame != null && mainForm.LeftNest.buffers.list.Selected == mainForm.FileTree.Buffer)
			    {
			        file = mainForm.FileTree.GetCurrentFile();
			    }
			}
			else
			{
			    file = lastBuffer.FullPath;
			}
			if (file == null)
			{
			    mainForm.Dialogs.ShowInfo(
					"Error", "No opened file in current frame for replace " + RunShellCommand.FileVar);
				return false;
			}
			commandText = commandText.Replace(RunShellCommand.FileVar, file);
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
				if (varValue.Length > 0 && !char.IsLetterOrDigit(varValue[0]) && varValue[0] != '_' && place.iChar > 0)
				{
					--place.iChar;
					string newValue = lastBuffer.Controller.GetWord(place);
					if (varValue.Length > 0 && (char.IsLetterOrDigit(newValue[0]) || newValue[0] == '_'))
					{
						varValue = newValue;
					}
				}
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
		table.Add("!!command").Add("*").Add("Run without output capture");
		table.NewRow();
		table.Add("!!!command").Add("*").Add("Run with output to info panel");
		table.NewRow();
		table.Add("!!!!command").Add("*").Add("Run with output to info panel, unfocused");
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
		commands.Add(new Command("omnisharp-getoverridetargets", "", "get override targets", DoOmnisharpGetOverrideTargets));
		commands.Add(new Command("omnisharp-findUsages", "", "find usages by omnisharp server", DoOmnisharpFindUsages));
		commands.Add(new Command("omnisharp-goToDefinition", "", "go to definition by omnisharp server", DoGoToDefinition));
		commands.Add(new Command("omnisharp-codecheck", "", "check code", DoCodeckeck));
		commands.Add(new Command("omnisharp-syntaxerrors", "", "show syntax errors", DoSyntaxErrors));
		commands.Add(new Command("omnisharp-rename", "", "rename", DoOmnisharpRename));
		commands.Add(new Command("omnisharp-reloadsolution", "", "reload solution", DoOmnisharpReloadSolution));
		commands.Add(new Command("omnisharp-buildcommand", "", "build", DoOmnisharpBuildcommand));
		commands.Add(new Command("omnisharp-currentfilemembers", "", "current file members", DoOmnisharpCurrentFileMembers));
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
		if (string.IsNullOrEmpty(path))
		{
		    mainForm.Dialogs.ShowInfo("Current directory", Directory.GetCurrentDirectory());
		}
		if (mainForm.SetCurrentDirectory(path, out error))
		{
		    if (error != null)
            {
                mainForm.Dialogs.ShowInfo("Error", error);
            }
            else
            {
			    mainForm.Dialogs.ShowInfo("Current directory changed to", Directory.GetCurrentDirectory());
			}
		}
		else
		{
		    mainForm.Dialogs.ShowInfo("Current directory", Directory.GetCurrentDirectory());
		}
	}

	private void ExecuteShellCommand(string commandText, bool showCommandInOutput)
	{
		new RunShellCommand(mainForm).Execute(commandText, showCommandInOutput, settings.shellRegexList.Value);
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
	
	public void DoOmnisharpGetOverrideTargets(string text)
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
		string lineText = lastBuffer.Controller.Lines[place.iLine].Text;
		
		Node node = new SharpRequest(mainForm)
			.Add("FileName", lastBuffer.FullPath)
			.Add("Buffer", editorText)
			.Add("Line", (place.iLine + 1) + "")
			.Add("Column", (place.iChar + 1) + "")
			.Send(mainForm.SharpManager.Url + "/getoverridetargets", true);
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
				string targetName = null;
				try
				{
					targetName = (string)node[i]["OverrideTargetName"];
				}
				catch (Exception)
				{
				}
				if (targetName != null)
				{
					string firstSpaces = settings.lineBreak.Value;
					for (int ii = 0; ii < lineText.Length; ii++)
					{
						char c = lineText[ii];
						if (c != ' ' && c != '\t')
							break;
						firstSpaces += c;
					}
					targetName = targetName.Trim().Replace(" (", "(");
					if (targetName.EndsWith(";"))
						targetName = targetName.Substring(0, targetName.Length - 1);
					Variant variant = new Variant();
					variant.CompletionText = targetName
						.Replace("virtual", "override")
						.Replace("abstract", "override")
						.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", firstSpaces);
					variant.DisplayText = targetName;
					variants.Add(variant);
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
				try
				{
					fullPath = Path.GetFullPath(fullPath);
				}
				catch (Exception)
				{
				}
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
	
	public void DoOmnisharpRename(string text)
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
		
		new SharpRenameAction().Execute(mainForm, tempSettings, lastBuffer);
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
	
	private void DoOmnisharpReloadSolution(string text)
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
		
		mainForm.Dialogs.ShowInfo("OmniSharp", "Solution reloading...");
		if (mainForm.LastFrame != null)
			mainForm.LastFrame.Focus();
		Thread thread = new Thread(new ThreadStart(OmnisharpReloadSolution));
		thread.Start();
	}
	
	private void OmnisharpReloadSolution()
	{
		string error;
		Node node = new SharpRequest(mainForm)
			.Send(mainForm.SharpManager.Url + "/reloadsolution", out error);
		if (error != null)
		{
			mainForm.Invoke(new Setter(delegate
			{
				mainForm.Dialogs.ShowInfo("OmniSharp", error);
				if (mainForm.LastFrame != null)
					mainForm.LastFrame.Focus();
			}));
			return;
		}
		if (node == null || !node.IsBool() || !(bool)node)
		{
			mainForm.Invoke(new Setter(delegate
			{
				mainForm.Dialogs.ShowInfo("OmniSharp", "Expected true, was: " + node);
				if (mainForm.LastFrame != null)
					mainForm.LastFrame.Focus();
			}));
			return;
		}
		mainForm.Invoke(new Setter(delegate
		{
			mainForm.Dialogs.HideInfo("OmniSharp", "Solution reloading...");
			if (mainForm.LastFrame != null)
				mainForm.LastFrame.Focus();
		}));
	}
	
	private void DoOmnisharpBuildcommand(string text)
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
		string error;
		string output = new SharpRequest(mainForm)
			.SendWithRawOutput(mainForm.SharpManager.Url + "/buildcommand", out error);
		if (error != null)
		{
			mainForm.Dialogs.ShowInfo("Error", error);
		}
		else if (output != null)
		{
			Execute("!" + output, true, true);
		}
		else
		{
			mainForm.Dialogs.ShowInfo("OmniSharp", "Response is empty");
			if (mainForm.LastFrame != null)
				mainForm.LastFrame.Focus();
		}
	}
	
	private void DoOmnisharpCurrentFileMembers(string text)
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
			.Send(mainForm.SharpManager.Url + "/currentfilemembersflat", true);
		if (node != null)
		{
			mainForm.Dialogs.ShowInfo("OmniSharp", node + "");
		}
	}
}
