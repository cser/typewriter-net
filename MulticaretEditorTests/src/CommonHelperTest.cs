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
		
		[Test]
		public void IsIdentifier0()
		{
			Assert.AreEqual(true, CommonHelper.IsIdentifier("item"));
			Assert.AreEqual(true, CommonHelper.IsIdentifier("_item"));
			Assert.AreEqual(true, CommonHelper.IsIdentifier("__item"));
			Assert.AreEqual(true, CommonHelper.IsIdentifier("item2"));
			Assert.AreEqual(true, CommonHelper.IsIdentifier("it3em23"));
			Assert.AreEqual(true, CommonHelper.IsIdentifier("it3em_23___"));
			Assert.AreEqual(false, CommonHelper.IsIdentifier("1item"));
			Assert.AreEqual(false, CommonHelper.IsIdentifier("0__ite__m102"));
		}
		
		[Test]
		public void IsIdentifier1()
		{
			Assert.AreEqual(false, CommonHelper.IsIdentifier(""));
			Assert.AreEqual(false, CommonHelper.IsIdentifier(null));
		}
		
		[Test]
		public void IsIdentifier2()
		{
			Assert.AreEqual(false, CommonHelper.IsIdentifier("_+"));
			Assert.AreEqual(false, CommonHelper.IsIdentifier("+item"));
			Assert.AreEqual(false, CommonHelper.IsIdentifier("/item"));
			Assert.AreEqual(false, CommonHelper.IsIdentifier("i/tem"));
			Assert.AreEqual(false, CommonHelper.IsIdentifier("item/"));
		}
		
		[Test]
		public void RomanConvertion()
		{
			Assert.AreEqual("I", CommonHelper.RomanOf(1));
			Assert.AreEqual("II", CommonHelper.RomanOf(2));
			Assert.AreEqual("III", CommonHelper.RomanOf(3));
			Assert.AreEqual("IV", CommonHelper.RomanOf(4));
			Assert.AreEqual("V", CommonHelper.RomanOf(5));
			Assert.AreEqual("VI", CommonHelper.RomanOf(6));
			Assert.AreEqual("VII", CommonHelper.RomanOf(7));
			Assert.AreEqual("VIII", CommonHelper.RomanOf(8));
			Assert.AreEqual("IX", CommonHelper.RomanOf(9));
			Assert.AreEqual("X", CommonHelper.RomanOf(10));
			Assert.AreEqual("XI", CommonHelper.RomanOf(11));
			Assert.AreEqual("XII", CommonHelper.RomanOf(12));
			Assert.AreEqual("XIII", CommonHelper.RomanOf(13));
			Assert.AreEqual("XIV", CommonHelper.RomanOf(14));
			Assert.AreEqual("XV", CommonHelper.RomanOf(15));
			Assert.AreEqual("XVI", CommonHelper.RomanOf(16));
			Assert.AreEqual("XXI", CommonHelper.RomanOf(21));
			Assert.AreEqual("XXIII", CommonHelper.RomanOf(23));
			Assert.AreEqual("XXIX", CommonHelper.RomanOf(29));
			Assert.AreEqual("L", CommonHelper.RomanOf(50));
			Assert.AreEqual("LXX", CommonHelper.RomanOf(70));
			Assert.AreEqual("XC", CommonHelper.RomanOf(90));
			Assert.AreEqual("C", CommonHelper.RomanOf(100));
			Assert.AreEqual("CCC", CommonHelper.RomanOf(300));
			Assert.AreEqual("CD", CommonHelper.RomanOf(400));
			Assert.AreEqual("D", CommonHelper.RomanOf(500));
			Assert.AreEqual("DC", CommonHelper.RomanOf(600));
			Assert.AreEqual("CM", CommonHelper.RomanOf(900));
			Assert.AreEqual("M", CommonHelper.RomanOf(1000));
			Assert.AreEqual("MMCIX", CommonHelper.RomanOf(2109));
			Assert.AreEqual("MMMCMXCVII", CommonHelper.RomanOf(3997));
			Assert.AreEqual("MMMCMXCVIII", CommonHelper.RomanOf(3998));
			Assert.AreEqual("MMMCMXCIX", CommonHelper.RomanOf(3999));
			Assert.AreEqual("0", CommonHelper.RomanOf(0));
			Assert.AreEqual("-1", CommonHelper.RomanOf(-1));
			Assert.AreEqual("-2", CommonHelper.RomanOf(-2));
			Assert.AreEqual("4000", CommonHelper.RomanOf(4000));
			Assert.AreEqual("4001", CommonHelper.RomanOf(4001));
			Assert.AreEqual("100103", CommonHelper.RomanOf(100103));
			Assert.AreEqual(1, CommonHelper.OfRoman("I"));
			Assert.AreEqual(2, CommonHelper.OfRoman("II"));
			Assert.AreEqual(3, CommonHelper.OfRoman("III"));
			Assert.AreEqual(4, CommonHelper.OfRoman("IV"));
			Assert.AreEqual(5, CommonHelper.OfRoman("V"));
			Assert.AreEqual(6, CommonHelper.OfRoman("VI"));
			Assert.AreEqual(7, CommonHelper.OfRoman("VII"));
			Assert.AreEqual(8, CommonHelper.OfRoman("VIII"));
			Assert.AreEqual(9, CommonHelper.OfRoman("IX"));
			Assert.AreEqual(10, CommonHelper.OfRoman("X"));
			Assert.AreEqual(11, CommonHelper.OfRoman("XI"));
			Assert.AreEqual(12, CommonHelper.OfRoman("XII"));
			Assert.AreEqual(13, CommonHelper.OfRoman("XIII"));
			Assert.AreEqual(14, CommonHelper.OfRoman("XIV"));
			Assert.AreEqual(15, CommonHelper.OfRoman("XV"));
			Assert.AreEqual(16, CommonHelper.OfRoman("XVI"));
			Assert.AreEqual(21, CommonHelper.OfRoman("XXI"));
			Assert.AreEqual(23, CommonHelper.OfRoman("XXIII"));
			Assert.AreEqual(29, CommonHelper.OfRoman("XXIX"));
			Assert.AreEqual(50, CommonHelper.OfRoman("L"));
			Assert.AreEqual(70, CommonHelper.OfRoman("LXX"));
			Assert.AreEqual(90, CommonHelper.OfRoman("XC"));
			Assert.AreEqual(100, CommonHelper.OfRoman("C"));
			Assert.AreEqual(300, CommonHelper.OfRoman("CCC"));
			Assert.AreEqual(400, CommonHelper.OfRoman("CD"));
			Assert.AreEqual(500, CommonHelper.OfRoman("D"));
			Assert.AreEqual(600, CommonHelper.OfRoman("DC"));
			Assert.AreEqual(900, CommonHelper.OfRoman("CM"));
			Assert.AreEqual(1000, CommonHelper.OfRoman("M"));
			Assert.AreEqual(2109, CommonHelper.OfRoman("MMCIX"));
			Assert.AreEqual(3997, CommonHelper.OfRoman("MMMCMXCVII"));
			Assert.AreEqual(3998, CommonHelper.OfRoman("MMMCMXCVIII"));
			Assert.AreEqual(3999, CommonHelper.OfRoman("MMMCMXCIX"));
			Assert.AreEqual(0, CommonHelper.OfRoman("ABC"));
			Assert.AreEqual(-10, CommonHelper.OfRoman("-10"));
			Assert.AreEqual(10, CommonHelper.OfRoman("10"));
		}
	}
}
