using System;

[Flags]
public enum BufferTag
{
	None = 0x00,
	File = 0x01,
	Console = 0x02,
	Other = 0x04,
	Connected = 0x8
}
