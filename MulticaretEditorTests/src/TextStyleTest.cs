using System;
using MulticaretEditor;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class TextStyleTest
	{
		[Test]
		public void StylesGettingAndSetting()
		{
			TextStyle style = new TextStyle();
			Assert.AreEqual(false, style.Italic);
			Assert.AreEqual(false, style.Bold);
			Assert.AreEqual(false, style.Underline);
			Assert.AreEqual(false, style.Strikeout);
			
			style.Italic = true;
			Assert.AreEqual(true, style.Italic);
			Assert.AreEqual(false, style.Bold);
			Assert.AreEqual(false, style.Underline);
			Assert.AreEqual(false, style.Strikeout);
			
			style.Italic = false;
			Assert.AreEqual(false, style.Italic);
			Assert.AreEqual(false, style.Bold);
			Assert.AreEqual(false, style.Underline);
			Assert.AreEqual(false, style.Strikeout);
			
			style.Strikeout = true;
			Assert.AreEqual(false, style.Italic);
			Assert.AreEqual(false, style.Bold);
			Assert.AreEqual(false, style.Underline);
			Assert.AreEqual(true, style.Strikeout);
			
			style.Strikeout = false;
			style.Bold = true;
			Assert.AreEqual(false, style.Italic);
			Assert.AreEqual(true, style.Bold);
			Assert.AreEqual(false, style.Underline);
			Assert.AreEqual(false, style.Strikeout);
			
			style.Underline = true;
			Assert.AreEqual(false, style.Italic);
			Assert.AreEqual(true, style.Bold);
			Assert.AreEqual(true, style.Underline);
			Assert.AreEqual(false, style.Strikeout);
			
			style.Bold = false;
			Assert.AreEqual(false, style.Italic);
			Assert.AreEqual(false, style.Bold);
			Assert.AreEqual(true, style.Underline);
			Assert.AreEqual(false, style.Strikeout);
		}
	}
}
