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
		int length = sequence.Length;
		for (int i = 0; i < length;)
		{
			char c = sequence[i];
			if (c == '[')
			{
				string specialText = "";
				++i;
				if (i < length)
				{
					c = sequence[i];
					if ((c == 'C' || c == 'S') && i + 1 < length && sequence[i + 1] == '-')
					{
						i += 2;
						specialText += c + "-";
						if (i + 1 < length &&
							sequence[i] == (c == 'C' ? 'S' : 'C') &&
							sequence[i + 1] == '-')
						{
							specialText += (c == 'C' ? 'S' : 'C') + "-";
							i += 2;
						}
						if (i < length)
						{
							specialText += sequence[i];
						}
					}
					else
					{
						specialText += c;
					}
					for (++i; i < length; ++i)
					{
						if (sequence[i] == ']')
						{
							++i;
							break;
						}
						specialText += sequence[i];
					}
					actions.Add(GetSpecial(specialText, errors));
				}
			}
			else
			{
				actions.Add(new MacrosExecutor.Action(c));
				++i;
			}
		}
		if (errors.Length > 0)
		{
			errors.Append(" - sequence: " + sequence);
		}
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
			specials["bs"] = new MacrosExecutor.Action(Keys.Back);
			specials["del"] = new MacrosExecutor.Action(Keys.Delete);
			specials["tab"] = new MacrosExecutor.Action(Keys.Tab);
			specials["f1"] = new MacrosExecutor.Action(Keys.F1);
			specials["f2"] = new MacrosExecutor.Action(Keys.F2);
			specials["f3"] = new MacrosExecutor.Action(Keys.F3);
			specials["f4"] = new MacrosExecutor.Action(Keys.F4);
			specials["f5"] = new MacrosExecutor.Action(Keys.F5);
			specials["f6"] = new MacrosExecutor.Action(Keys.F6);
			specials["f7"] = new MacrosExecutor.Action(Keys.F7);
			specials["f8"] = new MacrosExecutor.Action(Keys.F8);
			specials["f9"] = new MacrosExecutor.Action(Keys.F9);
			specials["f11"] = new MacrosExecutor.Action(Keys.F11);
			specials["f12"] = new MacrosExecutor.Action(Keys.F12);
			specials["leader"] = new MacrosExecutor.Action('\\');
			specials["lt"] = new MacrosExecutor.Action('<');
			specials["gt"] = new MacrosExecutor.Action('>');
			specials["bra"] = new MacrosExecutor.Action('[');
			specials["ket"] = new MacrosExecutor.Action(']');
		}
		return specials;
	}
	
	private static MacrosExecutor.Action GetSpecial(string specialText, StringBuilder errors)
	{
		string lowerText = specialText.ToLowerInvariant();
		MacrosExecutor.Action action;
		if (GetSpecials().TryGetValue(lowerText, out action))
		{
			return action;
		}
		if ((lowerText.StartsWith("c-s-") || lowerText.StartsWith("s-c-")) && specialText.Length >= 5)
		{
			action = new MacrosExecutor.Action();
			action.keys = Keys.Control | Keys.Shift;
			if (specialText.Length == 5)
			{
				char c = specialText[4];
				Keys charKey = GetKey(c);
				if (charKey != Keys.None)
				{
					action.keys |= charKey;
				}
				else
				{
					if (GetSpecials().TryGetValue(lowerText.Substring(4), out action))
					{
						action.keys |= Keys.Control | Keys.Shift;
						return action;
					}
					errors.Append("Unsupported control: " + c);
				}
				return action;
			}
			if (GetSpecials().TryGetValue(lowerText.Substring(4), out action))
			{
				action.keys |= Keys.Control | Keys.Shift;
				return action;
			}
		}
		if ((lowerText.StartsWith("c-") || lowerText.StartsWith("s-")) && specialText.Length >= 3)
		{
			action = new MacrosExecutor.Action();
			action.keys = lowerText[0] == 'c' ? Keys.Control : Keys.Shift;
			if (specialText.Length == 3)
			{
				char c = specialText[2];
				Keys charKey = GetKey(c);
				if (charKey != Keys.None)
				{
					action.keys |= charKey;
				}
				else
				{
					errors.Append("Unsupported control: " + c);
				}
				return action;
			}
			if (GetSpecials().TryGetValue(lowerText.Substring(2), out action))
			{
				action.keys |= lowerText[0] == 'c' ? Keys.Control : Keys.Shift;
				return action;
			}
		}
		errors.Append("Unexpected: [" + specialText + "]");
		return new MacrosExecutor.Action();
	}
	
	private static Keys GetKey(char c)
	{
		switch (c)
		{
			case ']': return Keys.OemCloseBrackets;
			case '[': return Keys.OemOpenBrackets;
			case ';': return Keys.OemSemicolon;
			case ':': return Keys.OemSemicolon | Keys.Shift;
			case 'a': return Keys.A;
			case 'A': return Keys.A | Keys.Shift;
			case 'b': return Keys.B;
			case 'B': return Keys.B | Keys.Shift;
			case 'c': return Keys.C;
			case 'C': return Keys.C | Keys.Shift;
			case 'd': return Keys.D;
			case 'D': return Keys.D | Keys.Shift;
			case 'e': return Keys.E;
			case 'E': return Keys.E | Keys.Shift;
			case 'f': return Keys.F;
			case 'F': return Keys.F | Keys.Shift;
			case 'g': return Keys.G;
			case 'G': return Keys.G | Keys.Shift;
			case 'h': return Keys.H;
			case 'H': return Keys.H | Keys.Shift;
			case 'i': return Keys.I;
			case 'I': return Keys.I | Keys.Shift;
			case 'j': return Keys.J;
			case 'J': return Keys.J | Keys.Shift;
			case 'k': return Keys.K;
			case 'K': return Keys.K | Keys.Shift;
			case 'l': return Keys.L;
			case 'L': return Keys.L | Keys.Shift;
			case 'm': return Keys.M;
			case 'M': return Keys.M | Keys.Shift;
			case 'n': return Keys.N;
			case 'N': return Keys.N | Keys.Shift;
			case 'o': return Keys.O;
			case 'O': return Keys.O | Keys.Shift;
			case 'p': return Keys.P;
			case 'P': return Keys.P | Keys.Shift;
			case 'q': return Keys.Q;
			case 'Q': return Keys.Q | Keys.Shift;
			case 'r': return Keys.R;
			case 'R': return Keys.R | Keys.Shift;
			case 's': return Keys.S;
			case 'S': return Keys.S | Keys.Shift;
			case 't': return Keys.T;
			case 'T': return Keys.T | Keys.Shift;
			case 'u': return Keys.U;
			case 'U': return Keys.U | Keys.Shift;
			case 'v': return Keys.V;
			case 'V': return Keys.V | Keys.Shift;
			case 'w': return Keys.W;
			case 'W': return Keys.W | Keys.Shift;
			case 'x': return Keys.X;
			case 'X': return Keys.X | Keys.Shift;
			case 'y': return Keys.Y;
			case 'Y': return Keys.Y | Keys.Shift;
			case 'z': return Keys.Z;
			case 'Z': return Keys.Z | Keys.Shift;
			case '/': return Keys.OemQuestion;
			case '?': return Keys.OemQuestion | Keys.Shift;
			default:
				return Keys.None;
		}
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
