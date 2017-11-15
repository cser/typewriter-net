using System;
using MulticaretEditor;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class PasteModeTest
	{
		private FakeFSProxy fs;
		private PasteFromClipboardAction action;
		
		private void AssertFS(string expected)
		{
			Assert.AreEqual(FakeFSProxy.NormalizeString(expected), fs.ToString());
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
		
		private void ExecuteNoErrors(string[] files, string targetDir, PasteMode mode)
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
				PasteMode.Cut);
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
				PasteMode.Copy);
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
			ExecuteNoErrors(new string[] { "c:\\dir1\\File1.cs" }, "c:\\dir1", PasteMode.Copy);
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
		public void AfterCopy_SeveralFilesDuplication(string meta)
		{
			Init(null, false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeDir("dir3")
						.Add(new FakeFSProxy.FakeFile("File3", 3))
					)
					.Add(new FakeFSProxy.FakeFile(".gitignore", 10))
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
			);
			ExecuteNoErrors(
				new string[] { "c:\\dir1\\File1.cs", "c:\\dir1\\.gitignore", "c:\\dir1\\dir3" },
				"c:\\dir1",
				PasteMode.Copy);
			AssertFS(@"c:
				-dir1
				--dir3
				---File3{3}
				--dir3-copy
				---File3{3}
				--.gitignore{10}
				--.gitignore-copy{10}
				--File1.cs{1}
				--File1-copy.cs{1}
				--File2.cs{2}");
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
			action.Execute(new string[] { "c:\\dir2\\File1.cs" }, "c:\\dir1", PasteMode.Copy);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			CollectionAssert.AreEqual(new string[] { "c:\\dir1\\File1.cs" }, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--File1.cs{3}");
			action.Execute(new string[] { "c:\\dir2\\File1.cs" }, "c:\\dir1", PasteMode.CopyOverwrite);
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
			action.Execute(new string[] { "c:\\dir2\\dir1" }, "c:", PasteMode.Copy);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			CollectionAssert.AreEqual(new string[] { "c:\\dir1" }, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--dir1
				---File1.cs{3}");
			action.Execute(new string[] { "c:\\dir2\\dir1" }, "c:", PasteMode.CopyOverwrite);
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
			ExecuteNoErrors(new string[] { "c:\\dir2" }, "c:\\dir2", PasteMode.Copy);
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
			action.Execute(new string[] { "c:\\dir2\\dir1" }, "c:", PasteMode.Cut);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			CollectionAssert.AreEqual(new string[] { "c:\\dir1" }, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--dir1
				---File1.cs{3}");
			action.Execute(new string[] { "c:\\dir2\\dir1" }, "c:", PasteMode.CutOverwrite);
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
			ExecuteNoErrors(new string[] { "c:\\dir2\\dir1" }, "c:\\dir2", PasteMode.Cut);
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
			ExecuteNoErrors(new string[] { "c:\\dir2", "c:\\dir2.meta" }, "c:\\dir1", PasteMode.Cut);
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
		public void MetaCopy_Disabled()
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
			ExecuteNoErrors(new string[] { "c:\\dir2", "c:\\dir2.meta" }, "c:\\dir1", PasteMode.Copy);
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
		public void MetaCopy_Enabled()
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
			ExecuteNoErrors(new string[] { "c:\\dir2", "c:\\dir2.meta" }, "c:\\dir1", PasteMode.Copy);
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
		
		[TestCase(null, false)]
		[TestCase(null, true)]
		[TestCase(".meta", false)]
		[TestCase(".meta", true)]
		public void AfterCut_DirAlreadyMissing(string meta, bool pastePostfixedAfterCopy)
		{
			Init(meta, pastePostfixedAfterCopy);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeDir("dir3")
						.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
					)
				)
			);
			ExecuteNoErrors(new string[] { "c:\\dir2\\dir3", "c:\\dir2\\dir4" }, "c:", PasteMode.Cut);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				-dir3
				--File3.cs{3}");
		}
		
		[TestCase(null)]
		[TestCase(".meta")]
		public void AfterMove_SimpleDirMoving_IfOtherOverrwrite(string meta)
		{
			Init(null, false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeDir("dir2")
						.Add(new FakeFSProxy.FakeFile("File2", 2))
					)
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("overwrite", 11))
				)
				.Add(new FakeFSProxy.FakeFile("overwrite", 10))
			);
			action.Execute(new string[] { "c:\\dir1\\dir2", "c:\\dir1\\overwrite" }, "c:", PasteMode.Cut);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			CollectionAssert.AreEqual(new string[] { "c:\\overwrite" }, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--dir2
				---File2{2}
				--File1.cs{1}
				--overwrite{11}
				-overwrite{10}");
			action.Execute(new string[] { "c:\\dir1\\dir2", "c:\\dir1\\overwrite" }, "c:", PasteMode.CutOverwrite);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				-dir2
				--File2{2}
				-overwrite{11}");
		}
		
		[TestCase(null)]
		[TestCase(".meta")]
		public void AfterMove_FileOverwriteByDirectory(string meta)
		{
			Init(null, false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeDir("dir2"))
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("overwrite", 11))
				)
				.Add(new FakeFSProxy.FakeDir("overwrite")
					.Add(new FakeFSProxy.FakeFile("File2", 2))
				)
			);
			action.Execute(new string[] { "c:\\overwrite" }, "c:\\dir1", PasteMode.Cut);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			CollectionAssert.AreEqual(new string[] { "c:\\dir1\\overwrite" }, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--dir2
				--File1.cs{1}
				--overwrite{11}
				-overwrite
				--File2{2}");
			action.Execute(new string[] { "c:\\overwrite" }, "c:\\dir1", PasteMode.CutOverwrite);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			AssertFS(@"c:
				-dir1
				--dir2
				--overwrite
				---File2{2}
				--File1.cs{1}");
		}
		
		[TestCase(null)]
		[TestCase(".meta")]
		public void AfterMove_DirectoryOverwriteByFile(string meta)
		{
			Init(null, false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeDir("dir2"))
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("overwrite", 11))
				)
				.Add(new FakeFSProxy.FakeDir("overwrite")
					.Add(new FakeFSProxy.FakeFile("File2", 2))
				)
			);
			action.Execute(new string[] { "c:\\dir1\\overwrite" }, "c:", PasteMode.Cut);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			CollectionAssert.AreEqual(new string[] { "c:\\overwrite" }, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--dir2
				--File1.cs{1}
				--overwrite{11}
				-overwrite
				--File2{2}");
			action.Execute(new string[] { "c:\\dir1\\overwrite" }, "c:", PasteMode.CutOverwrite);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			AssertFS(@"c:
				-dir1
				--dir2
				--File1.cs{1}
				-overwrite{11}");
		}
		
		[TestCase(null)]
		[TestCase(".meta")]
		public void AfterMove_OverrdieDirectoryContent(string meta)
		{
			Init(null, false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeDir("dir2")
						.Add(new FakeFSProxy.FakeFile("DIR3", 1))
					)
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeDir("DIR3")
						.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
						.Add(new FakeFSProxy.FakeFile("File4.cs", 4))
					)
					.Add(new FakeFSProxy.FakeFile("File5.cs", 5))
				)
			);
			action.Execute(new string[] { "c:\\dir2" }, "c:\\dir1", PasteMode.Cut);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			CollectionAssert.AreEqual(new string[] { "c:\\dir1\\dir2" }, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--dir2
				---DIR3{1}
				--File2.cs{2}
				-dir2
				--DIR3
				---File3.cs{3}
				---File4.cs{4}
				--File5.cs{5}");
			action.Execute(new string[] { "c:\\dir2" }, "c:\\dir1", PasteMode.CutOverwrite);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			AssertFS(@"c:
				-dir1
				--dir2
				---DIR3
				----File3.cs{3}
				----File4.cs{4}
				---File5.cs{5}
				--File2.cs{2}");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void MetaCopy_MetaDontLockPasting(bool pastePostfixedAfterCopy)
		{
			Init(".meta", pastePostfixedAfterCopy);
			fs.ApplyString(@"c:
				-dir1
				--f1{0}
				--f2.meta{0}
				-f2{1}");
			ExecuteNoErrors(new string[] { "c:\\f2" }, "c:\\dir1", PasteMode.Copy);
			AssertFS(@"c:
				-dir1
				--f1{0}
				--f2{1}
				--f2.meta{0}
				-f2{1}");
		}
		
		[Test]
		public void MetaCopy_RenameWithMeta()
		{
			Init(".meta", true);
			fs.ApplyString(@"c:
				-dir1
				--f1{0}
				--f2{1}
				--f2.meta{10}");
			ExecuteNoErrors(new string[] { "c:\\dir1\\f2", "c:\\dir1\\f2.meta" }, "c:\\dir1", PasteMode.Copy);
			AssertFS(@"c:
				-dir1
				--f1{0}
				--f2{1}
				--f2.meta{10}
				--f2-copy{1}
				--f2-copy.meta{10}");
		}
		
		[Test]
		public void MetaCopy_Rewrite()
		{
			Init(".meta", true);
			fs.ApplyString(@"c:
				-dir1
				--f1{0}
				--f2{11}
				--f2.meta{12}
				-f2{21}
				-f2.meta{22}");
			action.Execute(new string[] { "c:\\dir1\\f2", "c:\\dir1\\f2.meta" }, "c:", PasteMode.Copy);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			CollectionAssert.AreEqual(new string[] { "c:\\f2", "c:\\f2.meta" }, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--f1{0}
				--f2{11}
				--f2.meta{12}
				-f2{21}
				-f2.meta{22}");
			action.Execute(new string[] { "c:\\dir1\\f2", "c:\\dir1\\f2.meta" }, "c:", PasteMode.CopyOverwrite);
			AssertFS(@"c:
				-dir1
				--f1{0}
				--f2{11}
				--f2.meta{12}
				-f2{11}
				-f2.meta{12}");
		}
		
		[Test]
		public void MetaCopy_RewriteDir()
		{
			Init(".meta", true);
			fs.ApplyString(@"c:
				-dir1
				--f1{0}
				--f2{11}
				--f2.meta{12}
				-f2.meta
				--file{0}
				-f2{21}");
			action.Execute(new string[] { "c:\\dir1\\f2", "c:\\dir1\\f2.meta" }, "c:", PasteMode.Copy);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			CollectionAssert.AreEqual(new string[] { "c:\\f2", "c:\\f2.meta" }, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--f1{0}
				--f2{11}
				--f2.meta{12}
				-f2.meta
				--file{0}
				-f2{21}");
			action.Execute(new string[] { "c:\\dir1\\f2", "c:\\dir1\\f2.meta" }, "c:", PasteMode.CopyOverwrite);
			AssertFS(@"c:
				-dir1
				--f1{0}
				--f2{11}
				--f2.meta{12}
				-f2{11}
				-f2.meta{12}");
		}
		
		[Test]
		public void MetaCut_Rewrite()
		{
			Init(".meta", true);
			fs.ApplyString(@"c:
				-dir1
				--f1{0}
				--f2{11}
				--f2.meta{12}
				-f2{21}
				-f2.meta{22}");
			action.Execute(new string[] { "c:\\dir1\\f2", "c:\\dir1\\f2.meta" }, "c:", PasteMode.Cut);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			CollectionAssert.AreEqual(new string[] { "c:\\f2", "c:\\f2.meta" }, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--f1{0}
				--f2{11}
				--f2.meta{12}
				-f2{21}
				-f2.meta{22}");
			action.Execute(new string[] { "c:\\dir1\\f2", "c:\\dir1\\f2.meta" }, "c:", PasteMode.CutOverwrite);
			AssertFS(@"c:
				-dir1
				--f1{0}
				-f2{11}
				-f2.meta{12}");
		}
		
		[Test]
		public void MetaCut_RewriteDir()
		{
			Init(".meta", true);
			fs.ApplyString(@"c:
				-dir1
				--f1{0}
				--f2{11}
				--f2.meta{12}
				-f2.meta
				--file{0}
				-f2{21}");
			action.Execute(new string[] { "c:\\dir1\\f2", "c:\\dir1\\f2.meta" }, "c:", PasteMode.Cut);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			CollectionAssert.AreEqual(new string[] { "c:\\f2", "c:\\f2.meta" }, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--f1{0}
				--f2{11}
				--f2.meta{12}
				-f2.meta
				--file{0}
				-f2{21}");
			action.Execute(new string[] { "c:\\dir1\\f2", "c:\\dir1\\f2.meta" }, "c:", PasteMode.CutOverwrite);
			AssertFS(@"c:
				-dir1
				--f1{0}
				-f2{11}
				-f2.meta{12}");
		}
		
		[TestCase(null)]
		[TestCase(".meta")]
		public void AfterCopy_UnauthorizedError(string meta)
		{
			Init(null, false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1").SetUnauthorized()
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
				)
			);
			action.Execute(new string[] { "c:\\dir2\\File3.cs" }, "c:\\dir1", PasteMode.Copy);
			Assert.AreEqual(1, action.Errors.Count);
			Assert.IsTrue(action.Errors[0].Contains("Unauthorized access to: c:\\dir1\\File3.cs"));
			CollectionAssert.AreEqual(new string[] {}, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--File3.cs{3}");
		}
		
		[TestCase(null)]
		[TestCase(".meta")]
		public void AfterCopy_OneFileDuplication_UnauthorizedError(string meta)
		{
			Init(null, false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1").SetUnauthorized()
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
			);
			action.Execute(new string[] { "c:\\dir1\\File1.cs" }, "c:\\dir1", PasteMode.Copy);
			Assert.AreEqual(1, action.Errors.Count);
			Assert.IsTrue(action.Errors[0].Contains("Unauthorized access to:"));
			CollectionAssert.AreEqual(new string[] {}, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}");
		}
		
		[TestCase(null)]
		[TestCase(".meta")]
		public void AfterCopy_Overwrite_UnauthorizedError(string meta)
		{
			Init(null, false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeFile("File2.cs", 3))
				)
			);
			action.Execute(new string[] { "c:\\dir2\\File2.cs" }, "c:\\dir1", PasteMode.Copy);
			CollectionAssert.AreEqual(new string[] {}, action.Errors);
			CollectionAssert.AreEqual(new string[] { "c:\\dir1\\File2.cs" }, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--File2.cs{3}");
			fs.GetItem("c:").AsDir.GetItem("dir1").AsDir.SetUnauthorized();
			action.Execute(new string[] { "c:\\dir2\\File2.cs" }, "c:\\dir1", PasteMode.CopyOverwrite);
			Assert.AreEqual(1, action.Errors.Count);
			Assert.IsTrue(action.Errors[0].Contains("Unauthorized access to: c:\\dir1\\File2.cs"));
			CollectionAssert.AreEqual(new string[] {}, action.Overwrites);
			AssertFS(@"c:
				-dir1
				--File1.cs{1}
				--File2.cs{2}
				-dir2
				--File2.cs{3}");
		}
	}
}
