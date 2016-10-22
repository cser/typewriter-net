using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;

public class FindInFiles
{
	public struct Position
	{
		public readonly string fileName;
		public readonly Place place;
		public readonly int length;

		public Position(string fileName, Place place, int length)
		{
			this.fileName = fileName;
			this.place = place;
			this.length = length;
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

	private const int MaxPartChars = 100;

	private MainForm mainForm;
	private AlertForm alert;
	private List<Position> positions;
	private Buffer buffer;
	private Buffer finishBuffer;
	private Thread thread;

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
		alert = new AlertForm(mainForm, OnCanceled);
		
		tabSize = mainForm.Settings.tabSize.Value;
		thread = new Thread(
			new ThreadStart(delegate()
			{
				Search(directory, regex, pattern, findParams.ignoreCase, filter);
			})
		);
		thread.Start();
				
		alert.ShowDialog(mainForm);
		if (finishBuffer != null)
			mainForm.ShowConsoleBuffer(MainForm.FindResultsId, finishBuffer);
		return null;
	}
	
	private bool OnCanceled()
	{
		isStopped = true;
		if (!fsScanComplete)
		{
			thread.Abort();
			Buffer buffer = new Buffer(null, "Find in files results", SettingsMode.Normal);
			buffer.showEncoding = false;
			buffer.Controller.isReadonly = true;
			buffer.Controller.Lines.ClearAllUnsafely();
			{
				string text = "FILE SYSTEM SCANNING STOPPED";
				Line line = new Line();
				line.tabSize = tabSize;
				line.chars.Capacity = text.Length + 1;
				short style = Ds.Error.index;
				for (int i = 0; i < text.Length; i++)
				{
					line.chars.Add(new Char(text[i], style));
				}
				line.chars.Add(new Char('\n', 0));
				buffer.Controller.Lines.AddLineUnsafely(line);
			}
			finishBuffer = buffer;
			return true;
		}
		return false;
	}
	
	private bool isStopped;
	private bool fsScanComplete;
	private int tabSize;
	private LineArray lines;
	
	private void AddLine(string text, Ds ds)
	{
		Line line = new Line();
		line.tabSize = tabSize;
		line.chars.Capacity = text.Length + 1;
		short style = ds.index;
		for (int i = 0; i < text.Length; i++)
		{
			line.chars.Add(new Char(text[i], style));
		}
		line.chars.Add(new Char('\n', 0));
		lines.AddLineUnsafely(line);
	}
	
	private void AddLine(string path, string position, string text, int lineIndex, int lineLength, int sublineIndex, int sublineLength)
	{
		Line line = new Line();
		line.tabSize = tabSize;
		line.chars.Capacity = path.Length + 1 + position.Length + 1 + lineLength + 1;
		short style;
		style = Ds.String.index;
		for (int i = 0; i < path.Length; i++)
		{
			line.chars.Add(new Char(path[i], style));
		}
		line.chars.Add(new Char('|', Ds.Operator.index));
		style = Ds.DecVal.index;
		for (int i = 0; i < position.Length; i++)
		{
			line.chars.Add(new Char(position[i], style));
		}
		line.chars.Add(new Char('|', Ds.Operator.index));
		style = Ds.Keyword.index;
		int index0 = lineIndex;
		int index1 = lineIndex + sublineIndex;
		int index2 = lineIndex + sublineIndex + sublineLength;
		int index3 = lineIndex + lineLength;
		for (int i = index0; i < index1; i++)
		{
			line.chars.Add(new Char(text[i], 0));
		}
		for (int i = index1; i < index2; i++)
		{
			line.chars.Add(new Char(text[i], style));
		}
		for (int i = index2; i < index3; i++)
		{
			line.chars.Add(new Char(text[i], 0));
		}
		line.chars.Add(new Char('\n', 0));
		lines.AddLineUnsafely(line);
	}

	private void Search(string directory, Regex regex, string pattern, bool ignoreCase, string filter)
	{
		positions = new List<Position>();
		buffer = new Buffer(null, "Find in files results", SettingsMode.Normal);
		buffer.showEncoding = false;
		buffer.Controller.isReadonly = true;
		lines = buffer.Controller.Lines;
		lines.ClearAllUnsafely();
		
		bool needCutCurrent = false;
		FileNameFilter hardFilter = null;
		if (string.IsNullOrEmpty(filter))
		{
			filter = "*";
		}
		else if (filter.Contains(";"))
		{
			hardFilter = new FileNameFilter(filter);
			filter = "*";
		}
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
		catch (System.Exception e)
		{
			AddLine("Error: File list reading error: " + e.Message, Ds.Error);
			files = new string[0];
		}
		fsScanComplete = true;
		
		string currentDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;
		List<IndexAndLength> indices = new List<IndexAndLength>();
		CompareInfo ci = ignoreCase ? CultureInfo.InvariantCulture.CompareInfo : null;
		int remainsMatchesCount = 2000000;
		string stopReason = null;
		foreach (string file in files)
		{
			if (isStopped)
			{
				if (stopReason == null)
					stopReason = "STOPPED";
				break;
			}
			if (hardFilter != null)
			{
				string name = Path.GetFileName(file);
				if (!hardFilter.Match(name))
					continue;
			}
			string text = null;
			try
			{
				text = File.ReadAllText(file);
			}
			catch (IOException e)
			{
				--remainsMatchesCount;
				if (remainsMatchesCount < 0)
				{
					isStopped = true;
					stopReason = "TOO MANY LINES";
					break;
				}
				AddLine(file + ": " + e.Message, Ds.Error);
				continue;
			}
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
					int nrIndex = System.Math.Min(nIndex, rIndex);
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
				
				--remainsMatchesCount;
				if (remainsMatchesCount < 0)
				{
					isStopped = true;
					stopReason =  "TOO MANY MATCHES";
					break;
				}

				int trimOffset = 0;
				int rightTrimOffset = 0;
				int lineLength = lineEnd - offset;
				if (index - offset - MaxPartChars > 0)
					trimOffset = index - offset - MaxPartChars;
				if (lineLength - index + offset - length - MaxPartChars > 0)
					rightTrimOffset = lineLength - index + offset - length - MaxPartChars;
				if (trimOffset == 0)
				{
					int whitespaceLength = CommonHelper.GetFirstSpaces(text, offset, lineLength - rightTrimOffset);
					if (whitespaceLength > 0 && whitespaceLength <= index - offset)
						trimOffset = whitespaceLength;
				}
				positions.Add(new Position(file, new Place(index - offset, currentLineIndex), length));
				
				int index0 = offset + trimOffset;
				int length0 = lineLength - trimOffset - rightTrimOffset;
				AddLine(path, (currentLineIndex + 1) + " " + (index - offset + 1), text, index0, length0, index - offset - trimOffset, length);
			}
		}
		if (isStopped)
		{
			AddLine("…", Ds.Normal);
			AddLine(stopReason, Ds.Error);
		}
		if (lines.LinesCount == 0)
		{
			lines.AddLineUnsafely(new Line());
		}
		else
		{
			lines.CutLastLineBreakUnsafely();
		}
		buffer.additionKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("F&ind\\Navigate to finded", ExecuteEnter, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.None, null, action).SetDoubleClick(true));
		}
		finishBuffer = buffer;
		mainForm.Invoke(new Setter(CloseAlert));
	}
	
	private void CloseAlert()
	{
		alert.forcedClosing = true;
		alert.Close();
	}

	private bool ExecuteEnter(Controller controller)
	{
		if (positions.Count == 0)
			return true;
		Place place = controller.Lines.PlaceOf(controller.LastSelection.anchor);
		Position position = positions[place.iLine];
		mainForm.NavigateTo(Path.GetFullPath(position.fileName), position.place, position.length);
		return true;
	}
}
