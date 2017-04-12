/*
using System.Collections.Generic;

public static class Debug
{
	private static int tabIndex = 0;
	
	public static void Begin(string text)
	{
		System.Console.WriteLine(GetTabs() + text + " {");
		tabIndex++;
	}
	
	public static void End()
	{
		tabIndex--;
		System.Console.WriteLine(GetTabs() + "}");
	}
	
	public static void BeginIfLog(string text)
	{
		if (tabIndex > 0)
		{
			System.Console.WriteLine(GetTabs() + text + " {");
			tabIndex++;
		}
	}
	
	public static void EndIfLog()
	{
		if (tabIndex > 1)
		{
			tabIndex--;
			System.Console.WriteLine(GetTabs() + "}");
		}
	}
	
	private static string GetTabs()
	{
		string tabs = "";
		for (int i = 0; i < tabIndex; i++)
		{
			tabs += "\t";
		}
		return tabs;
	}
	
	public static void Log(string text)
	{
		if (tabIndex > 0)
		{
			System.Console.WriteLine(GetTabs() + text.Replace("\n", "\n" + GetTabs()));
		}
	}
	
	public static void Log<T>(IEnumerable<T> list)
	{
		Log(MulticaretEditor.ListUtil.ToString<T>(list));
	}
	
	public static void Log(IEnumerable<MulticaretEditor.Line> list)
	{
		Log(MulticaretEditor.ListUtil.ToString<MulticaretEditor.Line>(list, LineToString));
	}
	
	public static void Log(MulticaretEditor.Line line)
	{
		Log(LineToString(line));
	}
	
	public static string LineToString(MulticaretEditor.Line line)
	{
		return "\"" + line.Text.Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
	}
}
//*/