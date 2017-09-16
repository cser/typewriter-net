using System;
using System.Collections.Generic;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class IteratorsTest : ControllerTestBase
	{
		[Test]
		public void MoveLeftRight()
		{
			Init();
			
			lines.SetText("12345678");
			PlaceIterator iterator;
			char c;
			
			iterator = lines.GetCharIterator(0);
			
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('1', c);
			Assert.AreEqual(new Place(1, 0), iterator.Place);
			Assert.True(iterator.MoveLeft(out c));
			Assert.AreEqual('1', c);
			Assert.AreEqual(new Place(0, 0), iterator.Place);
			
			iterator = lines.GetCharIterator(6);
			Assert.True(iterator.MoveLeft(out c));
			Assert.AreEqual('6', c);
			Assert.AreEqual(new Place(5, 0), iterator.Place);
			Assert.True(iterator.MoveLeft(out c));
			Assert.AreEqual('5', c);
			Assert.AreEqual(new Place(4, 0), iterator.Place);
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('5', c);
			Assert.AreEqual(new Place(5, 0), iterator.Place);
			Assert.True(iterator.MoveRight(out c));
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('7', c);
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('8', c);
			Assert.AreEqual(new Place(8, 0), iterator.Place);
			Assert.True(iterator.MoveLeft(out c));
			Assert.AreEqual('8', c);
			Assert.AreEqual(new Place(7, 0), iterator.Place);
			Assert.True(iterator.MoveLeft(out c));
			Assert.AreEqual('7', c);
		}
		
		[Test]
		public void MoveRight()
		{
			Init();
			
			lines.SetText(
				"123\n" +
				"\n" +
				"1\n" +
				"1234"
			);
			PlaceIterator iterator = lines.GetCharIterator(0);
			
			char c;
			
			Assert.True(iterator.MoveRight(out c));
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('2', c);
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('3', c);
			
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('\n', c);
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('\n', c);
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('1', c);
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('\n', c);
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('1', c);
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('2', c);
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('3', c);
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('4', c);
			Assert.False(iterator.MoveRight(out c));
			Assert.AreEqual('\0', c);
			Assert.True(iterator.MoveLeft(out c));
			Assert.AreEqual('4', c);
		}
		
		[Test]
		public void MoveLeft()
		{
			Init();
			
			lines.SetText(
				"123\n" +
				"\n" +
				"1\n" +
				"1234"
			);
			PlaceIterator iterator = lines.GetCharIterator(11);
			
			char c;
			
			Assert.True(iterator.MoveLeft(out c));
			Assert.True(iterator.MoveLeft(out c));
			Assert.True(iterator.MoveLeft(out c));
			Assert.True(iterator.MoveLeft(out c));
			Assert.AreEqual('1', c);
			Assert.True(iterator.MoveLeft(out c));
			Assert.AreEqual('\n', c);
			Assert.True(iterator.MoveLeft(out c));
			Assert.AreEqual('1', c);
			Assert.True(iterator.MoveLeft(out c));
			Assert.AreEqual('\n', c);
			Assert.True(iterator.MoveLeft(out c));
			Assert.AreEqual('\n', c);
			Assert.True(iterator.MoveLeft(out c));
			Assert.AreEqual('3', c);
			Assert.True(iterator.MoveLeft(out c));
			Assert.AreEqual('2', c);
			Assert.True(iterator.MoveLeft(out c));
			Assert.AreEqual('1', c);
			Assert.False(iterator.MoveLeft(out c));
			Assert.AreEqual('\0', c);
		}
		
		[Test]
		public void LineIterator_Simple()
		{
			Init(4);
			lines.SetText(
				"123\n" +
				"\n" +
				"1\n" +
				"1234"
			);
			CollectionAssert.AreEqual(
				new Line[] { lines[0], lines[1], lines[2] },
				GetLines(lines.GetLineRange(0, 3)));

			CollectionAssert.AreEqual(
				new Line[] { lines[1], lines[2], lines[3] },
				GetLines(lines.GetLineRange(1, 3)));
		}
		
		[Test]
		public void LineIterator_SeveralBlocks()
		{
			Init(4);
			lines.SetText("0\n1\n2\n3\n4\n5\n6\n7\n8\n9\n10\n11\n12\n13\n14");
			CollectionAssert.AreEqual(new Line[] { lines[0], lines[1] }, GetLines(lines.GetLineRange(0, 2)));
			CollectionAssert.AreEqual(new Line[] { lines[1], lines[2] }, GetLines(lines.GetLineRange(1, 2)));
			CollectionAssert.AreEqual(new Line[] { lines[11], lines[12], lines[13] }, GetLines(lines.GetLineRange(11, 3)));
			CollectionAssert.AreEqual(new Line[] { lines[12], lines[13], lines[14] }, GetLines(lines.GetLineRange(12, 3)));
			CollectionAssert.AreEqual(
				new Line[]
				{
					lines[0], lines[1], lines[2], lines[3], lines[4], lines[5], lines[6], lines[7],
					lines[8], lines[9], lines[10], lines[11], lines[12], lines[13], lines[14]
				},
				GetLines(lines.GetLineRange(0, 15)));
		}
		
		[Test]
		public void LineIterator_GetNextRange()
		{
			Init(4);
			lines.SetText("0\n1\n2\n3\n4\n5\n6\n7\n8\n9\n10\n11\n12\n13\n14");
			CollectionAssert.AreEqual(new Line[] { lines[0], lines[1] }, GetLines(lines.GetLineRange(0, 2)));
			CollectionAssert.AreEqual(new Line[] { lines[1], lines[2] }, GetLines(lines.GetLineRange(1, 2)));
			CollectionAssert.AreEqual(new Line[] { lines[11], lines[12], lines[13] }, GetLines(lines.GetLineRange(11, 3)));
			CollectionAssert.AreEqual(new Line[] { lines[12], lines[13], lines[14] }, GetLines(lines.GetLineRange(12, 3)));
			LineIterator iterator = lines.GetLineRange(2, 10);
			iterator.MoveNext();
			Assert.AreEqual(lines[2], iterator.current);
			iterator.MoveNext();
			Assert.AreEqual(lines[3], iterator.current);
			CollectionAssert.AreEqual(
				new Line[] { lines[4], lines[5], lines[6], lines[7], lines[8] },
				GetLines(iterator.GetNextRange(5)));
		}
		
		[Test]
		public void LineIterator_FailOnOutOfRange()
		{
			Init(4);
			lines.SetText("0\n1\n2\n3");
			try
			{
				GetLines(lines.GetLineRange(2, 3));
				Assert.Fail("Mast exception throws");
			}
			catch (IndexOutOfRangeException e)
			{
				Assert.AreEqual("index=2, count=3 is out of [0, 4)", e.Message);
			}
			try
			{
				GetLines(lines.GetLineRange(-1, 3));
				Assert.Fail("Mast exception throws");
			}
			catch (IndexOutOfRangeException e)
			{
				Assert.AreEqual("index=-1, count=3 is out of [0, 4)", e.Message);
			}
		}
		
		[Test]
		public void LineIterator_Constraints()
		{
			Init(4);
			lines.SetText("0\n1\n2\n3");
			CollectionAssert.AreEqual(new Line[] { }, GetLines(lines.GetLineRange(1, 0)));
		}
		
		[Test]
		public void LineIterator_Inversed()
		{
			Init(4);
			lines.SetText("0\n1\n2\n3");
			CollectionAssert.AreEqual(new Line[] { lines[1] }, GetLines(lines.GetLineRange(1, -1)));
			CollectionAssert.AreEqual(new Line[] { lines[1], lines[0] }, GetLines(lines.GetLineRange(1, -2)));
		}
		
		private List<Line> GetLines(LineIterator iterator)
		{
			List<Line> lines = new List<Line>();
			while (iterator.MoveNext())
			{
				lines.Add(iterator.current);
			}
			return lines;
		}

		[Test]
		public void MoveRightBug()
		{
			Init();
			
			lines.SetText(
				"123\n" +
				""
			);
			PlaceIterator iterator = lines.GetCharIterator(0);
			
			char c;
			
			Assert.True(iterator.MoveRight(out c));
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('2', c);
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('3', c);
			
			Assert.True(iterator.MoveRight(out c));
			Assert.AreEqual('\n', c);
			Assert.False(iterator.MoveRight(out c));
			Assert.AreEqual('\0', c);
			Assert.True(iterator.MoveLeft(out c));
			Assert.AreEqual('\n', c);
		}
		
		private void AssertLeftRight(PlaceIterator iterator, char left, char right)
		{
			char iteratorLeft = iterator.LeftChar;
			char iteratorRight = iterator.RightChar;
			Assert.True(iteratorLeft == left && iteratorRight == right,
				("expected: (" + left + ", " + right + "), was: (" +
				iteratorLeft + ", " + iteratorRight + ")").Replace("\0", "\\0"));
		}
		
		[Test]
		public void LeftRightChar_Idle()
		{
			Init();
			
			lines.SetText("123456");
			PlaceIterator iterator = lines.GetCharIterator(1);
			AssertLeftRight(iterator, '1', '2');
		}
		
		[Test]
		public void LeftRightChar_AfterMoveRight()
		{
			Init();
			
			lines.SetText("123456");
			PlaceIterator iterator = lines.GetCharIterator(1);
			iterator.MoveRightWithRN();
			AssertLeftRight(iterator, '2', '3');
		}
		
		[Test]
		public void LeftRightChar_AfterMoveLeft()
		{
			Init();
			
			lines.SetText("123456");
			PlaceIterator iterator = lines.GetCharIterator(3);
			iterator.MoveLeftWithRN();
			AssertLeftRight(iterator, '2', '3');
		}
	}
}
