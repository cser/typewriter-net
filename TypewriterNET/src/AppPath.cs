using System;
using System.IO;
using System.Windows.Forms;

namespace TypewriterNET
{
	public struct AppPath
	{
		public const string Syntax = "syntax";
		public const string Schemes = "schemes";

		private static string startupDir;
		public static string StartupDir { get { return startupDir; } }

		private static string appDataDir;
		public static string AppDataDir { get { return appDataDir; } }

		private static AppPath syntaxDir;
		public static AppPath SyntaxDir { get { return syntaxDir; } }

		private static AppPath schemesDir;
		public static AppPath SchemesDir { get { return schemesDir; } }

		private static AppPath configPath;
		public static AppPath ConfigPath { get { return configPath; } }

		private static string configTemplatePath;
		public static string ConfigTemplatePath { get { return configTemplatePath; } }

		public static void Init(string startupDir, string appDataDir)
		{
			AppPath.startupDir = startupDir;
			AppPath.appDataDir = appDataDir;
			AppPath.syntaxDir = new AppPath(Path.Combine(appDataDir, Syntax));
			AppPath.schemesDir = new AppPath(Path.Combine(appDataDir, Schemes));
			AppPath.configPath = new AppPath("config.xml");
			AppPath.configTemplatePath = new AppPath("config-template.xml").startupPath;
		}

		public readonly string local;
		public readonly string appDataPath;
		public readonly string startupPath;
		public readonly string[] paths;

		public AppPath(string local)
		{
			this.local = local;
			appDataPath = Path.Combine(appDataDir, local);
			startupPath = Path.Combine(startupDir, local);
			paths = new string[]{ startupPath, appDataPath };
		}

		public bool HasPath(string path)
		{
			return path == appDataPath || path == startupPath;
		}

		public string GetExisted()
		{
			if (File.Exists(appDataPath))
				return appDataPath;
			if (File.Exists(startupPath))
				return startupPath;
			return null;
		}
	}
}
