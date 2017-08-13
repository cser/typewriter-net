using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using MulticaretEditor;

public struct CommandData
{
	public readonly string name;
	public readonly string sequence;

	public CommandData(string name, string sequence)
	{
		this.name = name;
		this.sequence = sequence;
	}
	
	public List<MacrosExecutor.Action> GetActions(StringBuilder errors)
	{
		List<MacrosExecutor.Action> actions = new List<MacrosExecutor.Action>();
		int state = 0;
		string specialText = "";
		foreach (char c in sequence)
		{
			if (state == 0)
			{
				if (c == '[')
				{
					state = 1;
				}
				else if (c == '\\')
				{
					state = 3;
				}
				else
				{
					actions.Add(new MacrosExecutor.Action(c));
				}
			}
			else if (state == 1)
			{
				if (c == ']')
				{
					actions.Add(GetSpecial(specialText, errors));
					specialText = "";
					state = 0;
				}
				else if (c == '\\')
				{
					state = 3;
				}
				else
				{
					specialText += c;
				}
			}
			else
			{
				actions.Add(new MacrosExecutor.Action(c));
				state = 0;
			}
		}
		MulticaretTextBox.initMacrosExecutor.Execute(actions);
		return actions;
	}
	
	private static Dictionary<string, MacrosExecutor.Action> specials;
	
	public static Dictionary<string, MacrosExecutor.Action> GetSpecials()
	{
		if (specials == null)
		{
			specials = new Dictionary<string, MacrosExecutor.Action>();
			specials["space"] = new MacrosExecutor.Action(' ');
			specials["escape"] = new MacrosExecutor.Action(Keys.Escape);
			specials["cr"] = new MacrosExecutor.Action(Keys.Enter);
			specials["tab"] = new MacrosExecutor.Action(Keys.Tab);
			specials["leader"] = new MacrosExecutor.Action('\\');
			specials["lt"] = new MacrosExecutor.Action('<');
			specials["gt"] = new MacrosExecutor.Action('>');
		}
		return specials;
	}
	
	private static MacrosExecutor.Action GetSpecial(string specialText, StringBuilder errors)
	{
		specialText = specialText.ToLowerInvariant();
		MacrosExecutor.Action action;
		if (GetSpecials().TryGetValue(specialText, out action))
		{
			return action;
		}
		if (specialText.StartsWith("C-"))
		{
			action = GetSpecial(specialText.Substring(2), errors);
			action.keys |= Keys.Control;
		}
		else
		{
			errors.Append("Unexpected: [" + specialText + "]");
		}
		return new MacrosExecutor.Action();
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
