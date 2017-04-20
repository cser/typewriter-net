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
	
	public string error;
	public readonly List<string> files = new List<string>();
	public bool done;
	
	public FileSystemScanner(string directory, string filter)
	{
		this.directory = directory;
		this.filter = filter;
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
		Console.WriteLine("files.Count=" + files.Count);
		done = true;
	}
}
