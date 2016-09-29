using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;

public class SharpManager
{
	private readonly MainForm mainForm;
	
	public SharpManager(MainForm mainForm)
	{
		this.mainForm = mainForm;
	}
	
	private List<string> srcs = new List<string>();
	private List<string> libs = new List<string>();
	private int omnisharpPort;
	
	public void UpdateSettings(Settings settings)
	{
		if (!AreEquals(srcs, settings.src.Value) ||
			!AreEquals(libs, settings.lib.Value) ||
			omnisharpPort != settings.omnisharpPort.Value)
		{
			srcs.Clear();
			srcs.AddRange(settings.src.Value);
			libs.Clear();
			libs.AddRange(settings.lib.Value);
			omnisharpPort = settings.omnisharpPort.Value;
			ApplySettings();
		}
	}
	
	public void Close()
	{
		if (server != null)
		{
			server.Close();
			server = null;
		}
	}
	
	private static bool AreEquals(IEnumerable<string> a, IEnumerable<string> b)
	{
		int aCount = 0;
		foreach (string s in a)
		{
			aCount++;
		}
		int bCount = 0;
		foreach (string s in b)
		{
			bCount++;
		}
		if (aCount != bCount)
			return false;
		IEnumerator<string> bEnumerator = b.GetEnumerator();
		foreach (string s in a)
		{
			bEnumerator.MoveNext();
			if (s != bEnumerator.Current)
				return false;
		}		
		return true;
	}
	
	private Process server;
	
	private void ApplySettings()
	{
		mainForm.Log.WriteInfo("SharpManager", "ApplySettings");
		mainForm.Log.Open();
		
		foreach (string src in srcs)
		{
			string[] files = null;
			try
			{
				files = Directory.GetFiles(src, "*.cs", SearchOption.AllDirectories);
			}
			catch (Exception e)
			{
				mainForm.Log.WriteError("SharpManager", "File list reading error: " + e.Message);
			}
			List<StyleRange> ranges = new List<StyleRange>();
			foreach (string file in files)
			{
				
			}
		}
		
		if (server != null)
		{
			server.Close();
			server = null;
		}
		if (srcs.Count > 0)
		{
			mainForm.Log.WriteInfo("OmniSharp", "Connecting to port: " + omnisharpPort);
			server = new Process();
			server.StartInfo.UseShellExecute = true;
			server.StartInfo.FileName = Path.Combine(AppPath.StartupDir, "omnisharp_server/OmniSharp.exe");
			server.StartInfo.Arguments = "-s " + srcs[0] + " -p=" + omnisharpPort;
			server.Start();
		}
	}
}
