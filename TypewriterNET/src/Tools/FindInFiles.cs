using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Forms;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;

public class FindInFiles
{
	public struct Position
	{
		public readonly string fileName;
		public readonly int position0;
		public readonly int position1;

		public Position(string fileName, int position0, int position1)
		{
			this.fileName = fileName;
			this.position0 = position0;
			this.position1 = position1;
		}
	}

	public struct IndexAndLength
	{
		public int index;
		public int length;

		public IndexAndLength(int index, int length)
		{
			this.index = index;
			this.length = length;
		}
	}

	private MainForm mainForm;
	private List<Position> positions;

	public FindInFiles(MainForm mainForm)
	{
		this.mainForm = mainForm;
	}

	public string Execute(string regexText, string directory, string filter)
	{
		Regex regex = null;
		string pattern = null;
		if (regexText.Length > 2 && regexText[0] == '/' && regexText.LastIndexOf("/") > 1)
		{
			RegexOptions options = RegexOptions.CultureInvariant | RegexOptions.Multiline;
			int lastIndex = regexText.LastIndexOf("/");
			string optionsText = regexText.Substring(lastIndex + 1);
			string rawRegex = regexText.Substring(1, lastIndex - 1);
			for (int i = 0; i < optionsText.Length; i++)
			{
				char c = optionsText[i];
				if (c == 'i')
					options |= RegexOptions.IgnoreCase;
				else if (c == 's')
					options &= ~RegexOptions.Multiline;
				else if (c == 'e')
					options |= RegexOptions.ExplicitCapture;
				else
				{
					return "Error: Unsupported regex option: " + c;
				}
			}
			try
			{
				regex = new Regex(rawRegex, options);
			}
			catch (Exception e)
			{
				return "Error: Incorrect regex: " + regexText + " - " + e.Message;
			}
		}
		else
		{
			pattern = regexText;
		}
		return Search(directory, regex, pattern, filter);
	}

	private string GetArg(string[] args, int i)
	{
		if (i >= args.Length)
			return null;
		string arg = args[i];
		if (arg.Length > 2 && arg[0] == '"' && arg[arg.Length - 1] == '"')
			return arg.Substring(1, arg.Length - 2);
		return arg;
	}

	private string Search(string directory, Regex regex, string pattern, string filter)
	{
		positions = new List<Position>();

		StringBuilder builder = new StringBuilder();
		bool needCutCurrent = false;
		if (string.IsNullOrEmpty(filter))
			filter = "*";
		if (string.IsNullOrEmpty(directory))
		{
			directory = Directory.GetCurrentDirectory();
			needCutCurrent = true;
		}
		string[] files = null;
		try
		{
			files = Directory.GetFiles(directory, filter, SearchOption.AllDirectories);
		}
		catch (Exception e)
		{
			return "Error: File list reading error: " + e.Message;
		}
		string currentDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;
		List<IndexAndLength> indices = new List<IndexAndLength>();
		foreach (string file in files)
		{
			string text = File.ReadAllText(file);
			indices.Clear();
			if (regex != null)
			{
				MatchCollection matches = regex.Matches(text);
				if (matches.Count == 0)
					continue;
				foreach (Match match in matches)
				{
					indices.Add(new IndexAndLength(match.Index, match.Length));
				}
			}
			else
			{
				int index = text.IndexOf(pattern);
				if (index == -1)
					continue;
				while (true)
				{
					indices.Add(new IndexAndLength(index, pattern.Length));
					index = text.IndexOf(pattern, index + 1);
					if (index == -1)
						break;
				}
			}
			string path = file;
			if (needCutCurrent && path.StartsWith(currentDirectory))
				path = file.Substring(currentDirectory.Length);
			int offset = 0;
			int currentLineIndex = 0;
			foreach (IndexAndLength indexAndLength in indices)
			{
				int index = indexAndLength.index;
				int length = indexAndLength.length;
				int lineEnd = -1;
				while (true)
				{
					int nIndex = text.IndexOf('\n', offset);
					int rIndex = text.IndexOf('\r', offset);
					if (nIndex == -1 && rIndex == -1)
					{
						lineEnd = text.Length;
						break;
					}
					int nrIndex = Math.Min(nIndex, rIndex);
					if (nrIndex == -1)
						nrIndex = nIndex != -1 ? nIndex : rIndex;
					if (nrIndex > index)
					{
						lineEnd = nrIndex;
						break;
					}
					currentLineIndex++;
					if (nrIndex == nIndex)
					{
						offset = nIndex + 1;
					}
					else
					{
						if (rIndex + 1 < text.Length || text[rIndex + 1] == '\n')
							offset = rIndex + 2;
						else
							offset = rIndex + 1;
					}
				}
				builder.AppendLine(
					path + "(" + (currentLineIndex + 1) + "," + (index - offset + 1) + "): " + text.Substring(offset, lineEnd - offset));
				positions.Add(new Position(file, index, index + length));
			}
		}

		Buffer buffer = new Buffer(null, "Find results");
		buffer.Controller.isReadonly = true;
		buffer.Controller.InitText(builder.ToString());
		buffer.additionKeyMap = new KeyMap();
		KeyAction action = new KeyAction("F&ind\\Navigate to finded", ExecuteEnter, null, false);
		buffer.additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
		buffer.additionKeyMap.AddItem(new KeyItem(Keys.None, null, action).SetDoubleClick(true));
		mainForm.ShowBuffer(mainForm.ConsoleNest, buffer);
		if (mainForm.ConsoleNest.Frame != null)
			mainForm.ConsoleNest.Frame.Focus();
		return null;
	}

	public bool ExecuteEnter(Controller controller)
	{
		Place place = controller.Lines.PlaceOf(controller.LastSelection.anchor);
		Position position = positions[place.iLine];
		mainForm.NavigateTo(Path.GetFullPath(position.fileName), position.position0, position.position1);
		return true;
	}
}
