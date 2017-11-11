using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class FakeFSProxyTest
	{
		private FakeFSProxy fs;
		
		[SetUp]
		public void SetUp()
		{
			fs = new FakeFSProxy();
		}
		
		private void AssertFS(string expected)
		{
			string[] lines = expected.Trim().Replace("\r\n", "\n").Split('\n');
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i] = lines[i].Trim();
			}
			Assert.AreEqual(string.Join("\n", lines), fs.ToString());
		}
		
		[Test]
		public void SimpleToString()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("root")
					.Add(new FakeFSProxy.FakeFile("Test.cs", 1))
					.Add(new FakeFSProxy.FakeFile("Test2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("root2")
					.Add(new FakeFSProxy.FakeFile("File.cs", 110))
				)
			);
			AssertFS(@"c:
				-root
				--Test.cs{1}
				--Test2.cs{2}
				-root2
				--File.cs{110}
			");
		}
		
		[Test]
		public void SimpleToString_Sorting()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("b")
					.Add(new FakeFSProxy.FakeFile("a.cs", 1))
					.Add(new FakeFSProxy.FakeFile("c.cs", 2))
					.Add(new FakeFSProxy.FakeFile("b.cs", 3))
				)
				.Add(new FakeFSProxy.FakeDir("a"))
			);
			AssertFS(@"c:
				-a
				-b
				--a.cs{1}
				--b.cs{3}
				--c.cs{2}
			");
		}
		
		[Test]
		public void Move()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
					.Add(new FakeFSProxy.FakeFile("File4.cs", 4))
				)
			);
			fs.File_Move("c:\\dir1\\File1.cs", "c:\\dir2\\File1.cs");
			AssertFS(@"c:
				-dir1
				--File2.cs{2}
				-dir2
				--File1.cs{1}
				--File3.cs{3}
				--File4.cs{4}
			");
			fs.File_Move("c:\\dir2\\File3.cs", "c:\\File5.cs");
			AssertFS(@"c:
				-dir1
				--File2.cs{2}
				-dir2
				--File1.cs{1}
				--File4.cs{4}
				-File5.cs{3}
			");
		}
		
		[Test]
		public void Copy()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
					.Add(new FakeFSProxy.FakeFile("File4.cs", 4))
				)
			);
			fs.File_Copy("c:\\dir1\\File1.cs", "c:\\dir2\\File1.cs");
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--File1.cs{1}
				--File3.cs{3}
				--File4.cs{4}
			");
		}
	}
}
