using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class CommonHelperTest
	{
		[Test]
		public void Clamp_Normal()
		{
			Assert.AreEqual(1, CommonHelper.Clamp(-1, 1, 10));
			Assert.AreEqual(1, CommonHelper.Clamp(0, 1, 10));
			Assert.AreEqual(1, CommonHelper.Clamp(1, 1, 10));
			Assert.AreEqual(2, CommonHelper.Clamp(2, 1, 10));
			Assert.AreEqual(5, CommonHelper.Clamp(5, 1, 10));
			Assert.AreEqual(9, CommonHelper.Clamp(9, 1, 10));
			Assert.AreEqual(10, CommonHelper.Clamp(10, 1, 10));
			Assert.AreEqual(10, CommonHelper.Clamp(11, 1, 10));
			Assert.AreEqual(10, CommonHelper.Clamp(12, 1, 10));
		}
		
		[Test]
		public void Clamp_Constraints()
		{
			Assert.AreEqual(0, CommonHelper.Clamp(-2, 0, -1));
			Assert.AreEqual(0, CommonHelper.Clamp(-1, 0, -1));
			Assert.AreEqual(0, CommonHelper.Clamp(0, 0, -1));
			Assert.AreEqual(0, CommonHelper.Clamp(0, 0, -1));
			
			Assert.AreEqual(2, CommonHelper.Clamp(0, 2, 0));
			Assert.AreEqual(2, CommonHelper.Clamp(1, 2, 0));
			Assert.AreEqual(2, CommonHelper.Clamp(2, 2, 0));
			Assert.AreEqual(2, CommonHelper.Clamp(3, 2, 0));
		}

		[Test]
		public void GetOneLine()
		{
			Assert.AreEqual("aaa", CommonHelper.GetOneLine("aaa\nbb\ncccc"));
			Assert.AreEqual("aaa", CommonHelper.GetOneLine("aaa\rbb\rcccc"));
			Assert.AreEqual("aaa", CommonHelper.GetOneLine("aaa\r\nbb\r\ncccc"));
			Assert.AreEqual("aaa", CommonHelper.GetOneLine("aaa\rbb\ncccc"));
			Assert.AreEqual("aaa", CommonHelper.GetOneLine("aaa\nbb\rcccc"));

			Assert.AreEqual("AAA", CommonHelper.GetOneLine("AAA"));
			Assert.AreEqual("AAA", CommonHelper.GetOneLine("AAA\r\nBBB"));
			Assert.AreEqual("AAA", CommonHelper.GetOneLine("AAA\nBBB\rCC"));
			Assert.AreEqual("", CommonHelper.GetOneLine("\nAAA\rBBB"));
			Assert.AreEqual("", CommonHelper.GetOneLine("\rAAA\nBBB"));
			Assert.AreEqual("", CommonHelper.GetOneLine("\r\nAAA\nBBB"));
		}
		
		[Test]
		public void GetShortText1()
		{
			Assert.AreEqual("ab", CommonHelper.GetShortText("ab", 2));
			Assert.AreEqual("a", CommonHelper.GetShortText("a", 2));
			Assert.AreEqual("a", CommonHelper.GetShortText("a", 1));
			Assert.AreEqual("", CommonHelper.GetShortText("", 1));
			Assert.AreEqual("", CommonHelper.GetShortText("", 0));
		}
		
		[Test]
		public void GetShortText2()
		{
			Assert.AreEqual("ab…fg", CommonHelper.GetShortText("abcdefg", 5));
			Assert.AreEqual("ab…ef", CommonHelper.GetShortText("abcdef", 5));
			Assert.AreEqual("a…g", CommonHelper.GetShortText("abcdefg", 3));
		}
		
		[Test]
		public void GetShortText3()
		{
			Assert.AreEqual("ab…g", CommonHelper.GetShortText("abcdefg", 4));
			Assert.AreEqual("ab…f", CommonHelper.GetShortText("abcdef", 4));
			Assert.AreEqual("a…", CommonHelper.GetShortText("abcdefg", 2));
		}
		
		[Test]
		public void GetShortText4()
		{
			Assert.AreEqual("…", CommonHelper.GetShortText("abcdefg", 1));
			Assert.AreEqual("", CommonHelper.GetShortText("a", 0));
		}
		
		[Test]
		public void GetShortText5()
		{
			Assert.AreEqual(null, CommonHelper.GetShortText(null, 0));
			Assert.AreEqual(null, CommonHelper.GetShortText(null, 1));
			Assert.AreEqual(null, CommonHelper.GetShortText(null, 2));
		}
	}
}
