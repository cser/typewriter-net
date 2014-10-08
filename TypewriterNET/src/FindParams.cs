using System;
using MulticaretEditor;

public class FindParams
{
	public bool regex;
	public bool ignoreCase;

	public string GetIndicationText()
	{
		return (regex ? "[r]" : " r ") + (ignoreCase ? "[i]" : " i ");
	}

	public void Unserialize(SValue value)
	{
		regex = value["regex"].Bool;
		ignoreCase = value["ignoreCase"].Bool;
	}

	public SValue Serialize()
	{
		SValue value = SValue.NewHash();
		value["regex"] = SValue.NewBool(regex);
		value["ignoreCase"] = SValue.NewBool(ignoreCase);
		return value;
	}
}
