using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;

public class FileTreeProcessor
{
	public class Node
	{
		public Node(bool isDirectory, string name, string fullPath)
		{
			this.isDirectory = isDirectory;
			this.name = name;
			this.fullPath = fullPath;
		}

		public bool isDirectory;
		public string name;
		public string fullPath;
		public List<Node> childs = new List<Node>();
		public bool expanded = false;
		public int line = 0;
	}

	private MainForm mainForm;

	private Buffer buffer;
	public Buffer Buffer { get { return buffer; } }

	public FileTreeProcessor(MainForm mainForm)
	{
		this.mainForm = mainForm;

		buffer = new Buffer(null, "File tree");
		buffer.Controller.isReadonly = true;
		buffer.additionKeyMap = new KeyMap();
		buffer.additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, new KeyAction("F&ind\\Open item", DoOnEnter, null, false)));
	}

	private bool DoOnEnter(Controller controller)
	{
		Place place = controller.Lines.PlaceOf(controller.LastSelection.anchor);
		foreach (Node nodeI in GetNodes())
		{
			if (nodeI.line == place.iLine)
			{
				if (nodeI.isDirectory)
				{
					if (nodeI.expanded)
						Collapse(nodeI);
					else
						Expand(nodeI);
					Rebuild();
				}
				else
				{
					mainForm.LoadFile(nodeI.fullPath);
					if (mainForm.MainNest.Frame != null)
						mainForm.MainNest.Frame.Focus();
				}
				return true;
			}
		}
		return false;
	}

	private Node node;

	private IEnumerable<Node> GetNodes()
	{
		if (node != null)
		{
			Stack<Node> nodes = new Stack<Node>();
			nodes.Push(node);
			while (nodes.Count > 0)
			{
				Node nodeI = nodes.Pop();
				yield return nodeI;
				foreach (Node nodeJ in nodeI.childs)
				{
					nodes.Push(nodeJ);
				}
			}
		}
	}

	public void Reload()
	{
		string root = Directory.GetCurrentDirectory();
		node = new Node(true, Path.GetFileName(root), Path.GetFullPath(root));
		Expand(node);
		Rebuild();
	}

	private void Expand(Node node)
	{
		if (node.isDirectory)
		{
			node.expanded = true;
			node.childs.Clear();
			foreach (string file in Directory.GetDirectories(node.fullPath))
			{
				node.childs.Add(new Node(true, Path.GetFileName(file), Path.GetFullPath(file)));
			}
			foreach (string file in Directory.GetFiles(node.fullPath))
			{
				node.childs.Add(new Node(false, Path.GetFileName(file), Path.GetFullPath(file)));
			}
		}
	}

	private void Collapse(Node node)
	{
		if (node.isDirectory)
		{
			node.expanded = false;
			node.childs.Clear();
		}
	}

	private void Rebuild()
	{
		StringBuilder builder = new StringBuilder();
		int line = 0;
		Rebuild(node, builder, "", ref line);
		buffer.Controller.InitText(builder.ToString());
	}

	private void Rebuild(Node node, StringBuilder builder, string indent, ref int line)
	{
		string prefix = "  ";
		if (node.isDirectory)
			prefix = node.expanded ? "- " : "+ ";
		builder.AppendLine(indent + prefix + node.name);
		node.line = line;
		line++;
		indent += "  ";
		foreach (Node child in node.childs)
		{
			Rebuild(child, builder, indent, ref line);
		}
	}
}
