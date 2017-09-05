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
	private List<Node> goToNodes;
	private Node lastGoToNode;
	
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
	
	public List<string> GetTags()
	{
		List<string> tags = new List<string>();
		ReloadIfNeed();
		string prevTag = null;
		for (int i = 0; i < lines.Length; ++i)
		{
			string line = lines[i];
			int index = line.IndexOf('\t');
			if (index > 0)
			{
				string tag = line.Substring(0, index);
				if (prevTag != tag)
				{
					tags.Add(prevTag);
					prevTag = tag;
				}
			}
		}
		return tags;
	}
	
	public void SetGoToTags(List<Node> nodes)
	{
		goToNodes = nodes;
	}
	
	public void GoToTag(Node node)
	{
		lastGoToNode = node;
		Buffer buffer = mainForm.LoadFile(node.path);
		if (buffer != null)
		{
			LineIterator iterator = buffer.Controller.Lines.GetLineRange(0, buffer.Controller.Lines.LinesCount);
			while (iterator.MoveNext())
			{
				string text = iterator.current.Text;
				if (text.StartsWith(node.address) &&
					(node.address.Length == text.Length ||
					node.address.Length == text.Length - 2 && text.EndsWith("\r\n") ||
					node.address.Length == text.Length - 1 && (text.EndsWith("\r") || text.EndsWith("\n"))))
				{
					buffer.Controller.PutCursor(new Place(0, iterator.Index), false);
					buffer.Controller.ViMoveHome(false, true);
					if (buffer.FullPath != null)
					{
						buffer.Controller.ViAddHistoryPosition(true);
					}
					buffer.Controller.NeedScrollToCaret();
					break;
				}
			}
		}
	}
	
	public void GoToNextTag()
	{
		if (goToNodes == null || goToNodes.Count == 0)
		{
			return;
		}
		if (lastGoToNode == null)
		{
			GoToTag(goToNodes[0]);
			return;
		}
		int index = goToNodes.IndexOf(lastGoToNode);
		++index;
		if (index >= goToNodes.Count)
		{
			return;
		}
		GoToTag(goToNodes[index]);
	}
	
	public void GoToPrevTag()
	{
		if (goToNodes == null || goToNodes.Count == 0)
		{
			return;
		}
		if (lastGoToNode == null)
		{
			GoToTag(goToNodes[0]);
			return;
		}
		int index = goToNodes.IndexOf(lastGoToNode);
		--index;
		if (index < 0)
		{
			return;
		}
		GoToTag(goToNodes[index]);
	}
}
