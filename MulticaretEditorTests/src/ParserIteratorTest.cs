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
			Assert.AreEqual(moved, iterator.MoveRight(), "Move right from " + iterator.Position + "/" + iterator.Place);
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
			NewIterator(0).AssertRight('\0').AssertPlace(0, 0).MoveRight(false).AssertRight('\0').AssertPlace(0, 0);
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
	}
}