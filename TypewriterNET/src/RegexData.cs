using System;
using System.Text.RegularExpressions;

public struct RegexData
{
	public readonly Regex regex;
	public readonly string pattern;

	public RegexData(Regex regex, string pattern)
	{
		this.regex = regex;
		this.pattern = pattern;
	}

	public static RegexData Parse(string raw, out string errors)
	{
		errors = null;
		Regex regex = null;
		if (raw.Length < 2 || raw[0] != '/' || raw.LastIndexOf("/") <= 1)
		{
			errors = "Regex mast be enclosed in / /";
			return new RegexData(null, null);
		}
		RegexOptions options = RegexOptions.CultureInvariant | RegexOptions.Multiline;
		int lastIndex = raw.LastIndexOf("/");
		string optionsText = raw.Substring(lastIndex + 1);
		string rawRegex = raw.Substring(1, lastIndex - 1);
		for (int i = 0; i < optionsText.Length; i++)
		{
			char c = optionsText[i];
			if (c == 'i')
				options |= RegexOptions.IgnoreCase;
			else if (c == 's')
				options &= ~RegexOptions.Multiline;
			else if (c == 'e')
				options |= RegexOptions.ExplicitCapture;
			else
			{
				errors = "Unsupported regex option: " + c;
				return new RegexData(null, null);
			}
		}
		try
		{
			regex = new Regex(rawRegex, options);
		}
		catch (Exception e)
		{
			errors = "Incorrect regex: " + raw + " - " + e.Message;
			return new RegexData(null, null);
		}
		return new RegexData(regex, raw);
	}
}
