using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class FileNameFilterTest
	{
		private bool Check(string pattern, string text)
		{
			return new FileNameFilter(pattern).Match(text);
		}
		
		[Test]
		public void Test01()
		{
			Assert.AreEqual(true, Check("*", "text.txt"));
		}
		
		[Test]
		public void Test02()
		{
			Assert.AreEqual(true, Check("*.txt", "text.txt"));
			Assert.AreEqual(true, Check("t*xt.txt", "text.txt"));
		}
		
		[Test]
		public void Test03()
		{
			Assert.AreEqual(true, Check("t*x*t.txt", "text.txt"));
		}
		
		[Test]
		public void Test04()
		{
			Assert.AreEqual(true, Check("t*x*t?txt", "text.txt"));
			Assert.AreEqual(true, Check("t??t.txt", "text.txt"));
		}
		
		[Test]
		public void Test05()
		{
			Assert.AreEqual(false, Check("a*", "text.txt"));
			Assert.AreEqual(false, Check("t?t.txt", "text.txt"));
		}
		
		[Test]
		public void Test06()
		{
			Assert.AreEqual(true, Check("t??t.tx?", "text.txt"));
			Assert.AreEqual(false, Check("t??t.te?", "text.txt"));
		}
		
		[Test]
		public void Critical01()
		{
			Assert.AreEqual(false, Check("", "text.txt"));
			Assert.AreEqual(true, Check("", ""));
			Assert.AreEqual(false, Check("?", ""));
			Assert.AreEqual(true, Check("?", "a"));
			Assert.AreEqual(true, Check("*", ""));
		}
		
		[Test]
		public void Critical02()
		{
			Assert.AreEqual(true, Check("t?**tx?", "text.txt"));
			Assert.AreEqual(false, Check("t??t.te?", "text.txt"));
		}
		
		[Test]
		public void Critical03()
		{
			Assert.AreEqual(true, Check("*?text", "atext"));
			Assert.AreEqual(false, Check("*?text", "text"));
		}
		
		[Test]
		public void Critical04()
		{
			Assert.AreEqual(true, Check("***ext", "text"));
			Assert.AreEqual(true, Check("**text", "text"));
		}
		
		[Test]
		public void Critical05()
		{
			Assert.AreEqual(true, Check("*?*text", "atext"));
			Assert.AreEqual(false, Check("*?*text", "text"));
		}
		
		[Test]
		public void Multipattern()
		{
			Assert.AreEqual(true, Check("*.txt;*.com", "text.txt"));
			Assert.AreEqual(true, Check("*.txt; *.com", "command.com"));
			
			Assert.AreEqual(false, Check("*.txt;*.com", "text.md"));
		}
	}
}
