using System;
using MulticaretEditor;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class LineArraySearchTest
	{
		private LineArray lines;
		
		private void Init(string text)
		{
			lines = new LineArray(4);
			lines.SetText(text);
		}
		
		[Test]
		public void IndexOf_OneLine()
		{
			Init("Some line for search text in line");
			Assert.AreEqual(5, lines.IndexOf("line", 0));
			Assert.AreEqual(29, lines.IndexOf("line", 6));
			
			Init("Du\nDu hast\r\nDu hast mich");
			Assert.AreEqual(20, lines.IndexOf("mich", 0));
			Assert.AreEqual(6, lines.IndexOf("hast", 3));
			Assert.AreEqual(15, lines.IndexOf("hast", 7));
		}
		
		[Test]
		public void IndexOf_Multiline()
		{
			Init("Du\nDu hast\r\nDu hast mich");
			Assert.AreEqual(1, lines.IndexOf("u\nDu", 1));
			Assert.AreEqual(2, lines.IndexOf("\nDu", 1));
		}
		
		[Test]
		public void LineSubdivider_GetLines()
		{
			CollectionAssert.AreEqual(new string[] { "Du\n", "Du hast\r\n", "Du hast mich" }, new LineSubdivider("Du\nDu hast\r\nDu hast mich").GetLines());
			CollectionAssert.AreEqual(new string[] { "Du" }, new LineSubdivider("Du").GetLines());
			CollectionAssert.AreEqual(new string[] { "Du\n", "\r\n", "" }, new LineSubdivider("Du\n\r\n").GetLines());
			CollectionAssert.AreEqual(new string[] { "\n", "Du hast\r\n", "Du hast mich" }, new LineSubdivider("\nDu hast\r\nDu hast mich").GetLines());
		}
		
		[Test]
		public void LineSubdivider_GetLinesCount()
		{
			Assert.AreEqual(3, new LineSubdivider("Du\nDu hast\r\nDu hast mich").GetLinesCount());
			Assert.AreEqual(1, new LineSubdivider("Du").GetLinesCount());
			Assert.AreEqual(3, new LineSubdivider("Du\n\r\n").GetLinesCount());
			Assert.AreEqual(3, new LineSubdivider("\nDu hast\r\nDu hast mich").GetLinesCount());
		}
		
		[Test]
		public void LineSubdivider_GetWithoutEndRN()
		{
			Assert.AreEqual("text", LineSubdivider.GetWithoutEndRN("text\r\n"));
			Assert.AreEqual("text", LineSubdivider.GetWithoutEndRN("text\n"));
			Assert.AreEqual("text", LineSubdivider.GetWithoutEndRN("text\r"));
			Assert.AreEqual("text", LineSubdivider.GetWithoutEndRN("text"));
			Assert.AreEqual("t", LineSubdivider.GetWithoutEndRN("t\r\n"));
			Assert.AreEqual("t", LineSubdivider.GetWithoutEndRN("t\n"));
			Assert.AreEqual("t", LineSubdivider.GetWithoutEndRN("t\r"));
			Assert.AreEqual("t", LineSubdivider.GetWithoutEndRN("t"));
			Assert.AreEqual("", LineSubdivider.GetWithoutEndRN("\r\n"));
			Assert.AreEqual("", LineSubdivider.GetWithoutEndRN("\n"));
			Assert.AreEqual("", LineSubdivider.GetWithoutEndRN("\r"));
			Assert.AreEqual("", LineSubdivider.GetWithoutEndRN(""));
		}
		
		[Test]
		public void IndexOf_SignOfStartIndex()
		{
			Init("Du\nDu hast\r\nDu hast\r\nDu hast mich");
			Assert.AreEqual(6, lines.IndexOf("hast\r\n", 0));
			Assert.AreEqual(6, lines.IndexOf("hast\r\n", 6));
			Assert.AreEqual(15, lines.IndexOf("hast\r\n", 7));
		}
		
		[Test]
		public void IndexOf_MoreThanTwoLines()
		{
			//                   1              2             3              4
			//    0 12 34 56 78 90 12 34 56 78 90 123 456 78 90 12 34 56 789 012 345
			Init("1\n2\n3\n4\n5\n6\n7\n1\n2\n8\n9\n10\n11\n1\n2\n3\n4\n5\n12\n13\n14");
			Assert.AreEqual(2, lines.IndexOf("2\n3\n4", 0));
			Assert.AreEqual(30, lines.IndexOf("2\n3\n4", 3));
		}
	}
}
