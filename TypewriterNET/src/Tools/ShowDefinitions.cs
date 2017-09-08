using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Forms;
using MulticaretEditor;

public class ShowDefinitions
{
	private MainForm mainForm;
	private List<Ctags.Node> positions;
	private Buffer buffer;

	public ShowDefinitions(MainForm mainForm)
	{
		this.mainForm = mainForm;
	}

	public string Execute(List<Ctags.Node> usages, string word)
	{
		positions = usages;
		Regex regex = new Regex("\\b" + word + "\\b");
		StringBuilder builder = new StringBuilder();
		List<StyleRange> ranges = new List<StyleRange>();
		int maxLength = 0;
		foreach (Ctags.Node usage in usages)
		{
			int length = usage.path.Length;
			if (usage.path.StartsWith("./"))
			{
				length -= 2;
			}
			if (maxLength < length)
			{
				maxLength = length;
			}
		}
		foreach (Ctags.Node usage in usages)
		{
			if (builder.Length > 0)
			{
				builder.Append(mainForm.Settings.lineBreak.Value);
			}
			string fileName = usage.path;
			if (fileName.StartsWith("./"))
			{
				fileName = fileName.Substring(2);
			}
			ranges.Add(new StyleRange(builder.Length, maxLength, Ds.String.index));
			builder.Append(fileName.PadRight(maxLength));
			ranges.Add(new StyleRange(builder.Length, 1, Ds.Operator.index));
			builder.Append("|");
			string address = usage.address != null ? usage.address.Trim() : "";
			Match match = regex.Match(address);
			if (match.Success)
			{
				ranges.Add(new StyleRange(builder.Length + match.Index, match.Length, Ds.Keyword.index));
			}
			builder.Append(address);
		}

		buffer = new Buffer(null, "Ctags definitions", SettingsMode.Normal);
		buffer.showEncoding = false;
		buffer.Controller.isReadonly = true;
		buffer.Controller.InitText(builder.ToString());
		buffer.Controller.SetStyleRanges(ranges);
		buffer.additionKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("F&ind\\Navigate to definition", ExecuteEnter, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.None, null, action).SetDoubleClick(true));
		}
		mainForm.ShowConsoleBuffer(MainForm.FindResultsId, buffer);
		return null;
	}

	private bool ExecuteEnter(Controller controller)
	{
		Place place = controller.Lines.PlaceOf(controller.LastSelection.anchor);
		if (place.iLine >= 0 && place.iLine < positions.Count)
		{
			Ctags.Node node = positions[place.iLine];
			mainForm.Ctags.SetGoToTags(positions);
			mainForm.Ctags.GoToTag(node);
			return true;
		}
		return false;
	}
}
