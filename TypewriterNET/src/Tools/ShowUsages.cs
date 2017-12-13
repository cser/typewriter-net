using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using MulticaretEditor;

public class ShowUsages
{
	private readonly MainForm mainForm;
	private List<Position> positions;
	private Buffer buffer;

	public ShowUsages(MainForm mainForm)
	{
		this.mainForm = mainForm;
	}

	public string Execute(List<Usage> usages, string word)
	{
		positions = new List<Position>();
		StringBuilder builder = new StringBuilder();
		List<StyleRange> ranges = new List<StyleRange>();
		int maxLength = 0;
		int maxPlaceLength = 0;
		foreach (Usage usage in usages)
		{
			string fileName = Path.GetFileName(usage.FileName);
			if (maxLength < fileName.Length)
			{
				maxLength = fileName.Length;
			}
			string place = usage.Line + " " + usage.Column;
			if (maxPlaceLength < place.Length)
				maxPlaceLength = place.Length;
		}
		foreach (Usage usage in usages)
		{
			if (builder.Length > 0)
			{
				builder.Append(mainForm.Settings.lineBreak.Value);
			}
			string fileName = Path.GetFileName(usage.FileName);
			ranges.Add(new StyleRange(builder.Length, maxLength, Ds.String.index));
			builder.Append(fileName.PadRight(maxLength));
			string place = usage.Line + " " + usage.Column;
			ranges.Add(new StyleRange(builder.Length, 1, Ds.Operator.index));
			builder.Append('|');
			ranges.Add(new StyleRange(builder.Length, maxPlaceLength, Ds.DecVal.index));
			builder.Append(place.PadRight(maxPlaceLength));
			ranges.Add(new StyleRange(builder.Length, 1, Ds.Operator.index));
			builder.Append('|');
			int spaces = 0;
			while (spaces < usage.Text.Length)
			{
				char c = usage.Text[spaces];
				if (c != '\t' && c != ' ')
					break;
				spaces++;
			}
			string text = usage.Text.Trim();
			if (!text.StartsWith("..."))
			{
				int index = usage.Column - spaces - 1;
				if (index < text.Length)
				{
					int length = word.Length;
					if (length > text.Length - index)
					{
						length = text.Length - index;
					}
					if (length > 0)
					{
						ranges.Add(new StyleRange(builder.Length + index, length, Ds.Keyword.index));
					}
				}
			}
			positions.Add(new Position(usage.FileName, new Place(usage.Column - 1, usage.Line - 1), word.Length));
			builder.Append(text);
		}

		buffer = new Buffer(null, "Usages", SettingsMode.Normal);
		buffer.showEncoding = false;
		buffer.Controller.isReadonly = true;
		buffer.Controller.InitText(builder.ToString());
		buffer.Controller.SetStyleRanges(ranges);
		buffer.additionKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("F&ind\\Navigate to found", ExecuteEnter, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.None, null, action).SetDoubleClick(true));
		}
		mainForm.Ctags.SetGoToPositions(positions);
		mainForm.ShowConsoleBuffer(MainForm.FindResultsId, buffer);
		return null;
	}

	private bool ExecuteEnter(Controller controller)
	{
		Place place = controller.Lines.PlaceOf(controller.LastSelection.anchor);
		if (place.iLine >= 0 && place.iLine < positions.Count)
		{
			mainForm.Ctags.SetGoToPositions(positions);
			mainForm.Ctags.GoToTag(place.iLine);
			return true;
		}
		return false;
	}
}
