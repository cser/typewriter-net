using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Diagnostics;
using MulticaretEditor;

public class SnippetFilesScanner
{
	private string[] dirs;

	public SnippetFilesScanner(string[] dirs)
	{
		this.dirs = dirs;
	}
	
	private string ignoreFilesRaw = "";
	private string forcedFilesRaw = "";
	private readonly Dictionary<string, bool> ignoreFiles = new Dictionary<string, bool>();
	private readonly Dictionary<string, bool> forcedFiles = new Dictionary<string, bool>();
	private bool needRescan = true;
	
	public void SetIgnoreFiles(string ignoreFilesRaw, string forcedFilesRaw)
	{
		if (this.ignoreFilesRaw != ignoreFilesRaw ||
			this.forcedFilesRaw != forcedFilesRaw)
		{
			this.ignoreFilesRaw = ignoreFilesRaw;
			this.forcedFilesRaw = forcedFilesRaw;
			ignoreFiles.Clear();
			forcedFiles.Clear();
			if (ignoreFilesRaw != null)
			{
				string[] files = ignoreFilesRaw.Split(';');
				foreach (string file in files)
				{
					ignoreFiles[file.Trim()] = true;
				}
			}
			if (forcedFilesRaw != null)
			{
				string[] files = forcedFilesRaw.Split(';');
				foreach (string file in files)
				{
					forcedFiles[file.Trim()] = true;
				}
			}
			Reset();
		}
	}
	
	public void Reset()
	{
		needRescan = true;
	}

	private void Rescan()
	{
		infos.Clear();
		snippetFiles.Clear();
		
		string tempFile = Path.Combine(Path.GetTempPath(), "typewriter-snippet.bin");
		SValue temp = File.Exists(tempFile) ? SValue.Unserialize(File.ReadAllBytes(tempFile)) : SValue.None;
		Dictionary<string, bool> scanned = new Dictionary<string, bool>();
		List<string> files = new List<string>();
		foreach (string dir in dirs)
		{
			if (!Directory.Exists(dir))
				continue;
			foreach (string fileI in Directory.GetFiles(dir, "*.snippets"))
			{
				string fileName = Path.GetFileName(fileI);
				if (!scanned.ContainsKey(fileName))
				{
					scanned[fileName] = true;
					files.Add(Path.Combine(dir, fileI));
				}
			}
		}
		files.Sort();
		scanned.Clear();

		SValue newTemp = SValue.NewHash();
		infos.Clear();
		foreach (string pathI in files)
		{
			string fileName = Path.GetFileName(pathI);
			SValue tempI = temp[fileName];
			long newTicks = File.GetLastWriteTime(pathI).Ticks;
			long ticks = tempI["ticks"].Long;
			if (newTicks == ticks)
			{
				SnippetInfo info = new SnippetInfo();
				info.path = pathI;
				info.patterns = ParseExtenstions(tempI["extensions"].String);
				infos.Add(info);

				newTemp[fileName] = tempI;
			}
			else
			{
				string line = null;
				using (StreamReader reader = new StreamReader(pathI))
				{
					line = reader.ReadLine();
				}
				string patterns;
				if (line != null && line.StartsWith("extensions:"))
				{
					patterns = line.Substring("extensions:".Length).Trim();
				}
				else
				{
					patterns = "";
				}
				
				SValue newTempI = SValue.NewHash();
				newTempI["patterns"] = SValue.NewString(patterns);
				newTemp[fileName] = newTempI;
				
				SnippetInfo info = new SnippetInfo();
				info.path = pathI;
				info.patterns = ParseExtenstions(patterns);
				infos.Add(info);
			}
		}
		File.WriteAllBytes(tempFile, SValue.Serialize(newTemp));
	}

	private Regex[] ParseExtenstions(string text)
	{
		string[] splitted = text.Split(';');
		List<Regex> patterns = new List<Regex>();
		for (int i = 0; i < splitted.Length; i++)
		{
			string splittedI = splitted[i].Trim();
			if (!string.IsNullOrEmpty(splittedI))
				patterns.Add(HighlighterUtil.GetFilenamePatternRegex(splittedI));
		}
		return patterns.ToArray();
	}

	//----------------------------------------------------------------------
	// Data
	//----------------------------------------------------------------------
	
	public void TryRescan()
	{
		if (needRescan)
		{
			needRescan = false;
			Rescan();
		}
	}
	
	private List<SnippetInfo> infos = new List<SnippetInfo>();
	public IEnumerable<SnippetInfo> Infos { get { return infos; } }
	
	private Dictionary<string, SnippetFile> snippetFiles = new Dictionary<string, SnippetFile>();
	
	public List<SnippetInfo> GetInfos(string file)
	{
		List<SnippetInfo> result = new List<SnippetInfo>();
		int count = infos.Count;
		for (int i = 0; i < count; i++)
		{
			SnippetInfo info = infos[i];
			string key = Path.GetFileNameWithoutExtension(info.path);
			Regex[] patterns = info.patterns;
			bool contains = false;
			for (int j = 0; j < patterns.Length; j++)
			{
				if (patterns[j].IsMatch(file))
				{
					contains = true;
					break;
				}
			}
			if (contains && !ignoreFiles.ContainsKey(key) ||
				forcedFiles.ContainsKey(key))
			{
				result.Add(info);
			}
		}
		return result;
	}
	
	public void LoadFiles(List<SnippetInfo> infos)
	{
		foreach (SnippetInfo info in infos)
		{
			string path = info.path;
			if (!snippetFiles.ContainsKey(path) && File.Exists(path))
			{
				string fileName = Path.GetFileNameWithoutExtension(path);
				string text = null;
				try
				{
					text = File.ReadAllText(path);
				}
				catch
				{
				}
				snippetFiles[path] = text != null ? new SnippetFile(text, fileName) : null;
			}
		}
	}
	
	public SnippetFile GetFile(string path)
	{
		SnippetFile snippetFile;
		snippetFiles.TryGetValue(path, out snippetFile);
		return snippetFile;
	}
}
