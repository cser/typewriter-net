using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class ControllerSelectionTest : ControllerTestBase
	{
		private const string SimpleText =
			"Du\n" +
			"Du hast\n" +
			"Du hast mich";
		//	0123456789012
	
		[Test]
		public void SelectionLeftRight()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Anchor(0, 0).Caret(0, 0);
			
			controller.MoveRight(false);
			AssertSelection().Anchor(1, 0).Caret(1, 0);
			
			controller.MoveLeft(false);
			AssertSelection().Anchor(0, 0).Caret(0, 0);
			
			controller.MoveDown(false);
			controller.MoveRight(false);
			AssertSelection().Anchor(1, 1).Caret(1, 1);
			
			controller.MoveRight(true);
			AssertSelection().Anchor(1, 1).Caret(2, 1);
			controller.MoveRight(true);
			AssertSelection().Anchor(1, 1).Caret(3, 1);
			
			controller.MoveRight(false);
			AssertSelection().Anchor(3, 1).Caret(3, 1);
			
			controller.MoveLeft(true);
			AssertSelection().Anchor(3, 1).Caret(2, 1);
			controller.MoveLeft(true);
			AssertSelection().Anchor(3, 1).Caret(1, 1);
			
			controller.MoveRight(false);
			AssertSelection().Anchor(3, 1).Caret(3, 1);
			
			controller.MoveLeft(true);
			controller.MoveLeft(true);
			AssertSelection().Anchor(3, 1).Caret(1, 1);
			controller.MoveLeft(false);
			AssertSelection().Anchor(1, 1).Caret(1, 1);
			
			controller.MoveRight(true);
			controller.MoveRight(true);
			AssertSelection().Anchor(1, 1).Caret(3, 1);
			controller.MoveLeft(false);
			AssertSelection().Anchor(1, 1).Caret(1, 1);
		}
		
		[Test]
		public void SelectionUpDown()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Anchor(0, 0).Caret(0, 0);
			
			controller.MoveDown(true);
			AssertSelection().Anchor(0, 0).Caret(0, 1);
			
			controller.MoveRight(true);
			AssertSelection().Anchor(0, 0).Caret(1, 1);
			
			controller.MoveUp(false);
			AssertSelection().Anchor(1, 0).Caret(1, 0);
			
			controller.MoveDown(true);
			AssertSelection().Anchor(1, 0).Caret(1, 1);
			
			controller.MoveUp(false);
			controller.MoveDown(false);
			controller.MoveRight(false);
			AssertSelection().Anchor(2, 1).Caret(2, 1);
			
			controller.MoveUp(true);
			AssertSelection().Anchor(2, 1).Caret(2, 0);
			controller.MoveDown(true);
			controller.MoveDown(true);
			AssertSelection().Anchor(2, 1).Caret(2, 2);
			controller.MoveUp(true);
			AssertSelection().Anchor(2, 1).Caret(2, 1);
		}
		
		[Test]
		public void HomeEnd()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Anchor(0, 0).Caret(0, 0);
			
			controller.MoveDown(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Anchor(2, 1).Caret(2, 1);
			
			controller.MoveHome(true);
			AssertSelection().Anchor(2, 1).Caret(0, 1);
			
			controller.MoveEnd(true);
			AssertSelection().Anchor(2, 1).Caret(7, 1);
		}
		
		[Test]
		public void HomeEnd_MastBeWithoutFirstSpaces0()
		{
			Init();
			lines.SetText("Du\r\n  Du hast\r\n  Du hast mich");
			
			controller.PutCursor(new Pos(5, 1), false);
			AssertSelection().Anchor(5, 1).Caret(5, 1).NoNext();
			
			controller.MoveHome(true);
			AssertSelection().Anchor(5, 1).Caret(2, 1).NoNext();
			
			controller.MoveHome(true);
			AssertSelection().Anchor(5, 1).Caret(0, 1).NoNext();
			
			controller.MoveEnd(true);
			AssertSelection().Anchor(5, 1).Caret(9, 1).NoNext();
		}
		
		[Test]
		public void HomeEnd_MastBeWithoutFirstSpaces1()
		{
			Init();
			lines.SetText("Du\r\n  Du hast\r\n  Du hast mich");
			
			controller.PutCursor(new Pos(1, 1), false);
			AssertSelection().Both(1, 1).NoNext();
			
			controller.MoveHome(false);
			AssertSelection().Both(0, 1).NoNext();
			
			controller.MoveEnd(false);
			AssertSelection().Both(9, 1).NoNext();
		}
		
		[Test]
		public void HomeEnd_MastBeWithoutFirstSpaces2()
		{
			Init();
			lines.SetText("Du\r\n  Du hast\r\n  Du hast mich");
			
			controller.PutCursor(new Pos(3, 1), false);
			AssertSelection().Both(3, 1).NoNext();
			
			controller.MoveHome(false);
			AssertSelection().Both(2, 1).NoNext();
			
			controller.MoveHome(false);
			AssertSelection().Both(0, 1).NoNext();
			
			controller.MoveEnd(false);
			AssertSelection().Both(9, 1).NoNext();
		}
		
		[Test]
		public void HomeEnd_MastBeWithoutFirstSpaces3()
		{
			Init();
			lines.SetText("Du\r\n\t\tDu hast\r\n  Du hast mich");
			
			controller.PutCursor(new Pos(100, 1), false);
			AssertSelection().Both(9, 1).NoNext();
			
			controller.MoveHome(false);
			AssertSelection().Both(2, 1).NoNext();
			
			controller.MoveHome(false);
			AssertSelection().Both(0, 1).NoNext();
			
			controller.MoveEnd(false);
			AssertSelection().Both(9, 1).NoNext();
		}
		
		[Test]
		public void EraseSelection()
		{
			Init();
			lines.SetText(SimpleText);
			
			// Du\n
			// Du hast\n
			// Du hast mich
			
			AssertSize().XY(12, 3);
			
			controller.PutCursor(new Pos(2, 2), false);
			controller.PutCursor(new Pos(7, 2), true);
			AssertSelection().Anchor(2, 2).Caret(7, 2);
			
			controller.EraseSelection();
			AssertSelection().Anchor(2, 2).Caret(2, 2);
			Assert.AreEqual("Du mich", GetLineText(2));
			
			// Du\n
			// Du hast\n
			// Du mich
			
			AssertSize().XY(8, 3);
		}
		
		[Test]
		public void EraseSelection_Multiline()
		{
			Init();
			lines.SetText(SimpleText);
			
			AssertSize().XY(12, 3);
			
			controller.PutCursor(new Pos(1, 0), false);
			controller.PutCursor(new Pos(9, 2), true);
			AssertSelection().Anchor(1, 0).Caret(9, 2);
			
			controller.EraseSelection();
			AssertSelection().Anchor(1, 0).Caret(1, 0);
			Assert.AreEqual("Dich", GetLineText(0));
			Assert.AreEqual(1, lines.LinesCount);
			
			AssertSize().XY(4, 1);
		}
		
		[Test]
		public void MoveWordLeftRight()
		{
			Init();
			lines.SetText("text text123 text");
			AssertSelection().Caret(0, 0).PreferredPos(0);
			
			controller.MoveWordRight(true);
			AssertSelection().Anchor(0, 0).Caret(4, 0).PreferredPos(4);
			controller.MoveWordLeft(false);
			AssertSelection().Both(0, 0).PreferredPos(0);
			controller.MoveWordRight(false);
			AssertSelection().Both(4, 0).PreferredPos(4);
			controller.MoveWordRight(true);
			AssertSelection().Anchor(4, 0).Caret(12, 0).PreferredPos(12);
			controller.MoveWordLeft(true);
			AssertSelection().Anchor(4, 0).Caret(5, 0).PreferredPos(5);
			controller.MoveWordLeft(true);
			AssertSelection().Anchor(4, 0).Caret(0, 0).PreferredPos(0);
		}
		
		[Test]
		public void ShiftLeftRight0()
		{
			Init();
			lines.SetText("\tDu\nDu hast\n    Du hast mich");			
			controller.PutCursor(new Pos(4, 0), false);
			controller.PutCursor(new Pos(6, 2), true);
			AssertSelection().Anchor(1, 0).Caret(6, 2).NoNext();
			
			controller.ShiftLeft();
			AssertText("Du\nDu hast\nDu hast mich");
			AssertSelection().Anchor(0, 0).Caret(2, 2).NoNext();
			
			controller.Undo();
			AssertText("\tDu\nDu hast\n    Du hast mich");
			AssertSelection().Anchor(1, 0).Caret(6, 2).NoNext();
			
			controller.Redo();
			AssertText("Du\nDu hast\nDu hast mich");
			AssertSelection().Anchor(0, 0).Caret(2, 2).NoNext();
			
			controller.Undo();
			AssertText("\tDu\nDu hast\n    Du hast mich");
			AssertSelection().Anchor(1, 0).Caret(6, 2).NoNext();
			
			controller.ShiftRight();
			AssertText("\t\tDu\n\tDu hast\n\t\tDu hast mich");
			AssertSelection().Anchor(2, 0).Caret(4, 2).NoNext();
			
			controller.Undo();
			AssertText("\tDu\nDu hast\n    Du hast mich");
			AssertSelection().Anchor(1, 0).Caret(6, 2).NoNext();
		}
		
		[Test]
		public void ShiftLeftRight1()
		{
			Init();
			lines.SetText("\tDu\nDu hast\n    Du hast mich");
			controller.PutCursor(new Pos(4, 0), false);
			controller.PutCursor(new Pos(16, 2), true);
			AssertSelection().Anchor(1, 0).Caret(16, 2).NoNext();
			
			controller.ShiftRight();
			AssertText("\t\tDu\n\tDu hast\n\t\tDu hast mich");
			AssertSelection().Anchor(2, 0).Caret(14, 2).NoNext();
			
			controller.ShiftLeft();
			AssertText("\tDu\nDu hast\n\tDu hast mich");
			AssertSelection().Anchor(1, 0).Caret(13, 2).NoNext();
			
			controller.Undo();
			AssertText("\t\tDu\n\tDu hast\n\t\tDu hast mich");
			controller.Undo();
			AssertText("\tDu\nDu hast\n    Du hast mich");
		}
		
		[Test]
		public void ShiftLeftRight2()
		{
			Init();
			lines.SetText("\tline0\n  line1\n\tline2\n\tline3");
			controller.PutCursor(new Pos(0, 1), false);
			controller.PutCursor(new Pos(9, 3), true);
			AssertSelection().Anchor(0, 1).Caret(6, 3).NoNext();
			
			controller.ShiftLeft();
			AssertText("\tline0\nline1\nline2\nline3");
			AssertSelection().Anchor(0, 1).Caret(5, 3).NoNext();
			
			controller.ShiftRight();
			AssertText("\tline0\n\tline1\n\tline2\n\tline3");
			AssertSelection().Anchor(1, 1).Caret(6, 3).NoNext();
		}
		
		[Test]
		public void ShiftLeftRight3()
		{
			Init();
			lines.SetText("\tline0\n  line1\n\tline2\n\tline3");
			controller.PutCursor(new Pos(1, 1), false);
			controller.PutCursor(new Pos(9, 3), true);
			AssertSelection().Anchor(1, 1).Caret(6, 3).NoNext();
			
			controller.ShiftLeft();
			AssertText("\tline0\nline1\nline2\nline3");
			AssertSelection().Anchor(0, 1).Caret(5, 3).NoNext();
			
			controller.ShiftRight();
			AssertText("\tline0\n\tline1\n\tline2\n\tline3");
			AssertSelection().Anchor(1, 1).Caret(6, 3).NoNext();
		}
		
		[Test]
		public void EraseSelection_ChangePrefPosition()
		{
			Init();
			
			lines.SetText("line0\nline1\nline2\nline3");
			lines.wwValidator.Validate(10);
			controller.PutCursor(new Pos(1, 1), false);
			controller.PutCursor(new Pos(4, 1), true);
			AssertSelection().Anchor(1, 1).Caret(4, 1).NoNext();
			
			controller.EraseSelection();
			AssertText("line0\nl1\nline2\nline3");
			AssertSelection().Both(1, 1).NoNext();
			
			controller.MoveDown(false);
			AssertSelection().Both(1, 2).NoNext();
		}

		[Test]
		public void ClearFirstMinorSelections()
		{
			Init();
			
			lines.SetText("line0\nline1\nline2\nline3");
			lines.wwValidator.Validate(10);
			controller.PutCursor(new Place(1, 1), false);
			controller.PutCursor(new Place(4, 1), true);
			controller.PutNewCursor(new Place(1, 2));
			controller.PutCursor(new Place(4, 2), true);
			controller.PutNewCursor(new Place(1, 3));
			controller.PutCursor(new Place(4, 3), true);
			AssertSelection().Anchor(1, 1).Caret(4, 1).Next().Anchor(1, 2).Caret(4, 2).Next().Anchor(1, 3).Caret(4, 3).NoNext();

			controller.ClearFirstMinorSelections();
			AssertSelection().Anchor(1, 3).Caret(4, 3).NoNext();
		}
	}
}
