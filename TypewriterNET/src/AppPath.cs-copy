using System;
using System.IO;
using System.Windows.Forms;

public struct AppPath
{
	public const string Syntax = "syntax";
	public const string Schemes = "schemes";
	public const string Templates = "templates";

	private static string startupDir;
	public static string StartupDir { get { return startupDir; } }

	private static string appDataDir;
	public static string AppDataDir { get { return appDataDir; } }

	private static string templatesDir;
	public static string TemplatesDir { get { return templatesDir; } }

	private static AppPath syntaxDir;
	public static AppPath SyntaxDir { get { return syntaxDir; } }

	private static AppPath syntaxDtd;
	public static AppPath SyntaxDtd { get { return syntaxDtd; } }

	private static AppPath schemesDir;
	public static AppPath SchemesDir { get { return schemesDir; } }

	private static AppPath configPath;
	public static AppPath ConfigPath { get { return configPath; } }

	public static void Init(string startupDir, string appDataDir, string postfix)
	{
		AppPath.startupDir = startupDir;
		AppPath.appDataDir = appDataDir;
		AppPath.templatesDir = Path.Combine(startupDir, Templates);
		AppPath.syntaxDir = new AppPath(Syntax, null);
		AppPath.syntaxDtd = new AppPath(Path.Combine(Syntax, "language.dtd"), null);
		AppPath.schemesDir = new AppPath(Schemes, null);
		AppPath.configPath = new AppPath("tw-config.xml", !string.IsNullOrEmpty(postfix) ? "tw-config-" + postfix + ".xml" : null);
	}

	public readonly string local;
	public readonly string appDataPath;
	public readonly string startupPath;

	public AppPath(string local, string postfixed)
	{
		this.local = local;
		appDataPath = Path.Combine(appDataDir, postfixed != null ? postfixed : local);
		startupPath = Path.Combine(startupDir, local);
	}

	public string GetCurrentPath()
	{
		return Path.Combine(Directory.GetCurrentDirectory(), local);
	}

	public string GetExisted()
	{
		if (File.Exists(appDataPath))
			return appDataPath;
		if (File.Exists(startupPath))
			return startupPath;
		return null;
	}

	public string[] GetBoth()
	{
		return new string[] { startupPath, appDataPath };
	}
}
