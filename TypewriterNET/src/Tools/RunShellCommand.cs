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
		public int shellStart;
		public int shellLength;

		public Position(string fileName, Place place, int shellStart, int shellLength)
		{
			this.fileName = fileName;
			this.place = place;
			this.shellStart = shellStart;
			this.shellLength = shellLength;
		}
	}

	public const string FileVar = "%f%";
	public const string FileNameVar = "%n%";
	public const string FileVarSoftly = "%f?%";
	public const string FileDirVar = "%d%";
	public const string LineVar = "%l%";
	public const string CharVar = "%c%";
	public const string SelectedVar = "%s%";
	public const string WordVar = "%w%";
	public const string AppDataDirVar = "%a%";

	private MainForm mainForm;

	public RunShellCommand(MainForm mainForm)
	{
		this.mainForm = mainForm;
	}

	private Buffer buffer;
	private Dictionary<int, List<Position>> positions;

	public void Execute(
		string commandText, bool showCommandInOutput, IRList<RegexData> regexList,
		bool stayTop, bool silentIfNoOutput, string parameters)
	{
		positions = new Dictionary<int, List<Position>>();

		Encoding encoding = GetEncoding(parameters);
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
		string text = (showCommandInOutput ? ">> " + commandText + "\n" + output : output);
		p.WaitForExit();
		ShowInOutput(output, errors, text, regexList, stayTop, silentIfNoOutput, parameters);
	}
	
	public void ShowInOutput(string text, IRList<RegexData> regexList, bool stayTop, bool silentIfNoOutput, string parameters)
	{
		positions = new Dictionary<int, List<Position>>();
		ShowInOutput(null, null, text, regexList, stayTop, silentIfNoOutput, parameters);
	}
	
	private Encoding GetEncoding(string parameters)
	{
		if (!string.IsNullOrEmpty(parameters))
		{
			int index = parameters.IndexOf("e:");
			if (index != -1)
			{
				int index2 = parameters.IndexOf(";", index);
				string rawEncoding = index2 != -1 ?
					parameters.Substring(index + 2, index2 - index - 2) :
					parameters.Substring(index + 2);
				string error;
				EncodingPair newValue = EncodingPair.ParseEncoding(rawEncoding, out error);
				if (!newValue.IsNull)
				{
					return newValue.encoding;
				}
				if (mainForm.Dialogs != null)
				{
					mainForm.Dialogs.ShowInfo("Shell encoding", error);
				}
			}
		}
		return mainForm.Settings.shellEncoding.Value.encoding ?? Encoding.UTF8;
	}
	
	private void ShowInOutput(
		string output, string errors, string text, IRList<RegexData> regexList,
		bool stayTop, bool silentIfNoOutput, string parameters)
	{
		List<StyleRange> ranges = new List<StyleRange>();
		if (!string.IsNullOrEmpty(errors))
		{
			string left = !string.IsNullOrEmpty(output) ? output + "\n" : output;
			text = left + errors;
			ranges.Add(new StyleRange(left.Length, errors.Length, Ds.Error.index));
		}
		if (string.IsNullOrEmpty(text) && silentIfNoOutput)
		{
			mainForm.CloseConsoleBuffer(MainForm.ShellResultsId);
			mainForm.CheckFilesChanges();
			return;
		}

		buffer = new Buffer(null, "Shell command results", SettingsMode.Normal);
		buffer.Controller.isReadonly = true;
		buffer.Controller.InitText(text);
		buffer.encodingPair = new EncodingPair(GetEncoding(parameters), false);
		if (regexList != null)
		{
			foreach (RegexData regexData in regexList)
			{
				string currentDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;
				foreach (Match match in regexData.regex.Matches(text))
				{
					if (match.Groups.Count >= 2)
					{
						string path = match.Groups[1].Value;
						int shellStart = match.Groups[1].Index;
						int shellLength = match.Groups[1].Length;
						ranges.Add(new StyleRange(shellStart, shellLength, Ds.String.index));
						int iLine = 1;
						if (match.Groups.Count >= 3)
						{
							try
							{
								iLine = int.Parse(match.Groups[2].Value);
							}
							catch
							{
							}
							int start = match.Groups[2].Index;
							int length = match.Groups[2].Length;
							ranges.Add(new StyleRange(start, length, Ds.DecVal.index));
							shellLength = Math.Max(shellStart + shellLength, start + length) - shellStart;
						}
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
							int start = match.Groups[3].Index;
							int length = match.Groups[3].Length;
							ranges.Add(new StyleRange(start, length, Ds.DecVal.index));
							shellLength = Math.Max(shellStart + shellLength, start + length) - shellStart;
						}
						Place place = buffer.Controller.Lines.PlaceOf(match.Index);
						List<Position> list;
						positions.TryGetValue(place.iLine, out list);
						if (list == null)
						{
							list = new List<Position>();
							positions[place.iLine] = list;
						}
						list.Add(new Position(path, new Place(iChar - 1, iLine - 1), shellStart, shellLength));
					}
				}
			}
		}
		buffer.Controller.SetStyleRanges(ranges);
		buffer.additionKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("F&ind\\Navigate to position", ExecuteEnter, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.None, null, action).SetDoubleClick(true));
		}
		if (stayTop)
		{
			buffer.Controller.DocumentStart(false);
		}
		else
		{
			buffer.Controller.DocumentEnd(false);
		}
		buffer.Controller.NeedScrollToCaret();
		if (!string.IsNullOrEmpty(parameters))
		{
			int index = parameters.IndexOf("s:");
			if (index != -1)
			{
				int index2 = parameters.IndexOf(";", index);
				buffer.customSyntax = index2 != -1 ?
					parameters.Substring(index + 2, index2 - index - 2) :
					parameters.Substring(index + 2);
			}
		}
		mainForm.ShowConsoleBuffer(MainForm.ShellResultsId, buffer);
		mainForm.CheckFilesChanges();
	}

	private bool ExecuteEnter(Controller controller)
	{
		int caret = buffer.Controller.LastSelection.caret;
		Place place = buffer.Controller.Lines.PlaceOf(caret);
		List<Position> list;
		if (positions.TryGetValue(place.iLine, out list))
		{
			list.Sort(ComparePositions);
			Position position = list[0];
			for (int i = 0; i < list.Count; i++)
			{
				Position positionI = list[i];
				int index = caret - positionI.shellStart;
				if (index >= 0 && index < positionI.shellLength)
				{
					position = positionI;
					break;
				}
			}
			if (string.IsNullOrEmpty(position.fileName) || position.fileName.Trim() == "")
			{
				mainForm.NavigateTo(position.place, position.place);
				return true;
			}
			string fullPath = null;
			try
			{
				fullPath = Path.GetFullPath(position.fileName);
			}
			catch
			{
				mainForm.Dialogs.ShowInfo("Error", "Incorrect path: " + position.fileName);
				return true;
			}
			mainForm.NavigateTo(fullPath, position.place, position.place);
			return true;
		}
		return false;
	}

	private static int ComparePositions(Position position0, Position position1)
	{
		return position0.shellStart - position1.shellStart;
	}
}
