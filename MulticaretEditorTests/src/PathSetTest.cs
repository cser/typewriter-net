using System;
using MulticaretEditor;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class PathSetTest
	{
		private PathSet _pathSet;
		
		[SetUp]
		public void SetUp()
		{
			_pathSet = new PathSet();
		}
		
		[Test]
		public void GetNorm()
		{
			Assert.AreEqual("c:\\a\\b", PathSet.GetNorm("c:\\a\\b"));
			Assert.AreEqual("c:\\a\\b", PathSet.GetNorm("c:\\a\\b\\"));
			Assert.AreEqual("c:\\a\\b", PathSet.GetNorm("C:\\A\\b\\"));
		}
		
		[Test]
		public void Add()
		{
			Assert.AreEqual("c:\\a\\b.txt", _pathSet.Add("c:\\a\\b.txt"));
			Assert.AreEqual("c:\\a\\c.txt", _pathSet.Add("c:\\a\\c.txt"));
			Assert.AreEqual(2, _pathSet.Count);
			Assert.AreEqual(true, _pathSet.Contains("c:\\a\\b.txt"));
			Assert.AreEqual(true, _pathSet.Contains("C:\\A\\B.TXT"));
			Assert.AreEqual(false, _pathSet.Contains("c:\\c\\b.txt"));
			Assert.AreEqual(true, _pathSet.Contains("c:\\a\\c.txt"));
		}
		
		[Test]
		public void Remove()
		{
			Assert.AreEqual("c:\\a\\b.txt", _pathSet.Add("c:\\a\\b.txt"));
			Assert.AreEqual("c:\\a\\c.txt", _pathSet.Add("c:\\a\\c.txt"));
			Assert.AreEqual(true, _pathSet.Contains("c:\\a\\b.txt"));
			_pathSet.Remove("c:\\a\\b.txt");
			_pathSet.Remove("c:\\c\\b.txt");
			Assert.AreEqual(1, _pathSet.Count);
			Assert.AreEqual(false, _pathSet.Contains("c:\\a\\b.txt"));
			Assert.AreEqual(true, _pathSet.Contains("C:\\A\\C.TXT"));
		}
		
		[Test]
		public void ContainsBug()
		{
			Assert.AreEqual("c:\\a\\b\\", _pathSet.Add("c:\\a\\b\\"));
			Assert.AreEqual(true, _pathSet.Contains("c:\\a\\b"));
			Assert.AreEqual(true, _pathSet.Contains("c:\\a\\b\\"));
			Assert.AreEqual(false, _pathSet.Contains("c:\\a\\c\\"));
		}
		
		[Test]
		public void RemoveBug()
		{
			Assert.AreEqual("c:\\a\\b", _pathSet.Add("c:\\a\\b"));
			Assert.AreEqual(true, _pathSet.Contains("c:\\a\\b"));
			_pathSet.Remove("c:\\a\\b\\");
			Assert.AreEqual(false, _pathSet.Contains("c:\\a\\b"));
		}
		
		[Test]
		public void Add_DontChangeInputString()
		{
			Assert.AreEqual("c:\\A\\b\\", _pathSet.Add("c:\\A\\b\\"));
			Assert.AreEqual("D:\\a\\b\\", _pathSet.Add("D:\\a\\b\\"));
			Assert.AreEqual(true, _pathSet.Contains("c:\\a\\b"));
			Assert.AreEqual(true, _pathSet.Contains("c:\\a\\b\\"));
			Assert.AreEqual(true, _pathSet.Contains("d:\\a\\b"));
			Assert.AreEqual(true, _pathSet.Contains("d:\\a\\b\\"));
			Assert.AreEqual(false, _pathSet.Contains("d:\\a\\c\\"));
		}
	}
}
