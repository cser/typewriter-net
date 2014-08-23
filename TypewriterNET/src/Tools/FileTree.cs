using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;

public class FileTree
{
	public class Node
	{
		public Node(bool isDirectory, string name, string fullPath)
		{
			this.isDirectory = isDirectory;
			this.name = name;
			this.fullPath = fullPath;
			hash = fullPath.GetHashCode();
		}

		public readonly bool isDirectory;
		public readonly string name;
		public readonly string fullPath;
		public readonly int hash;
		public readonly List<Node> childs = new List<Node>();
		public bool expanded = false;
		public int line = -1;
	}

	public struct SelectionData
	{
		public int hash0;
		public int iChar0;
		public int hash1;
		public int iChar1;

		public SelectionData(int hash0, int iChar0, int hash1, int iChar1)
		{
			this.hash0 = hash0;
			this.iChar0 = iChar0;
			this.hash1 = hash1;
			this.iChar1 = iChar1;
		}
	}

	private MainForm mainForm;
	private Dictionary<int, bool> expanded;
	private Dictionary<Selection, SelectionData> selectionDatas;

	private Buffer buffer;
	public Buffer Buffer { get { return buffer; } }

	public FileTree(MainForm mainForm)
	{
		this.mainForm = mainForm;

		expanded = new Dictionary<int, bool>();
		selectionDatas = new Dictionary<Selection, SelectionData>();
		buffer = new Buffer(null, "File tree");
		buffer.OverrideWordWrap = false;
		buffer.Controller.isReadonly = true;
		buffer.additionKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("&View\\File tree\\Open item, no switch", DoOnEnterNoSwitch, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Shift | Keys.Enter, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.None, Keys.Shift, action).SetDoubleClick(true));
		}
		{
			KeyAction action = new KeyAction("&View\\File tree\\Open item", DoOnEnter, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.None, null, action).SetDoubleClick(true));
		}
		{
			KeyAction action = new KeyAction("&View\\File tree\\Set cwd", DoOnSetCwd, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Alt | Keys.Enter, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.None, Keys.Alt, action).SetDoubleClick(true));
		}
		buffer.onSelected = OnBufferSelected;
		buffer.onUpdateSettings = OnBufferUpdateSettings;
	}

	private void OnBufferSelected(Buffer buffer)
	{
		Reload();
	}

	private void OnBufferUpdateSettings(Buffer buffer, UpdatePhase phase)
	{
		if (phase == UpdatePhase.ChangeCurrentDirectory)
			Reload();
	}

	private bool DoOnEnterNoSwitch(Controller controller)
	{
		return ProcessEnter(controller, true);
	}

	private bool DoOnEnter(Controller controller)
	{
		return ProcessEnter(controller, false);
	}

	private bool DoOnSetCwd(Controller controller)
	{
		if (nodes.Count == 0)
			return false;
		Place place = controller.Lines.PlaceOf(controller.LastSelection.anchor);
		Node node = nodes[place.iLine];
		if (!node.isDirectory)
			return false;
		string error;
		if (!mainForm.SetCurrentDirectory(node.fullPath, out error))
			mainForm.Dialogs.ShowInfo("Error", error);
		return true;
	}

	private List<Node> nodes = new List<Node>();

	private bool ProcessEnter(Controller controller, bool noSwitch)
	{
		if (nodes.Count == 0)
			return false;
		Dictionary<int, bool> selections = new Dictionary<int, bool>();
		foreach (Selection selection in controller.Selections)
		{
			Place place0 = controller.Lines.PlaceOf(selection.anchor);
			Place place1 = controller.Lines.PlaceOf(selection.caret);
			selectionDatas[selection] = new SelectionData(
				nodes[place0.iLine].hash, place0.iChar,
				nodes[place1.iLine].hash, place1.iChar);
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
		foreach (Node nodeI in nodes)
		{
			if (selections.ContainsKey(nodeI.line))
			{
				if (nodeI.isDirectory)
				{
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
		if (needRebuild)
		{
			Rebuild();
			List<Selection> selectionsToRemove = new List<Selection>();
			Dictionary<int, Node> nodeOf = new Dictionary<int, Node>();
			foreach (Node nodeI in nodes)
			{
				nodeOf[nodeI.hash] = nodeI;
			}
			foreach (Selection selection in controller.Selections)
			{
				SelectionData selectionData;
				if (!selectionDatas.TryGetValue(selection, out selectionData))
				{
					selectionsToRemove.Add(selection);
					continue;
				}
				Node node0;
				nodeOf.TryGetValue(selectionData.hash0, out node0);
				Node node1;
				nodeOf.TryGetValue(selectionData.hash1, out node1);
				if (node0 == null && node1 == null)
				{
					selectionsToRemove.Add(selection);
					continue;
				}
				if (node0 != null && node1 == null)
				{
					selection.anchor = selection.caret = controller.Lines.IndexOf(new Place(node0.line, selectionData.iChar0));
				}
				else if (node0 == null && node1 != null)
				{
					selection.anchor = selection.caret = controller.Lines.IndexOf(new Place(node1.line, selectionData.iChar1));
				}
				else
				{
					selection.anchor = controller.Lines.IndexOf(new Place(selectionData.iChar0, node0.line));
					selection.caret = controller.Lines.IndexOf(new Place(selectionData.iChar1, node1.line));
				}
			}
		}
		if (!noSwitch && fileOpened && mainForm.MainNest.Frame != null)
			mainForm.MainNest.Frame.Focus();
		return true;
	}

	private Node node;

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
				Node nodeI = new Node(true, Path.GetFileName(file), Path.GetFullPath(file));
				if (expanded.ContainsKey(nodeI.hash))
					Expand(nodeI);
				node.childs.Add(nodeI);
			}
			foreach (string file in Directory.GetFiles(node.fullPath))
			{
				Node nodeI = new Node(false, Path.GetFileName(file), Path.GetFullPath(file));
				if (expanded.ContainsKey(nodeI.hash))
					Expand(nodeI);
				node.childs.Add(nodeI);
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
		nodes.Clear();
		StringBuilder builder = new StringBuilder();
		bool first = true;
		List<StyleRange> ranges = new List<StyleRange>();
		Rebuild(node, builder, "", ref first, ranges);
		buffer.Controller.InitText(builder.ToString());
		buffer.Controller.SetStyleRanges(ranges);
	}

	private void Rebuild(Node node, StringBuilder builder, string indent, ref bool first, List<StyleRange> ranges)
	{
		if (!first)
			builder.AppendLine();
		if (node.isDirectory)
		{
			if (node.expanded)
				expanded[node.hash] = true;
			else
				expanded.Remove(node.hash);
		}
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
		node.line = nodes.Count;
		nodes.Add(node);
		indent += "  ";
		foreach (Node child in node.childs)
		{
			Rebuild(child, builder, indent, ref first, ranges);
		}
	}
}
