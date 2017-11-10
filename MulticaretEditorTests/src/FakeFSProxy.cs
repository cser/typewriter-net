using System.IO;
using System.Text;
using System.Collections.Generic;

namespace UnitTests
{
	public class FakeFSProxy : IFSProxy
	{
		public class FakeDir
		{
			public string name;
			public readonly List<FakeDir> dirs = new List<FakeDir>();
			public readonly List<FakeFile> files = new List<FakeFile>();
			
			public FakeDir(string name)
			{
				this.name = name;
			}
			
			public FakeDir Add(FakeDir dir)
			{
				dirs.Add(dir);
				return this;
			}
			
			public FakeDir Add(FakeFile file)
			{
				files.Add(file);
				return this;
			}
		}
		
		public class FakeFile
		{
			public string name;
			public int content;
			
			public FakeFile(string name, int content)
			{
				this.name = name;
				this.content = content;
			}
		}
		
		private readonly List<FakeDir> dirs = new List<FakeDir>();
		
		public FakeFSProxy Add(FakeDir dir)
		{
			dirs.Add(dir);
			return this;
		}
		
		public bool File_Exists(string path)
		{
			return false;
		}
		
		public void File_Copy(string source, string target)
		{
		}
		
		public void File_Move(string source, string target)
		{
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
			ToString(lines, dirs, "");
			return string.Join("\n", lines.ToArray());
		}
		
		private void ToString(List<string> lines, List<FakeDir> dirs, string indent)
		{
			foreach (FakeDir dir in dirs)
			{
				lines.Add(indent + dir.name);
				ToString(lines, dir.dirs, indent + "-");
				foreach (FakeFile file in dir.files)
				{
					lines.Add(indent + "-" + file.name + "{" + file.content + "}");
				}
			}
		}
	}
}