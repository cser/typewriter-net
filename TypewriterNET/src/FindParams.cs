using System;
using MulticaretEditor;

public class FindParams
{
	public bool regex;
	public bool ignoreCase;
	public bool escape;

	public string GetIndicationText()
	{
		return (regex ? "[r]" : " r ") + (ignoreCase ? "[i]" : " i ");
	}

	public string GetIndicationTextWithEscape()
	{
		return (regex ? "[r]" : " r ") + (ignoreCase ? "[i]" : " i ") + (escape ? "[e]" : " e ");
	}

	public void Unserialize(SValue value)
	{
		regex = value["regex"].Bool;
		ignoreCase = value["ignoreCase"].Bool;
		escape = value["escape"].Bool;
	}

	public SValue Serialize()
	{
		SValue value = SValue.NewHash();
		value["regex"] = SValue.NewBool(regex);
		value["ignoreCase"] = SValue.NewBool(ignoreCase);
		value["escape"] = SValue.NewBool(escape);
		return value;
	}
}
