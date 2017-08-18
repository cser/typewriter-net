using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class BookmarksTest
	{
		private MacrosExecutor executor;
		
		[SetUp]
		public void SetUp()
		{
			executor = new MacrosExecutor(null);
		}
		
		private void AssertBookmark(char c, string expectedFile, int expectedPosition)
		{
			string file;
			int position;
			executor.GetBookmark(c, out file, out position);
			Assert.AreEqual("file=" + expectedFile + ",position=" + expectedPosition,
				"file=" + file + ",position=" + position, c + "");
		}
		
		[Test]
		public void Simple()
		{
			executor.SetBookmark('A', "file0", 2);
			AssertBookmark('A', "file0", 2);
			executor.SetBookmark('B', "file1", 3);
			AssertBookmark('B', "file1", 3);
			executor.SetBookmark('Z', "file2", 10);
			AssertBookmark('Z', "file2", 10);
		}
		
		[Test]
		public void FileDuplication()
		{
			executor.SetBookmark('A', "file0", 2);
			executor.SetBookmark('B', "file0", 3);
			executor.SetBookmark('Z', "file2", 10);
			AssertBookmark('A', "file0", 2);
			AssertBookmark('B', "file0", 3);
			AssertBookmark('Z', "file2", 10);
		}
		
		[Test]
		public void FileChange()
		{
			executor.SetBookmark('A', "file0", 2);
			executor.SetBookmark('B', "file0", 3);
			executor.SetBookmark('A', "file1", 18);
			executor.SetBookmark('Z', "file2", 10);
			executor.SetBookmark('B', "file3", 21);
			AssertBookmark('A', "file1", 18);
			AssertBookmark('B', "file3", 21);
			AssertBookmark('Z', "file2", 10);
		}
		
		[Test]
		public void OutBoundsDoesNothing()
		{
			executor.SetBookmark('A', "file1", 2);
			executor.SetBookmark((char)('A' - 1), "file0", 2);
			executor.SetBookmark((char)('Z' + 1), "file0", 3);
			executor.SetBookmark('Z', "file2", 10);
			AssertBookmark('A', "file1", 2);
			AssertBookmark('B', null, -1);
			AssertBookmark('Z', "file2", 10);
		}
	}
}