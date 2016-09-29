using System;
using System.Collections.Generic;
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
	
	public void UpdateSettings(Settings settings)
	{
		if (!AreEquals(srcs, settings.src.Value) || !AreEquals(libs, settings.lib.Value))
		{
			srcs.Clear();
			srcs.AddRange(settings.src.Value);
			libs.Clear();
			libs.AddRange(settings.lib.Value);
			ApplySettings();
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
		/*
		Process p = new Process();
		p.StartInfo.UseShellExecute = true;
		p.StartInfo.FileName = "cmd.exe";
		p.StartInfo.Arguments = "/C " + commandText;
		p.Start();*/
	}
}
