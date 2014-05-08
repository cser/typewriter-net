using System;
using System.Collections.Generic;
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
		public readonly List<Arg> argsDescs;
		public readonly int neededArgsCount;

		public Command(string name, string desc, Setter<List<string>> execute, List<Arg> argsDescs)
		{
			this.name = name;
			this.desc = desc;
			this.execute = execute;
			this.argsDescs = argsDescs;
			int count = 0;
			for (int i = 0; i < argsDescs.Count; i++)
			{
				if (argsDescs[i].needed)
					count++;
			}
			neededArgsCount = count;
		}
	}

	private MainForm mainForm;

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

	public void Init(MainForm mainForm)
	{
		this.mainForm = mainForm;
		commands.Add(new Command("help", "open tab with help text", DoHelp, Args()));
	}

	private void DoHelp(List<string> args)
	{
		mainForm.ProcessHelp();
	}
}
