using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;
using MulticaretEditor;

public class EncodingIncrementalSearch : IncrementalSearchBase
{
	private TempSettings tempSettings;
	private bool isSave;
	
	public EncodingIncrementalSearch(TempSettings tempSettings, bool isSave) : base(
		isSave ? "Save encoding" : "Reload with encoding",
		isSave ? "Save encoding" : "Reload with encoding"
	)
	{
		this.tempSettings = tempSettings;
		this.isSave = isSave;
	}

	private List<EncodingPair> items = new List<EncodingPair>();
	private EncodingPair currentItem;
	private Buffer buffer;

	override protected bool Prebuild()
	{
		buffer = MainForm.LastBuffer;
		if (buffer == null || buffer.Controller.isReadonly)
		{
			MainForm.Dialogs.ShowInfo("Error", "No file in current frame");
			return false;
		}
		
		items.Clear();
		foreach (EncodingInfo info in Encoding.GetEncodings())
		{
			Encoding encoding = info.GetEncoding();
			items.Add(new EncodingPair(encoding, false));
			if (encoding.GetPreamble().Length > 0)
				items.Add(new EncodingPair(encoding, true));
		}
		currentItem = buffer.encodingPair;
		return true;
	}
	
	private static string StringOf(EncodingPair pair)
	{
		return !pair.IsNull ? pair.ToString() : "[reset]";
	}

	private string compareText;
	private List<EncodingPair> sortedItems = new List<EncodingPair>();

	override protected string GetVariantsText(string text)
	{
		compareText = text;
		sortedItems.Clear();
		foreach (EncodingPair item in items)
		{
			if (StringOf(item).Contains(text))
				sortedItems.Add(item);
		}
		sortedItems.Sort(CompareItems);
		if (!isSave)
			sortedItems.Insert(0, new EncodingPair());
		compareText = text;
		StringBuilder builder = new StringBuilder();
		bool first = true;
		foreach (EncodingPair item in sortedItems)
		{
			if (!first)
				builder.AppendLine();
			first = false;
			builder.Append(StringOf(item));
		}
		return builder.ToString();
	}

	private int CompareItems(EncodingPair item0, EncodingPair item1)
	{
		int equals0;
		int equals1;
		equals0 = item0.Equals(currentItem) ? 1 : 0;
		equals1 = item1.Equals(currentItem) ? 1 : 0;
		if (equals0 != equals1)
			return equals0 - equals1;
		equals0 = item0.encoding == currentItem.encoding ? 1 : 0;
		equals1 = item1.encoding == currentItem.encoding ? 1 : 0;
		if (equals0 != equals1)
			return equals0 - equals1;
		int index0 = StringOf(item0).IndexOf(compareText);
		int index1 = StringOf(item1).IndexOf(compareText);
		if (index0 != index1)
			return index1 - index0;
		return string.Compare(StringOf(item1), StringOf(item0));
	}

	override protected void Execute(int line, string lineText)
	{
		if (line >= sortedItems.Count)
			return;
		EncodingPair pair = sortedItems[line];
		
		if (buffer == null || buffer.Controller.isReadonly)
		{
			MainForm.Dialogs.ShowInfo("Error", "No file in current frame");
			return;
		}
		if (isSave)
		{
			buffer.encodingPair = pair;
		}
		else
		{
			tempSettings.ResetQualitiesEncoding(buffer);
			buffer.settedEncodingPair = pair;
			MainForm.ReloadFile(buffer);
		}
		DispatchNeedClose();
	}
}