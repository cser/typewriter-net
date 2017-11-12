using System;
using System.IO;
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
		public void File_Move()
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
		public void File_Move_IgnoreCase()
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
			fs.File_Move("C:\\Dir1\\file1.cs", "c:\\dir2\\File1.cs");
			AssertFS(@"c:
				-dir1
				--File2.cs{2}
				-dir2
				--File1.cs{1}
				--File3.cs{3}
				--File4.cs{4}
			");
		}
		
		[TestCase("c:\\dir1\\File1.cs", "c:\\dir2\\File1.cs")]
		[TestCase("c:\\Dir1\\File1.cs", "c:\\dir2\\File1.cs")]
		[TestCase("C:\\dir1\\file1.cs", "c:\\dir2\\File1.cs")]
		public void File_Copy(string path1, string path2)
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
			fs.File_Copy(path1, path2);
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
		
		[TestCase("c:\\dir1", "c:\\dir3")]
		[TestCase("c:\\Dir1", "c:\\dir3")]
		[TestCase("C:\\dir1", "c:\\dir3")]
		public void Directory_Move(string path1, string path2)
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
			fs.Directory_Move(path1, path2);
			AssertFS(@"c:
				-dir2
				--File3.cs{3}
				--File4.cs{4}
				-dir3
				--File1.cs{1}
				--File2.cs{2}
			");
		}
		
		[Test]
		public void Exists()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeDir("dir3"))
					.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
					.Add(new FakeFSProxy.FakeFile("File4.cs", 4))
				)
				.Add(new FakeFSProxy.FakeFile("File5.cs", 5))
			);
			Assert.AreEqual(true, fs.Directory_Exists("c:\\dir1"));
			Assert.AreEqual(true, fs.Directory_Exists("c:\\dir2"));
			Assert.AreEqual(false, fs.Directory_Exists("c:\\dir3"));
			Assert.AreEqual(true, fs.Directory_Exists("c:\\dir2\\dir3"));
			Assert.AreEqual(true, fs.File_Exists("c:\\dir1\\File1.cs"));
			Assert.AreEqual(true, fs.File_Exists("c:\\dir1\\File2.cs"));
			Assert.AreEqual(false, fs.File_Exists("c:\\dir1\\File3.cs"));
			Assert.AreEqual(true, fs.File_Exists("c:\\dir2\\File3.cs"));
			Assert.AreEqual(false, fs.File_Exists("c:\\dir2\\File5.cs"));
			Assert.AreEqual(true, fs.File_Exists("c:\\File5.cs"));
			Assert.AreEqual(false, fs.File_Exists("c:\\dir3\\dir4\\File5.cs"));
		}
		
		[Test]
		public void Exists_MastIgnoreCase()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("Dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeDir("dir3"))
					.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
					.Add(new FakeFSProxy.FakeFile("File4.cs", 4))
				)
				.Add(new FakeFSProxy.FakeFile("File5.cs", 5))
			);
			Assert.AreEqual(true, fs.Directory_Exists("c:\\dir1"));
			Assert.AreEqual(false, fs.Directory_Exists("c:\\dir1missing"));
			Assert.AreEqual(true, fs.Directory_Exists("C:\\Dir2"));
			Assert.AreEqual(true, fs.Directory_Exists("c:\\Dir2\\dir3"));
			Assert.AreEqual(true, fs.File_Exists("c:\\dir1\\file1.cs"));
			Assert.AreEqual(false, fs.File_Exists("c:\\dir1\\file1missing.cs"));
			Assert.AreEqual(true, fs.File_Exists("C:\\Dir1\\File2.cs"));
			Assert.AreEqual(true, fs.File_Exists("c:\\Dir2\\file3.cs"));
			Assert.AreEqual(true, fs.File_Exists("c:\\file5.cs"));
		}
		
		[Test(Description="It's deviation from real file system behaviour - I'm just not sure that it's needed")]
		public void Exists_Slashed_MORE_STRICT_THEN_REAL()
		{
			fs.Add(new FakeFSProxy.FakeDir("C:")
				.Add(new FakeFSProxy.FakeDir("Dir1")
					.Add(new FakeFSProxy.FakeDir("Dir2"))
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
				)
			);
			Assert.AreEqual(true, fs.Directory_Exists("c:\\Dir1"));
			Assert.AreEqual(false, fs.Directory_Exists("c:\\Dir1\\"));
			Assert.AreEqual(true, fs.Directory_Exists("c:\\Dir1\\Dir2"));
			Assert.AreEqual(false, fs.Directory_Exists("c:\\Dir1\\Dir2\\"));
			Assert.AreEqual(true, fs.File_Exists("c:\\Dir1\\File1.cs"));
			Assert.AreEqual(false, fs.File_Exists("c:\\Dir1\\File1.cs\\"));
		}
		
		[Test]
		public void CreateDirectory()
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
			fs.Directory_CreateDirectory("c:\\dir1\\dir3");
			fs.Directory_CreateDirectory("c:\\dir4");
			AssertFS(@"c:
				-dir1
				--dir3
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--File3.cs{3}
				--File4.cs{4}
				-dir4
			");
		}
		
		[Test]
		public void GetFilesAndDirectories()
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
			CollectionAssert.AreEqual(
				new string[] { "c:\\dir1\\File1.cs", "c:\\dir1\\File2.cs" },
				fs.Directory_GetFiles("c:\\dir1"));
			CollectionAssert.AreEqual(
				new string[] { "c:\\dir1", "c:\\dir2" },
				fs.Directory_GetDirectories("c:"));
		}
		
		[Test]
		public void CreateDir_WithParents()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
				)
			);
			fs.Directory_CreateDirectory("c:\\dir2\\Dir3");
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				-dir2
				--Dir3
			");
		}
		
		[TestCase("c:\\dir1\\dir3")]
		[TestCase("C:\\dir1\\dir3")]
		[TestCase("C:\\Dir1\\dir3")]
		public void CreateDir_AlreadyExists(string dir)
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeDir("dir3"))
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
				)
				.Add(new FakeFSProxy.FakeDir("dir2"))
			);
			fs.Directory_CreateDirectory(dir);
			AssertFS(@"c:
				-dir1
				--dir3
				--File1.cs{1}
				-dir2
			");
		}
		
		[Test]
		public void Directory_MoveMissing()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
				)
			);
			try
			{
				fs.Directory_Move("c:\\dir3", "c:\\dir4");
				Assert.Fail("Exception expected");
			}
			catch (DirectoryNotFoundException e)
			{
				Assert.IsTrue(e.Message.Contains("Missing directory:"));
			}
			try
			{
				fs.Directory_Move("c:\\dir1", "c:\\dir3\\dir4");
				Assert.Fail("Exception expected");
			}
			catch (DirectoryNotFoundException e)
			{
				Assert.IsTrue(e.Message.Contains("Missing target path part:"));
			}
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--File3.cs{3}
			");
		}
		
		[Test]
		public void File_MoveMissing()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
				)
			);
			try
			{
				fs.File_Move("c:\\dir1\\File4.cs", "c:\\dir2\\File4.cs");
				Assert.Fail("Exception expected");
			}
			catch (FileNotFoundException e)
			{
				Assert.IsTrue(e.Message.Contains("Missing file:"));
			}
			try
			{
				fs.File_Move("c:\\dir1\\File1.cs", "c:\\dir3\\File1.cs");
				Assert.Fail("Exception expected");
			}
			catch (DirectoryNotFoundException e)
			{
				Assert.IsTrue(e.Message.Contains("Missing target path part:"));
			}
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--File3.cs{3}
			");
		}
		
		[Test]
		public void File_CopyMissing()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
				)
			);
			try
			{
				fs.File_Copy("c:\\dir1\\File4.cs", "c:\\dir2\\File4.cs");
				Assert.Fail("Exception expected");
			}
			catch (FileNotFoundException e)
			{
				Assert.IsTrue(e.Message.Contains("Missing file:"));
			}
			try
			{
				fs.File_Copy("c:\\dir1\\File1.cs", "c:\\dir3\\File1.cs");
				Assert.Fail("Exception expected");
			}
			catch (DirectoryNotFoundException e)
			{
				Assert.IsTrue(e.Message.Contains("Missing target path part:"));
			}
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--File3.cs{3}
			");
		}
		
		[Test]
		public void File_CopyToExists()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
				)
			);
			try
			{
				fs.File_Copy("c:\\dir1\\File1.cs", "c:\\dir2\\File3.cs");
				Assert.Fail("Exception expected");
			}
			catch (IOException e)
			{
				Assert.IsTrue(e.Message.Contains("File already exists:"));
			}
			try
			{
				fs.File_Move("c:\\dir1\\File1.cs", "c:\\dir2\\File3.cs");
				Assert.Fail("Exception expected");
			}
			catch (IOException e)
			{
				Assert.IsTrue(e.Message.Contains("File already exists:"));
			}
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--File3.cs{3}
			");
		}
		
		[Test]
		public void Directory_MoveToExists()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
				)
			);
			try
			{
				fs.Directory_Move("c:\\dir1", "c:\\dir2");
				Assert.Fail("Exception expected");
			}
			catch (IOException e)
			{
				Assert.IsTrue(e.Message.Contains("File already exists:"));
			}
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--File3.cs{3}
			");
		}
		
		[Test]
		public void MoveMissing_IgnoreCase()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
				)
			);
			try
			{
				fs.Directory_Move("c:\\dir1", "c:\\Dir2");
				Assert.Fail("Exception expected");
			}
			catch (IOException)
			{
			}
			try
			{
				fs.File_Move("c:\\dir1\\File1.cs", "c:\\Dir2\\File3.cs");
				Assert.Fail("Exception expected");
			}
			catch (IOException)
			{
			}
			try
			{
				fs.File_Move("c:\\dir1\\File1.cs", "c:\\dir2\\file3.cs");
				Assert.Fail("Exception expected");
			}
			catch (IOException)
			{
			}
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--File3.cs{3}
			");
		}
		
		[Test]
		public void File_Copy_Rename()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
				)
			);
			fs.File_Copy("c:\\dir1\\file1.cs", "c:\\dir1\\File1-copy.cs");
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File1-copy.cs{1}
			");
		}
		
		[Test]
		public void File_Delete()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
				)
			);
			fs.File_Delete("c:\\dir1\\File1.cs");
			AssertFS(@"c:
				-dir1
			");
		}
		
		[Test]
		public void Directory_Delete()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
				)
			);
			fs.Directory_DeleteRecursive("c:\\dir1");
			AssertFS(@"c:
			");
		}
	}
}