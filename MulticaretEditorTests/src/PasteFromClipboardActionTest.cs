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
		
		[Test]
		public void Simple()
		{
			Init(".meta", false);
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("dir1")
					.Add(new FakeFSProxy.FakeFile("File1.cs", 1))
					.Add(new FakeFSProxy.FakeFile("File2.cs", 2))
				)
				.Add(new FakeFSProxy.FakeDir("dir2")
					.Add(new FakeFSProxy.FakeFile("File3.cs", 3))
				)
			);
			action.Execute(new string[] { "c:\\dir1\\File1.cs", "c:\\dir1\\File2.cs" }, "c:\\dir2", true);
			AssertFS(@"c:
				-dir1
				-dir2
				--File1.cs{1}
				--File2.cs{2}
				--File3.cs{3}
			");
		}
	}
}
