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

	private const int MaxPartChars = 100;

	private MainForm mainForm;
	private AlertForm alert;
	private List<Position> positions;
	private Buffer buffer;
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
		if (buffer != null)
			mainForm.ShowConsoleBuffer(MainForm.FindResultsId, buffer);
		return null;
	}
	
	private void OnCanceled()
	{
		isCanceled = true;
	}
	
	private bool isCanceled;
	private int tabSize;
	private Line line;
	private LineArray lines;
	
	private void NewLine()
	{
		if (line != null)
		{
			line.chars.Add(new Char('\n', 0));
			lines.AddLineUnsafely(line);
			line = null;
		}
	}
	
	private void AddText(string text, Ds ds)
	{
		if (line == null)
		{
			line = new Line();
			line.tabSize = tabSize;
		}
		short style = ds.index;
		for (int i = 0; i < text.Length; i++)
		{
			line.chars.Add(new Char(text[i], style));
		}
	}
	
	private void AddText(string text, Ds ds, int index, int length, Ds markDs)
	{
		if (line == null)
		{
			line = new Line();
			line.tabSize = buffer.Controller.Lines.tabSize;
		}
		short style = ds.index;
		if (index > text.Length)
			index = text.Length;
		for (int i = 0; i < index; i++)
		{
			line.chars.Add(new Char(text[i], style));
		}
		short markStyle = markDs.index;
		int index2 = index + length;
		if (index2 > text.Length)
			index2 = text.Length;
		for (int i = index; i < index2; i++)
		{
			line.chars.Add(new Char(text[i], markStyle));
		}
		for (int i = index2; i < text.Length; i++)
		{
			line.chars.Add(new Char(text[i], style));
		}
	}

	private string Search(string directory, Regex regex, string pattern, bool ignoreCase, string filter)
	{
		positions = new List<Position>();

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
		catch (System.Exception e)
		{
			return "Error: File list reading error: " + e.Message;
		}
		buffer = new Buffer(null, "Find in files results", SettingsMode.Normal);
		buffer.showEncoding = false;
		buffer.Controller.isReadonly = true;
		lines = buffer.Controller.Lines;
		lines.ClearAllUnsafely();
		string currentDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;
		List<IndexAndLength> indices = new List<IndexAndLength>();
		CompareInfo ci = ignoreCase ? CultureInfo.InvariantCulture.CompareInfo : null;
		int matchesCount = 0;
		foreach (string file in files)
		{
			if (isCanceled)
			{
				NewLine();
				AddText("…", Ds.Normal);
				NewLine();
				AddText("STOPPED", Ds.Error);
				break;
			}
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
				
				++matchesCount;
				if (matchesCount > 200000)
				{
					NewLine();
					AddText("…", Ds.Normal);
					NewLine();
					AddText("TOO MANY MATCHES", Ds.Error);
					isCanceled = true;
					break;
				}

				NewLine();
				AddText(path, Ds.String);
				AddText("|", Ds.Operator);
				AddText((currentLineIndex + 1) + " " + (index - offset + 1), Ds.DecVal);
				AddText("|", Ds.Operator);

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
					if (whitespaceLength > 0 && whitespaceLength <= (index - offset))
						trimOffset = whitespaceLength;
				}
				string line = text.Substring(offset + trimOffset, lineLength - trimOffset - rightTrimOffset);
				AddText(line, Ds.Normal, index - offset - trimOffset, length, Ds.Keyword);
				positions.Add(new Position(file, index, index + length));
			}
		}
		NewLine();
		buffer.additionKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("F&ind\\Navigate to finded", ExecuteEnter, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.None, null, action).SetDoubleClick(true));
		}
		mainForm.Invoke(new Setter(CloseAlert));
		return null;
	}
	
	private void CloseAlert()
	{
		alert.forcedClosing = true;
		alert.Close();
	}

	private bool ExecuteEnter(Controller controller)
	{
		Place place = controller.Lines.PlaceOf(controller.LastSelection.anchor);
		Position position = positions[place.iLine];
		mainForm.NavigateTo(Path.GetFullPath(position.fileName), position.position0, position.position1);
		return true;
	}
}
