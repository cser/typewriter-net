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

	private readonly RWList<Command> commands = new RWList<Command>();
	public IRList<Command> Commands { get { return commands; } }

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

	public void Execute(
		string text, bool dontPutInHistory, bool showCommandInOutput, Getter<string, string> getAltCommandText,
		OnceCallback close)
	{
		if (string.IsNullOrEmpty(text))
		{
			close.Execute();
			return;
		}
		bool needFileTreeReload = mainForm.FileTreeFocused;
		string args;
		string name = FirstWord(text, out args);
		if (name == "")
		{
			close.Execute();
			return;
		}
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
		if (command == null && getAltCommandText != null)
		{
			name = getAltCommandText(name);
			foreach (Command commandI in commands)
			{
				if (commandI.name == name)
				{
					command = commandI;
					break;
				}
			}
		}
		if (command != null)
		{
			close.Execute();
			command.execute(args);
			return;
		}
		foreach (CommandData data in settings.command.Value)
		{
			if (data.name == name)
			{
				close.Execute();
				if (MulticaretTextBox.initMacrosExecutor != null)
				{
					StringBuilder errors = new StringBuilder();
					List<MacrosExecutor.Action> actions = CommandData.WithWorkaround(data.GetActions(errors));
					if (errors.Length == 0)
					{
						MulticaretTextBox.initMacrosExecutor.Execute(actions);
					}
					else
					{
						mainForm.Dialogs.ShowInfo("Error sequence of \"" + data.name + "\"", errors.ToString());
					}
				}
				return;
			}
		}
		if (!string.IsNullOrEmpty(Properties.NameOfName(name)) && settings[Properties.NameOfName(name)] != null)
		{
			close.Execute();
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
			return;
		}
		if (name.StartsWith("!!!"))
		{
			close.Execute();
			string commandText = text.Substring(3);
			bool dontChangeFocus = false;
			bool silentIfNoOutput = false;
			if (commandText.StartsWith("!"))
			{
				commandText = commandText.Substring(1);
				dontChangeFocus = true;
			}
			if (commandText.StartsWith("?"))
			{
				commandText = commandText.Substring(1);
				silentIfNoOutput = true;
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
				if (silentIfNoOutput)
				{
					p.StartInfo.CreateNoWindow = true;
				}
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
				if (!string.IsNullOrEmpty(infoText) || !silentIfNoOutput)
				{
					mainForm.Dialogs.ShowInfo(commandText, infoText);
				}
				if (dontChangeFocus && mainForm.LastFrame != null)
					mainForm.LastFrame.Focus();
				if (needFileTreeReload)
				    mainForm.FileTreeReload();
			}
			return;
		}
		if (name.StartsWith("!!"))
		{
			close.Execute();
			string commandText = text.Substring(2);
			if (ReplaceVars(ref commandText))
			{
				Process p = new Process();
				p.StartInfo.UseShellExecute = true;
				p.StartInfo.FileName = "cmd.exe";
				p.StartInfo.Arguments = "/C " + commandText;
				p.Start();
			}
			return;
		}
		if (name.StartsWith("!"))
		{
			close.Execute();
			bool scrollUp = false;
			bool silentIfNoOutput = false;
			string commandText = text.Substring(1);
			if (commandText.StartsWith("?^"))
			{
				commandText = commandText.Substring(2);
				scrollUp = true;
				silentIfNoOutput = true;
			}
			else
			{
				if (commandText.StartsWith("^"))
				{
					commandText = commandText.Substring(1);
					scrollUp = true;
				}
				if (commandText.StartsWith("?"))
				{
					commandText = commandText.Substring(1);
					silentIfNoOutput = true;
				}
			}
			string parameters = RunShellCommand.CutParametersFromLeft(ref commandText);
			commandText = commandText.Trim();
			if (ReplaceVars(ref commandText))
			{
				ExecuteShellCommand(commandText, showCommandInOutput, scrollUp, silentIfNoOutput, parameters);
				if (needFileTreeReload)
				    mainForm.FileTreeReload();
		    }
		    return;
		}
		if (name.StartsWith("<") || name.StartsWith(">"))
		{
			string commandText = text;
			bool writeStdOutput = commandText.StartsWith("<");
			if (writeStdOutput)
			{
				commandText = commandText.Substring(1);
			}
			bool writeStdInput = commandText.StartsWith(">");
			if (writeStdInput)
			{
				commandText = commandText.Substring(1);
			}
			string parameters = RunShellCommand.CutParametersFromLeft(ref commandText);
			Encoding encoding = RunShellCommand.GetEncoding(mainForm, parameters);
			if (ReplaceVars(ref commandText))
			{
				Buffer buffer = mainForm.LastBuffer;
				Process p = new Process();
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.RedirectStandardError = true;
				if (writeStdInput)
				{
					p.StartInfo.RedirectStandardInput = true;
				}
				p.StartInfo.StandardOutputEncoding = encoding;
				p.StartInfo.StandardErrorEncoding = encoding;
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.FileName = "cmd.exe";
				p.StartInfo.Arguments = "/C " + commandText;
				p.Start();
				if (writeStdInput && buffer != null)
				{
					p.StandardInput.Write(buffer.Controller.GetSelectedText());
					p.StandardInput.Close();
				}
				string output = p.StandardOutput.ReadToEnd();
				string errors = p.StandardError.ReadToEnd();
				p.WaitForExit();
				if (!string.IsNullOrEmpty(errors))
				{
					mainForm.Dialogs.ShowInfo("Error", errors);
				}
				else
				{
					if (buffer != null)
					{
						if (output.EndsWith("\r\n"))
						{
							output = output.Substring(0, output.Length - 2);
						}
						else if (output.EndsWith("\n") || output.EndsWith("\r"))
						{
							output = output.Substring(0, output.Length - 1);
						}
						if (writeStdOutput)
						{
							buffer.Controller.processor.BeginBatch();
							buffer.Controller.EraseSelection();
							buffer.Controller.InsertText(output);
							buffer.Controller.processor.EndBatch();
						}
						else
						{
							new RunShellCommand(mainForm).ShowInOutput(
								output, settings.shellRegexList.Value, false, false, parameters);
						}
					}
					else
					{
						mainForm.Dialogs.ShowInfo("Error", "No buffer for output");
					}
					if (needFileTreeReload)
						mainForm.FileTreeReload();
				}
		    }
			close.Execute();
		    return;
		}
		close.Execute();
		mainForm.Dialogs.ShowInfo("Error", "Unknown command/property \"" + name + "\"");
	}
	
	private string GetFile()
	{
		Buffer lastBuffer = mainForm.LastBuffer;
		if (lastBuffer == null || string.IsNullOrEmpty(lastBuffer.FullPath))
		{
			if (mainForm.LastFrame == mainForm.LeftNest.AFrame)
			if (mainForm.LeftNest.AFrame != null && mainForm.LeftNest.buffers.list.Selected == mainForm.FileTree.Buffer)
			{
				return mainForm.FileTree.GetCurrentFile();
			}
			return null;
		}
		return lastBuffer.FullPath;
	}
	
	private static string ReplaceVar(string text, string key, string value)
	{
		string changedKey;
		changedKey = key + ":upper;";
		if (text.Contains(changedKey))
		{
			text = text.Replace(changedKey, value.ToUpperInvariant());
		}
		changedKey = key + ":lower;";
		if (text.Contains(changedKey))
		{
			text = text.Replace(changedKey, value.ToLowerInvariant());
		}
		changedKey = key + ":first_upper;";
		if (text.Contains(changedKey))
		{
			text = text.Replace(changedKey, value.Length > 0 ?
				char.ToUpperInvariant(value[0]) + value.Substring(1) : "");
		}
		changedKey = key + ":first_lower;";
		if (text.Contains(changedKey))
		{
			text = text.Replace(changedKey, value.Length > 0 ?
				char.ToLowerInvariant(value[0]) + value.Substring(1) : "");
		}
		return text.Replace(key, value);
	}
	
	public static bool ReplaceVars(MainForm mainForm, Getter<string> getFile, Settings settings,
		ref string commandText, out string error)
	{
		error = null;
		if (commandText.Contains(RunShellCommand.FileVar))
		{
			string file = getFile();
			if (file == null)
			{
			    error = "No opened file in current frame for replace " + RunShellCommand.FileVar;
				return false;
			}
			commandText = ReplaceVar(commandText, RunShellCommand.FileVar, file);
		}
		if (commandText.Contains(RunShellCommand.AppDataDirVar))
		{
			commandText = ReplaceVar(commandText, RunShellCommand.AppDataDirVar, AppPath.AppDataDir);
		}
		if (commandText.Contains(RunShellCommand.FileNameVar))
		{
			string file = getFile();
			if (file == null)
			{
			    error = "No opened file in current frame for replace " + RunShellCommand.FileNameVar;
				return false;
			}
			commandText = ReplaceVar(commandText, RunShellCommand.FileNameVar, Path.GetFileNameWithoutExtension(file));
		}
		if (commandText.Contains(RunShellCommand.FileVarSoftly))
		{
			Buffer lastBuffer = mainForm.LastBuffer;
			if (lastBuffer == null)
			{
				error = "No last selected buffer for " + RunShellCommand.FileVarSoftly;
				return false;
			}
			string file = getFile();
			commandText = ReplaceVar(commandText, RunShellCommand.FileVarSoftly, file ?? "");
		}
		if (commandText.Contains(RunShellCommand.FileDirVar))
		{
			string file = getFile();
			if (file == null)
			{
				error = "No opened file in current frame for replace " + RunShellCommand.FileDirVar;
				return false;
			}
			string dir = Path.GetDirectoryName(file);
			commandText = ReplaceVar(commandText, RunShellCommand.FileDirVar, dir);
		}
		if (commandText.Contains(RunShellCommand.LineVar))
		{
			Buffer lastBuffer = mainForm.LastBuffer;
			if (lastBuffer == null)
			{
				error = "No last selected buffer for " + RunShellCommand.LineVar;
				return false;
			}
			commandText = ReplaceVar(commandText, 
				RunShellCommand.LineVar,
				(lastBuffer.Controller.Lines.PlaceOf(lastBuffer.Controller.LastSelection.caret).iLine + 1) + ""
			);
		}
		if (commandText.Contains(RunShellCommand.CharVar))
		{
			Buffer lastBuffer = mainForm.LastBuffer;
			if (lastBuffer == null)
			{
				error = "No last selected buffer for " + RunShellCommand.CharVar;
				return false;
			}
			commandText = ReplaceVar(commandText, 
				RunShellCommand.CharVar,
				lastBuffer.Controller.Lines.PlaceOf(lastBuffer.Controller.LastSelection.caret).iChar + ""
			);
		}
		if (commandText.Contains(RunShellCommand.SelectedVar))
		{
			Buffer lastBuffer = mainForm.LastBuffer;
			if (lastBuffer == null)
			{
				error = "No buffer with selection for replace " + RunShellCommand.SelectedVar;
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
			commandText = ReplaceVar(commandText, RunShellCommand.SelectedVar, EscapeForCommandLine(builder.ToString()));
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
			commandText = ReplaceVar(commandText, RunShellCommand.WordVar, EscapeForCommandLine(varValue));
		}
		return true;
	}

	private bool ReplaceVars(ref string commandText)
	{
		string error;
		bool result = ReplaceVars(mainForm, GetFile, settings, ref commandText, out error);
		if (error != null)
		{
			mainForm.Dialogs.ShowInfo("Error", error);
		}
		return result;
	}

	public string GetHelpText()
	{
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("# Commands");
		builder.AppendLine();

		TextTable table = new TextTable().SetMaxColWidth(40);
		table.Add("Command").Add("Arguments").Add("Description");
		table.AddLine();
		table.Add("!command").Add("*").Add("Run shell command").NewRow();
		table.Add("!{s:syntax;e:encoding}command").Add("*").Add("Run with custom syntax/encoding").NewRow();
		table.Add("!^command").Add("*").Add("Run shell command, stay output up").NewRow();
		table.Add("!^{s:syntax;e:encoding}command").Add("*").Add("Run with custom syntax/encoding").NewRow();
		table.Add("!?command").Add("*").Add("Run and show only non-empty output\n" + 
			"  Usable for syntax checkers\n" +
			"  For example, if you have jshint,\n" +
			"  write line in config (open by F2):\n" +
			"    <item name=\"afterSaveCommand:*.js\"\n" +
			"          value=\"!?jshint %f%\"/>").NewRow();
		table.Add("<command").Add("*").Add("Shell command output into document").NewRow();
		table.Add(">command").Add("*").Add("Selected text writes into command stdin,\n  output into console").NewRow();
		table.Add("<>command").Add("*").Add("Selected text writes into command stdin,\n  output writes back into document").NewRow();
		table.Add("!!command").Add("*").Add("Run without output capture").NewRow();
		table.Add("!!!command").Add("*").Add("Run with output to info panel").NewRow();
		table.Add("!!!!command").Add("*").Add("Run with output to info panel, unfocused").NewRow();
		table.Add("").Add("").Add("Variables: ").NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.FileVar + " - current file path").NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.FileVarSoftly + " - current file path (no errors)").NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.FileNameVar + " - current file name (no extension)").NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.FileDirVar + " - current file directory path").NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.LineVar + " - current file line at cursor").NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.CharVar + " - current file char at cursor").NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.SelectedVar + " - current selected text or line").NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.WordVar + " - current selected text or word").NewRow();
		table.Add("").Add("").Add("  " + RunShellCommand.AppDataDirVar + " - AppData subfolder").NewRow();
		table.Add("").Add("").Add("Suffixes (example: %n%:upper;): ").NewRow();
		table.Add("").Add("").Add("  :upper;").NewRow();
		table.Add("").Add("").Add("  :lower;").NewRow();
		table.Add("").Add("").Add("  :first_upper;").NewRow();
		table.Add("").Add("").Add("  :first_lower;");
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
		commands.Add(new Command("vi-help", "", "Open/close tab with vi-help text", DoViHelp));
		commands.Add(new Command("cd", "path", "Change/show current directory", DoChangeCurrentDirectory));
		commands.Add(new Command("exit", "", "Close window", DoExit));
		commands.Add(new Command("lclear", "", "Clear editor log", DoClearLog));
		commands.Add(new Command("reset", "name", "Reset property", DoResetProperty));
		commands.Add(new Command("edit", "file", "Edit file/new file", DoEditFile));
		commands.Add(new Command("open", "file", "Open file", DoOpenFile));
		commands.Add(new Command("md", "directory", "Create directory", DoCreateDirectory));
		commands.Add(new Command("explorer", "[file]", "Open in explorer", DoOpenInExplorer));
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
		commands.Add(new Command("omnisharp-updatebuffer", "", "update buffer", DoOmnisharpUpdateBuffer));
		
		commands.Add(new Command("ctags", "[parameters]", "rebuild tags (default parameters -R *)", DoCtagsRebuild));
		commands.Add(new Command("ctags-goToDefinition", "", "jump to tag definition", DoCtagsGoToDefinition));
		
		commands.Add(new Command("w", "", "Save file", DoViSaveFile));
		commands.Add(new Command("e", "", "Edit file (new file if no parameter)", DoEditFile));
		commands.Add(new Command("q", "", "Close window", DoExit));
		commands.Add(new Command("h", "", "Open/close tab with help text", DoHelp));
		commands.Add(new Command("vh", "", "Open/close tab with vi-help text", DoViHelp));
	}
	
	private void DoViSaveFile(string args)
	{
		mainForm.SaveFile(mainForm.LastBuffer);
	}

	private void DoHelp(string args)
	{
		mainForm.ProcessHelp();
	}
	
	private void DoViHelp(string args)
	{
		mainForm.ProcessViHelp();
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

	private void ExecuteShellCommand(string commandText, bool showCommandInOutput, bool stayTop, bool silentIfNoOutput, string parameters)
	{
		new RunShellCommand(mainForm).Execute(
			commandText, showCommandInOutput, settings.shellRegexList.Value,
			stayTop, silentIfNoOutput, parameters);
	}

	private void DoEditFile(string file)
	{
		if (ReplaceVars(ref file))
		{
			if (string.IsNullOrEmpty(file))
			{
				mainForm.OpenNew();
			}
			else
			{
				Buffer buffer = mainForm.ForcedLoadFile(file);
				if (buffer != null)
				{
					buffer.needSaveAs = false;
				}
			}
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
	
	public void DoOpenInExplorer(string file)
	{
		if (string.IsNullOrEmpty(file))
		{
			file = GetFile();
		}
		else if (!ReplaceVars(ref file))
		{
			return;
		}
		if (string.IsNullOrEmpty(file))
		{
			mainForm.Dialogs.ShowInfo("Error", "No file for open in explorer");
			return;
		}
		bool isDirectory = Directory.Exists(file);
		Process p = new Process();
		p.StartInfo.UseShellExecute = true;
		p.StartInfo.FileName = "explorer.exe";
		p.StartInfo.Arguments = isDirectory ? "\"" + file + "\"" : "/select, \"" + file + "\"";
		p.Start();
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
			.Send(mainForm.SharpManager.Url + "/getoverridetargets", false);
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
		
		mainForm.Dialogs.ShowInfo("OmniSharp", "Solution reloading…");
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
			mainForm.Dialogs.HideInfo("OmniSharp", "Solution reloading…");
			if (mainForm.LastFrame != null)
				mainForm.LastFrame.Focus();
		}));
	}
	
	private void DoOmnisharpUpdateBuffer(string text)
	{
		if (!mainForm.SharpManager.Started)
		{
			return;
		}
		
		Buffer lastBuffer = mainForm.LastBuffer;
		if (lastBuffer == null)
		{
			return;
		}
		
		UpdateBufferWork work = new UpdateBufferWork();
		work.editorText = lastBuffer.Controller.Lines.GetText();
		work.fullPath = lastBuffer.FullPath;
		work.mainForm = mainForm;
		Thread thread = new Thread(new ThreadStart(work.Execute));
		thread.Start();
	}
	
	public class UpdateBufferWork
	{
		public string editorText;
		public string fullPath;
		public MainForm mainForm;
		
		public void Execute()
		{
			string error;
			Node node = new SharpRequest(mainForm)
				.Add("FileName", fullPath)
				.Add("Buffer", editorText)
				.Send(mainForm.SharpManager.Url + "/updatebuffer", out error);
		}
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
			Execute("!" + output, true, true, null, new OnceCallback());
		}
		else
		{
			mainForm.Dialogs.ShowInfo("OmniSharp", "Response is empty");
			if (mainForm.LastFrame != null)
				mainForm.LastFrame.Focus();
		}
	}
	
	private void DoCtagsRebuild(string text)
	{
		mainForm.Dialogs.ShowInfo("Ctags", "Rebuilding…");
		if (mainForm.LastFrame != null)
			mainForm.LastFrame.Focus();
		Thread thread = new Thread(new ThreadStart(delegate { CtagsRebuild(text); }));
		thread.Start();
	}
	
	private void CtagsRebuild(string parameters)
	{
		Process p = new Process();
		p.StartInfo.UseShellExecute = false;
		p.StartInfo.CreateNoWindow = true;
		p.StartInfo.RedirectStandardError = true;
		p.StartInfo.RedirectStandardOutput = true;
		p.StartInfo.FileName = Path.Combine(AppPath.StartupDir, "ctags/ctags.exe");
		p.StartInfo.Arguments = !string.IsNullOrEmpty(parameters) ? parameters : "-R *";
		string output = null;
		string error = null;
		try
		{
			p.Start();
			output = p.StandardOutput.ReadToEnd();
			error = p.StandardError.ReadToEnd();
			p.WaitForExit();
		}
		catch (Exception e)
		{
			mainForm.Invoke(new Setter(delegate
			{
				mainForm.Dialogs.ShowInfo("Ctags", e.Message);
				if (mainForm.LastFrame != null)
					mainForm.LastFrame.Focus();
			}));
			return;
		}
		if (!string.IsNullOrEmpty(output) || !string.IsNullOrEmpty(error))
		{
			string text = "";
			if (!string.IsNullOrEmpty(output))
			{
				text += output.Trim();
			}
			if (!string.IsNullOrEmpty(error))
			{
				if (text != "")
				{
					text += "\n";
				}
				text += error.Trim();
			}
			mainForm.Invoke(new Setter(delegate
			{
				mainForm.Dialogs.ShowInfo("Ctags", text);
				if (mainForm.LastFrame != null)
					mainForm.LastFrame.Focus();
			}));
			return;
		}
		mainForm.Invoke(new Setter(delegate
		{
			mainForm.Dialogs.HideInfo("Ctags", "Rebuilding…");
			mainForm.Ctags.NeedReload();
			if (mainForm.LastFrame != null)
				mainForm.LastFrame.Focus();
		}));
	}
	
	private void DoCtagsGoToDefinition(string text)
	{
		Buffer lastBuffer = mainForm.LastBuffer;
		if (lastBuffer == null)
		{
			return;
		}
		string word = lastBuffer.Controller.GetWord(
			lastBuffer.Controller.Lines.PlaceOf(lastBuffer.Controller.LastSelection.caret));
		if (string.IsNullOrEmpty(word))
		{
			return;
		}
		List<Ctags.Node> nodes = mainForm.Ctags.GetNodes(word);
		string currentDir = Directory.GetCurrentDirectory();
		Ctags.Node target = nodes.Count > 0 ? nodes[0] : null;
		foreach (Ctags.Node node in nodes)
		{
			if (Path.Combine(currentDir, node.path).ToLowerInvariant() == lastBuffer.FullPath.ToLowerInvariant())
			{
				target = node;
				break;
			}
		}
		if (target != null)
		{
			GoToTag(target);
		}
	}
	
	private void GoToTag(Ctags.Node node)
	{
		Buffer buffer = mainForm.LoadFile(node.path);
		if (buffer != null)
		{
			LineIterator iterator = buffer.Controller.Lines.GetLineRange(0, buffer.Controller.Lines.LinesCount);
			while (iterator.MoveNext())
			{
				string text = iterator.current.Text;
				if (text.StartsWith(node.address) &&
					(node.address.Length == text.Length ||
					node.address.Length == text.Length - 2 && text.EndsWith("\r\n") ||
					node.address.Length == text.Length - 1 && (text.EndsWith("\r") || text.EndsWith("\n"))))
				{
					buffer.Controller.PutCursor(new Place(0, iterator.Index), false);
					buffer.Controller.ViMoveHome(false, true);
					if (buffer.FullPath != null)
					{
						buffer.Controller.ViAddHistoryPosition(true);
					}
					buffer.Controller.NeedScrollToCaret();
					break;
				}
			}
		}
	}
}
