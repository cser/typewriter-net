using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;
using System.Threading;

public class FileSystemScanner
{
	private readonly string directory;
	private readonly string filter;
	private readonly string ignoreDirs;
	
	public string error;
	public readonly List<string> files = new List<string>();
	public bool done;
	
	public FileSystemScanner(string directory, string filter, string ignoreDirs)
	{
		this.directory = directory;
		this.filter = filter;
		this.ignoreDirs = ignoreDirs;
	}
	
	public void Scan()
	{
		files.Clear();
		error = null;
		string filter = this.filter;
		FileNameFilter hardFilter = null;
		if (string.IsNullOrEmpty(filter))
		{
			filter = "*";
		}
		else if (filter.Contains(";"))
		{
			hardFilter = new FileNameFilter(filter);
			filter = "*";
		}
		
		List<string> ignoreDirsList = new List<string>();
		string[] ignoreDirs = this.ignoreDirs.Replace('/', '\\').Split(';');
		bool ignoreRoot = false;
		for (int i = 0; i < ignoreDirs.Length; ++i)
		{
			string dir = ignoreDirs[i];
			dir = dir.Trim();
			if (string.IsNullOrEmpty(dir))
			{
				continue;
			}
			if (dir.Length > 0 && dir[0] == '\\')
			{
				dir = dir.Substring(1);
			}
			if (dir.Length > 0 && dir[dir.Length - 1] == '\\')
			{
				dir = dir.Substring(0, dir.Length - 1);
			}
			if (dir == "" || dir == ".")
			{
				ignoreRoot = true;
				continue;
			}
			if (dir.IndexOf('\\') != -1)
			{
				error = "Unexpected: \"" + dir + "\"\n(only single root dirs supported in ignore dirs list)";
				dir = dir.Substring(0, dir.IndexOf('\\'));
				if (dir == "")
				{
					continue;
				}
			}
			ignoreDirsList.Add(dir.ToLowerInvariant());
		}
		ignoreDirs = ignoreDirsList.ToArray();
		
		if (ignoreDirs.Length == 0 && !ignoreRoot)
		{
			AddFilesRecursive(directory, filter, hardFilter);
		}
		else
		{
			if (!ignoreRoot)
			{
				AddFiles(directory, filter, hardFilter);
			}
			string[] dirs = null;
			try
			{
				dirs = Directory.GetDirectories(directory);
			}
			catch (System.Exception e)
			{
				error = e.Message + " (root directories)";
				done = true;
				return;
			}
			if (dirs == null)
			{
				error = "Unknown error (root directories is null)";
				done = true;
				return;
			}
			for (int i = 0; i < dirs.Length; ++i)
			{
				string dir = dirs[i];
				string dirToMatch = dir;
				if (dirToMatch.IndexOf('/') != -1)
				{
					dirToMatch = dirToMatch.Replace('/', '\\');
				}
				int index = dirToMatch.LastIndexOf('\\');
				if (index != -1)
				{
					string dirName = dirToMatch.Substring(index + 1);
					if (!string.IsNullOrEmpty(dirName) &&
						Array.IndexOf(ignoreDirs, dirName.ToLowerInvariant()) != -1)
					{
						continue;
					}
				}
				AddFilesRecursive(dir, filter, hardFilter);
			}
		}
		done = true;
	}
	
	private void AddFilesRecursive(string directory, string filter, FileNameFilter hardFilter)
	{
		string[] fileArray = null;
		try
		{
			fileArray = Directory.GetFiles(directory, filter, SearchOption.AllDirectories);
		}
		catch (System.Exception e)
		{
			error = e.Message;
			return;
		}
		if (fileArray == null)
		{
			error = "Unknown error (files is null)";
			return;
		}
		for (int i = 0; i < fileArray.Length; ++i)
		{
			string file = fileArray[i];
			if (hardFilter != null)
			{
				string name = Path.GetFileName(file);
				if (hardFilter.Match(name))
				{
					files.Add(file);
				}
			}
			else
			{
				files.Add(file);	
			}
		}
	}
	
	private void AddFiles(string directory, string filter, FileNameFilter hardFilter)
	{
		string[] fileArray = null;
		try
		{
			fileArray = Directory.GetFiles(directory, filter, SearchOption.TopDirectoryOnly);
		}
		catch (System.Exception e)
		{
			error = e.Message;
			return;
		}
		if (fileArray == null)
		{
			error = "Unknown error (files is null)";
			return;
		}
		for (int i = 0; i < fileArray.Length; ++i)
		{
			string file = fileArray[i];
			if (hardFilter != null)
			{
				string name = Path.GetFileName(file);
				if (hardFilter.Match(name))
				{
					files.Add(file);
				}
			}
			else
			{
				files.Add(file);	
			}
		}
	}
}
