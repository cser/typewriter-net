using System;
using System.IO;
using System.Windows.Forms;

namespace TypewriterNET
{
	public struct OldAppPath
	{
		public const string Syntax = "syntax";
		public const string Schemes = "schemes";

		private static string startupDir;
		public static string StartupDir { get { return startupDir; } }

		private static string appDataDir;
		public static string AppDataDir { get { return appDataDir; } }

		private static OldAppPath syntaxDir;
		public static OldAppPath SyntaxDir { get { return syntaxDir; } }

		private static OldAppPath schemesDir;
		public static OldAppPath SchemesDir { get { return schemesDir; } }

		private static OldAppPath configPath;
		public static OldAppPath ConfigPath { get { return configPath; } }

		private static string configTemplatePath;
		public static string ConfigTemplatePath { get { return configTemplatePath; } }

		public static void Init(string startupDir, string appDataDir)
		{
			OldAppPath.startupDir = startupDir;
			OldAppPath.appDataDir = appDataDir;
			OldAppPath.syntaxDir = new OldAppPath(Path.Combine(appDataDir, Syntax));
			OldAppPath.schemesDir = new OldAppPath(Path.Combine(appDataDir, Schemes));
			OldAppPath.configPath = new OldAppPath("config.xml");
			OldAppPath.configTemplatePath = new OldAppPath("config-template.xml").startupPath;
		}

		public readonly string local;
		public readonly string appDataPath;
		public readonly string startupPath;
		public readonly string[] paths;

		public OldAppPath(string local)
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
