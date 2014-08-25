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
		Regex regex = null;
		errors = null;
		try
		{
			regex = new Regex(raw, RegexOptions.Multiline);
		}
		catch (Exception e)
		{
			errors = e.Message;
			return new RegexData(null, null);
		}
		return new RegexData(regex, raw);
	}
}
