using System;
using System.Collections.Generic;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class ParserIteratorTest : ControllerTestBase
	{
		private ParserIterator iterator;
		
		[SetUp]
		public void SetUp()
		{
			iterator = null;
		}
		
		private ParserIteratorTest NewIterator(int position)
		{
			iterator = lines.GetParserIterator(position);
			return this;
		}
		
		private ParserIteratorTest MoveRight(bool moved)
		{
			Assert.AreEqual(!moved, iterator.IsEnd,
				"IsEnd before move at " + iterator.Position + "/" + iterator.Place);
			iterator.MoveRight();
			return this;
		}
		
		private ParserIteratorTest AssertEnd(bool isEnd)
		{
			Assert.AreEqual(isEnd, iterator.IsEnd, "IsEnd at " + iterator.Position + "/" + iterator.Place);
			return this;
		}
		
		private ParserIteratorTest AssertPlace(int iChar, int iLine)
		{
			Place place = iterator.Place;
			Assert.AreEqual("(" + iChar + ", " + iLine + ")", "(" + place.iChar + ", " + place.iLine + ")");
			return this;
		}
		
		private ParserIteratorTest AssertRight(char c)
		{
			if (c != iterator.RightChar)
			{
				Assert.Fail(
					"Expected: " +
					("'" + c + "'").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\0", "\\0") +
					", was: " +
					("'" + iterator.RightChar + "'").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\0", "\\0") +
					", position: " + iterator.Position + ", place: " + iterator.Place);
			}
			return this;
		}
		
		[Test]
		public void EmptyLine()
		{
			Init();
			lines.SetText("");
			NewIterator(0).AssertRight('\0').AssertPlace(0, 0).MoveRight(false).AssertEnd(true);
		}
		
		[Test]
		public void SingleLine()
		{
			Init();
			lines.SetText("12345678");
			NewIterator(0).AssertRight('1').AssertPlace(0, 0).MoveRight(true).AssertRight('2').AssertPlace(1, 0);
			NewIterator(6).AssertRight('7').AssertPlace(6, 0).MoveRight(true).AssertRight('8').AssertPlace(7, 0);
		}
		
		[Test]
		public void SingleLine_End()
		{
			Init();
			lines.SetText("12345678");
			NewIterator(6)
				.AssertRight('7').AssertPlace(6, 0).MoveRight(true)
				.AssertRight('8').AssertPlace(7, 0).MoveRight(true)
				.AssertRight('\0').AssertPlace(8, 0).MoveRight(false).AssertRight('\0').AssertPlace(8, 0);
		}
		
		[Test]
		public void Multiline()
		{
			Init();
			lines.SetText("1234\r\nabcd\nEFG");
			NewIterator(3)
				.AssertRight('4').MoveRight(true)
				.AssertRight('\r').MoveRight(true)
				.AssertRight('\n').MoveRight(true)
				.AssertRight('a').MoveRight(true)
				.AssertRight('b').MoveRight(true)
				.AssertRight('c').MoveRight(true)
				.AssertRight('d').MoveRight(true)
				.AssertRight('\n').MoveRight(true)
				.AssertRight('E').MoveRight(true)
				.AssertRight('F').MoveRight(true)
				.AssertRight('G').MoveRight(true)
				.AssertRight('\0').MoveRight(false).AssertRight('\0');
		}
		
		[Test]
		public void Multiline_Multiparts()
		{
			Init(3);
			lines.SetText("1234\r\nabcd\nEFG\n" + "hijk\nl\nm\n" + "\nnop\rQR\n" + "uvw\nxwz");
			NewIterator(lines.IndexOf(new Place(3, 1)))
				.AssertRight('d').AssertPlace(3, 1).MoveRight(true)
				.AssertRight('\n').MoveRight(true)
				.AssertRight('E').MoveRight(true)
				.AssertRight('F').MoveRight(true)
				.AssertRight('G').MoveRight(true)
				.AssertRight('\n').MoveRight(true)
				.AssertRight('h').MoveRight(true)
				.AssertRight('i').MoveRight(true)
				.AssertRight('j').MoveRight(true)
				.AssertRight('k').MoveRight(true)
				.AssertRight('\n').MoveRight(true)
				.AssertRight('l').MoveRight(true)
				.AssertRight('\n').MoveRight(true)
				.AssertRight('m').MoveRight(true)
				.AssertRight('\n').MoveRight(true)
				.AssertRight('\n').MoveRight(true)
				.AssertRight('n').MoveRight(true)
				.AssertRight('o').MoveRight(true)
				.AssertRight('p').AssertPlace(2, 7).MoveRight(true);
			NewIterator(lines.IndexOf(new Place(1, 10)))
				.AssertRight('w').AssertPlace(1, 10).MoveRight(true)
				.AssertRight('z').AssertPlace(2, 10).MoveRight(true)
				.AssertRight('\0').AssertPlace(3, 10).MoveRight(false).AssertPlace(3, 10);
		}
		
		[Test]
		public void IsRightOnLine()
		{
			Init(3);
			lines.SetText("1234\r\nabcd\nEFG\n" + "hijk\nl\nm\n" + "\nnop\rQR\n" + "uvw\nxwz");
			
			NewIterator(lines.IndexOf(new Place(3, 1)));
			Assert.AreEqual(false, iterator.IsRightOnLine("a"));
			Assert.AreEqual(true, iterator.IsRightOnLine("d"));
			Assert.AreEqual(false, iterator.IsRightOnLine("x"));
			
			NewIterator(lines.IndexOf(new Place(1, 1)));
			Assert.AreEqual(true, iterator.IsRightOnLine("bc"));
			Assert.AreEqual(false, iterator.IsRightOnLine("by"));
			
			NewIterator(lines.IndexOf(new Place(1, 1)));
			Assert.AreEqual(true, iterator.IsRightOnLine("bcd\n"));
			Assert.AreEqual(false, iterator.IsRightOnLine("bcd\r"));
			
			NewIterator(lines.IndexOf(new Place(1, 7)));
			Assert.AreEqual(true, iterator.IsRightOnLine("op"),
				"at " + iterator.Place + ", right: '" + iterator.RightChar + "'");
			Assert.AreEqual(false, iterator.IsRightOnLine("xp"));
		}
		
		[Test]
		public void IsRightOnLine_Empty()
		{
			Init(3);
			lines.SetText("1234\r\nabcd\nEFG\n" + "hijk\nl\nm\n" + "\nnop\rQR\n" + "uvw\nxwz");
			NewIterator(lines.IndexOf(new Place(3, 1)));
			Assert.AreEqual(true, iterator.IsRightOnLine(""));
		}
		
		[Test]
		public void IsRightWord()
		{
			Init(3);
			lines.SetText("1234\r\nabcd\nEFG\n" + "hijk\nitem1 item2 item3\nm");
			
			NewIterator(lines.IndexOf(new Place(6, 4)));
			Assert.AreEqual(true, iterator.IsRightWord("item2"));
			
			NewIterator(lines.IndexOf(new Place(7, 4)));
			Assert.AreEqual(false, iterator.IsRightWord("tem2"));
			
			NewIterator(lines.IndexOf(new Place(5, 4)));
			Assert.AreEqual(false, iterator.IsRightWord(" item2"));
			
			NewIterator(lines.IndexOf(new Place(0, 4)));
			Assert.AreEqual(true, iterator.IsRightWord("item1"));
			Assert.AreEqual(false, iterator.IsRightWord("item"));
			
			NewIterator(lines.IndexOf(new Place(12, 4)));
			Assert.AreEqual(true, iterator.IsRightWord("item3"));
			Assert.AreEqual(false, iterator.IsRightWord("item"));
		}
		
		[Test]
		public void IsRightWord_Punctuation()
		{
			Init(3);
			lines.SetText("1234\r\nabcd\nEFG\n" + "item1;item2.item3\nm");
			
			NewIterator(lines.IndexOf(new Place(6, 3)));
			Assert.AreEqual(true, iterator.IsRightWord("item2"));
			Assert.AreEqual(false, iterator.IsRightWord("item"));
			
			NewIterator(lines.IndexOf(new Place(7, 3)));
			Assert.AreEqual(false, iterator.IsRightWord("tem2"));
			Assert.AreEqual(false, iterator.IsRightWord("item2"));
			
			NewIterator(lines.IndexOf(new Place(0, 3)));
			Assert.AreEqual(true, iterator.IsRightWord("item1"));
			
			NewIterator(lines.IndexOf(new Place(1, 3)));
			Assert.AreEqual(false, iterator.IsRightWord("tem1"));
			
			NewIterator(lines.IndexOf(new Place(12, 3)));
			Assert.AreEqual(true, iterator.IsRightWord("item3"));
			Assert.AreEqual(false, iterator.IsRightWord("item"));
			
			NewIterator(lines.IndexOf(new Place(11, 3)));
			Assert.AreEqual(false, iterator.IsRightWord("item3"));
			
			NewIterator(lines.IndexOf(new Place(13, 3)));
			Assert.AreEqual(false, iterator.IsRightWord("tem3"));
		}
		
		[Test]
		public void MoveRightOnLine()
		{
			Init(3);
			lines.SetText("1234\r\nabcd\nEFG\n" + "item1;item2.item3\nmnop");
			NewIterator(lines.IndexOf(new Place(6, 3)));
			AssertRight('i');
			iterator.MoveRightOnLine(1);
			AssertRight('t');
			iterator.MoveRightOnLine(2);
			AssertRight('m');
			iterator.MoveRightOnLine(1);
			AssertRight('2');
			iterator.MoveRightOnLine(7);
			AssertRight('\n');
			iterator.MoveRightOnLine(1);
			AssertRight('m');
		}
		
		[Test]
		public void MoveRightOnLine_Overflow()
		{
			Init(3);
			lines.SetText("1234\r\nabcd\nEFG\n" + "item1;item2.item3\nmnop");
			NewIterator(lines.IndexOf(new Place(12, 3)));
			AssertRight('i');
			iterator.MoveRightOnLine(9);
			
			AssertRight('m');
			iterator.MoveRightOnLine(1);
			AssertRight('n');
			iterator.MoveRightOnLine(1);
			AssertRight('o');
			iterator.MoveRightOnLine(3);
			AssertRight('\0');
			iterator.MoveRightOnLine(1);
			AssertRight('\0');
		}
	}
}