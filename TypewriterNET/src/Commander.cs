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
		if (string.IsNullOrEmpty(text))
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
		else if (settings[name] != null)
		{
			if (args != "")
			{
				string errors = settings[name].SetText(args);
				settings.DispatchChange();
				if (!string.IsNullOrEmpty(errors))
					mainForm.Dialogs.ShowInfo("Error assign of \"" + name + "\"", errors);
			}
			else
			{
				mainForm.Dialogs.ShowInfo("Value of \"" + name + "\"", settings[name].Text);
			}
		}
		else
		{
			mainForm.Dialogs.ShowInfo("Error", "Unknown command/property \"" + name + "\"");
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
		commands.Add(new Command("lclear", "", "Clear editor log", DoClearLog));
		commands.Add(new Command("lopen", "", "Open editor log", DoOpenLog));
		commands.Add(new Command("lclose", "", "Close editor log", DoCloseLog));
		commands.Add(new Command("reset", "name", "Reset property", DoResetProperty));
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

	private void DoOpenLog(string args)
	{
		mainForm.Log.Open();
	}

	private void DoCloseLog(string args)
	{
		mainForm.Log.Close();
	}

	private void DoResetProperty(string args)
	{
		if (settings[args] != null)
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
}
