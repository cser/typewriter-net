using System;
using MulticaretEditor;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class ControllerWordSelectionTest : ControllerTestBase
	{
		[Test]
		public void SelectWordAtPos()
		{
			Init();
			lines.SetText(
				"Du\n" +
				"Du hast\n" +
				"Du hast mich");
			controller.SelectWordAtPlace(new Place(1, 0), false);
			AssertSelection().Anchor(0, 0).Caret(2, 0);
			controller.SelectWordAtPlace(new Place(3, 1), false);
			AssertSelection().Anchor(3, 1).Caret(7, 1);
		}
		
		[Test]
		public void SelectWordAtPos_NewSelection()
		{
			Init();
			lines.SetText(
				"Du\n" +
				"Du hast\n" +
				"Du hast mich");
			controller.SelectWordAtPlace(new Place(1, 0), false);
			AssertSelection().Anchor(0, 0).Caret(2, 0).NoNext();
			controller.SelectWordAtPlace(new Place(3, 1), true);
			AssertSelection().Anchor(0, 0).Caret(2, 0).Next().Anchor(3, 1).Caret(7, 1).NoNext();
			
			Init();
			lines.SetText(
				"Du\n" +
				"Du hast\n" +
				"Du hast mich");
			controller.SelectWordAtPlace(new Place(1, 0), false);
			AssertSelection().Anchor(0, 0).Caret(2, 0);
			controller.PutNewCursor(new Pos(4, 1));
			controller.SelectWordAtPlace(new Place(4, 1), true);
			AssertSelection().Anchor(0, 0).Caret(2, 0).Next().Anchor(3, 1).Caret(7, 1).NoNext();
		}
		
		[Test]
		public void SelectWordAtPos_EndWord()
		{
			Init();
			lines.SetText(
				"Du\n" +
				"Du hast\n" +
				"Du hast mich");
			controller.SelectWordAtPlace(new Place(10, 1), false);
			AssertSelection().Anchor(3, 1).Caret(7, 1).NoNext();
			
			Init();
			lines.SetText(
				"Du\n" +
				"\n" +
				"Du hast mich");
			controller.SelectWordAtPlace(new Place(10, 1), false);
			AssertSelection().Both(0, 1).NoNext();
		}
		
		[Test]
		public void SelectNextText_FirstWord()
		{
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			controller.PutCursor(new Pos(4, 1), false);
			AssertSelection().Both(4, 1).NoNext();
			controller.SelectNextText();
			AssertSelection().Anchor(3, 1).Caret(7, 1).NoNext();
			
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			controller.PutCursor(new Pos(3, 2), false);
			AssertSelection().Both(3, 2).NoNext();
			controller.SelectNextText();
			AssertSelection().Anchor(3, 2).Caret(7, 2).NoNext();
			
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			controller.PutCursor(new Pos(2, 2), false);
			AssertSelection().Both(2, 2).NoNext();
			controller.SelectNextText();
			AssertSelection().Anchor(2, 2).Caret(3, 2).NoNext();
			
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			controller.PutCursor(new Pos(6, 2), false);
			AssertSelection().Both(6, 2).NoNext();
			controller.SelectNextText();
			AssertSelection().Anchor(3, 2).Caret(7, 2).NoNext();
		}
		
		[Test]
		public void SelectNextText_FirstWord_SpecialCases()
		{
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			controller.PutCursor(new Pos(7, 1), false);
			AssertSelection().Both(7, 1).NoNext();
			controller.SelectNextText();
			AssertSelection().Anchor(3, 1).Caret(7, 1).NoNext();
			
			Init();
			lines.SetText("Du\n\nDu hast mich");
			controller.PutCursor(new Pos(0, 1), false);
			AssertSelection().Both(0, 1).NoNext();
			controller.SelectNextText();
			AssertSelection().Anchor(0, 1).Caret(0, 1).NoNext();
		}
		
		[Test]
		public void SelectNextText_WordSeletedIfLastEmpty()
		{
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			controller.PutCursor(new Pos(4, 1), false);
			controller.PutCursor(new Pos(5, 1), true);
			controller.PutNewCursor(new Pos(4, 2));
			AssertSelection().Anchor(4, 1).Caret(5, 1).Next().Both(4, 2).NoNext();
			controller.SelectNextText();
			AssertSelection().Anchor(4, 1).Caret(5, 1).Next().Anchor(3, 2).Caret(7, 2).NoNext();
			
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			controller.PutCursor(new Pos(0, 0), false);
			controller.PutCursor(new Pos(2, 0), true);
			controller.PutNewCursor(new Pos(4, 1));
			controller.PutNewCursor(new Pos(4, 2));
			AssertSelection().Anchor(0, 0).Caret(2, 0).Next().Both(4, 1).Next().Both(4, 2).NoNext();
			controller.SelectNextText();
			AssertSelection().Anchor(0, 0).Caret(2, 0).Next().Anchor(3, 1).Caret(7, 1).Next().Anchor(3, 2).Caret(7, 2).NoNext();
		}
		
		[Test]
		public void SelectNextText_Next()
		{
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			controller.PutCursor(new Pos(4, 1), false);
			AssertSelection().Both(4, 1).NoNext();
			controller.SelectNextText();
			AssertSelection().Anchor(3, 1).Caret(7, 1).NoNext();
			
			controller.SelectNextText();
			AssertSelection().Anchor(3, 1).Caret(7, 1).Next().Anchor(3, 2).Caret(7, 2).NoNext();
		}
		
		[Test]
		public void PreferredPosOfSelection0()
		{
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			controller.PutCursor(new Pos(4, 1), false);
			AssertSelection().Both(4, 1).NoNext();
			controller.SelectNextText();
			AssertSelection().Anchor(3, 1).Caret(7, 1).NoNext();
			
			controller.MoveDown(false);
			AssertSelection().Both(7, 2).NoNext();
		}
		
		[Test]
		public void PreferredPosOfSelection1()
		{
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			controller.PutCursor(new Pos(4, 1), false);
			AssertSelection().Both(4, 1).NoNext();
			controller.SelectNextText();
			AssertSelection().Anchor(3, 1).Caret(7, 1).NoNext();
			controller.SelectNextText();
			AssertSelection().Anchor(3, 1).Caret(7, 1).Next().Anchor(3, 2).Caret(7, 2).NoNext();
			
			controller.MoveUp(false);
			AssertSelection().Both(2, 0).Next().Both(7, 1).NoNext();
		}
	}
}
