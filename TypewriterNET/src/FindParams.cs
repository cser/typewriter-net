using System;
using MulticaretEditor;

public class FindParams
{
	public bool regex;
	public bool ignoreCase;
	public bool escape;
	
	public string GetIgnoreCaseIndicationText()
	{
		return ignoreCase ? "[i]" : " i ";
	}
	
	public string GetIgnoreCaseIndicationHint()
	{
		return "Ctrl+Shift+I(ignore case)";
	}

	public string GetIndicationText()
	{
		return (regex ? "[r]" : " r ") + (ignoreCase ? "[i]" : " i ");
	}
	
	public string GetIndicationHint()
	{
		return "Ctrl+Shift+R(regex)/I(ignore case)";
	}

	public string GetIndicationWithEscapeText()
	{
		return (regex ? "[r]" : " r ") + (ignoreCase ? "[i]" : " i ") + (escape ? "[e]" : " e ");
	}
	
	public string GetIndicationWithEscapeHint()
	{
		return "Ctrl+Shift+R(regex)/I(ignore case)/E(replace as escape sequence)";
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
