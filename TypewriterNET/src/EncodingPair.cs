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
		
		EncodingInfo[] infos = Encoding.GetEncodings();
		int cols = 4;
		int rows = Math.Max(1, (infos.Length + cols - 1) / cols);
		EncodingInfo[,] grid = new EncodingInfo[cols, rows];
		for (int i = 0; i < infos.Length; ++i)
		{
			EncodingInfo info = infos[i];
			grid[i / rows, i % rows] = info;
		}
		int[] maxSizes = new int[cols];
		for (int col = 0; col < cols; ++col)
		{
			for (int row = 0; row < rows; ++row)
			{
				EncodingInfo info = grid[col, row];
				string name = info.Name + " bom";
				if (info != null && maxSizes[col] < name.Length)
				{
					maxSizes[col] = name.Length;
				}
			}
		}
		for (int row = 0; row < rows; ++row)
		{
			for (int col = 0; col < cols; ++col)
			{
				EncodingInfo info = grid[col, row];
				if (info != null)
				{
					string name = info.Name + " bom";
					builder.Append(name);
					builder.Append(new string(' ', maxSizes[col] - name.Length));
				}
				else
				{
					builder.Append(new string(' ', maxSizes[col]));
				}
				if (col != cols - 1)
				{
					builder.Append(" │ ");
				}
			}
			builder.AppendLine();
		}
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
