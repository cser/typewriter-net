using System;
using System.Collections.Generic;
using System.Text;

public class SchemeIncrementalSearch : IncrementalSearchBase
{
	public SchemeIncrementalSearch(TempSettings tempSettings, Settings settings) : base(
		tempSettings,
		"Preview color scheme" + (settings.scheme.initedByConfig ? " - need configure to save" : ""),
		"Preview color scheme",
		null)
	{
	}

	private List<string> items = new List<string>();
	private string currentItem;
	private string oldItem;
	private bool executed;

	override protected bool Prebuild()
	{
		currentItem = MainForm.Settings.scheme.Value;
		items.Clear();
		foreach (string item in MainForm.Settings.scheme.GetVariants())
		{
			items.Add(item);
		}
		oldItem = currentItem;
		return true;
	}
	
	private List<string> sortedItems = new List<string>();
	
	private int _startVariantIndex = -1;
	override protected int StartVariantIndex { get { return _startVariantIndex; } }

	override protected string GetVariantsText()
	{
		sortedItems.Clear();
		foreach (string item in items)
		{
			if (GetIndex(item) != -1)
				sortedItems.Add(item);
		}
		sortedItems.Sort(CompareItems);
		StringBuilder builder = new StringBuilder();
		_startVariantIndex = -1;
		int index = 0;
		bool first = true;
		foreach (string item in sortedItems)
		{
			if (!first)
				builder.AppendLine();
			first = false;
			builder.Append(item);
			if (item == currentItem)
			{
				_startVariantIndex = index;
			}
			index++;
		}
		return builder.ToString();
	}

	private int CompareItems(string item0, string item1)
	{
		if (!string.IsNullOrEmpty(Pattern))
		{
			int equals0 = item0 == currentItem ? 1 : 0;
			int equals1 = item1 == currentItem ? 1 : 0;
			if (equals0 != equals1)
				return equals0 - equals1;
		}
		int index0 = GetIndex(item0);
		int index1 = GetIndex(item1);
		if (index0 != index1)
			return index1 - index0;
		return string.Compare(item1, item0);
	}
	
	private void ChangeSelection(string lineText)
	{
		if (MainForm.Settings.scheme.Value != lineText)
		{
			MainForm.Settings.scheme.SetText(lineText, "");
			MainForm.Settings.DispatchChange();
		}
	}
	
	override protected void DoOnSelectionChange(int line, string lineText)
	{
		ChangeSelection(lineText);
	}

	override protected void Execute(int line, string lineText)
	{
		ChangeSelection(lineText);
		executed = true;
		DispatchNeedClose();
	}
	
	override protected void DoDestroy()
	{
		if (!executed)
			ChangeSelection(oldItem);
		base.DoDestroy();
	}
}