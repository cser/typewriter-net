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
		System.Console.WriteLine(GetTabs() + text);
	}
}