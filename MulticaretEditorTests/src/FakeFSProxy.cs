using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace UnitTests
{
	public class FakeFSProxy : IFSProxy
	{
		public class FakeItem
		{
			public string name;
			public virtual FakeDir AsDir { get { return null; } }
			public virtual FakeFile AsFile { get { return null; } }
			
			public FakeItem(string name)
			{
				this.name = name;
			}
		}
		
		public struct Pair
		{
			public readonly FakeDir parent;
			public readonly FakeItem item;
			
			public Pair(FakeDir parent, FakeItem item)
			{
				this.parent = parent;
				this.item = item;
			}
		}
		
		public class FakeDir : FakeItem
		{
			public readonly List<FakeItem> items = new List<FakeItem>();
			public override FakeDir AsDir { get { return this; } }
			
			private bool unauthorized;
			public bool Unauthorized { get { return unauthorized; } }
			
			public FakeDir(string name) : base(name)
			{
			}
			
			public void Clear()
			{
				items.Clear();
			}
			
			public FakeDir Add(FakeItem item)
			{
				if (GetItem(item.name) != null)
				{
					throw new ArgumentException("Name \"" + item.name + "\" already exists in \"" + name + "\"");
				}
				items.Add(item);
				return this;
			}
			
			public void Remove(FakeItem item)
			{
				items.Remove(item);
			}
			
			public FakeDir SetUnauthorized()
			{
				unauthorized = true;
				return this;
			}
			
			public FakeItem GetItem(string name)
			{
				foreach (FakeItem item in items)
				{
					if (string.Compare(item.name, name, true) == 0)
					{
						return item;
					}
				}
				return null;
			}
		}
		
		public class Node
		{
			public readonly string name;
			public readonly Node next;
			
			public Node(string name, Node next)
			{
				this.name = name;
				this.next = next;
			}
			
			public override string ToString()
			{
				return name + (next != null ? "\\" + next : "");
			}
			
			public static Node Of(string path)
			{
				int index = path.IndexOf('\\');
				return new Node(
					index != -1 ? path.Substring(0, index) : path,
					index != -1 ? Of(path.Substring(index + 1)) : null);
			}
			
			public static Node CutEnd(Node node)
			{
				return node != null && node.next != null ? new Node(node.name, CutEnd(node.next)) : null;
			}
			
			public static string EndName(Node node)
			{
				if (node == null)
				{
					return null;
				}
				if (node.next == null)
				{
					return node.name;
				}
				return EndName(node.next);
			}
		}
		
		private FakeItem GetItem(Node node)
		{
			return GetItem(node, root, node);
		}
		
		private FakeItem GetItem(Node node, FakeDir dir, Node originNode)
		{
			FakeItem item = dir.GetItem(node.name);
			if (node != null && node.next == null)
			{
				return item;
			}
			if (item != null && item.AsDir != null)
			{
				return GetItem(node.next, item.AsDir, originNode);
			}
			return null;
		}
		
		private Node GetRealNames(Node node)
		{
			return GetRealNames(node, root, node);
		}
		
		private Node GetRealNames(Node node, FakeDir dir, Node originNode)
		{
			FakeItem item = dir.GetItem(node.name);
			if (node != null && node.next == null)
			{
				return new Node(item.name, null);
			}
			if (item != null && item.AsDir != null)
			{
				return new Node(item.name, GetRealNames(node.next, item.AsDir, originNode));
			}
			return null;
		}
		
		public class FakeFile : FakeItem
		{
			public int content;
			public override FakeFile AsFile { get { return this; } }
			
			public FakeFile(string name, int content) : base(name)
			{
				this.content = content;
			}
			
			public FakeFile Clone(string name)
			{
				return new FakeFile(name, content);
			}
		}
		
		private readonly FakeDir root = new FakeDir("");
		
		public FakeFSProxy Add(FakeDir dir)
		{
			root.Add(dir);
			return this;
		}
		
		public FakeItem GetItem(string name)
		{
			return root.GetItem(name);
		}
		
		public bool File_Exists(string path)
		{
			FakeItem item = GetItem(Node.Of(path));
			return item != null && item.AsFile != null;
		}
		
		public void File_Copy(string source, string target)
		{
			FakeItem sourceItem = GetItem(Node.Of(source));
			FakeFile sourceFile = sourceItem != null ? sourceItem.AsFile : null;
			if (sourceFile == null)
			{
				throw new FileNotFoundException("Missing file: " + source);
			}
			FakeDir sourceOwner = GetItem(Node.CutEnd(Node.Of(source))).AsDir;
			FakeItem targetItem = GetItem(Node.CutEnd(Node.Of(target)));
			FakeDir targetOwner = targetItem != null ? targetItem.AsDir : null;
			if (targetOwner == null)
			{
				throw new DirectoryNotFoundException("Missing target path part: " + source);
			}
			if (targetOwner.Unauthorized)
			{
				throw new UnauthorizedAccessException("Unauthorized access to: " + target);
			}
			string name = Node.EndName(Node.Of(target));
			if (targetOwner.GetItem(name) != null)
			{
				throw new IOException("File already exists: " + source);
			}
			targetOwner.Add(sourceFile.Clone(name));
		}
		
		public void File_Move(string source, string target)
		{
			FakeItem sourceItem = GetItem(Node.Of(source));
			FakeFile sourceFile = sourceItem != null ? sourceItem.AsFile : null;
			if (sourceFile == null)
			{
				throw new FileNotFoundException("Missing file: " + source);
			}
			FakeDir sourceOwner = GetItem(Node.CutEnd(Node.Of(source))).AsDir;
			FakeItem targetItem = GetItem(Node.CutEnd(Node.Of(target)));
			FakeDir targetOwner = targetItem != null ? targetItem.AsDir : null;
			if (targetOwner == null)
			{
				throw new DirectoryNotFoundException("Missing target path part: " + source);
			}
			if (targetOwner.Unauthorized)
			{
				throw new UnauthorizedAccessException("Unauthorized access to: " + target);
			}
			string name = Node.EndName(Node.Of(target));
			if (targetOwner.GetItem(name) != null)
			{
				throw new IOException("File already exists: " + source);
			}
			sourceOwner.Remove(sourceFile);
			sourceFile.name = name;
			targetOwner.Add(sourceFile);
		}
		
		public bool Directory_Exists(string path)
		{
			FakeItem item = GetItem(Node.Of(path));
			return item != null && item.AsDir != null;
		}
		
		public void Directory_Move(string source, string target)
		{
			FakeItem sourceItem = GetItem(Node.Of(source));
			FakeDir sourceDir = sourceItem != null ? sourceItem.AsDir : null;
			if (sourceDir == null)
			{
				throw new DirectoryNotFoundException("Missing directory: " + source);
			}
			FakeDir sourceOwner = GetItem(Node.CutEnd(Node.Of(source))).AsDir;
			FakeItem targetItem = GetItem(Node.CutEnd(Node.Of(target)));
			FakeDir targetOwner = targetItem != null ? targetItem.AsDir : null;
			if (targetOwner == null)
			{
				throw new DirectoryNotFoundException("Missing target path part: " + source);
			}
			if (targetOwner.Unauthorized)
			{
				throw new IOException("Unable access to: " + target);
			}
			string name = Node.EndName(Node.Of(target));
			if (targetOwner.GetItem(name) != null)
			{
				throw new IOException("File already exists: " + target);
			}
			sourceOwner.Remove(sourceDir);
			sourceDir.name = name;
			targetOwner.Add(sourceDir);
		}
		
		public void Directory_CreateDirectory(string path)
		{
			Node nodes = Node.Of(path);
			Directory_CreateDirectory(nodes, root, path);
		}
		
		private void Directory_CreateDirectory(Node node, FakeDir dir, string path)
		{
			if (node == null)
			{
				return;
			}
			if (dir.Unauthorized)
			{
				throw new UnauthorizedAccessException("Unauthorized access to: " + path);
			}
			FakeItem item = dir.GetItem(node.name);
			if (item == null)
			{
				item = new FakeDir(node.name);
				dir.Add(item);
			}
			if (node.next == null)
			{
				return;
			}
			Directory_CreateDirectory(node.next, item.AsDir, path);
		}
		
		public string[] Directory_GetFiles(string path)
		{
			List<string> paths = new List<string>();
			FakeItem sourceItem = GetItem(Node.Of(path));
			if (sourceItem != null && sourceItem.AsFile != null)
			{
				throw new IOException("Incorrect directory: " + path);
			}
			FakeDir sourceOwner = sourceItem != null ? sourceItem.AsDir : null;
			if (sourceOwner == null)
			{
				throw new DirectoryNotFoundException("Missing directory: " + path);
			}
			if (sourceOwner.Unauthorized)
			{
				throw new UnauthorizedAccessException("Unauthorized access to: " + path);
			}
			Node dir = GetRealNames(Node.Of(path));
			foreach (FakeItem item in sourceOwner.items)
			{
				if (item.AsFile != null)
				{
					paths.Add(dir + "\\" + item.name);
				}
			}
			return paths.ToArray();
		}
		
		public string[] Directory_GetDirectories(string path)
		{
			List<string> paths = new List<string>();
			FakeItem sourceItem = GetItem(Node.Of(path));
			if (sourceItem != null && sourceItem.AsFile != null)
			{
				throw new IOException("Incorrect directory: " + path);
			}
			FakeDir sourceOwner = sourceItem != null ? sourceItem.AsDir : null;
			if (sourceOwner == null)
			{
				throw new DirectoryNotFoundException("Missing directory: " + path);
			}
			Node dir = GetRealNames(Node.Of(path));
			if (sourceOwner.Unauthorized)
			{
				throw new UnauthorizedAccessException("Unauthorized access to: " + path);
			}
			foreach (FakeItem item in sourceOwner.items)
			{
				if (item.AsDir != null)
				{
					paths.Add(dir + "\\" + item.name);
				}
			}
			return paths.ToArray();
		}
		
		public void File_Delete(string path)
		{
			FakeItem ownerItem = GetItem(Node.CutEnd(Node.Of(path)));
			FakeDir owner = ownerItem != null ? ownerItem.AsDir : null;
			FakeItem fileItem = GetItem(Node.Of(path));
			FakeFile file = fileItem != null ? fileItem.AsFile : null;
			if (fileItem != null && fileItem.AsDir != null)
			{
				throw new UnauthorizedAccessException("Disallow access to: " + path);
			}
			if (owner != null && file != null)
			{
				if (owner.Unauthorized)
				{
					return;
				}
				owner.Remove(file);
			}
		}
		
		public void Directory_DeleteRecursive(string path)
		{
			FakeItem ownerItem = GetItem(Node.CutEnd(Node.Of(path)));
			FakeDir owner = ownerItem != null ? ownerItem.AsDir : null;
			if (owner != null && owner.Unauthorized)
			{
				throw new DirectoryNotFoundException("Missing directory: " + path);
			}
			FakeItem dirItem = GetItem(Node.Of(path));
			if (dirItem == null)
			{
				throw new DirectoryNotFoundException("Missing target path part: " + path);
			}
			FakeDir dir = dirItem.AsDir;
			if (dir == null)
			{
				throw new IOException("Incorrect directory name: " + path);
			}
			if (dir.Unauthorized)
			{
				throw new UnauthorizedAccessException("Unauthorized access to: " + path);
			}
			owner.Remove(dir);
		}
		
		public string GetFileName(string path)
		{
			return Path.GetFileName(path);
		}
		
		public string GetFileNameWithoutExtension(string path)
		{
			return Path.GetFileNameWithoutExtension(path);
		}
		
		public string GetExtension(string path)
		{
			return Path.GetExtension(path);
		}
		
		public string Combine(string path1, string path2)
		{
			return Path.Combine(path1, path2);
		}
		
		public char Separator { get { return Path.DirectorySeparatorChar; } }
		
		public override string ToString()
		{
			List<string> lines = new List<string>();
			ToString(lines, root, "");
			return string.Join("\n", lines.ToArray());
		}
		
		private void ToString(List<string> lines, FakeDir dir, string indent)
		{
			List<FakeItem> sorted = new List<FakeItem>(dir.items);
			sorted.Sort(CompareItems);
			foreach (FakeItem item in sorted)
			{
				if (item.AsDir != null)
				{
					lines.Add(indent + item.name);
					ToString(lines, item.AsDir, indent + "-");
				}
			}
			foreach (FakeItem item in sorted)
			{
				if (item.AsFile != null)
				{
					lines.Add(indent + item.name + "{" + item.AsFile.content + "}");
				}
			}
		}
		
		private int CompareItems(FakeItem item0, FakeItem item1)
		{
			return string.Compare(item0.name, item1.name);
		}
		
		public static string NormalizeString(string text)
		{
			string[] lines = text.Trim().Replace("\r\n", "\n").Split('\n');
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i] = lines[i].Trim();
			}
			return string.Join("\n", lines);
		}
		
		public void ApplyString(string text)
		{
			text = NormalizeString(text);
			List<string> lines = new List<string>(text.Trim().Split('\n'));
			if (lines.Count > 0)
			{
				root.Clear();
				if (lines[0].StartsWith("-"))
				{
					throw new ArgumentException("Unexpected \"-\" at start");
				}
				ApplyString(root, lines);
			}
		}
		
		private void ApplyString(FakeDir dir, List<string> lines)
		{
			string lastFile = null;
			FakeDir subdir = null;
			List<string> sublines = null;
			foreach (string line in lines)
			{
				if (!line.StartsWith("-"))
				{
					if (subdir != null && sublines != null)
					{
						ApplyString(subdir, sublines);
						subdir = null;
						sublines = null;
					}
					if (line.EndsWith("}") && line.Contains("{"))
					{
						int index = line.LastIndexOf("{");
						string name = line.Substring(0, index);
						string rawContent = line.Substring(index + 1, line.Length - index - 2);
						int content;
						int.TryParse(rawContent, out content);
						dir.Add(new FakeFile(name, content));
						lastFile = line;
					}
					else
					{
						subdir = new FakeDir(line);
						sublines = new List<string>();
						dir.Add(subdir);
					}
					continue;
				}
				if (sublines == null && lastFile != null)
				{
					throw new ArgumentException("Insertion into file: " + lastFile);
				}
				if (sublines == null)
				{
					throw new ArgumentException("Incorrect depth");
				}
				sublines.Add(line.Substring(1));
			}
			if (subdir != null && sublines != null)
			{
				ApplyString(subdir, sublines);
				subdir = null;
				sublines = null;
			}
		}
	}
}