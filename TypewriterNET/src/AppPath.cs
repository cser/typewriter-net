using System;
using System.IO;
using System.Windows.Forms;

namespace TypewriterNET
{
	public struct AppPath
	{
		public static readonly string appDataDir = Path.GetDirectoryName(Application.CommonAppDataPath);
		public static readonly string startupDir = Application.StartupPath;

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
