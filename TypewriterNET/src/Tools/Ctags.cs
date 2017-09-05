using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;

public class Ctags
{
	public class Node
	{
		public string path;
		public string address;
		
		public override string ToString()
		{
			return "(path=" + path + " address=/^" + address + "$/)";
		}
	}
	
	private readonly MainForm mainForm;
	private bool needReload = true;
	private string[] lines;
	
	public Ctags(MainForm mainForm)
	{
		this.mainForm = mainForm;
	}
	
	public void NeedReload()
	{
		needReload = true;
	}
	
	private void ReloadIfNeed()
	{
		if (needReload)
		{
			needReload = false;
			lines = null;
			string path = "tags";
			if (File.Exists(path))
			{
				try
				{
					lines = File.ReadAllLines(path, Encoding.UTF8);
				}
				catch (IOException e)
				{
					mainForm.Dialogs.ShowInfo("Ctags", "Error: " + e.Message);
				}
			}
		}
	}
	
	public List<Node> GetNodes(string name)
	{
		ReloadIfNeed();
		List<Node> nodes = new List<Node>();
		if (lines == null)
		{
			return nodes;
		}
		for (int i = 0; i < lines.Length; ++i)
		{
			string line = lines[i];
			if (line.StartsWith(name) && line.Length > name.Length && line[name.Length] == '\t')
			{
				Node node = new Node();
				int index0 = name.Length + 1;
				int index1 = line.IndexOf('\t', index0);
				if (index1 == -1)
				{
					continue;
				}
				node.path = line.Substring(index0, index1 - index0);
				++index1;
				string bra = "/^";
				string ket = "$/;\"";
				if (line.Substring(index1, 2) != bra)
				{
					continue;
				}
				index1 += bra.Length;
				int index2 = line.IndexOf(ket, index1);
				if (index2 == -1)
				{
					continue;
				}
				node.address = line.Substring(index1, index2 - index1);
				nodes.Add(node);
			}
		}
		return nodes;
	}
}
