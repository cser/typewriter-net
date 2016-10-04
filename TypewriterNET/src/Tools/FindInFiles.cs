using System;
using System.Globalization;
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

	private const int MaxPartChars = 150;

	private MainForm mainForm;
	private List<Position> positions;
	Buffer buffer;

	public FindInFiles(MainForm mainForm)
	{
		this.mainForm = mainForm;
	}

	public string Execute(string regexText, FindParams findParams, string directory, string filter)
	{
		if (string.IsNullOrEmpty(regexText))
			return null;
		Regex regex = null;
		string pattern = null;
		if (findParams.regex)
		{
			string error;
			regex = DialogManager.ParseRegex(regexText, out error);
			if (regex == null || error != null)
				return "Error: " + error;
		}
		else
		{
			pattern = regexText;
		}
		return Search(directory, regex, pattern, findParams.ignoreCase, filter);
	}

	private string Search(string directory, Regex regex, string pattern, bool ignoreCase, string filter)
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
		bool first = true;
		List<StyleRange> ranges = new List<StyleRange>();
		string currentDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;
		List<IndexAndLength> indices = new List<IndexAndLength>();
		CompareInfo ci = ignoreCase ? CultureInfo.InvariantCulture.CompareInfo : null;
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
				int index = ci != null ?  ci.IndexOf(text, pattern, CompareOptions.IgnoreCase) : text.IndexOf(pattern);
				if (index == -1)
					continue;
				while (true)
				{
					indices.Add(new IndexAndLength(index, pattern.Length));
					index = ci != null ?  ci.IndexOf(text, pattern, index + 1, CompareOptions.IgnoreCase) : text.IndexOf(pattern, index + 1);
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
				if (!first)
					builder.AppendLine();
				first = false;

				ranges.Add(new StyleRange(builder.Length, path.Length, Ds.String.index));
				builder.Append(path);
				ranges.Add(new StyleRange(builder.Length, 1, Ds.Operator.index));
				builder.Append("|");

				string lineNumber = (currentLineIndex + 1) + "";
				ranges.Add(new StyleRange(builder.Length, lineNumber.Length, Ds.DecVal.index));
				builder.Append(lineNumber);

				builder.Append(" ");

				string charNumber = (index - offset + 1) + "";
				ranges.Add(new StyleRange(builder.Length, charNumber.Length, Ds.DecVal.index));
				builder.Append(charNumber);

				ranges.Add(new StyleRange(builder.Length, 1, Ds.Operator.index));
				builder.Append("| ");

				int trimOffset = 0;
				int rightTrimOffset = 0;
				int lineLength = lineEnd - offset;
				if (index - offset - MaxPartChars > 0)
					trimOffset = index - offset - MaxPartChars;
				if (lineLength - index + offset - length - MaxPartChars > 0)
					rightTrimOffset = lineLength - index + offset - length - MaxPartChars;
				string line = text.Substring(offset, lineLength);
				if (trimOffset == 0)
				{
					int whitespaceLength = CommonHelper.GetFirstSpaces(line);
					if (whitespaceLength > 0 && whitespaceLength <= (index - offset))
						trimOffset = whitespaceLength;
				}
				ranges.Add(new StyleRange(builder.Length + index - offset - trimOffset, length, Ds.Keyword.index));
				builder.Append(line, trimOffset, line.Length - trimOffset - rightTrimOffset);
				positions.Add(new Position(file, index, index + length));
			}
		}

		buffer = new Buffer(null, "Find in files results", SettingsMode.Normal);
		buffer.showEncoding = false;
		buffer.Controller.isReadonly = true;
		buffer.Controller.InitText(builder.ToString());
		buffer.Controller.SetStyleRanges(ranges);
		buffer.additionKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("F&ind\\Navigate to finded", ExecuteEnter, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.None, null, action).SetDoubleClick(true));
		}
		mainForm.ShowConsoleBuffer(MainForm.FindResultsId, buffer);
		return null;
	}

	private bool ExecuteEnter(Controller controller)
	{
		Place place = controller.Lines.PlaceOf(controller.LastSelection.anchor);
		Position position = positions[place.iLine];
		mainForm.NavigateTo(Path.GetFullPath(position.fileName), position.position0, position.position1);
		return true;
	}
}
