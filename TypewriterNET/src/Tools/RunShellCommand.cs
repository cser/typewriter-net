using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using MulticaretEditor;

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
	private List<Position> positions;
	private Dictionary<int, List<Position>> positionsOf;

	public void Execute(
		string commandText, bool showCommandInOutput, IRList<RegexData> regexList,
		bool stayTop, bool silentIfNoOutput, string parameters)
	{
		positions = new List<Position>();
		positionsOf = new Dictionary<int, List<Position>>();

		Encoding encoding = GetEncoding(mainForm, parameters);
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
		positionsOf = new Dictionary<int, List<Position>>();
		positions = new List<Position>();
		ShowInOutput(null, null, text, regexList, stayTop, silentIfNoOutput, parameters);
	}
	
	public static Encoding GetEncoding(MainForm mainForm, string parameters)
	{
		string rawEncoding = TryGetParameter(parameters, 'e');
		if (!string.IsNullOrEmpty(rawEncoding))
		{
			string error;
			EncodingPair newValue = EncodingPair.ParseEncoding(rawEncoding, out error);
			if (!newValue.IsNull)
			{
				return newValue.encoding;
			}
			if (mainForm.Dialogs != null)
			{
				mainForm.Dialogs.ShowInfo("Shell encoding", error + "");
			}
		}
		return mainForm.Settings.shellEncoding.Value.encoding ?? Encoding.UTF8;
	}
	
	public static string TryGetSyntax(string parameters)
	{
		return TryGetParameter(parameters, 's');
	}
	
	public static string TryGetParameter(string parameters, char symbol)
	{
		if (parameters != null)
		{
			int index = -1;
			int sublength = parameters.Length - 1;
			for (int i = 0; i < sublength; i++)
			{
				if (parameters[i] == symbol &&
					parameters[i + 1] == ':' &&
					(i == 0 || !char.IsLetterOrDigit(parameters[i - 1])))
				{
					index = i;
					break;
				}
			}
			if (index != -1)
			{
				int index2 = parameters.IndexOf(";", index);
				return index2 != -1 ?
					parameters.Substring(index + 2, index2 - index - 2) :
					parameters.Substring(index + 2);
			}
		}
		return null;
	}
	
	public static string CutParametersFromLeft(ref string commandText)
	{
		commandText = commandText.Trim();
		string parameters = "";
		if (commandText.StartsWith("{"))
		{
			int index = commandText.IndexOf("}");
			if (index != -1)
			{
				parameters = commandText.Substring(1, index - 1);
				commandText = commandText.Substring(index + 1);
			}
		}
		return parameters;
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
		buffer.encodingPair = new EncodingPair(GetEncoding(mainForm, parameters), false);
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
						positionsOf.TryGetValue(place.iLine, out list);
						if (list == null)
						{
							list = new List<Position>();
							positionsOf[place.iLine] = list;
						}
						Position position = new Position(path, new Place(iChar - 1, iLine - 1), shellStart, shellLength);
						list.Add(position);
						positions.Add(position);
					}
					else
					{
						string path = match.Groups[0].Value;
						int shellStart = match.Groups[0].Index;
						int shellLength = match.Groups[0].Length;
						ranges.Add(new StyleRange(shellStart, shellLength, Ds.String2.index));
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
			string syntax = TryGetSyntax(parameters);
			if (syntax != null)
			{
				buffer.customSyntax = syntax;
			}
		}
		mainForm.Ctags.SetGoToPositions(positions);
		mainForm.ShowConsoleBuffer(MainForm.ShellResultsId, buffer);
		mainForm.CheckFilesChanges();
	}

	private bool ExecuteEnter(Controller controller)
	{
		int caret = buffer.Controller.LastSelection.caret;
		Place place = buffer.Controller.Lines.PlaceOf(caret);
		List<Position> list;
		if (positionsOf.TryGetValue(place.iLine, out list))
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
			mainForm.Ctags.SetGoToPositions(positions);
			mainForm.Ctags.GoToTag(position);
			return true;
		}
		return false;
	}

	private static int ComparePositions(Position position0, Position position1)
	{
		return position0.shellStart - position1.shellStart;
	}
}
