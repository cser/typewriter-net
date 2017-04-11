using System;
using System.Text;
using MulticaretEditor;
using MulticaretEditor.Highlighting;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class LineArrayTest
	{
		public delegate void Void();
		
		private LineArray lines;
		
		private void Init()
		{
			lines = new LineArray(4);
		}
		
		protected void AssertIndexOutOfRangeException(string expectedMessage, Void action)
		{
			try
			{
				action();
				Assert.Fail("IndexOutOfRangeException expected");
			}
			catch (IndexOutOfRangeException e)
			{
				Assert.AreEqual(expectedMessage, e.Message);
			}
		}
		
		protected void AssertLines(params string[] expectedLines)
		{
			CollectionAssert.AreEqual(expectedLines, lines.Debug_GetLinesText(), "Lines");
		}
		
		[Test]
		public void AlwaysHasOneLine()
		{
			LineArray lines = new LineArray(4);
			Assert.AreEqual(1, lines.LinesCount);
			Assert.AreEqual("", lines[0].Text);
			
			lines.SetText("");
			Assert.AreEqual(0, lines.charsCount);
			Assert.AreEqual(1, lines.LinesCount);
			Assert.AreEqual("", lines[0].Text);
		}
		
		[Test]
		public void SetText()
		{
			Init();
			
			lines.SetText("text");
			Assert.AreEqual(4, lines.charsCount);
			Assert.AreEqual(1, lines.LinesCount);
			Assert.AreEqual(4, lines[0].charsCount);
			Assert.AreEqual("text", lines[0].Text);
			
			lines.SetText("line0\nline1 text\n\r\nline3 text text text");
			Assert.AreEqual("line0#line1 text###line3 text text text".Length, lines.charsCount);
			Assert.AreEqual(4, lines.LinesCount);
			Assert.AreEqual("line0\n", lines[0].Text);
			Assert.AreEqual("line1 text\n", lines[1].Text);
			Assert.AreEqual("\r\n", lines[2].Text);
			Assert.AreEqual("line3 text text text", lines[3].Text);
			
			lines.SetText("\r\n\r\n\r\n\n\n\r\r\r");
			Assert.AreEqual("rnrnrnnnrrr".Length, lines.charsCount);
			CollectionAssert.AreEqual(new string[] { "\r\n", "\r\n", "\r\n", "\n", "\n", "\r", "\r", "\r", "" }, lines.Debug_GetLinesText());
			Assert.AreEqual(9, lines.LinesCount);
		}
		
		[Test]
		public void IndexOf_Simple()
		{
			Init();
			
			lines.SetText("line0\nline1 text\n\r\nline3 text text text");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\n", "\r\n", "line3 text text text" }, lines.Debug_GetLinesText());		
			
			// line0N
			// line1 textN
			// RN
			// line3 text text text
			
			Assert.AreEqual(2, lines.IndexOf(new Place(2, 0)));
			Assert.AreEqual(5, lines.IndexOf(new Place(5, 0)));
			Assert.AreEqual(6, lines.IndexOf(new Place(6, 0)));
			
			Assert.AreEqual(6, lines.IndexOf(new Place(0, 1)));
			Assert.AreEqual(7, lines.IndexOf(new Place(1, 1)));
			Assert.AreEqual(16, lines.IndexOf(new Place(10, 1)));
			Assert.AreEqual(17, lines.IndexOf(new Place(11, 1)));
			
			Assert.AreEqual(17, lines.IndexOf(new Place(0, 2)));
			Assert.AreEqual(18, lines.IndexOf(new Place(1, 2)));
			Assert.AreEqual(19, lines.IndexOf(new Place(2, 2)));
			
			Assert.AreEqual(19, lines.IndexOf(new Place(0, 3)));
			Assert.AreEqual(20, lines.IndexOf(new Place(1, 3)));
			Assert.AreEqual(21, lines.IndexOf(new Place(2, 3)));
			Assert.AreEqual(38, lines.IndexOf(new Place(19, 3)));
			Assert.AreEqual(39, lines.IndexOf(new Place(20, 3)));
		}
		
		[Test]
		public void IndexOf_SeveralBlocks()
		{
			Init();
			
			lines.SetText("abc\ndefjh\nij\nklmno\np\nq\nr\ns\nt\nu\nvwx\ny\nz");
			CollectionAssert.AreEqual(
				new string[] {"abc\n", "defjh\n", "ij\n", "klmno\n", "p\n", "q\n", "r\n", "s\n", "t\n", "u\n", "vwx\n", "y\n", "z" },
				lines.Debug_GetLinesText());
			
			//0 abcN
			//1 defjhN
			//2 ijN
			//3 klmnoN
			//4 pN
			//5 qN
			//6 rN
			//7 sN
			//8 tN
			//9 uN
			//10 vwxN
			//11 yN
			//12 z
			
			Assert.AreEqual(0, lines.IndexOf(new Place(0, 0)));
			Assert.AreEqual(1, lines.IndexOf(new Place(1, 0)));
			Assert.AreEqual(2, lines.IndexOf(new Place(2, 0)));
			Assert.AreEqual(3, lines.IndexOf(new Place(3, 0)));
			Assert.AreEqual(4, lines.IndexOf(new Place(4, 0)));
			
			Assert.AreEqual(4, lines.IndexOf(new Place(0, 1)));
			Assert.AreEqual(5, lines.IndexOf(new Place(1, 1)));
			Assert.AreEqual(6, lines.IndexOf(new Place(2, 1)));
			Assert.AreEqual(7, lines.IndexOf(new Place(3, 1)));
			Assert.AreEqual(8, lines.IndexOf(new Place(4, 1)));
			Assert.AreEqual(9, lines.IndexOf(new Place(5, 1)));
			Assert.AreEqual(10, lines.IndexOf(new Place(6, 1)));
			
			Assert.AreEqual(10, lines.IndexOf(new Place(0, 2)));
			Assert.AreEqual(11, lines.IndexOf(new Place(1, 2)));
			Assert.AreEqual(12, lines.IndexOf(new Place(2, 2)));
			Assert.AreEqual(13, lines.IndexOf(new Place(3, 2)));
			
			Assert.AreEqual(13, lines.IndexOf(new Place(0, 3)));
			Assert.AreEqual(14, lines.IndexOf(new Place(1, 3)));
			Assert.AreEqual(15, lines.IndexOf(new Place(2, 3)));
			Assert.AreEqual(16, lines.IndexOf(new Place(3, 3)));
			Assert.AreEqual(17, lines.IndexOf(new Place(4, 3)));
			Assert.AreEqual(18, lines.IndexOf(new Place(5, 3)));
			Assert.AreEqual(19, lines.IndexOf(new Place(6, 3)));
			
			Assert.AreEqual(19, lines.IndexOf(new Place(0, 4)));
			Assert.AreEqual(20, lines.IndexOf(new Place(1, 4)));
			Assert.AreEqual(21, lines.IndexOf(new Place(2, 4)));
			
			Assert.AreEqual(21, lines.IndexOf(new Place(0, 5)));
			Assert.AreEqual(22, lines.IndexOf(new Place(1, 5)));
			Assert.AreEqual(23, lines.IndexOf(new Place(2, 5)));
			
			Assert.AreEqual(23, lines.IndexOf(new Place(0, 6)));
			Assert.AreEqual(24, lines.IndexOf(new Place(1, 6)));
			Assert.AreEqual(25, lines.IndexOf(new Place(2, 6)));
			
			Assert.AreEqual(25, lines.IndexOf(new Place(0, 7)));
			Assert.AreEqual(26, lines.IndexOf(new Place(1, 7)));
			Assert.AreEqual(27, lines.IndexOf(new Place(2, 7)));
			
			Assert.AreEqual(27, lines.IndexOf(new Place(0, 8)));
			Assert.AreEqual(28, lines.IndexOf(new Place(1, 8)));
			Assert.AreEqual(29, lines.IndexOf(new Place(2, 8)));
			
			Assert.AreEqual(29, lines.IndexOf(new Place(0, 9)));
			Assert.AreEqual(30, lines.IndexOf(new Place(1, 9)));
			Assert.AreEqual(31, lines.IndexOf(new Place(2, 9)));
			
			Assert.AreEqual(31, lines.IndexOf(new Place(0, 10)));
			Assert.AreEqual(32, lines.IndexOf(new Place(1, 10)));
			Assert.AreEqual(33, lines.IndexOf(new Place(2, 10)));
			Assert.AreEqual(34, lines.IndexOf(new Place(3, 10)));
			Assert.AreEqual(35, lines.IndexOf(new Place(4, 10)));
			
			Assert.AreEqual(35, lines.IndexOf(new Place(0, 11)));
			Assert.AreEqual(36, lines.IndexOf(new Place(1, 11)));
			Assert.AreEqual(37, lines.IndexOf(new Place(2, 11)));
			
			Assert.AreEqual(37, lines.IndexOf(new Place(0, 12)));
			Assert.AreEqual(38, lines.IndexOf(new Place(1, 12)));
		}
		
		[Test]
		public void IndexOf_BoundaryValues()
		{
			Init();
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			
			// line0N
			// line1 textRN
			// line3
			
			Assert.AreEqual(6, lines.IndexOf(new Place(6, 0)));
			Assert.AreEqual(6, lines.IndexOf(new Place(7, 0)));
			Assert.AreEqual(6, lines.IndexOf(new Place(8, 0)));
			Assert.AreEqual(6, lines.IndexOf(new Place(9, 0)));
			
			Assert.AreEqual(18, lines.IndexOf(new Place(12, 1)));
			Assert.AreEqual(18, lines.IndexOf(new Place(13, 1)));
			Assert.AreEqual(18, lines.IndexOf(new Place(14, 1)));
			
			Assert.AreEqual(23, lines.IndexOf(new Place(5, 2)));
			Assert.AreEqual(23, lines.IndexOf(new Place(6, 2)));
			Assert.AreEqual(23, lines.IndexOf(new Place(7, 2)));
			
			Assert.AreEqual(23, lines.IndexOf(new Place(0, 3)));
			Assert.AreEqual(23, lines.IndexOf(new Place(1, 3)));
		}
		
		[Test]
		public void PlaceOf_Simple()
		{
			Init();
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());		
			
			// line0N
			// line1 textRN
			// line3
			
			Assert.AreEqual(new Place(0, 0), lines.PlaceOf(0));
			Assert.AreEqual(new Place(1, 0), lines.PlaceOf(1));
			Assert.AreEqual(new Place(2, 0), lines.PlaceOf(2));
			Assert.AreEqual(new Place(3, 0), lines.PlaceOf(3));
			Assert.AreEqual(new Place(4, 0), lines.PlaceOf(4));
			Assert.AreEqual(new Place(5, 0), lines.PlaceOf(5));
			
			Assert.AreEqual(new Place(0, 1), lines.PlaceOf(6));
			Assert.AreEqual(new Place(1, 1), lines.PlaceOf(7));
			Assert.AreEqual(new Place(2, 1), lines.PlaceOf(8));
			Assert.AreEqual(new Place(3, 1), lines.PlaceOf(9));
			Assert.AreEqual(new Place(4, 1), lines.PlaceOf(10));
			Assert.AreEqual(new Place(9, 1), lines.PlaceOf(15));
			Assert.AreEqual(new Place(10, 1), lines.PlaceOf(16));
			Assert.AreEqual(new Place(11, 1), lines.PlaceOf(17));
			
			Assert.AreEqual(new Place(0, 2), lines.PlaceOf(18));
			Assert.AreEqual(new Place(1, 2), lines.PlaceOf(19));
			Assert.AreEqual(new Place(2, 2), lines.PlaceOf(20));
			Assert.AreEqual(new Place(3, 2), lines.PlaceOf(21));
			Assert.AreEqual(new Place(4, 2), lines.PlaceOf(22));
			Assert.AreEqual(new Place(5, 2), lines.PlaceOf(23));
		}
		
		[Test]
		public void PlaceOf_SeveralBlocks()
		{
			Init();
			
			lines.SetText("abc\ndefjh\nij\nklmno\np\nq\nr\ns\nt\nu\nvwx\ny\nz");
			CollectionAssert.AreEqual(
				new string[] {"abc\n", "defjh\n", "ij\n", "klmno\n", "p\n", "q\n", "r\n", "s\n", "t\n", "u\n", "vwx\n", "y\n", "z" },
				lines.Debug_GetLinesText());
			
			//0 abcN
			//1 defjhN
			//2 ijN
			//3 klmnoN
			//4 pN
			//5 qN
			//6 rN
			//7 sN
			//8 tN
			//9 uN
			//10 vwxN
			//11 yN
			//12 z
			
			Assert.AreEqual(new Place(0, 0), lines.PlaceOf(0));
			Assert.AreEqual(new Place(1, 0), lines.PlaceOf(1));
			Assert.AreEqual(new Place(2, 0), lines.PlaceOf(2));
			Assert.AreEqual(new Place(3, 0), lines.PlaceOf(3));
			
			Assert.AreEqual(new Place(0, 1), lines.PlaceOf(4));
			Assert.AreEqual(new Place(1, 1), lines.PlaceOf(5));
			Assert.AreEqual(new Place(2, 1), lines.PlaceOf(6));
			Assert.AreEqual(new Place(3, 1), lines.PlaceOf(7));
			Assert.AreEqual(new Place(4, 1), lines.PlaceOf(8));
			Assert.AreEqual(new Place(5, 1), lines.PlaceOf(9));
			
			Assert.AreEqual(new Place(0, 2), lines.PlaceOf(10));
			Assert.AreEqual(new Place(1, 2), lines.PlaceOf(11));
			Assert.AreEqual(new Place(2, 2), lines.PlaceOf(12));
			
			Assert.AreEqual(new Place(0, 3), lines.PlaceOf(13));
			Assert.AreEqual(new Place(1, 3), lines.PlaceOf(14));
			Assert.AreEqual(new Place(2, 3), lines.PlaceOf(15));
			Assert.AreEqual(new Place(3, 3), lines.PlaceOf(16));
			Assert.AreEqual(new Place(4, 3), lines.PlaceOf(17));
			Assert.AreEqual(new Place(5, 3), lines.PlaceOf(18));
			
			Assert.AreEqual(new Place(0, 4), lines.PlaceOf(19));
			Assert.AreEqual(new Place(1, 4), lines.PlaceOf(20));
			
			Assert.AreEqual(new Place(0, 5), lines.PlaceOf(21));
			Assert.AreEqual(new Place(1, 5), lines.PlaceOf(22));
			
			Assert.AreEqual(new Place(0, 6), lines.PlaceOf(23));
			Assert.AreEqual(new Place(1, 6), lines.PlaceOf(24));
			
			Assert.AreEqual(new Place(0, 7), lines.PlaceOf(25));
			Assert.AreEqual(new Place(1, 7), lines.PlaceOf(26));
			
			Assert.AreEqual(new Place(0, 8), lines.PlaceOf(27));
			Assert.AreEqual(new Place(1, 8), lines.PlaceOf(28));
			
			Assert.AreEqual(new Place(0, 9), lines.PlaceOf(29));
			Assert.AreEqual(new Place(1, 9), lines.PlaceOf(30));
			
			Assert.AreEqual(new Place(0, 10), lines.PlaceOf(31));
			Assert.AreEqual(new Place(1, 10), lines.PlaceOf(32));
			Assert.AreEqual(new Place(2, 10), lines.PlaceOf(33));
			Assert.AreEqual(new Place(3, 10), lines.PlaceOf(34));
			
			Assert.AreEqual(new Place(0, 11), lines.PlaceOf(35));
			Assert.AreEqual(new Place(1, 11), lines.PlaceOf(36));
			
			Assert.AreEqual(new Place(0, 12), lines.PlaceOf(37));
			Assert.AreEqual(new Place(1, 12), lines.PlaceOf(38));
		}
		
		[Test]
		public void GetText_Simple()
		{
			Init();
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());		
			
			// line0N
			// line1 textRN
			// line3
			
			Assert.AreEqual("l", lines.GetText(0, 1));
			Assert.AreEqual("in", lines.GetText(1, 2));
			Assert.AreEqual("ine0", lines.GetText(1, 4));
			Assert.AreEqual("ine0\n", lines.GetText(1, 5));
			
			Assert.AreEqual("l", lines.GetText(6, 1));
			Assert.AreEqual("li", lines.GetText(6, 2));
			Assert.AreEqual("ne1 text\r", lines.GetText(8, 9));
			Assert.AreEqual("ne1 text\r\n", lines.GetText(8, 10));
			
			Assert.AreEqual("in", lines.GetText(19, 2));
			Assert.AreEqual("e", lines.GetText(21, 1));
			Assert.AreEqual("e3", lines.GetText(21, 2));
			
			Assert.AreEqual("line0\nline", lines.GetText(0, 10));
			Assert.AreEqual("e0\nline1 t", lines.GetText(3, 10));
			Assert.AreEqual("line0\nline1 text\r\nline3", lines.GetText(0, 23));
			Assert.AreEqual("ine0\nline1 text\r\nlin", lines.GetText(1, 20));
		}
		
		[Test]
		public void GetText_SeveralBlocks0()
		{
			Init();
			
			lines.SetText("abc\ndefjh\nij\nklmno\np\nq\nr\ns\nt\nu\nvwx\ny\nz");
			CollectionAssert.AreEqual(
				new string[] {"abc\n", "defjh\n", "ij\n", "klmno\n", "p\n", "q\n", "r\n", "s\n", "t\n", "u\n", "vwx\n", "y\n", "z" },
				lines.Debug_GetLinesText());
			
			//0 abcN
			//1 defjhN
			//2 ijN
			//3 klmnoN
			//4 pN
			//5 qN
			//6 rN
			//7 sN
			//8 tN
			//9 uN
			//10 vwxN
			//11 yN
			//12 z
			
			Assert.AreEqual("bc\ndefjh\nij\nklmno\np\nq\nr\ns\n", lines.GetText(1, 26));
			Assert.AreEqual("abc\ndefjh\nij\nklmno\np\nq\nr\ns\nt\nu\nvwx\ny\nz", lines.GetText(0, 38));
		}
		
		[Test]
		public void GetText_SeveralBlocks1()
		{
			Init();
			
			lines.SetText("abc\ndefjh\nij\nklmno\np\nq\nr\ns\nt\nu\nvwx\ny\nz");
			CollectionAssert.AreEqual(
				new string[] {"abc\n", "defjh\n", "ij\n", "klmno\n", "p\n", "q\n", "r\n", "s\n", "t\n", "u\n", "vwx\n", "y\n", "z" },
				lines.Debug_GetLinesText());
			
			//0 abcN
			//1 defjhN
			//2 ijN
			//3 klmnoN
			//4 pN
			//5 qN
			//6 rN
			//7 sN
			//8 tN
			//9 uN
			//10 vwxN
			//11 yN
			//12 z
			
			Assert.AreEqual("u", lines.GetText(29, 1));
		}
		
		[Test]
		public void RemoveText_Simple()
		{
			Init();
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			
			// line0N
			// line1 textRN
			// line3
			
			lines.RemoveText(3, 1);
			CollectionAssert.AreEqual(new string[] { "lin0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			Assert.AreEqual(22, lines.charsCount);
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			lines.RemoveText(6, 9);
			CollectionAssert.AreEqual(new string[] { "line0\n", "t\r\n", "line3" }, lines.Debug_GetLinesText());
			Assert.AreEqual(14, lines.charsCount);
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			lines.RemoveText(19, 3);
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "l3" }, lines.Debug_GetLinesText());
			Assert.AreEqual(20, lines.charsCount);
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			lines.RemoveText(20, 3);
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "li" }, lines.Debug_GetLinesText());
			Assert.AreEqual(20, lines.charsCount);
		}
		
		[Test]
		public void RemoveText_Multiline()
		{
			Init();
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			
			// line0N
			// line1 textRN
			// line3
			
			lines.RemoveText(3, 4);
			CollectionAssert.AreEqual(new string[] { "linine1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			Assert.AreEqual(19, lines.charsCount);
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			lines.RemoveText(3, 16);
			CollectionAssert.AreEqual(new string[] { "linine3" }, lines.Debug_GetLinesText());
			Assert.AreEqual(7, lines.charsCount);
		}
		
		[Test]
		public void RemoveText_LineEndChanging()
		{
			Init();
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			
			// line0N
			// line1 textRN
			// line3
			
			lines.RemoveText(5, 1);
			CollectionAssert.AreEqual(new string[] { "line0line1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			Assert.AreEqual(22, lines.charsCount);
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());		
			lines.RemoveText(0, 6);
			CollectionAssert.AreEqual(new string[] { "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());		
			Assert.AreEqual(17, lines.charsCount);
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());		
			lines.RemoveText(3, 15);
			CollectionAssert.AreEqual(new string[] { "linline3" }, lines.Debug_GetLinesText());		
			Assert.AreEqual(8, lines.charsCount);
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());		
			lines.RemoveText(6, 12);
			CollectionAssert.AreEqual(new string[] { "line0\n", "line3" }, lines.Debug_GetLinesText());		
			Assert.AreEqual(11, lines.charsCount);
		}
		
		[Test]
		public void RemoveText_SeveralBlocks()
		{
			Init();
			
			lines.SetText("abc\ndefjh\nij\nklmno\np\nq\nr\ns\nt\nu\nvwx\ny\nz");
			CollectionAssert.AreEqual(
				new string[] {"abc\n", "defjh\n", "ij\n", "klmno\n", "p\n", "q\n", "r\n", "s\n", "t\n", "u\n", "vwx\n", "y\n", "z" },
				lines.Debug_GetLinesText());
			
			//0 abcN
			//1 de[fjhN
			//2 ijN
			//3 klmnoN
			//4 pN
			//5 qN
			//6 rN
			//7 sN
			//8 tN
			//9 u]N
			//10 vwxN
			//11 yN
			//12 z
			
			lines.RemoveText(6, 24);
			Assert.AreEqual(new string[] {"abc\n", "de\n", "vwx\n", "y\n", "z" }, lines.Debug_GetLinesText());
			
			lines.SetText("abc\ndefjh\nij\nklmno\np\nq\nr\ns\nt\nu\nvwx\ny\nz");
			CollectionAssert.AreEqual(
				new string[] {"abc\n", "defjh\n", "ij\n", "klmno\n", "p\n", "q\n", "r\n", "s\n", "t\n", "u\n", "vwx\n", "y\n", "z" },
				lines.Debug_GetLinesText());
			
			//0 abcN
			//1 de[fjhN
			//2 ijN
			//3 klmnoN
			//4 pN
			//5 qN
			//6 rN
			//7 sN
			//8 tN]
			//9 uN
			//10 vwxN
			//11 yN
			//12 z
			
			lines.RemoveText(6, 23);
			Assert.AreEqual(new string[] {"abc\n", "deu\n", "vwx\n", "y\n", "z" }, lines.Debug_GetLinesText());
		}
		
		[Test]
		public void InsertText_Simple()
		{
			Init();
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			
			// line0N
			// line1 textRN
			// line3
			
			lines.InsertText(4, "ABCDEFGH");
			CollectionAssert.AreEqual(new string[] { "lineABCDEFGH0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			Assert.AreEqual(31, lines.charsCount);
		}
		
		[Test]
		public void InsertText_Multiline()
		{
			Init();
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			
			// line0N
			// li[]ne1 textRN
			// line3
			
			lines.InsertText(8, "ABCD\r\nEF\nGHIJK\rLMNOPQR");
			Assert.AreEqual("line0\n; liABCD\r\n; EF\n; GHIJK\r; LMNOPQRne1 text\r\n; line3", string.Join("; ", lines.Debug_GetLinesText()));
			Assert.AreEqual(45, lines.charsCount);
		}
		
		[Test]
		public void InsertText_SeveralBlock()
		{
			Init();
			
			lines.SetText("abc\ndefjh\nij\nklmno\np\nq\nr\ns\nt\nu\nvwx\ny\nz");
			CollectionAssert.AreEqual(
				new string[] {"abc\n", "defjh\n", "ij\n", "klmno\n", "p\n", "q\n", "r\n", "s\n", "t\n", "u\n", "vwx\n", "y\n", "z" },
				lines.Debug_GetLinesText());
			
			lines.InsertText(33, "AB\nCD\r\nEFG\rH\rI\nJKLMNO\nP\nQ");
			CollectionAssert.AreEqual(
				//             012 3    45678 9    01 2    34567 8    9 0    1 2    3 4    5 6    7 8    9 0    12
				new string[] {"abc\n", "defjh\n", "ij\n", "klmno\n", "p\n", "q\n", "r\n", "s\n", "t\n", "u\n", "vwAB\n", "CD\r\n",
					"EFG\r", "H\r", "I\n", "JKLMNO\n", "P\n", "Qx\n", "y\n", "z" },
				lines.Debug_GetLinesText());
			Assert.AreEqual(
				"abc\ndefjh\nij\nklmno\np\nq\nr\ns\nt\nu\nvwx\ny\nz".Length + "AB\nCD\r\nEFG\rH\rI\nJKLMNO\nP\nQ".Length,
				lines.charsCount);
		}
		
		[Test]
		public void InsertAndRemoveText_ToEmpty0()
		{
			Init();
			lines.SetText("");
			CollectionAssert.AreEqual(new string[] { "" }, lines.Debug_GetLinesText());
			lines.InsertText(0, "text");
			CollectionAssert.AreEqual(new string[] { "text" }, lines.Debug_GetLinesText());
			
			lines.RemoveText(0, 4);
			CollectionAssert.AreEqual(new string[] { "" }, lines.Debug_GetLinesText());
			lines.InsertText(0, "text");
			CollectionAssert.AreEqual(new string[] { "text" }, lines.Debug_GetLinesText());
		}
		
		[Test]
		public void InsertAndRemoveText_ToEmpty1()
		{
			Init();
			lines.SetText("");
			CollectionAssert.AreEqual(new string[] { "" }, lines.Debug_GetLinesText());
			lines.InsertText(0, "line0\nline1");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1" }, lines.Debug_GetLinesText());
			
			lines.RemoveText(0, 11);
			CollectionAssert.AreEqual(new string[] { "" }, lines.Debug_GetLinesText());
			lines.InsertText(0, "line0\nline1");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1" }, lines.Debug_GetLinesText());
		}
		
		[Test]
		public void InsertText_LineEndChanging()
		{
			Init();
			
			lines.SetText("line0\nline1 text\r\nline3");
			CollectionAssert.AreEqual(new string[] { "line0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			
			// line0N
			// line1 textRN
			// line3
			
			lines.InsertText(4, "ABCDEFGH\n");
			CollectionAssert.AreEqual(new string[] { "lineABCDEFGH\n", "0\n", "line1 text\r\n", "line3" }, lines.Debug_GetLinesText());
			Assert.AreEqual(32, lines.charsCount);
		}
		
		[Test]
		public void SeveralChanges0_Check()
		{
			Init();
			
			lines.SetText("line0\nline1\nline2\nline3\nliAAAe6\r\nline7\nline8\nline9");
			CollectionAssert.AreEqual(
				new string[] { "line0\n", "line1\n", "line2\n", "line3\n", "liAAAe6\r\n", "line7\n", "line8\n", "line9" },
				lines.Debug_GetLinesText());
			Assert.AreEqual(50, lines.charsCount);
			
			lines.InsertText(42, "BBB\r\nC");
			CollectionAssert.AreEqual(
				new string[] { "line0\n", "line1\n", "line2\n", "line3\n", "liAAAe6\r\n", "line7\n", "linBBB\r\n", "Ce8\n", "line9" },
				lines.Debug_GetLinesText());
			Assert.AreEqual(56, lines.charsCount);
		}
		
		[Test]
		public void SeveralChanges0()
		{
			Init();
			//                                           [                 ]
			//             01234 567890 123456 789012 345678 9 012345 6 789012 3 456789 012345 678901
			lines.SetText("line0\nline1\nline2\nline3\nline4\r\nline5\r\nline6\r\nline7\nline8\nline9");
			CollectionAssert.AreEqual(
				new string[] { "line0\n", "line1\n", "line2\n", "line3\n", "line4\r\n", "line5\r\n", "line6\r\n", "line7\n", "line8\n", "line9" },
				lines.Debug_GetLinesText());
			
			lines.RemoveText(26, 15);
			lines.InsertText(26, "AAA");
			CollectionAssert.AreEqual(
				//                                                                                      []
				//              01234 5    67890 1    23456 7    89012 3    4567890 1 2    34567 8    90123 4    56789
				new string[] { "line0\n", "line1\n", "line2\n", "line3\n", "liAAAe6\r\n", "line7\n", "line8\n", "line9" },
				lines.Debug_GetLinesText());
			Assert.AreEqual(50, lines.charsCount);
			
			lines.InsertText(42, "BBB\r\nC");
			CollectionAssert.AreEqual(
				//              01234 5    67890 1    23456 7    89012 3    4567890 1 2    34567 8    901234 5 6    789 0    12345
				new string[] { "line0\n", "line1\n", "line2\n", "line3\n", "liAAAe6\r\n", "line7\n", "linBBB\r\n", "Ce8\n", "line9" },
				lines.Debug_GetLinesText());
			Assert.AreEqual(56, lines.charsCount);
		}
		
		[Test]
		public void SeveralChanges1()
		{
			Init();
			
			lines.SetText("line0\nline1\nline2\nline3\nline4\r\nline5\r\nline6\r\nline7\nline8\nline9");
			CollectionAssert.AreEqual(
				new string[] { "line0\n", "line1\n", "line2\n", "line3\n", "line4\r\n", "line5\r\n", "line6\r\n", "line7\n", "line8\n", "line9" },
				lines.Debug_GetLinesText());
			
			lines.RemoveText(26, 15);
			lines.InsertText(26, "AAA");
			lines.RemoveText(8, 2);
			CollectionAssert.AreEqual(
				new string[] { "line0\n", "li1\n", "line2\n", "line3\n", "liAAAe6\r\n", "line7\n", "line8\n", "line9" },
				lines.Debug_GetLinesText());
			Assert.AreEqual(48, lines.charsCount);
			
			lines.InsertText(39, "CCCCC\r\nJJJJ");
			CollectionAssert.AreEqual(
				new string[] { "line0\n", "li1\n", "line2\n", "line3\n", "liAAAe6\r\n", "line7\n", "liCCCCC\r\n", "JJJJne8\n", "line9" },
				lines.Debug_GetLinesText());
			Assert.AreEqual(59, lines.charsCount);
		}
		
		private void AssertText(string expectedText)
		{
			Assert.AreEqual(expectedText, lines.GetText());
			Assert.AreEqual(expectedText.Length, lines.charsCount);
		}
		
		[Test]
		public void LastLineRemovingAtSecondBlock()
		{
			Init();

			//             0          1           2            3           4            5          6
			//             01234 567890 123456 789012 345678 9 012345 6 789012 3 456789 012345 678901
			lines.SetText("line0\nline1\nline2\nline3\nline4\r\nline5\r\nline6\r\nline7\nline8\nline9");
			
			lines.InsertText(62, "\n");
			AssertText("line0\nline1\nline2\nline3\nline4\r\nline5\r\nline6\r\nline7\nline8\nline9\n");
			
			lines.RemoveText(62, 1);
			AssertText("line0\nline1\nline2\nline3\nline4\r\nline5\r\nline6\r\nline7\nline8\nline9");
		}
		
		[Test]
		public void LastLineCharRemovingAtSecondBlock()
		{
			Init();

			//             0              1     
			//             0 12 34 56 78 901
			lines.SetText("1\n2\n3\n4\n5\n60");
			
			Assert.AreEqual(new Place(1, 5), lines.PlaceOf(11));
			
			lines.RemoveText(11, 1);
			AssertText("1\n2\n3\n4\n5\n6");
		}
		
		[Test]
		public void LastSeveralLinesRemovingAtSecondBlock()
		{
			Init();

			//             0              1     
			//             0 12 34 56 78 90 12 34
			lines.SetText("1\n2\n3\n4\n5\n6\n7\n8");
			
			lines.RemoveText(9, 6);
			AssertText("1\n2\n3\n4\n5");
		}
		
		[Test]
		public void OutOfRangeChecking_InsertText()
		{
			Init();
			lines.SetText("123");
			lines.InsertText(0, "a");
			AssertText("a123");
			
			Init();
			lines.SetText("123");
			lines.InsertText(3, "end");
			AssertText("123end");
			
			Init();
			lines.SetText("123");
			AssertIndexOutOfRangeException("text index=-1, count=1 is out of [0, 3]", delegate { lines.InsertText(-1, "a"); });
			AssertIndexOutOfRangeException("text index=4, count=3 is out of [0, 3]", delegate { lines.InsertText(4, "end"); });
			AssertText("123");
		}
		
		[Test]
		public void OutOfRangeChecking_RemoveText()
		{
			Init();
			lines.SetText("123");
			lines.RemoveText(0, 1);
			AssertText("23");
			
			Init();
			lines.SetText("123");
			lines.RemoveText(1, 2);
			AssertText("1");
			
			Init();
			lines.SetText("123");
			lines.RemoveText(3, 0);
			AssertText("123");
			
			Init();
			lines.SetText("123");
			AssertIndexOutOfRangeException("text index=-1, count=1 is out of [0, 3]", delegate { lines.RemoveText(-1, 1); });
			AssertIndexOutOfRangeException("text index=1, count=3 is out of [0, 3]", delegate { lines.RemoveText(1, 3); });
			AssertIndexOutOfRangeException("text index=3, count=1 is out of [0, 3]", delegate { lines.RemoveText(3, 1); });
			AssertText("123");
		}
		
		[Test]
		public void OutOfRangeChecking_GetText()
		{
			Init();
			lines.SetText("123");
			
			Assert.AreEqual("1", lines.GetText(0, 1));
			Assert.AreEqual("23", lines.GetText(1, 2));
			Assert.AreEqual("", lines.GetText(3, 0));
			
			AssertIndexOutOfRangeException("text index=-1, count=1 is out of [0, 3]", delegate { lines.GetText(-1, 1); });
			AssertIndexOutOfRangeException("text index=1, count=3 is out of [0, 3]", delegate { lines.GetText(1, 3); });
			AssertIndexOutOfRangeException("text index=3, count=1 is out of [0, 3]", delegate { lines.GetText(3, 1); });
		}
		
		[Test]
		public void OutOfRangeChecking_PlaceOf()
		{
			Init();
			lines.SetText("123");
			
			Assert.AreEqual(new Place(0, 0), lines.PlaceOf(0));
			Assert.AreEqual(new Place(3, 0), lines.PlaceOf(3));
			
			AssertIndexOutOfRangeException("text index=-1 is out of [0, 3]", delegate { lines.PlaceOf(-1); });
			AssertIndexOutOfRangeException("text index=4 is out of [0, 3]", delegate { lines.PlaceOf(4); });
		}
		
		[Test]
		public void SwapLinesWithPrev_SingleBlock()
		{
			Init();
			lines.SetText("line0\nline1\nline2\nline3");
			
			LineIterator iterator = lines.GetLineRange(0, lines.LinesCount);
			iterator.MoveNext();
			iterator.MoveNext();
			Assert.AreEqual("line1\n", iterator.current.Text);
			
			iterator.SwapCurrent(true);
			AssertText("line1\nline0\nline2\nline3");
			
			iterator.MoveNext();
			Assert.AreEqual("line2\n", iterator.current.Text);
			iterator.SwapCurrent(true);
			AssertText("line1\nline2\nline0\nline3");
		}
		
		[Test]
		public void SwapWithPrev_LastNR()
		{
			Init();
			lines.SetText("line0\nline1\nline2\nline3");
			
			LineIterator iterator = lines.GetLineRange(0, lines.LinesCount);
			iterator.MoveNext();
			iterator.MoveNext();
			iterator.MoveNext();
			iterator.MoveNext();
			Assert.AreEqual("line3", iterator.current.Text);
			iterator.SwapCurrent(true);
			AssertText("line0\nline1\nline3\nline2");
		}
		
		[Test]
		public void SwapLinesWithNext_SingleBlock()
		{
			Init();
			lines.SetText("line0\nline1\nline2\nline3");
			
			LineIterator iterator = lines.GetLineRange(0, lines.LinesCount);
			iterator.MoveNext();
			iterator.MoveNext();
			Assert.AreEqual("line1\n", iterator.current.Text);
			
			iterator.SwapCurrent(false);
			AssertText("line0\nline2\nline1\nline3");
		}
		
		[Test]
		public void SwapWithNext_LastNR()
		{
			Init();
			lines.SetText("line0\nline1\nline2\nline3");
			
			LineIterator iterator = lines.GetLineRange(0, lines.LinesCount);
			iterator.MoveNext();
			iterator.MoveNext();
			iterator.MoveNext();
			Assert.AreEqual("line2\n", iterator.current.Text);
			iterator.SwapCurrent(false);
			AssertText("line0\nline1\nline3\nline2");
		}
		
		[Test]
		public void StyleRangeTest()
		{
			Init();
			lines.SetText(
				/*  0*/"Eiszeit\n" +
			    /*  8*/"und wir sind verloren im Meer\n" +
			    /* 38*/"Eiszeit\n" +
			    /* 46*/"und das Atmen faellt so schwer\n" +
			    /* 77*/"Oh, Eiszeit\n" +
			    /* 89*/"mit dir werd ich untergehen\n" +
			    /*117*/"Eiszeit\n" +
			    /*125*/"und nie wieder auferstehen");
			
			lines.SetStyleRange(new StyleRange(3, 4, 1));
			AssertHighlighting("00011110", lines[0]);
			lines.SetStyleRange(new StyleRange(12, 3, 1));
			//                  und wir sind verloren im Meer\n
			AssertHighlighting("000011100000000000000000000000", lines[1]);			
			lines.SetStyleRange(new StyleRange(89, 27, 2));
			//                  mit dir werd ich untergehen\n
			AssertHighlighting("2222222222222222222222222220", lines[5]);
			
			lines.SetStyleRange(new StyleRange(41, 8, 2));
			//                  Eiszeit\n
			AssertHighlighting("00022222", lines[2]);
			//                  und das Atmen faellt so schwer\n
			AssertHighlighting("2220000000000000000000000000000", lines[3]);
			
			lines.SetStyleRange(new StyleRange(41, 38, 3));
			//                  Eiszeit\n
			AssertHighlighting("00033333", lines[2]);
			//                  und das Atmen faellt so schwer\n
			AssertHighlighting("3333333333333333333333333333333", lines[3]);
			//                  Oh, Eiszeit\n
			AssertHighlighting("330000000000", lines[4]);
		}
		
		private void AssertHighlighting(string expected, Line line)
		{
			StringBuilder got = new StringBuilder();
			for (int i = 0; i < line.charsCount; i++)
			{
				got.Append(line.chars[i].style + "");
			}
			Assert.AreEqual(expected, got.ToString(), "\"" + line.Text + "\"");
		}

		[Test]
		public void IntersectsWithSelections_OneSelection()
		{
			Init();

			lines.selections[0].anchor = 1;
			lines.selections[0].caret = 1;
			Assert.AreEqual(false, lines.IntersectSelections(2, 3));
			Assert.AreEqual(false, lines.IntersectSelections(0, 0));
			Assert.AreEqual(true, lines.IntersectSelections(0, 1));
			Assert.AreEqual(true, lines.IntersectSelections(1, 2));

			lines.selections[0].anchor = 5;
			lines.selections[0].caret = 7;
			Assert.AreEqual(false, lines.IntersectSelections(2, 5));
			Assert.AreEqual(false, lines.IntersectSelections(7, 8));
			Assert.AreEqual(true, lines.IntersectSelections(5, 5));
			Assert.AreEqual(true, lines.IntersectSelections(4, 6));
			Assert.AreEqual(true, lines.IntersectSelections(6, 7));
			Assert.AreEqual(true, lines.IntersectSelections(6, 8));
		}
		
		[Test]
		public void IntersectsWithSelections_SeveralSelections()
		{
			Init();

			lines.selections[0].anchor = 1;
			lines.selections[0].caret = 2;
			lines.selections.Add(new Selection());
			lines.selections[1].anchor = 4;
			lines.selections[1].caret = 8;

			Assert.AreEqual(false, lines.IntersectSelections(0, 0));
			Assert.AreEqual(false, lines.IntersectSelections(3, 3));
			Assert.AreEqual(false, lines.IntersectSelections(9, 9));
			Assert.AreEqual(false, lines.IntersectSelections(8, 10));
			Assert.AreEqual(false, lines.IntersectSelections(0, 1));
			Assert.AreEqual(false, lines.IntersectSelections(2, 4));
			Assert.AreEqual(true, lines.IntersectSelections(1, 1));
			Assert.AreEqual(true, lines.IntersectSelections(2, 2));
			Assert.AreEqual(true, lines.IntersectSelections(4, 4));
			Assert.AreEqual(true, lines.IntersectSelections(8, 8));
			Assert.AreEqual(true, lines.IntersectSelections(0, 2));
			Assert.AreEqual(true, lines.IntersectSelections(1, 2));
			Assert.AreEqual(true, lines.IntersectSelections(3, 5));
			Assert.AreEqual(true, lines.IntersectSelections(5, 6));
			Assert.AreEqual(true, lines.IntersectSelections(7, 11));
		}
		
		[Test]
		public void RemoveFromRN()
		{
			Init();
			
			lines.SetText("aaa\r\nbb");
			lines.RemoveText(4, 1);
			AssertText("aaa\rbb");
			AssertLines("aaa\r", "bb");
			
			lines.SetText("aaa\r\nbb");
			lines.RemoveText(3, 1);
			AssertText("aaa\nbb");
			AssertLines("aaa\n", "bb");
			
			lines.SetText("aaa\r\nbb");
			lines.RemoveText(3, 2);
			AssertText("aaabb");
			AssertLines("aaabb");
		}
		
		[Test]
		public void RemoveFromRN_Joining1()
		{
			Init();
			
			lines.SetText("aaa\r\nbb\ncc");
			AssertLines("aaa\r\n", "bb\n", "cc");
			lines.RemoveText(4, 3);
			AssertText("aaa\r\ncc");
			AssertLines("aaa\r\n", "cc");
		}
		
		[Test]
		public void RemoveFromRN_Joining2()
		{
			Init();
			
			lines.SetText("aaa\rbb\ncc");
			AssertLines("aaa\r", "bb\n", "cc");
			lines.RemoveText(4, 2);
			AssertText("aaa\r\ncc");
			AssertLines("aaa\r\n", "cc");
			
			lines.SetText("aaa\rbb\r\ncc");
			AssertLines("aaa\r", "bb\r\n", "cc");
			lines.RemoveText(4, 3);
			AssertText("aaa\r\ncc");
			AssertLines("aaa\r\n", "cc");
		}
		
		[Test]
		public void RemoveFromRN_Joining3()
		{
			Init();
			
			lines.SetText("aaa\r\nbb\ncc");
			AssertLines("aaa\r\n", "bb\n", "cc");
			lines.RemoveText(5, 2);
			AssertText("aaa\r\n\ncc");
			AssertLines("aaa\r\n", "\n", "cc");
		}
		
		[Test]
		public void RemoveFromRN_Joining4()
		{
			Init();
			
			lines.SetText("aaa\r\nbb\r\ncc");
			AssertLines("aaa\r\n", "bb\r\n", "cc");
			lines.RemoveText(5, 3);
			AssertText("aaa\r\n\ncc");
			AssertLines("aaa\r\n", "\n", "cc");
		}
		
		[Test]
		public void RemoveFromRN_Joining_Multiline()
		{
			Init();
			//             012 345 6 789 0 123 456
			lines.SetText("aaa\rbb\r\ndd\r\nee\ncc");
			AssertLines("aaa\r", "bb\r\n", "dd\r\n", "ee\n", "cc");
			lines.RemoveText(4, 10);
			AssertText("aaa\r\ncc");
			AssertLines("aaa\r\n", "cc");
		}
		
		[Test]
		public void InsertInsideRN_R()
		{
			Init();
			
			lines.SetText("aaa\ncc");
			AssertLines("aaa\n", "cc");
			lines.InsertText(3, "\r");
			AssertText("aaa\r\ncc");
			AssertLines("aaa\r\n", "cc");
		}
		
		[Test]
		public void InsertInsideRN_R2()
		{
			Init();
			
			lines.SetText("aaa\ncc");
			AssertLines("aaa\n", "cc");
			lines.InsertText(3, "BB\r");
			AssertText("aaaBB\r\ncc");
			AssertLines("aaaBB\r\n", "cc");
		}
		
		[Test]
		public void InsertInsideRN_N()
		{
			Init();
			
			lines.SetText("aaa\rcc");
			AssertLines("aaa\r", "cc");
			lines.InsertText(4, "\n");
			AssertText("aaa\r\ncc");
			AssertLines("aaa\r\n", "cc");
		}
		
		public void RemoveWholeLine()
		{
			Init();
			
			lines.SetText("aaa\rbb\ncc");
			AssertLines("aaa\r", "bb\n", "cc");
			lines.RemoveText(4, 3);
			AssertText("aaa\rcc");
			AssertLines("aaa\r", "cc");
			
			lines.SetText("aaa\rbb\ncc");
			AssertLines("aaa\r", "bb\n", "cc");
			lines.RemoveText(4, 9);
			AssertText("aaa\r");
			AssertLines("aaa\r", "");
		}
		
		[Test]
		public void InsertInsideRN_Singleline()
		{
			Init();
			lines.SetText("aaa\r\nbb");
			lines.InsertText(4, "C");
			AssertText("aaa\rC\nbb");
			AssertLines("aaa\r", "C\n", "bb");
		}
		
		[Test]
		public void InsertInsideRN_Multiline()
		{
			Init();
			lines.SetText("aaa\r\nbb");
			lines.InsertText(4, "C\nB");
			AssertText("aaa\rC\nB\nbb");
			AssertLines("aaa\r", "C\n", "B\n", "bb");
		}
		
		[Test]
		public void RemoveFromRN_Multiblocks1()
		{
			Init();
			lines.SetText(
				"abcd\r\n" +
				"efg\r\n" +
				"hi\r\n" +
				"jklmnop\r\n" +
				
				"qrst\r\n" +
				"uvw\r\n" +
				"x\r\n" +
				"w\r\n" +
				
				"z\r\n" +
				"12\r\n" +
				"34\r\n" +
				"5678\r\n" +
				
				"9\r\n" +
				"abcde\r\n" +
				"fghijklmno");
			
			lines.RemoveText(14, 53);
			AssertText("abcd\r\nefg\r\nhi\r\nfghijklmno");
			AssertLines("abcd\r\n", "efg\r\n", "hi\r\n", "fghijklmno");
		}
		
		[Test]
		public void RemoveFromRN_Multiblocks2()
		{
			Init();
			lines.SetText(
				"abcd\r\n" +
				"efg\r\n" +
				"hi\r\n" +
				"jklmnop\n" +
			//  [
				"qrst\r\n" +
				"uvw\r\n" +
				"x\r\n" +
				"w\r\n" +
				
				"z\r\n" +
				"12\r\n" +
				"34\r\n" +
				"5678\r\n" +
			//      ]
				"9\r\n" +
				"abcde\r\n" +
				"fghijklmno");
			
			lines.RemoveText(23, 36);
			AssertText("abcd\r\nefg\r\nhi\r\njklmnop\n\nabcde\r\nfghijklmno");
			AssertLines("abcd\r\n", "efg\r\n", "hi\r\n", "jklmnop\n", "\n", "abcde\r\n", "fghijklmno");
		}
		
		[Test]
		public void RemoveFromRN_Multiblocks3()
		{
			Init();
			lines.SetText(
				"abc\r\n" +
				"def\r\n" +
				"ghi\r\n" +
				"jkl\r\n" +
				
				"mno\r\n" +
				"pqr\r\n" +
				"stu\r\n" +
				"vwx\r\n" +
				
				"yza\r\n" +
				"bcd\r\n" +
				"efg\r\n" +
				"hij\r\n" +
				
				"klm\r\n" +
				"nop\r\n" +
				"qrstu");
			
			lines.RemoveText(20, 4);
			AssertText(
				"abc\r\ndef\r\nghi\r\njkl\r\n\npqr\r\nstu\r\nvwx\r\nyza\r\nbcd\r\nefg\r\nhij\r\nklm\r\nnop\r\nqrstu");
			AssertLines(
				"abc\r\n", "def\r\n", "ghi\r\n", "jkl\r\n", "\n", "pqr\r\n", "stu\r\n", "vwx\r\n", "yza\r\n",
				"bcd\r\n", "efg\r\n", "hij\r\n", "klm\r\n", "nop\r\n", "qrstu");
		}
	}
}
