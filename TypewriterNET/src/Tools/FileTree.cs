using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;
using System.Threading;
using System.Collections.Specialized;

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
        
        ResetReload();
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
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Alt | Keys.Space, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.C, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.None, Keys.Alt, action).SetDoubleClick(true));
		}
		{
			KeyAction action = new KeyAction("&View\\File tree\\Close file tree", DoCloseBuffer, null, false);
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Escape, null, action));
			buffer.additionKeyMap.AddItem(new KeyItem(Keys.Control | Keys.OemOpenBrackets, null, action));
		}
		buffer.additionBeforeKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("&View\\File tree\\Remove item", DoRemoveItem, null, false);
			buffer.additionBeforeKeyMap.AddItem(new KeyItem(Keys.Delete, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\File tree\\Add item", DoAddItem, null, false);
			buffer.additionBeforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.N, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\File tree\\Add directory", DoAddDir, null, false);
			buffer.additionBeforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.N, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\File tree\\Move item", DoMoveItem, null, false);
			buffer.additionBeforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.M, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\File tree\\Rename item", DoRenameItem, null, false);
			buffer.additionBeforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.R, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\File tree\\Rename item", DoRenameItem, null, false);
			buffer.additionBeforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.R, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\File tree\\Copy item to clipboard", DoCopyToClipboard, null, false);
			buffer.additionBeforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.C, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\File tree\\Cut item to clipboard", DoCutToClipboard, null, false);
			buffer.additionBeforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.X, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\File tree\\Paste from clipboard", DoPasteFromClipboard, null, false);
			buffer.additionBeforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.V, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\File tree\\Select item", DoSelectItem, null, false);
			buffer.additionBeforeKeyMap.AddItem(new KeyItem(Keys.Tab, null, action));
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
	    wasReloaded = true;
	    ResetReload();
		currentDirectory = Directory.GetCurrentDirectory();
		string root = currentDirectory;
		node = new Node(NodeType.Directory, Path.GetFileName(root), Path.GetFullPath(root));
		Expand(node);
		if (!expandedTemp.IsNone)
		{
			SetExpandedTemp(expandedTemp);
			expandedTemp = SValue.None;
		}
		else
		{
			Rebuild();
		}
	}

	public bool Find(string fullPath)
	{
		fullPath = fullPath.ToLowerInvariant();
		if (!fullPath.StartsWith(currentDirectory.ToLowerInvariant()))
		{
			string dir = Path.GetDirectoryName(fullPath);
			SetCurrentDirectory(dir);
		}
		ExpandTo(node, fullPath);
		Rebuild();
		if (TrySelectPath(fullPath))
		{
			return true;
		}
		Reload();
		ExpandTo(node, fullPath);
		if (TrySelectPath(fullPath))
		{
			return true;
		}
		return false;
	}
	
	private bool TrySelectPath(string fullPath)
	{
		for (int i = 0, count = nodes.Count; i < count; i++)
		{
			Node nodeI = nodes[i];
			if (nodeI.fullPath.ToLowerInvariant() == fullPath)
			{
				buffer.Controller.ClearMinorSelections();
				Place place = new Place(0, i);
				buffer.Controller.LastSelection.anchor = buffer.Controller.LastSelection.caret = buffer.Controller.Lines.IndexOf(place);
				buffer.Controller.NeedScrollToCaret();
				return true;
			}
		}
		return false;
	}

	private void ExpandTo(Node node, string fullPath)
	{
		if (!fullPath.StartsWith(node.fullPath.ToLowerInvariant()))
			return;
		if (node.fullPath.ToLowerInvariant() == fullPath)
			return;
		if (!node.expanded)
		{
			Expand(node);
		}
		foreach (Node nodeI in node.childs)
		{
			string nodeFullPath = !nodeI.fullPath.EndsWith("\\") ? nodeI.fullPath + "\\" : nodeI.fullPath;
			if (fullPath.StartsWith(nodeFullPath.ToLowerInvariant()))
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
			List<string> directories = null;
			List<string> files = null;
			try
			{
				string[] rawDirectories = Directory.GetDirectories(node.fullPath);
				string[] rawFiles = Directory.GetFiles(node.fullPath);
				directories = new List<string>();
				files = new List<string>();
				FileNameFilter filter = !string.IsNullOrEmpty(hideInFileTree) ?
				    new FileNameFilter(hideInFileTree) : null;
				foreach (string directory in rawDirectories)
				{
				    if (filter == null || !filter.Match(Path.GetFileName(directory)))
				        directories.Add(directory);
				}
				foreach (string file in rawFiles)
				{
				    if (filter == null || !filter.Match(Path.GetFileName(file)))
				        files.Add(file);
				}
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

			Node currentDirectoryNode = new Node(NodeType.Directory, node.name, node.fullPath);
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

	private bool DoCloseBuffer(Controller controller)
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
		ExpandCollection(node, expanded, "");
		Rebuild();
	}

	private void ExpandCollection(Node node, Dictionary<int, bool> expanded, string indent)
	{
		if ((!node.expanded) && expanded.ContainsKey(node.hash))
			Expand(node);
		foreach (Node nodeI in node.childs)
		{
			ExpandCollection(nodeI, expanded, indent + "  ");
		}
	}

	private List<Node> GetFilesAndDirs(Controller controller)
	{
		List<int> indices = controller.Lines.GetSelectionIndices();
		Dictionary<Node, bool> nodesHash = new Dictionary<Node, bool>();
		List<Node> nodesToRemove = new List<Node>();
		foreach (int index in indices)
		{
			nodesHash[nodes[index]] = true;
			nodesToRemove.Add(nodes[index]);
		}
		foreach (Node nodeI in nodesToRemove)
		{
			foreach (Node nodeJ in nodeI.childs)
			{
				nodesHash.Remove(nodeJ);
			}
		}
		List<Node> result = new List<Node>();
		foreach (KeyValuePair<Node, bool> pair in nodesHash)
		{
			result.Add(pair.Key);
		}
		return result;
	}
	
	private Node GetOneFileOrDir(Controller controller)
	{
		Selection selection = controller.LastSelection;
		Place place = controller.Lines.PlaceOf(selection.caret);
		return place.iLine >= 0 && place.iLine < nodes.Count ? nodes[place.iLine] : null;
	}
	
	private List<Node> GetFilesAndDirsHard(Controller controller)
	{
		List<Node> result = new List<Node>();
		foreach (int index in controller.Lines.GetSelectionIndices())
		{
			result.Add(nodes[index]);
		}
		return result;
	}

	private bool DoRemoveItem(Controller controller)
	{
		List<Node> filesAndDirs = GetFilesAndDirs(controller);
		int count = 0;
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("Remove " + (filesAndDirs.Count > 1 ? "items" : "item") + "?");
		foreach (Node nodeI in filesAndDirs)
		{
			count++;
			if (count > 10)
			{
				builder.AppendLine("…");
				break;
			}
			builder.AppendLine(nodeI.fullPath);
		}

		DialogResult result = MessageBox.Show(builder.ToString(), mainForm.Name, MessageBoxButtons.YesNo);
		if (result == DialogResult.Yes)
		{
			foreach (Node nodeI in filesAndDirs)
			{
				bool isFileDeleted = false;
				try
				{
					if (nodeI.type == NodeType.Directory)
					{
						Directory.Delete(nodeI.fullPath, true);
					}
					else if (nodeI.type == NodeType.File)
					{
						File.Delete(nodeI.fullPath);
						isFileDeleted = true;
					}
				}
				catch (Exception e)
				{
					mainForm.Log.WriteError("Remove error", e.Message);
					mainForm.Log.Open();
				}
				if (isFileDeleted)
				{
					Buffer buffer = mainForm.GetBuffer(nodeI.fullPath);
					if (buffer != null)
					{
						buffer.fileInfo = null;
						buffer.unsaved = true;
					}
				}
			}
			mainForm.UpdateAfterFileRenamed();
			Reload();
		}
		return true;
	}

	private bool DoAddItem(Controller controller)
	{
		mainForm.Dialogs.OpenInput("Add item", "unnamed.txt", DoInputItemName);
		return true;
	}

	private bool DoInputItemName(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			return true;
		mainForm.Dialogs.CloseInput();
		if (fileName.EndsWith("\\") || fileName.EndsWith("/"))
		{
			return DoInputDirName(fileName.Substring(0, fileName.Length - 1));
		}
		if (fileName.Contains("\\") || fileName.Contains("/"))
		{
			mainForm.Dialogs.ShowInfo("Error", "Slashes unsupported");
			return true;
		}
		List<int> indices = this.buffer.Controller.Lines.GetSelectionIndices();
		Dictionary<string, bool> dirsSet = new Dictionary<string, bool>();
		List<string> fullPaths = new List<string>();
		Dictionary<string, bool> fullPathsSet = new Dictionary<string, bool>();
		List<Node> unexpandedNodes = new List<Node>();
		foreach (int index in indices)
		{
			Node nodeI = nodes[index];
			string dir = null;
			if (nodeI.type == NodeType.Directory)
			{
				dir = nodeI.fullPath;
				if (!nodeI.expanded)
					unexpandedNodes.Add(nodeI);
			}
			else if (nodeI.type == NodeType.File)
			{
				dir = Path.GetDirectoryName(nodeI.fullPath);
			}
			if (dir != null && !dirsSet.ContainsKey(dir))
			{
				string fullPath = Path.Combine(dir, fileName);
				dirsSet[dir] = true;
				fullPaths.Add(fullPath);
				fullPathsSet[NormalizedWithoutSlash(fullPath)] = true;
			}
		}
		foreach (Node node in unexpandedNodes)
		{
			Expand(node);
		}
		Rebuild();
		foreach (string fullPath in fullPaths)
		{
			Buffer buffer = mainForm.ForcedLoadFile(fullPath);
			buffer.needSaveAs = false;
			mainForm.SaveFileOnAdd(buffer);
		}
		Reload();
		this.buffer.Controller.ClearMinorSelections();
		bool first = true;
		for (int i = 0, count = nodes.Count; i < count; i++)
		{
			Node nodeI = nodes[i];
			if (fullPathsSet.ContainsKey(NormalizedWithoutSlash(nodeI.fullPath)))
			{
				Place place = new Place(0, i);
				if (first)
				{
					first = false;
					this.buffer.Controller.PutCursor(place, false);
				}
				else
				{
					this.buffer.Controller.PutNewCursor(place);
				}
			}
		}
		this.buffer.Controller.NeedScrollToCaret();
		return true;
	}
	
	private static string NormalizedWithoutSlash(string fullPath)
	{
		if (fullPath == null)
			return null;
		if (fullPath.EndsWith("\\"))
			fullPath = fullPath.Substring(0, fullPath.Length);
		return fullPath.ToLowerInvariant();
	}

	private bool DoAddDir(Controller controller)
	{
		mainForm.Dialogs.OpenInput("Add directory", "unnamed", DoInputDirName);
		return true;
	}

	private bool DoInputDirName(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			return true;
		mainForm.Dialogs.CloseInput();
		List<int> indices = this.buffer.Controller.Lines.GetSelectionIndices();
		Dictionary<string, bool> dirsSet = new Dictionary<string, bool>();
		List<string> fullPaths = new List<string>();
		Dictionary<string, bool> fullPathsSet = new Dictionary<string, bool>();
		List<Node> unexpandedNodes = new List<Node>();
		foreach (int index in indices)
		{
			Node nodeI = nodes[index];
			string dir = null;
			if (nodeI.type == NodeType.Directory)
			{
				dir = nodeI.fullPath;
				if (!nodeI.expanded)
					unexpandedNodes.Add(nodeI);
			}
			else if (nodeI.type == NodeType.File)
			{
				dir = Path.GetDirectoryName(nodeI.fullPath);
			}
			if (dir != null && !dirsSet.ContainsKey(dir))
			{
				string fullPath = Path.Combine(dir, fileName);
				dirsSet[dir] = true;
				fullPaths.Add(fullPath);
				fullPathsSet[NormalizedWithoutSlash(fullPath)] = true;
			}
		}
		foreach (Node node in unexpandedNodes)
		{
			Expand(node);
		}
		Rebuild();
		foreach (string fullPath in fullPaths)
		{
			try
			{
				Directory.CreateDirectory(fullPath);
			}
			catch (Exception e)
			{
				mainForm.Log.WriteError("Add directory error", e.Message);
				mainForm.Log.Open();
			}
		}
		Reload();
		this.buffer.Controller.ClearMinorSelections();
		bool first = true;
		for (int i = 0, count = nodes.Count; i < count; i++)
		{
			Node nodeI = nodes[i];
			if (fullPathsSet.ContainsKey(NormalizedWithoutSlash(nodeI.fullPath)))
			{
				Place place = new Place(0, i);
				if (first)
				{
					first = false;
					this.buffer.Controller.PutCursor(place, false);
				}
				else
				{
					this.buffer.Controller.PutNewCursor(place);
				}
			}
		}
		this.buffer.Controller.NeedScrollToCaret();
		return true;
	}

	private string GetRelativePath(string fullPath)
	{
		return fullPath.StartsWith(currentDirectory) && fullPath.Length >= currentDirectory.Length + 1 ?
			fullPath.Substring(currentDirectory.Length + 1) : fullPath;
	}

	private bool DoMoveItem(Controller controller)
	{
		Place place = controller.Lines.PlaceOf(controller.LastSelection.caret);
		int index = place.iLine;
		Node node = nodes[index];
		string path = Path.GetDirectoryName(GetRelativePath(node.fullPath));
		mainForm.Dialogs.OpenMove("Move item", path, DoInputNewDir);
		return true;
	}

	private bool DoInputNewDir(string fileName)
	{
		if (fileName == "" || fileName == ".")
			fileName = Directory.GetCurrentDirectory();
		fileName = Path.GetFullPath(fileName);
		if (!Directory.Exists(fileName))
		{
			mainForm.Log.WriteError("Move error", "No directory: " + fileName);
			mainForm.Log.Open();
			return true;
		}
		mainForm.Dialogs.CloseInput();
		List<Node> filesAndDirs = GetFilesAndDirs(buffer.Controller);
		PathSet newFullPaths = new PathSet();
		PathSet oldPostfixed = new PathSet();
		foreach (Node nodeI in filesAndDirs)
		{
			if (!string.IsNullOrEmpty(renamePostfixed))
			{
				oldPostfixed.Add(nodeI.fullPath + renamePostfixed);
			}
		}
		foreach (Node nodeI in filesAndDirs)
		{
			if (!string.IsNullOrEmpty(renamePostfixed))
			{
				oldPostfixed.Remove(nodeI.fullPath);
			}
		}
		foreach (Node nodeI in filesAndDirs)
		{
			try
			{
				if (nodeI.type == NodeType.Directory)
				{
					DirectoryMove(nodeI.fullPath, newFullPaths.Add(Path.Combine(fileName, Path.GetFileName(nodeI.fullPath))));
				}
				else if (nodeI.type == NodeType.File)
				{
					FileMove(nodeI.fullPath, newFullPaths.Add(Path.Combine(fileName, Path.GetFileName(nodeI.fullPath))));
				}
				if (!string.IsNullOrEmpty(renamePostfixed) && oldPostfixed.Contains(nodeI.fullPath + renamePostfixed) &&
					File.Exists(nodeI.fullPath + renamePostfixed))
				{
				    FileMove(nodeI.fullPath + renamePostfixed, Path.Combine(fileName, Path.GetFileName(nodeI.fullPath) + renamePostfixed));
				}
			}
			catch (IOException e)
			{
				mainForm.Log.WriteError("Move error", e.Message);
				mainForm.Log.Open();
			}
		}
		Reload();
		PutCursors(newFullPaths);
		return true;
	}
	
	private bool DoRenameItem(Controller controller)
	{
		List<int> indices = controller.Lines.GetSelectionIndices();
		List<Node> nodes = new List<Node>();
		StringBuilder builder = new StringBuilder();
		List<bool> isDirectory = new List<bool>();
		bool first = true;
		foreach (int index in indices)
		{
			Node node = this.nodes[index];
			if (node.type == NodeType.File || node.type == NodeType.Directory)
			{
				if (!first)
					builder.AppendLine();
				builder.Append(node.name);
				nodes.Add(node);
				isDirectory.Add(node.type == NodeType.Directory);
				first = false;
			}
		}
		string text = builder.ToString();
		if (text == null)
		{
			return true;
		}
		mainForm.Dialogs.OpenRename("Rename item", text, isDirectory, DoInputNewFileName);
		return true;
	}
	
	private void FileMove(string oldFile, string newFile)
	{
		File.Move(oldFile, newFile);
		Buffer buffer = mainForm.GetBuffer(oldFile);
		if (buffer != null)
			buffer.SetFile(newFile, Path.GetFileName(newFile));
		MulticaretTextBox.initMacrosExecutor.ViRenameFile(oldFile, newFile);
	}
	
	private void DirectoryMove(string oldDir, string newDir)
	{
		Directory.Move(oldDir, newDir);
		if (oldDir.EndsWith("/"))
			oldDir = oldDir.Substring(0, oldDir.Length - 1) + "\\";
		if (!oldDir.EndsWith("\\"))
			oldDir += "\\";
		if (newDir.EndsWith("/"))
			newDir = newDir.Substring(0, newDir.Length - 1) + "\\";
		if (!newDir.EndsWith("\\"))
			newDir += "\\";
		foreach (Buffer buffer in mainForm.GetFileBuffers())
		{
			if (!string.IsNullOrEmpty(buffer.FullPath) && buffer.FullPath.StartsWith(oldDir))
			{
				string newFile = newDir + buffer.FullPath.Substring(oldDir.Length);
				buffer.SetFile(newFile, Path.GetFileName(newFile));
			}
		}
	}
	
	private bool DoInputNewFileName(string newText)
	{
		if (string.IsNullOrEmpty(newText))
			return true;
		string[] newFileNames = newText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
		List<Node> filesAndDirs = GetFilesAndDirsHard(buffer.Controller);
		if (newFileNames.Length != filesAndDirs.Count)
			return true;
		for (int i = 0; i < newFileNames.Length; i++)
		{
			if (filesAndDirs[i].fullPath == ".." || newFileNames[i] == "..")
				return true;
			newFileNames[i] = newFileNames[i].TrimStart();
			if (string.IsNullOrEmpty(newFileNames[i]))
				return true;
		}
		mainForm.Dialogs.CloseRename();
		PathSet oldPostfixed = new PathSet();
		PathSet newFullPaths = new PathSet();
		List<KeyValuePair<Node, string>> pairs = new List<KeyValuePair<Node, string>>();
		for (int i = 0; i < filesAndDirs.Count; i++)
		{
			Node nodeI = filesAndDirs[i];
			string fileName = newFileNames[i];
			pairs.Add(new KeyValuePair<Node, string>(nodeI, fileName));
			if (!string.IsNullOrEmpty(renamePostfixed))
			{
				oldPostfixed.Add(filesAndDirs[i].fullPath + renamePostfixed);
			}
		}
		foreach (KeyValuePair<Node, string> pair in pairs)
		{
			Node nodeI = pair.Key;
			string fileName = pair.Value;
			if (!string.IsNullOrEmpty(renamePostfixed))
			{
				oldPostfixed.Remove(nodeI.fullPath);
			}
		}
		List<List<KeyValuePair<Node, string>>> levels = new List<List<KeyValuePair<Node, string>>>();
		foreach (KeyValuePair<Node, string> pair in pairs)
		{
			int level = GetPartsLevel(pair);
			if (level >= levels.Count)
			{
				while (levels.Count < level + 1)
				{
					levels.Add(null);
				}
			}
			if (levels[level] == null)
			{
				levels[level] = new List<KeyValuePair<Node, string>>();
			}
			levels[level].Add(pair);
		}
		for (int i = levels.Count; i-- > 0;)
		{
			List<KeyValuePair<Node, string>> pairsI = levels[i];
			if (pairsI == null)
				continue;
			foreach (KeyValuePair<Node, string> pair in pairsI)
			{
				Node nodeI = pair.Key;
				string fileName = pair.Value;
				if (nodeI.type == NodeType.File || nodeI.type == NodeType.Directory)
				try
				{
					if (nodeI.type == NodeType.File)
					{
						FileMove(nodeI.fullPath,
							newFullPaths.Add(Path.Combine(Path.GetDirectoryName(nodeI.fullPath), fileName)));
					}
					else if (nodeI.type == NodeType.Directory)
					{
						DirectoryMove(nodeI.fullPath,
							newFullPaths.AddDirectory(Path.Combine(Path.GetDirectoryName(nodeI.fullPath), fileName), nodeI.fullPath));
					}
					if (!string.IsNullOrEmpty(renamePostfixed) && oldPostfixed.Contains(nodeI.fullPath + renamePostfixed) &&
						File.Exists(nodeI.fullPath + renamePostfixed))
					{
						FileMove(nodeI.fullPath + renamePostfixed,
							Path.Combine(Path.GetDirectoryName(nodeI.fullPath), fileName + renamePostfixed));
					}
				}
				catch (IOException e)
				{
					mainForm.Log.WriteError("Rename error", e.Message);
					mainForm.Log.Open();
					break;
				}
			}
		}
		mainForm.UpdateAfterFileRenamed();
		Reload();
		PutCursors(newFullPaths);
		return true;
	}
	
	private static int GetPartsLevel(KeyValuePair<Node, string> pair)
	{
		string path = pair.Key.fullPath;
		if (string.IsNullOrEmpty(path))
			return 0;
		if (path.EndsWith("\\"))
			path = path.Substring(0, path.Length - 1);
		path = path.Replace("/", "\\");
		return CommonHelper.MatchesCount(path, '\\') + 1;
	}
	
	private void PutCursors(PathSet newFullPaths)
	{
		foreach (string pathI in newFullPaths.NormalizedPaths)
		{
			ExpandTo(node, pathI);
		}
		Rebuild();
		buffer.Controller.ClearMinorSelections();
		bool first = true;
		for (int i = 0, count = nodes.Count; i < count; i++)
		{
			Node nodeI = nodes[i];
			if (newFullPaths.Contains(nodeI.fullPath))
			{
				if (first)
				{
					buffer.Controller.PutCursor(new Place(0, i), false);
				}
				else
				{
					buffer.Controller.PutNewCursor(new Place(0, i));
				}
				first = false;
			}
		}
		buffer.Controller.NeedScrollToCaret();
	}
	
	private bool wasReloaded;
	private string renamePostfixed;
	private bool pastePostfixedAfterCopy;
	private string hideInFileTree;
	
	private void ResetReload()
	{
	    renamePostfixed = mainForm.Settings.renamePostfixed.Value + "";
	    pastePostfixedAfterCopy = mainForm.Settings.pastePostfixedAfterCopy.Value;
	    hideInFileTree = mainForm.Settings.hideInFileTree.Value + "";
	}
	
	public void ReloadIfNeedForSettings()
	{
	    if (wasReloaded && (
	        renamePostfixed != mainForm.Settings.renamePostfixed.Value + "" ||
	        pastePostfixedAfterCopy != mainForm.Settings.pastePostfixedAfterCopy.Value ||
	        hideInFileTree != mainForm.Settings.hideInFileTree.Value + ""
	    ))
        {
            renamePostfixed = mainForm.Settings.renamePostfixed.Value + "";
	        pastePostfixedAfterCopy = mainForm.Settings.pastePostfixedAfterCopy.Value;
	        hideInFileTree = mainForm.Settings.hideInFileTree.Value + "";
            Reload();
        }
	}
	
	public string GetCurrentFile()
	{
	    if (nodes.Count > 0)
	    {
            Place place = buffer.Controller.Lines.PlaceOf(buffer.Controller.LastSelection.anchor);
            Node node = nodes[place.iLine];
            if (node.type == NodeType.File || node.type == NodeType.Directory)
            {
                return node.fullPath;
            }
        }
		return null;
	}
	
	public bool DoOnViShortcut(Controller controller, string shortcut)
	{
		if (buffer.Controller == controller)
		{
			if (shortcut == "o")
			{
				return DoOnEnter(controller);
			}
			if (shortcut == "O")
			{
				return DoOnEnterNoSwitch(controller);
			}
			if (shortcut == "dd" || shortcut == "v_d" || shortcut == "v_x")
			{
				return DoRemoveItem(controller);
			}
		}
		return false;
	}
	
	public bool IsFolderOpen(string dir)
	{
		if (string.IsNullOrEmpty(dir))
		{
			return false;
		}
		if (!dir.EndsWith("\\"))
		{
			dir += "\\";
		}
		return FindTo(node, dir.ToLowerInvariant());
	}
	
	private bool FindTo(Node node, string fullPath)
	{
		if (!fullPath.StartsWith(node.fullPath.ToLowerInvariant()))
		{
			return false;
		}
		{
			string nodeFullPath = !node.fullPath.EndsWith("\\") ? node.fullPath + "\\" : node.fullPath;
			if (nodeFullPath.ToLowerInvariant() == fullPath)
			{
				return node.type == NodeType.Directory && node.expanded;
			}
		}
		if (node.expanded)
		{
			foreach (Node nodeI in node.childs)
			{
				string nodeFullPath = !nodeI.fullPath.EndsWith("\\") ? nodeI.fullPath + "\\" : nodeI.fullPath;
				if (fullPath.StartsWith(nodeFullPath.ToLowerInvariant()))
				{
					return FindTo(nodeI, fullPath);
				}
			}
		}
		return false;
	}
	
	private bool DoCopyToClipboard(Controller controller)
	{
		CopyToClipboard(controller, false);
		return true;
	}
	
	private bool DoCutToClipboard(Controller controller)
	{
		CopyToClipboard(controller, true);
		return true;
	}
	
	private bool DoPasteFromClipboard(Controller controller)
	{
		IDataObject data = Clipboard.GetDataObject();
		if (data == null || !data.GetDataPresent(DataFormats.FileDrop))
		{
			return false;
		}
		string[] paths = (string[])data.GetData(DataFormats.FileDrop);
		if (paths == null)
		{
			return false;
		}
		bool cutMode = false;
		MemoryStream stream = (MemoryStream)data.GetData("Preferred DropEffect", true);
		if (stream != null)
		{
			int flag = stream.ReadByte();
			if (flag != 2 && flag != 5)
			{
				return false;
			}
			cutMode = flag == 2;
		}
		Node node = GetOneFileOrDir(controller);
		string targetDir = null;
		if (node != null && node.type == NodeType.File)
		{
			targetDir = Path.GetDirectoryName(node.fullPath);
		}
		else if (node != null && node.type == NodeType.Directory)
		{
			if (cutMode)
			{
				for (int i = 0; i < paths.Length; i++)
				{
					if (PathSet.GetNorm(node.fullPath) == PathSet.GetNorm(paths[i]))
					{
						mainForm.Dialogs.ShowInfo("Paste error", "Try move into self: " + node.fullPath);
						return true;
					}
				}
			}
			targetDir = node.fullPath;
		}
		if (targetDir == null)
		{
			mainForm.Dialogs.ShowInfo("Paste error", "No target directory under cursor");
			return true;
		}
		if (!Directory.Exists(targetDir))
		{
			mainForm.Log.WriteError("Paste error", "Missing target directory: " + targetDir);
			mainForm.Log.Open();
			return true;
		}
		PasteFromClipboardAction action = new PasteFromClipboardAction(new FSProxy(), renamePostfixed, pastePostfixedAfterCopy);
		action.Execute(paths, targetDir, cutMode ? PasteMode.Cut : PasteMode.Copy);
		if (action.Errors.Count == 0 && action.Overwrites.Count > 0)
		{
			int count = 0;
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("Overwrite " + (action.Overwrites.Count > 1 ? "items" : "item") + "?");
			foreach (string overwrite in action.Overwrites)
			{
				count++;
				if (count > 10)
				{
					builder.AppendLine("…");
					break;
				}
				builder.AppendLine(overwrite);
			}
			DialogResult result = MessageBox.Show(builder.ToString(), mainForm.Name, MessageBoxButtons.YesNo);
			if (result == DialogResult.Yes)
			{
				action.Execute(paths, targetDir, cutMode ? PasteMode.CutOverwrite : PasteMode.CopyOverwrite);
			}
		}
		Reload();
		PutCursors(action.NewFullPaths);
		if (action.Errors.Count > 0)
		{
			foreach (string error in action.Errors)
			{
				mainForm.Log.WriteError("Paste error", error);
			}
			mainForm.Log.Open();
		}
		if (cutMode)
		{
			Clipboard.Clear();
		}
		return true;
	}
	
	private void CopyToClipboard(Controller controller, bool cutMode)
	{
		List<Node> nodes = GetFilesAndDirs(controller);
		StringCollection paths = new StringCollection();
		PathSet nodePaths = new PathSet();
		foreach (Node node in nodes)
		{
			if (node.type == NodeType.File || node.type == NodeType.Directory)
			{
				nodePaths.Add(node.fullPath);
			}
		}
		foreach (Node node in nodes)
		{
			if (node.type == NodeType.File || node.type == NodeType.Directory)
			{
				paths.Add(node.fullPath);
				if (!string.IsNullOrEmpty(renamePostfixed))
				{
					string postfixed = node.fullPath + renamePostfixed;
					if (!nodePaths.Contains(postfixed) && File.Exists(postfixed))
					{
						paths.Add(postfixed);
					}
				}
			}
		}
		if (paths.Count == 0)
		{
			mainForm.Dialogs.ShowInfo("Error", "No files/directories to copy");
			return;
		}
        MemoryStream stream = new MemoryStream(4);
        byte[] bytes = new byte[]{(byte)(cutMode ? 2 : 5), 0, 0, 0};
        stream.Write(bytes, 0, bytes.Length);
		DataObject data = new DataObject();
		data.SetFileDropList(paths);
        data.SetData("Preferred DropEffect", stream);
        Clipboard.Clear();
        Clipboard.SetDataObject(data);
	}
	
	private bool DoSelectItem(Controller controller)
	{
		Selection selection = controller.LastSelection;
		Place place = controller.Lines.PlaceOf(selection.caret);
		List<int> indices = controller.Lines.GetSelectionIndices();
		int lastIndexOfIndex = -1;
		for (int i = indices.Count; i-- > 0;)
		{
			if (indices[i] == place.iLine && selection.Empty)
			{
				lastIndexOfIndex = i;
				break;
			}
		}
		if (lastIndexOfIndex != -1)
		{
			foreach (Selection selectionI in controller.Selections)
			{
				if (selectionI != selection && selectionI.Left <= selection.Right && selectionI.Right >= selection.Left)
				{
					indices.RemoveAt(lastIndexOfIndex);
					break;
				}
			}
		}
		indices.Add(place.iLine + 1 < controller.Lines.LinesCount ? place.iLine + 1 : place.iLine);
		controller.lastSelectionFree = false;
		controller.ClearMinorSelections();
		for (int i = 0; i < indices.Count; ++i)
		{
			int iLine = indices[i];
			Selection selectionI;
			if (i == 0)
			{
				selectionI = controller.LastSelection;
				controller.PutCursor(new Place(0, iLine), false);
				controller.PutCursor(new Place(controller.Lines[iLine].NormalCount, iLine), true);
			}
			else if (i == indices.Count - 1)
			{
				controller.PutNewCursorForcedly(new Place(0, iLine));
			}
			else
			{
				controller.PutNewCursorForcedly(new Place(0, iLine));
				controller.PutCursor(new Place(controller.Lines[iLine].NormalCount, iLine), true);
			}
		}
		if (controller.SelectionsCount > 0)
		{
			controller.lastSelectionFree = true;
		}
		controller.NeedScrollToCaret();
		return true;
	}
}
