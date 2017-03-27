using System;
using System.Text;
using System.Collections.Generic;

public struct EncodingPair
{
	public readonly Encoding encoding;
	public readonly bool bom;
	
	private readonly string text;

	public EncodingPair(Encoding encoding, bool bom)
	{
		this.encoding = encoding;
		this.bom = bom;
		if (encoding != null)
			text = GetName(encoding) + (bom ? " bom" : "");
		else
			text = "Null";
	}

	public bool IsNull { get { return encoding == null; } }
	
	public int CorrectBomLength(byte[] bytes)
	{
		int length = 0;
		if (bom)
		{
			byte[] preamble = encoding.GetPreamble();
			if (preamble.Length <= bytes.Length)
			{
				bool matched = true;
				for (int i = 0; i < preamble.Length; i++)
				{
					if (bytes[i] != preamble[i])
					{
						matched = false;
						break;
					}
				}
				if (matched)
				{
					length = preamble.Length;
				}
			}
		}
		return length;
	}

	public string GetString(byte[] bytes, int bomLength)
	{
		return encoding.GetString(bytes, bomLength, bytes.Length - bomLength);
	}

	override public string ToString()
	{
		return text;
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
		builder.AppendLine("# Awailable encodings");
		builder.AppendLine();
		TextTable table = new TextTable().SetMaxColWidth(35);
		int index = 0;
		foreach (EncodingInfo info in Encoding.GetEncodings())
		{
			table.Add(info.Name);
			index++;
			if (index % 3 == 0)
				table.NewRow();
			if (info.GetEncoding().GetPreamble().Length > 0)
			{
				table.Add(info.Name + " bom");
				index++;
				if (index % 3 == 0)
					table.NewRow();
			}
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
