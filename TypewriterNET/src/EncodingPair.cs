using System;
using System.Text;
using System.Collections.Generic;

public struct EncodingPair
{
	public Encoding encoding;
	public bool bom;

	public EncodingPair(Encoding encoding, bool bom)
	{
		this.encoding = encoding;
		this.bom = bom;
	}

	public bool IsNull { get { return encoding == null; } }

	public string GetString(byte[] bytes)
	{
		int length = bom ? encoding.GetPreamble().Length : 0;
		return encoding.GetString(bytes, length, bytes.Length - length);
	}

	override public string ToString()
	{
		if (encoding == null)
			return "Null";
		return GetName(encoding) + (bom ? " bom" : "");
	}

	public static EncodingPair ParseEncoding(string raw, out string error)
	{
		error = null;
		if (string.IsNullOrEmpty(raw))
		{
			error = "Empty encoding name";
			return new EncodingPair();
		}
		string[] array = raw.Split(' ');
		string name = array.Length > 0 ? array[0] : "";
		bool bom = array.Length > 1 && array[1] == "bom";
		Encoding encoding = null;
		try
		{
			encoding = Encoding.GetEncoding(name);
		}
		catch (Exception e)
		{
			error = e.Message;
		}
		return encoding != null ? new EncodingPair(encoding, bom) : new EncodingPair();
	}

	public static string GetEncodingsText()
	{
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("Awailable encodings:");
		TextTable table = new TextTable().SetMaxColWidth(20);
		int index = 0;
		foreach (EncodingInfo info in Encoding.GetEncodings())
		{
			table.Add(info.Name);
			index++;
			if (index % 3 == 0)
				table.NewRow();
		}
		builder.Append(table.ToString());
		return builder.ToString();
	}

	private static Dictionary<int, string> nameByCodePage;

	private static string GetName(Encoding encoding)
	{
		if (nameByCodePage == null)
		{
			nameByCodePage = new Dictionary<int, string>();
			foreach (EncodingInfo info in Encoding.GetEncodings())
			{
				nameByCodePage[info.CodePage] = info.Name;
			}
		}
		string name;
		return nameByCodePage.TryGetValue(encoding.CodePage, out name) ? name : encoding.EncodingName;
	}
}
