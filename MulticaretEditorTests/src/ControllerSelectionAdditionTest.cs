using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class ControllerSelectionAdditionTest : ControllerTestBase
	{
		private const string SimpleText =
			"Du\n" +
			"Du hast\n" +
			"Du hast mich";
		//	0123456789012
		
		[Test]
		public void PutCursorDown()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Both(0, 0).NoNext();
			
			controller.PutCursorDown();
			AssertSelection().Both(0, 0).Next().Both(0, 1).NoNext();
			
			controller.PutCursorDown();
			AssertSelection().Both(0, 0).Next().Both(0, 1).Next().Both(0, 2).NoNext();
		}
		
		[Test]
		public void PutCursorDown_PrefferedPos()
		{
			Init();
			lines.SetText(
				"Du\n" +
				"Du hast\n" +
				"12345\n" +
				"Du hast mich"
			);
			controller.PutCursor(new Pos(7, 1), false);
			AssertSelection().Both(7, 1).NoNext();
			
			controller.PutCursorDown();
			AssertSelection().Both(7, 1).Next().Both(5, 2).NoNext();
			
			controller.PutCursorDown();
			AssertSelection().Both(7, 1).Next().Both(5, 2).Next().Both(7, 3).NoNext();
		}
		
		[Test]
		public void PutCursorUp()
		{
			Init();
			lines.SetText(SimpleText);
			controller.PutCursor(new Pos(0, 2), false);
			AssertSelection().Both(0, 2).NoNext();
			
			controller.PutCursorUp();
			AssertSelection().Both(0, 2).Next().Both(0, 1).NoNext();
			
			controller.PutCursorUp();
			AssertSelection().Both(0, 2).Next().Both(0, 1).Next().Both(0, 0).NoNext();
		}
		
		[Test]
		public void PutCursorUp_PrefferedPos()
		{
			Init();
			lines.SetText(
				"Du\n" +
				"Du hast\n" +
				"12345\n" +
				"Du hast mich"
			);
			controller.PutCursor(new Pos(7, 3), false);
			AssertSelection().Both(7, 3).NoNext();
			
			controller.PutCursorUp();
			AssertSelection().Both(7, 3).Next().Both(5, 2).NoNext();
			
			controller.PutCursorUp();
			AssertSelection().Both(7, 3).Next().Both(5, 2).Next().Both(7, 1).NoNext();
		}
		
		[Test]
		public void RemoveCursorByUpDown()
		{
			Init();
			lines.SetText("0\n1\n2\n3\n4\n5");
			controller.PutCursor(new Pos(0, 3), false);
			AssertSelection().Both(0, 3).NoNext();
			
			controller.PutCursorDown();
			AssertSelection().Both(0, 3).Next().Both(0, 4).NoNext();
			
			controller.PutCursorDown();
			AssertSelection().Both(0, 3).Next().Both(0, 4).Next().Both(0, 5).NoNext();
			
			controller.PutCursorUp();
			AssertSelection().Both(0, 3).Next().Both(0, 4).NoNext();
			
			controller.PutCursorUp();
			AssertSelection().Both(0, 3).NoNext();
			
			controller.PutCursorUp();
			AssertSelection().Both(0, 3).Next().Both(0, 2).NoNext();
			
			controller.PutCursorUp();
			AssertSelection().Both(0, 3).Next().Both(0, 2).Next().Both(0, 1).NoNext();
			
			controller.PutCursorDown();
			AssertSelection().Both(0, 3).Next().Both(0, 2).NoNext();
			
			controller.PutCursorDown();
			AssertSelection().Both(0, 3).NoNext();
		}
		
		[Test]
		public void PutCursorUpDown_Constraints()
		{
			Init();
			lines.SetText("line0\nline1\nline2\nline3\nline4");
			controller.PutCursor(new Place(2, 2), false);
			AssertSelection().Both(2, 2).NoNext();
			
			controller.PutCursorUp();
			AssertSelection().Both(2, 2).Next().Both(2, 1).NoNext();
			controller.PutCursorUp();
			AssertSelection().Both(2, 2).Next().Both(2, 1).Next().Both(2, 0).NoNext();
			
			controller.PutCursorUp();
			AssertSelection().Both(2, 2).Next().Both(2, 1).Next().Both(2, 0).NoNext();
			
			controller.PutCursorDown();
			AssertSelection().Both(2, 2).Next().Both(2, 1).NoNext();			
			controller.PutCursorDown();
			AssertSelection().Both(2, 2).NoNext();
			controller.PutCursorDown();
			AssertSelection().Both(2, 2).Next().Both(2, 3).NoNext();
			controller.PutCursorDown();
			AssertSelection().Both(2, 2).Next().Both(2, 3).Next().Both(2, 4).NoNext();
			
			controller.PutCursorDown();
			AssertSelection().Both(2, 2).Next().Both(2, 3).Next().Both(2, 4).NoNext();
			
			controller.PutCursorUp();
			AssertSelection().Both(2, 2).Next().Both(2, 3).NoNext();
		}
	}
}