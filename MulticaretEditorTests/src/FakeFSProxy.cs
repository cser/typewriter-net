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
			
			public FakeFile File
			{
				get
				{
					if (AsFile == null)
					{
						throw new Exception("Isn't file: " + name);
					}
					return AsFile;
				}
			}
			
			public FakeDir Dir
			{
				get
				{
					if (AsDir == null)
					{
						throw new Exception("Isn't dir: " + name);
					}
					return AsDir;
				}
			}
			
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
			
			public FakeDir(string name) : base(name)
			{
			}
			
			public FakeDir Add(FakeItem item)
			{
				items.Add(item);
				return this;
			}
			
			public void Remove(FakeItem item)
			{
				items.Remove(item);
			}
			
			public FakeItem GetItem(string name)
			{
				foreach (FakeItem item in items)
				{
					if (item.name == name)
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
			if (item == null)
			{
				throw new Exception("item == null: path=" + originNode + ", name=" + node.name);
			}
			if (node.next == null)
			{
				return item;
			}
			if (item.AsDir == null)
			{
				throw new Exception("item isn't dir: path=" + originNode + ", name=" + node.name);
			}
			return GetItem(node.next, item.AsDir, originNode);
		}
		
		public class FakeFile : FakeItem
		{
			public int content;
			public override FakeFile AsFile { get { return this; } }
			
			public FakeFile(string name, int content) : base(name)
			{
				this.content = content;
			}
			
			public FakeFile Clone()
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
		
		public bool File_Exists(string path)
		{
			return false;
		}
		
		public void File_Copy(string source, string target)
		{
			FakeFile sourceFile = GetItem(Node.Of(source)).File;
			FakeDir sourceDir = GetItem(Node.CutEnd(Node.Of(source))).Dir;
			FakeDir targetDir = GetItem(Node.CutEnd(Node.Of(target))).Dir;
			sourceFile.name = Node.EndName(Node.Of(target));
			targetDir.Add(sourceFile.Clone());
		}
		
		public void File_Move(string source, string target)
		{
			FakeFile sourceFile = GetItem(Node.Of(source)).File;
			FakeDir sourceDir = GetItem(Node.CutEnd(Node.Of(source))).Dir;
			FakeDir targetDir = GetItem(Node.CutEnd(Node.Of(target))).Dir;
			sourceDir.Remove(sourceFile);
			sourceFile.name = Node.EndName(Node.Of(target));
			targetDir.Add(sourceFile);
		}
		
		public bool Directory_Exists(string path)
		{
			return false;
		}
		
		public void Directory_Move(string source, string target)
		{
		}
		
		public void Directory_CreateDirectory(string path)
		{
		}
		
		public string[] Directory_GetFiles(string path)
		{
			return null;
		}
		
		public string[] Directory_GetDirectories(string path)
		{
			return null;
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
	}
}