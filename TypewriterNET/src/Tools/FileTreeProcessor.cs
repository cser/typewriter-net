using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;

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

		public readonly bool isDirectory;
		public readonly string name;
		public readonly string fullPath;
		public readonly List<Node> childs = new List<Node>();
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
		KeyAction action = new KeyAction("F&ind\\Open item", DoOnEnter, null, false);
		buffer.additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
		buffer.additionKeyMap.AddItem(new KeyItem(Keys.None, null, action).SetDoubleClick(true));
	}

	private bool DoOnEnter(Controller controller)
	{
		Dictionary<int, bool> selections = new Dictionary<int, bool>();
		foreach (Selection selection in controller.Selections)
		{
			Place place0 = controller.Lines.PlaceOf(selection.anchor);
			Place place1 = controller.Lines.PlaceOf(selection.caret);
			int i0 = Math.Min(place0.iLine, place1.iLine);
			int i1 = Math.Max(place0.iLine, place1.iLine);
			for (int i = i0; i <= i1; i++)
			{
				selections[i] = selection == controller.LastSelection && i == place1.iLine;
			}
		}
		bool needRebuild = false;
		Node mainFileNode = null;
		bool fileOpened = false;
		bool directoryOpened = false;
		foreach (Node nodeI in GetNodes())
		{
			if (selections.ContainsKey(nodeI.line))
			{
				if (nodeI.isDirectory)
				{
					directoryOpened = true;
					if (nodeI.expanded)
						Collapse(nodeI);
					else
						Expand(nodeI);
					needRebuild = true;
				}
				else
				{
					fileOpened = true;
					if (selections[nodeI.line])
						mainFileNode = nodeI;
					else
						mainForm.LoadFile(nodeI.fullPath);
				}
			}
		}
		if (mainFileNode != null)
			mainForm.LoadFile(mainFileNode.fullPath);
		if (fileOpened && mainForm.MainNest.Frame != null)
			mainForm.MainNest.Frame.Focus();
		if (needRebuild)
			Rebuild();
		return fileOpened || directoryOpened;
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
		bool first = true;
		List<StyleRange> ranges = new List<StyleRange>();
		Rebuild(node, builder, "", ref line, ref first, ranges);
		buffer.Controller.InitText(builder.ToString());
		buffer.Controller.SetStyleRanges(ranges);
	}

	private void Rebuild(Node node, StringBuilder builder, string indent, ref int line, ref bool first, List<StyleRange> ranges)
	{
		if (!first)
			builder.AppendLine();
		first = false;
		string prefix = "  ";
		if (node.isDirectory)
			prefix = node.expanded ? "- " : "+ ";
		builder.Append(indent + prefix);
		if (node.isDirectory)
		{
			ranges.Add(new StyleRange(builder.Length, node.name.Length, Ds.Keyword.index));
		}
		else
		{
			string extension = Path.GetExtension(node.name).ToLowerInvariant();
			if (extension == ".exe" || extension == ".bat" || extension == ".cmd")
				ranges.Add(new StyleRange(builder.Length, node.name.Length, Ds.DataType.index));
			else if (extension == ".dll")
				ranges.Add(new StyleRange(builder.Length, node.name.Length, Ds.Function.index));
			else if (extension == ".txt" || extension == ".md" || extension == ".xml" || extension == ".ini")
				ranges.Add(new StyleRange(builder.Length, node.name.Length, Ds.String.index));
			else if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".gif")
				ranges.Add(new StyleRange(builder.Length, node.name.Length, Ds.Others.index));
			else if (node.name.StartsWith("."))
				ranges.Add(new StyleRange(builder.Length, node.name.Length, Ds.Comment.index));
		}
		builder.Append(node.name);
		node.line = line;
		line++;
		indent += "  ";
		foreach (Node child in node.childs)
		{
			Rebuild(child, builder, indent, ref line, ref first, ranges);
		}
	}
}
