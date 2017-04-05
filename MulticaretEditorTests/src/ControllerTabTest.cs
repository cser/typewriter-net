using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class ControllerTabTest : ControllerTestBase
	{
		private const string SimpleText =
			"Du\n" +
			"Du hast\n" +
			"Du hast mich";
		//  Du
		//  Du hast
		//  Du hast mich
		// 0123456789012
		
		private const string TabbedText =
			"Du\n" +
			"\tDu\thast\n" +
			"Du hast\tmich";
		// 0123456789012
		//  Du
		//  	Du	hast
		//  Du hast	mich
		// 0123456789012
		
		[Test]
		public void Move()
		{
			Init();
			lines.SetText(TabbedText);
			AssertSelection().Both(0, 0);
			
			controller.MoveDown(false);
			AssertSelection().Both(0, 1);
			controller.MoveRight(false);
			AssertSelection().Both(1, 1);
			
			controller.MoveDown(false);
			AssertSelection().Both(4, 2);
			controller.MoveRight(false);
			AssertSelection().Both(5, 2);
			controller.MoveUp(false);
			AssertSelection().Both(2, 1);
		}
		
		[Test]
		public void LineSize()
		{
			Init();
			lines.SetText("\ta");
			Assert.AreEqual(2, lines[0].charsCount);
			Assert.AreEqual(5, lines[0].Size);
			
			Init();
			lines.SetText("\ta\tbc\tdfg\thi");
		//		a	bc	dfg	hi
			Assert.AreEqual(12, lines[0].charsCount);
			Assert.AreEqual(18, lines[0].Size);
			
			Init();
			lines.SetText("abcd\te");
		//	abcd	e
			Assert.AreEqual(6, lines[0].charsCount);
			Assert.AreEqual(9, lines[0].Size);
		}
		
		[Test]
		public void IndexOfPosCharToPosition_Simple()
		{
			Init();
			lines.SetText(SimpleText);
			
			Assert.AreEqual(0, lines[0].IndexOfPos(0));
			Assert.AreEqual(1, lines[0].IndexOfPos(1));
			Assert.AreEqual(2, lines[0].IndexOfPos(2));
			
			Assert.AreEqual(0, lines[1].IndexOfPos(0));
			Assert.AreEqual(7, lines[1].IndexOfPos(7));
			
			Assert.AreEqual(12, lines[2].IndexOfPos(12));
			
			Assert.AreEqual(0, lines[0].PosOfIndex(0));
			Assert.AreEqual(1, lines[0].PosOfIndex(1));
			Assert.AreEqual(2, lines[0].PosOfIndex(2));
			
			Assert.AreEqual(0, lines[1].PosOfIndex(0));
			Assert.AreEqual(7, lines[1].PosOfIndex(7));
			
			Assert.AreEqual(12, lines[2].PosOfIndex(12));
		}
		
		[Test]
		public void IndexOfPos_Tabbed()
		{
			Init();
			lines.SetText(TabbedText);
			
			Assert.AreEqual(0, lines[0].IndexOfPos(0));
			Assert.AreEqual(1, lines[0].IndexOfPos(1));
			Assert.AreEqual(2, lines[0].IndexOfPos(2));
			
			Assert.AreEqual(0, lines[1].IndexOfPos(0));
			Assert.AreEqual(0, lines[1].IndexOfPos(1));
			Assert.AreEqual(0, lines[1].IndexOfPos(2));
			Assert.AreEqual(1, lines[1].IndexOfPos(3));
			Assert.AreEqual(1, lines[1].IndexOfPos(4));
			Assert.AreEqual(2, lines[1].IndexOfPos(5));
			Assert.AreEqual(3, lines[1].IndexOfPos(6));
			Assert.AreEqual(3, lines[1].IndexOfPos(7));
			Assert.AreEqual(4, lines[1].IndexOfPos(8));
			Assert.AreEqual(5, lines[1].IndexOfPos(9));
			Assert.AreEqual(6, lines[1].IndexOfPos(10));
			Assert.AreEqual(7, lines[1].IndexOfPos(11));
			Assert.AreEqual(8, lines[1].IndexOfPos(12));
		}
		
		[Test]
		public void PosOfIndex_Tabbed()
		{
			Init();
			lines.SetText(TabbedText);
			
			Assert.AreEqual(0, lines[0].PosOfIndex(0));
			Assert.AreEqual(1, lines[0].PosOfIndex(1));
			Assert.AreEqual(2, lines[0].PosOfIndex(2));
			
			Assert.AreEqual(0, lines[1].PosOfIndex(0));
			Assert.AreEqual(4, lines[1].PosOfIndex(1));
			Assert.AreEqual(5, lines[1].PosOfIndex(2));
			Assert.AreEqual(6, lines[1].PosOfIndex(3));
			Assert.AreEqual(8, lines[1].PosOfIndex(4));
			Assert.AreEqual(9, lines[1].PosOfIndex(5));
			Assert.AreEqual(10, lines[1].PosOfIndex(6));
			Assert.AreEqual(11, lines[1].PosOfIndex(7));
			Assert.AreEqual(12, lines[1].PosOfIndex(8));
		}
		
		[Test]
		public void NormalPlaceOfPos()
		{
			Init();
			lines.SetText(TabbedText);
			
			// 0123456789012
			//  Du
			//  	Du	hast
			//  Du hast	mich
			// 0123456789012
			
			Assert.AreEqual(new Place(0, 0), lines.Normalize(lines.PlaceOf(new Pos(0, 0))));
			
			Assert.AreEqual(new Place(0, 1), lines.Normalize(lines.PlaceOf(new Pos(0, 1))));
			Assert.AreEqual(new Place(0, 1), lines.Normalize(lines.PlaceOf(new Pos(1, 1))));
			Assert.AreEqual(new Place(0, 1), lines.Normalize(lines.PlaceOf(new Pos(2, 1))));
			Assert.AreEqual(new Place(1, 1), lines.Normalize(lines.PlaceOf(new Pos(3, 1))));
			Assert.AreEqual(new Place(1, 1), lines.Normalize(lines.PlaceOf(new Pos(4, 1))));
			
			Assert.AreEqual(new Place(7, 1), lines.Normalize(lines.PlaceOf(new Pos(11, 1))));
			Assert.AreEqual(new Place(8, 1), lines.Normalize(lines.PlaceOf(new Pos(12, 1))));
			Assert.AreEqual(new Place(8, 1), lines.Normalize(lines.PlaceOf(new Pos(13, 1))));
			
			Assert.AreEqual(new Place(3, 1), lines.Normalize(lines.PlaceOf(new Pos(6, 1))));
			Assert.AreEqual(new Place(3, 1), lines.Normalize(lines.PlaceOf(new Pos(7, 1))));
			Assert.AreEqual(new Place(4, 1), lines.Normalize(lines.PlaceOf(new Pos(8, 1))));
			
			Assert.AreEqual(new Place(11, 2), lines.Normalize(lines.PlaceOf(new Pos(11, 2))));
			Assert.AreEqual(new Place(12, 2), lines.Normalize(lines.PlaceOf(new Pos(12, 2))));
		}
	}
}