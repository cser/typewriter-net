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
			CollectionAssert.AreEqual(new string[]{}, action.Errors, "Expected no errors");
			CollectionAssert.AreEqual(new string[]{}, action.Overwrites, "Expected no overwrites");
		}
		
		[TestCase(null)]
		[TestCase(".meta")]
		public void AfterCut(string meta)
		{
			Init(meta, false);
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
		
		[TestCase(null)]
		[TestCase(".meta")]
		public void AfterCopy(string meta)
		{
			Init(meta, false);
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
		
		[TestCase(null)]
		[TestCase(".meta")]
		public void AfterCopy_OneFileDuplication(string meta)
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
		
		[TestCase(null)]
		[TestCase(".meta")]
		public void AfterCopy_OneFileOverwrite(string meta)
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
			action.Execute(new string[] { "c:\\dir2\\File1.cs" }, "c:\\dir1", PasteFromClipboardAction.CopyOverwrite);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			AssertFS(@"c:
				-dir1
				--File1.cs{3}
				--File2.cs{2}
				-dir2
				--File1.cs{3}");
		}
		
		[TestCase(null)]
		[TestCase(".meta")]
		public void AfterCopy_DirOverwrite(string meta)
		{
			Init(meta, false);
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
			action.Execute(new string[] { "c:\\dir2\\dir1" }, "c:", PasteFromClipboardAction.CopyOverwrite);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			AssertFS(@"c:
				-dir1
				--File1.cs{3}
				--File2.cs{2}
				-dir2
				--dir1
				---File1.cs{3}");
		}
		
		[TestCase(null)]
		[TestCase(".meta")]
		public void AfterCopy_NoInsertSelfLoop(string meta)
		{
			Init(meta, false);
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
		
		[TestCase(null)]
		[TestCase(".meta")]
		public void AfterCut_DirOverwrite(string meta)
		{
			Init(meta, false);
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
			action.Execute(new string[] { "c:\\dir2\\dir1" }, "c:", PasteFromClipboardAction.CutOverwrite);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			AssertFS(@"c:
				-dir1
				--File1.cs{3}
				--File2.cs{2}
				-dir2");
		}
		
		[TestCase(null)]
		[TestCase(".meta")]
		public void AfterCut_ToItself(string meta)
		{
			Init(meta, false);
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
		
		[Test]
		public void MetaCut()
		{
			Init(".meta", false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File1.cs.meta", 10))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
					.Add(new FakeFSProxy.FakeFile("File2.cs.meta", 20))
				)
				.Add(new FakeFSProxy.FakeFile("dir1.meta", 11))
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeDir("dir3")
						.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
						.Add(new FakeFSProxy.FakeFile("File3.cs.meta", 30))
					)
					.Add(new FakeFSProxy.FakeFile("dir3.meta", 31))
				)
				.Add(new FakeFSProxy.FakeFile("dir2.meta", 21))
			);
			ExecuteNoErrors(new string[] { "c:\\dir2", "c:\\dir2.meta" }, "c:\\dir1", PasteFromClipboardAction.Cut);
			AssertFS(@"c:
				-dir1
				--dir2
				---dir3
				----File3.cs{3}
				----File3.cs.meta{30}
				---dir3.meta{31}
				--dir2.meta{21}
				--File1.cs{1}
				--File1.cs.meta{10}
				--File2.cs{2}
				--File2.cs.meta{20}
				-dir1.meta{11}");
		}
		
		[Test]
		public void MetaCopy_DontCopyMeta()
		{
			Init(".meta", false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File1.cs.meta", 10))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
					.Add(new FakeFSProxy.FakeFile("File2.cs.meta", 20))
				)
				.Add(new FakeFSProxy.FakeFile("dir1.meta", 11))
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeDir("dir3")
						.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
						.Add(new FakeFSProxy.FakeFile("File3.cs.meta", 30))
					)
					.Add(new FakeFSProxy.FakeFile("dir3.meta", 31))
				)
				.Add(new FakeFSProxy.FakeFile("dir2.meta", 21))
			);
			ExecuteNoErrors(new string[] { "c:\\dir2", "c:\\dir2.meta" }, "c:\\dir1", PasteFromClipboardAction.Copy);
			AssertFS(@"c:
				-dir1
				--dir2
				---dir3
				----File3.cs{3}
				--File1.cs{1}
				--File1.cs.meta{10}
				--File2.cs{2}
				--File2.cs.meta{20}
				-dir2
				--dir3
				---File3.cs{3}
				---File3.cs.meta{30}
				--dir3.meta{31}
				-dir1.meta{11}
				-dir2.meta{21}
			");
		}
		
		[Test]
		public void MetaCopy_CopyMeta()
		{
			Init(".meta", true);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File1.cs.meta", 10))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
					.Add(new FakeFSProxy.FakeFile("File2.cs.meta", 20))
				)
				.Add(new FakeFSProxy.FakeFile("dir1.meta", 11))
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeDir("dir3")
						.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
						.Add(new FakeFSProxy.FakeFile("File3.cs.meta", 30))
					)
					.Add(new FakeFSProxy.FakeFile("dir3.meta", 31))
				)
				.Add(new FakeFSProxy.FakeFile("dir2.meta", 21))
			);
			ExecuteNoErrors(new string[] { "c:\\dir2", "c:\\dir2.meta" }, "c:\\dir1", PasteFromClipboardAction.Copy);
			AssertFS(@"c:
				-dir1
				--dir2
				---dir3
				----File3.cs{3}
				----File3.cs.meta{30}
				---dir3.meta{31}
				--dir2.meta{21}
				--File1.cs{1}
				--File1.cs.meta{10}
				--File2.cs{2}
				--File2.cs.meta{20}
				-dir2
				--dir3
				---File3.cs{3}
				---File3.cs.meta{30}
				--dir3.meta{31}
				-dir1.meta{11}
				-dir2.meta{21}
			");
		}
	}
}
