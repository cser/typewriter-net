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

public class ShowCodecheck
{
	public struct Position
	{
		public readonly string fullPath;
		public readonly Place place;

		public Position(string fullPath, Place place)
		{
			this.fullPath = fullPath;
			this.place = place;
		}
	}

	private MainForm mainForm;
	private List<Position> positions;
	private Buffer buffer;
	private string name;

	public ShowCodecheck(MainForm mainForm, string name)
	{
		this.mainForm = mainForm;
		this.name = name;
	}

	public string Execute(List<Codecheck> codechecks, string word)
	{
		positions = new List<Position>();
		StringBuilder builder = new StringBuilder();
		List<StyleRange> ranges = new List<StyleRange>();
		int maxLength = 0;
		int maxPlaceLength = 0;
		foreach (Codecheck codecheck in codechecks)
		{
			string fileName = Path.GetFileName(codecheck.FileName);
			if (maxLength < fileName.Length)
			{
				maxLength = fileName.Length;
			}
			string place = codecheck.Line + " " + codecheck.Column;
			if (maxPlaceLength < place.Length)
				maxPlaceLength = place.Length;
		}
		foreach (Codecheck codecheck in codechecks)
		{
			string fileName = Path.GetFileName(codecheck.FileName);
			ranges.Add(new StyleRange(builder.Length, maxLength, Ds.String.index));
			builder.Append(fileName.PadRight(maxLength));
			string place = codecheck.Line + " " + codecheck.Column;
			ranges.Add(new StyleRange(builder.Length, 1, Ds.Operator.index));
			builder.Append('|');
			ranges.Add(new StyleRange(builder.Length, maxPlaceLength, Ds.DecVal.index));
			builder.Append(place.PadRight(maxPlaceLength));
			ranges.Add(new StyleRange(builder.Length, 1, Ds.Operator.index));
			builder.Append('|');
			int length = (codecheck.LogLevel + ":").Length;
			if (codecheck.LogLevel == "Error")
			{
				ranges.Add(new StyleRange(builder.Length, length, Ds.Error.index));
			}
			else
			{
				ranges.Add(new StyleRange(builder.Length, length, Ds.Others.index));
			}
			builder.Append(codecheck.LogLevel + ": ");
			string text = codecheck.Text.Trim();
			positions.Add(new Position(codecheck.FileName, new Place(codecheck.Column - 1, codecheck.Line - 1)));
			builder.Append(text);
			builder.Append(mainForm.Settings.lineBreak.Value);
		}

		buffer = new Buffer(null, name, SettingsMode.Normal);
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
		mainForm.ShowConsoleBuffer(MainForm.FindResultsId, buffer);
		return null;
	}

	private bool ExecuteEnter(Controller controller)
	{
		Place place = controller.Lines.PlaceOf(controller.LastSelection.anchor);
		Position position = positions[place.iLine];
		mainForm.NavigateTo(position.fullPath, position.place, position.place);
		return true;
	}
}
