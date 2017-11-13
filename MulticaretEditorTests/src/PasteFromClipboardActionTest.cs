using System;
using MulticaretEditor;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class PasteFromClipboardActionTest
	{
		private FakeFSProxy fs;
		private PasteFromClipboardAction action;
		
		private void AssertFS(string expected)
		{
			string[] lines = expected.Trim().Replace("\r\n", "\n").Split('\n');
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i] = lines[i].Trim();
			}
			Assert.AreEqual(string.Join("\n", lines), fs.ToString());
		}
		
		private void Init(string renamePostfixed, bool pastePostfixedAfterCopy)
		{
			fs = new FakeFSProxy();
			action = new PasteFromClipboardAction(fs, renamePostfixed, pastePostfixedAfterCopy);
		}
		
		private void InitFS1()
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
		}
		
		private void ExecuteNoErrors(string[] files, string targetDir, PasteFromClipboardAction.Mode mode)
		{
			action.Execute(files, targetDir, mode);
			Assert.AreEqual(0, action.Errors.Count, "Expected no errors");
			Assert.AreEqual(0, action.Overwrites.Count, "Expected no overwrites");
		}
		
		[Test]
		public void AfterCut()
		{
			Init(null, false);
			InitFS1();
			ExecuteNoErrors(
				new string[] { "c:\\dir1\\File1.cs", "c:\\dir1\\File2.cs" },
				"c:\\dir2",
				PasteFromClipboardAction.Cut);
			AssertFS(@"c:
				-dir1
				-dir2
				--File1.cs{1}
				--File2.cs{2}
				--File3.cs{3}");
		}
		
		[Test]
		public void AfterCopy()
		{
			Init(null, false);
			InitFS1();
			ExecuteNoErrors(
				new string[] { "c:\\dir1\\File1.cs", "c:\\dir1\\File2.cs" },
				"c:\\dir2",
				PasteFromClipboardAction.Copy);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--File1.cs{1}
				--File2.cs{2}
				--File3.cs{3}");
		}
		
		[Test]
		public void AfterCopy_OneFileDuplication()
		{
			Init(null, false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
				)
			);
			ExecuteNoErrors(new string[] { "c:\\dir1\\File1.cs" }, "c:\\dir1", PasteFromClipboardAction.Copy);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File1-copy.cs{1}
				--File2.cs{2}
				-dir2
				--File3.cs{3}");
		}
		
		[Test]
		public void AfterCopy_OneFileOverwrite()
		{
			Init(null, false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 3))
				)
			);
			action.Execute(new string[] { "c:\\dir2\\File1.cs" }, "c:\\dir1", PasteFromClipboardAction.Copy);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			CollectionAssert.AreEqual(new string[] { "c:\\dir1\\File1.cs" }, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--File1.cs{3}");
			action.Execute(new string[] { "c:\\dir2\\File1.cs" }, "c:\\dir1", PasteFromClipboardAction.CopyOverride);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			AssertFS(@"c:
				-dir1
				--File1.cs{3}
				--File2.cs{2}
				-dir2
				--File1.cs{3}");
		}
		
		[Test]
		public void AfterCopy_DirOverwrite()
		{
			Init(null, false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeDir("dir1")
						.Add(new FakeFSProxy.FakeFile("File1.cs", 3))
					)
				)
			);
			action.Execute(new string[] { "c:\\dir2\\dir1" }, "c:", PasteFromClipboardAction.Copy);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			CollectionAssert.AreEqual(new string[] { "c:\\dir1" }, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--dir1
				---File1.cs{3}");
			action.Execute(new string[] { "c:\\dir2\\dir1" }, "c:", PasteFromClipboardAction.CopyOverride);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			AssertFS(@"c:
				-dir1
				--File1.cs{3}
				--File2.cs{2}
				-dir2
				--dir1
				---File1.cs{3}");
		}
		
		[Test]
		public void AfterCopy_NoInsertSelfLoop()
		{
			Init(null, false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeDir("dir1")
						.Add(new FakeFSProxy.FakeFile("File1.cs", 3))
					)
				)
			);
			ExecuteNoErrors(new string[] { "c:\\dir2" }, "c:\\dir2", PasteFromClipboardAction.Copy);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--dir1
				---File1.cs{3}
				--dir2
				---dir1
				----File1.cs{3}
			");
		}
		
		[Test]
		public void AfterCut_DirOverwrite()
		{
			Init(null, false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeDir("dir1")
						.Add(new FakeFSProxy.FakeFile("File1.cs", 3))
					)
				)
			);
			action.Execute(new string[] { "c:\\dir2\\dir1" }, "c:", PasteFromClipboardAction.Cut);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			CollectionAssert.AreEqual(new string[] { "c:\\dir1" }, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--dir1
				---File1.cs{3}");
			action.Execute(new string[] { "c:\\dir2\\dir1" }, "c:", PasteFromClipboardAction.CutOverride);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			AssertFS(@"c:
				-dir1
				--File1.cs{3}
				--File2.cs{2}
				-dir2");
		}
		
		[Test]
		public void AfterCut_ToItself()
		{
			Init(null, false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeDir("dir1")
						.Add(new FakeFSProxy.FakeFile("File1.cs", 3))
					)
				)
			);
			ExecuteNoErrors(new string[] { "c:\\dir2\\dir1" }, "c:\\dir2", PasteFromClipboardAction.Cut);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--dir1
				---File1.cs{3}");
		}
	}
}
