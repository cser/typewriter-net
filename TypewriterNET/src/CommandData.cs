using System;
using System.Text.RegularExpressions;

public struct CommandData
{
	public readonly string name;
	public readonly string sequence;

	public CommandData(string name, string sequence)
	{
		this.name = name;
		this.sequence = sequence;
	}

	public static CommandData Parse(string raw, out string errors)
	{
		errors = null;
		string name = null;
		string sequence = null;
		int index = raw.IndexOf("|");
		if (index != -1)
		{
			name = raw.Substring(0, index).Trim();
			sequence = raw.Substring(index + 1).Trim();
		}
		if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(sequence))
		{
			errors = "Expected 2 values, separated by \"|\": \"commandName|name\"";
			return new CommandData(null, null);
		}
		return new CommandData(name, sequence);
	}
}
