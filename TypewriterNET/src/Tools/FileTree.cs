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
	public enum NodeType
	{
		None, Directory, File, Error
	}

	public class Node
	{
		public Node(NodeType type, string name, string fullPath)
		{
			this.type = type;
			this.name = name;
			this.fullPath = fullPath;
			hash = fullPath.GetHashCode();
		}

		public readonly NodeType type;
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
		buffer = new Buffer(null, "File tree", SettingsMode.FileTree);
		buffer.showEncoding = false;
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
			KeyAction action = new KeyAction("&View\\File tree\\Set current directory", DoOnSetCurrentDirectory, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Alt | Keys.Enter, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.None, Keys.Alt, action).SetDoubleClick(true));
		}
		{
			KeyAction action = new KeyAction("&View\\File tree\\Close file tree", CloseBuffer, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Escape, null, action));
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

	private bool DoOnSetCurrentDirectory(Controller controller)
	{
		if (nodes.Count == 0)
			return false;
		Place place = controller.Lines.PlaceOf(controller.LastSelection.anchor);
		Node node = nodes[place.iLine];
		if (node.type != NodeType.Directory)
			return false;
		SetCurrentDirectory(node);
		return true;
	}

	private void SetCurrentDirectory(Node node)
	{
		SetCurrentDirectory(node.fullPath);
	}

	private void SetCurrentDirectory(string path)
	{
		string error;
		if (!mainForm.SetCurrentDirectory(path, out error))
			mainForm.Dialogs.ShowInfo("Error", error);
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
				if (nodeI.type == NodeType.Directory)
				{
					if (nodeI.fullPath == "..")
					{
						SetCurrentDirectory(nodeI);
						return true;
					}
					if (nodeI.expanded)
						Collapse(nodeI);
					else
						Expand(nodeI);
					needRebuild = true;
				}
				else if (nodeI.type == NodeType.File)
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
	private string currentDirectory;

	public void Reload()
	{
		currentDirectory = Directory.GetCurrentDirectory();
		string root = currentDirectory;
		node = new Node(NodeType.Directory, Path.GetFileName(root), Path.GetFullPath(root));
		Expand(node);
		Rebuild();
		if (!expandedTemp.Equals(SValue.None))
		{
			SetExpandedTemp(expandedTemp);
			expandedTemp = SValue.None;
		}
	}

	public void Find(string fullPath)
	{
		if (!fullPath.StartsWith(currentDirectory))
		{
			string dir = Path.GetDirectoryName(fullPath);
			SetCurrentDirectory(dir);
		}
		ExpandTo(node, fullPath);
		Rebuild();
		for (int i = 0, count = nodes.Count; i < count; i++)
		{
			Node nodeI = nodes[i];
			if (nodeI.fullPath == fullPath)
			{
				buffer.Controller.ClearMinorSelections();
				Place place = new Place(0, i);
				buffer.Controller.LastSelection.anchor = buffer.Controller.LastSelection.caret = buffer.Controller.Lines.IndexOf(place);
				buffer.Controller.NeedScrollToCaret();
				break;
			}
		}
	}

	private void ExpandTo(Node node, string fullPath)
	{
		if (!fullPath.StartsWith(node.fullPath))
			return;
		if (node.fullPath == fullPath)
			return;
		if (!node.expanded)
		{
			Expand(node);
			return;
		}
		foreach (Node nodeI in node.childs)
		{
			if (fullPath.StartsWith(nodeI.fullPath))
			{
				ExpandTo(nodeI, fullPath);
				break;
			}
		}
	}

	private void Expand(Node node)
	{
		if (node.type == NodeType.Directory)
		{
			node.expanded = true;
			node.childs.Clear();
			string[] directories = null;
			string[] files = null;
			try
			{
				directories = Directory.GetDirectories(node.fullPath);
				files = Directory.GetFiles(node.fullPath);
			}
			catch (Exception e)
			{
				node.childs.Add(new Node(NodeType.Error, CommonHelper.GetOneLine(e.Message), node.fullPath + "#error"));
				return;
			}
			foreach (string file in directories)
			{
				Node nodeI = new Node(NodeType.Directory, Path.GetFileName(file), Path.GetFullPath(file));
				if (expanded.ContainsKey(nodeI.hash))
					Expand(nodeI);
				node.childs.Add(nodeI);
			}
			foreach (string file in files)
			{
				Node nodeI = new Node(NodeType.File, Path.GetFileName(file), Path.GetFullPath(file));
				if (expanded.ContainsKey(nodeI.hash))
					Expand(nodeI);
				node.childs.Add(nodeI);
			}
		}
	}

	private void Collapse(Node node)
	{
		if (node.type == NodeType.Directory)
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

		int charsCount = buffer.Controller.Lines.charsCount;
		foreach (Selection selection in buffer.Controller.Selections)
		{
			if (selection.anchor > charsCount)
				selection.anchor = charsCount;
			if (selection.caret > charsCount)
				selection.caret = charsCount;
		}
		buffer.Controller.JoinSelections();
	}

	private void Rebuild(Node node, StringBuilder builder, string indent, ref bool first, List<StyleRange> ranges)
	{
		if (!first)
			builder.AppendLine();
		if (node.type == NodeType.Directory)
		{
			if (node.expanded)
				expanded[node.hash] = true;
			else
				expanded.Remove(node.hash);
		}
		string prefix = "";
		if (!first)
		{
			prefix = "  ";
			if (node.type == NodeType.Directory)
				prefix = node.expanded ? "- " : "+ ";
			builder.Append(indent + prefix);
			if (node.type == NodeType.Directory)
			{
				ranges.Add(new StyleRange(builder.Length, node.name.Length, Ds.Keyword.index));
			}
			else if (node.type == NodeType.File)
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
			else if (node.type == NodeType.Error)
			{
				ranges.Add(new StyleRange(builder.Length, node.name.Length, Ds.Error.index));
			}
			builder.Append(node.name);
			node.line = nodes.Count;
			nodes.Add(node);
		}
		else
		{
			Node upNode = new Node(NodeType.Directory, "..", "..");
			upNode.line = nodes.Count;
			nodes.Add(upNode);
			builder.Append(upNode.name);

			builder.AppendLine();

			Node currentDirectoryNode = new Node(NodeType.Directory, node.name, "");
			currentDirectoryNode.line = nodes.Count;
			nodes.Add(currentDirectoryNode);
			ranges.Add(new StyleRange(builder.Length, currentDirectory.Length, Ds.Constructor.index));
			builder.Append(currentDirectory);
		}
		if (!first)
			indent += "  ";
		first = false;
		foreach (Node child in node.childs)
		{
			Rebuild(child, builder, indent, ref first, ranges);
		}
	}

	private bool CloseBuffer(Controller controller)
	{
		if (buffer != null && buffer.Frame != null)
			buffer.Frame.RemoveBuffer(buffer);
		return true;
	}

	public SValue GetExpandedTemp()
	{
		SValue value = SValue.NewList();
		foreach (Node nodeI in nodes)
		{
			if (nodeI.expanded)
			{
				value.Add(SValue.NewInt(nodeI.hash));
			}
		}
		return value;
	}

	private SValue expandedTemp = SValue.None;

	public void SetExpandedTemp(SValue value)
	{
		if (node == null)
		{
			expandedTemp = value;
			return;
		}
		Dictionary<int, bool> expanded = new Dictionary<int, bool>();
		foreach (SValue valueI in value.List)
		{
			expanded[valueI.Int] = true;
		}
		ExpandCollection(node, expanded);
		Rebuild();
	}

	private void ExpandCollection(Node node, Dictionary<int, bool> expanded)
	{
		if ((!node.expanded) && expanded.ContainsKey(node.hash))
			Expand(node);
		foreach (Node nodeI in node.childs)
		{
			ExpandCollection(nodeI, expanded);
		}
	}
}
