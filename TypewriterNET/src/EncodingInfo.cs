using System;
using System.Text;

public struct EncodingInfo
{
	public readonly Encoding encoding;
	public readonly bool bom;
	public readonly string name;

	public EncodingInfo(Encoding encoding, bool bom, string name)
	{
		this.encoding = encoding;
		this.bom = bom;
		this.name = name;
	}
}
