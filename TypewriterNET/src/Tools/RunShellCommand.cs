using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;

public class RunShellCommand
{
	public class Position
	{
		public string fileName;
		public Place place;

		public Position(string fileName, Place place)
		{
			this.fileName = fileName;
			this.place = place;
		}
	}

	private MainForm mainForm;

	public RunShellCommand(MainForm mainForm)
	{
		this.mainForm = mainForm;
	}

	private Buffer buffer;
	private Dictionary<int, Position> positions;

	public string Execute(string commandText, IRList<RegexData> regexList)
	{
		positions = new Dictionary<int, Position>();

		Process p = new Process();
		p.StartInfo.UseShellExecute = false;
		p.StartInfo.RedirectStandardOutput = true;
		p.StartInfo.FileName = "cmd.exe";
		p.StartInfo.Arguments = "/C " + commandText;
		p.Start();
		string output = p.StandardOutput.ReadToEnd();
		p.WaitForExit();

		buffer = new Buffer(null, "Shell command results");
		buffer.Controller.isReadonly = true;
		buffer.Controller.InitText(output);
		if (regexList != null)
		{
			List<StyleRange> ranges = new List<StyleRange>();
			foreach (RegexData regexData in regexList)
			{
				string currentDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;
				foreach (Match match in regexData.regex.Matches(output))
				{
					if (match.Groups.Count >= 3)
					{
						string path = match.Groups[1].Value;
						ranges.Add(new StyleRange(match.Groups[1].Index, match.Groups[1].Length, Ds.String.index));
						int iLine = 1;
						try
						{
							iLine = int.Parse(match.Groups[2].Value);
						}
						catch
						{
						}
						ranges.Add(new StyleRange(match.Groups[2].Index, match.Groups[2].Length, Ds.DecVal.index));
						int iChar = 1;
						if (match.Groups.Count >= 4)
						{
							try
							{
								iChar = int.Parse(match.Groups[3].Value);
							}
							catch
							{
							}
							ranges.Add(new StyleRange(match.Groups[3].Index, match.Groups[3].Length, Ds.DecVal.index));
						}
						Place place = buffer.Controller.Lines.PlaceOf(match.Index);
						positions[place.iLine] = new Position(path, new Place(iChar - 1, iLine - 1));
					}
				}
			}
			buffer.Controller.SetStyleRanges(ranges);
		}
		buffer.additionKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("F&ind\\Navigate to position", ExecuteEnter, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.None, null, action).SetDoubleClick(true));
		}
		{
			KeyAction action = new KeyAction("F&ind\\Close execution", CloseBuffer, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Escape, null, action));
		}
		mainForm.ShowBuffer(mainForm.ConsoleNest, buffer);
		if (mainForm.ConsoleNest.Frame != null)
			mainForm.ConsoleNest.Frame.Focus();
		return null;
	}

	public bool ExecuteEnter(Controller controller)
	{
		Place place = buffer.Controller.Lines.PlaceOf(buffer.Controller.LastSelection.caret);
		Position position;
		if (positions.TryGetValue(place.iLine, out position))
		{
			mainForm.NavigateTo(Path.GetFullPath(position.fileName), position.place, position.place);
			return true;
		}
		return false;
	}

	private bool CloseBuffer(Controller controller)
	{
		if (buffer != null && buffer.Frame != null)
			buffer.Frame.RemoveBuffer(buffer);
		return true;
	}
}
