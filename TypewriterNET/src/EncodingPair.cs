using System;
using System.Text;

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

	public string Name { get { return encoding.EncodingName + (bom ? " bom" : ""); } }

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
}
