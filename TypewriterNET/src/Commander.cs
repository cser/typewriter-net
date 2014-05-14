using System;
using System.Collections.Generic;
using System.Text;
using MulticaretEditor;

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
	private Settings settings;

	private readonly List<Command> commands = new List<Command>();

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
		if (text == null)
			return;
		string args;
		string name = FirstWord(text, out args);
		if (name == "")
			return;
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
		else
		{
			mainForm.Dialogs.ShowInfo("Error", "Unknown command \"" + name + "\"");
		}
	}

	public string GetHelpText()
	{
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("# Commands");
		builder.AppendLine();
		foreach (Command command in commands)
		{
			builder.Append(command.name + (!string.IsNullOrEmpty(command.desc) ? " " + command.argNames : "") + "\n" +
				(!string.IsNullOrEmpty(command.desc) ? "  " + command.desc.Replace("\n", "\n  ") + "\n" : "") + "\n");
		}
		return builder.ToString();
	}

	public void Init(MainForm mainForm, Settings settings)
	{
		this.mainForm = mainForm;
		this.settings = settings;
		commands.Add(new Command("help", "", "Open tab with help text", DoHelp));
		commands.Add(new Command("exit", "", "Close window", DoExit));
		commands.Add(new Command("set", "name value", "Change parameter", DoSet));
		commands.Add(new Command("get", "name", "Show parameter", DoGet));
		commands.Add(new Command("lclear", "", "Clear editor log", DoClearLog));
		commands.Add(new Command("lopen", "", "Open editor log", DoOpenLog));
		commands.Add(new Command("lclose", "", "Close editor log", DoCloseLog));
	}

	private void DoHelp(string args)
	{
		mainForm.ProcessHelp();
	}

	private void DoExit(string args)
	{
		mainForm.Close();
	}

	private void DoSet(string args)
	{
		string value;
		string name = FirstWord(args, out value);
		if (settings[name] == null)
		{
			mainForm.Dialogs.ShowInfo("Error", "Missing property \"" + name + "\"");
			return;
		}
		settings[name].Text = value;
		settings.DispatchChange();
	}

	private void DoGet(string args)
	{
		string value;
		string name = FirstWord(args, out value);
		if (settings[name] == null)
		{
			mainForm.Dialogs.ShowInfo("Error", "Missing property \"" + name + "\"");
			return;
		}
		mainForm.Dialogs.ShowInfo("Value of \"" + name + "\"", settings[name].Text);
	}

	private void DoClearLog(string args)
	{
		mainForm.Log.Clear();
	}

	private void DoOpenLog(string args)
	{
		mainForm.Log.Open();
	}

	private void DoCloseLog(string args)
	{
		mainForm.Log.Close();
	}
}
