using System;
using System.Collections.Generic;
using System.Text;
using MulticaretEditor;

public class Commander
{
	public class Arg
	{
		public readonly string name;
		public readonly string desc;
		public readonly bool needed;

		public Arg(string name, string desc, bool needed)
		{
			this.name = name;
			this.desc = desc;
			this.needed = needed;
		}
	}

	public class Command
	{
		public readonly string name;
		public readonly string desc;
		public readonly Setter<List<string>> execute;
		public readonly List<Arg> args;
		public readonly int neededArgsCount;

		public Command(string name, string desc, Setter<List<string>> execute, List<Arg> args)
		{
			this.name = name;
			this.desc = desc;
			this.execute = execute;
			this.args = args;
			int count = 0;
			for (int i = 0; i < args.Count; i++)
			{
				if (args[i].needed)
					count++;
			}
			neededArgsCount = count;
		}
	}

	private MainForm mainForm;
	private Settings settings;

	private readonly List<Command> commands = new List<Command>();

	public void Execute(string text)
	{
		string[] texts = text.Trim().Split(' ');
		string name = texts[0];
		int argsCount = texts.Length - 1;
		if (string.IsNullOrEmpty(name))
			return;
		Command command = null;
		foreach (Command commandI in commands)
		{
			if (commandI.name == name && commandI.neededArgsCount <= argsCount)
			{
				command = commandI;
				break;
			}
		}
		if (command != null)
		{
			List<string> args = new List<string>(texts);
			args.RemoveAt(0);
			command.execute(args);
		}
	}

	private List<Arg> Args(params Arg[] args)
	{
		return new List<Arg>(args);
	}

	public string GetHelpText()
	{
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("# Commands");
		builder.AppendLine();
		foreach (Command command in commands)
		{
			builder.Append(command.name);
			foreach (Arg arg in command.args)
			{
				if (arg.needed)
					builder.Append(" " + arg.name);
				else
					builder.Append(" [" + arg.name + "]");
			}
			builder.AppendLine();
			if (!string.IsNullOrEmpty(command.desc))
				builder.AppendLine(command.desc);
			foreach (Arg arg in command.args)
			{
				if (!string.IsNullOrEmpty(arg.desc))
				{
					if (arg.needed)
						builder.AppendLine("    " + arg.name + " - " + arg.desc);
					else
						builder.AppendLine("    [" + arg.name + " - " + arg.desc + "]");
				}
			}
			builder.AppendLine();
		}
		return builder.ToString();
	}

	public void Init(MainForm mainForm, Settings settings)
	{
		this.mainForm = mainForm;
		this.settings = settings;
		commands.Add(new Command("help", "Open tab with help text", DoHelp, Args()));
		commands.Add(new Command("set", "Change parameter", DoSet, Args(
			new Arg("name", "parameter name in settings", true),
			new Arg("value", "default value is true", false))));
	}

	private void DoHelp(List<string> args)
	{
		mainForm.ProcessHelp();
	}

	private void DoSet(List<string> args)
	{
		if (args.Count == 2)
		{
			settings[args[0]].Text = args[1];
			settings.DispatchChange();
		}
	}
}
