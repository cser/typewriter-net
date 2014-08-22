using System;

public class TempSettingsInt
{
	public readonly string id;
	public int priority;
	public int value;
	public bool changed;

	public TempSettingsInt(string id)
	{
		this.id = id;
	}
}
