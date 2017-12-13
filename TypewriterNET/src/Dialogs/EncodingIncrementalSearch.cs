using System;
using System.Collections.Generic;
using System.Text;

public class EncodingIncrementalSearch : IncrementalSearchBase
{
	private bool isSave;
	
	public EncodingIncrementalSearch(TempSettings tempSettings, bool isSave) : base(
		tempSettings,
		isSave ? "Save encoding" : "Reload with encoding",
		isSave ? "Save encoding" : "Reload with encoding",
		null
	)
	{
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

	private List<EncodingPair> sortedItems = new List<EncodingPair>();

	override protected string GetVariantsText()
	{
		sortedItems.Clear();
		foreach (EncodingPair item in items)
		{
			if (GetIndex(StringOf(item)) != -1)
				sortedItems.Add(item);
		}
		sortedItems.Sort(CompareItems);
		if (!isSave)
			sortedItems.Insert(0, new EncodingPair());
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
		int index0 = GetIndex(StringOf(item0));
		int index1 = GetIndex(StringOf(item1));
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