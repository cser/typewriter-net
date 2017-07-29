using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class ControllerTest : ControllerTestBase
	{
		private const string SimpleText =
			"Du\n" +
			"Du hast\n" +
			"Du hast mich";
		//	0123456789012
	
		[Test]
		public void MoveLeftRight()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Both(0, 0);
			
			controller.MoveRight(false);
			AssertSelection().Both(1, 0);
			
			controller.MoveRight(false);
			AssertSelection().Both(2, 0);
			
			controller.MoveRight(false);
			AssertSelection().Both(0, 1);
			
			controller.MoveRight(false);
			AssertSelection().Both(1, 1);
			
			for (int i = 0; i < 18; i++)
			{
				controller.MoveRight(false);
			}
			AssertSelection().Both(11, 2);
			
			controller.MoveRight(false);
			AssertSelection().Both(12, 2);
			
			controller.MoveRight(false);
			AssertSelection().Both(12, 2);
			
			controller.MoveLeft(false);
			AssertSelection().Both(11, 2);
			
			for (int i = 0; i < 10; i++)
			{
				controller.MoveLeft(false);
			}
			AssertSelection().Both(1, 2);
			
			controller.MoveLeft(false);
			AssertSelection().Both(0, 2);
			
			controller.MoveLeft(false);
			AssertSelection().Both(7, 1);
			
			for (int i = 0; i < 9; i++)
			{
				controller.MoveLeft(false);
			}
			AssertSelection().Both(1, 0);
			
			controller.MoveLeft(false);
			AssertSelection().Both(0, 0);
			
			controller.MoveLeft(false);
			AssertSelection().Both(0, 0);
			
			controller.MoveRight(false);
			AssertSelection().Both(1, 0);
		}
		
		[Test]
		public void MoveLeftRight_RN()
		{
			Init();
			lines.SetText(
				"Du\r\n" +
				"Du hast\n" +
				"\r" +
				"Du hast mich");
			Assert.AreEqual(4, lines.LinesCount);
			AssertSelection().Both(0, 0);
			
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(2, 0);
			controller.MoveRight(false);
			AssertSelection().Both(0, 1);
			
			controller.MoveRight(false);
			AssertSelection().Both(1, 1);
			
			controller.MoveLeft(false);
			AssertSelection().Both(0, 1);
			controller.MoveLeft(false);
			AssertSelection().Both(2, 0);
			controller.MoveLeft(false);
			AssertSelection().Both(1, 0);
			
			controller.PutCursor(new Pos(6, 1), false);
			AssertSelection().Both(6, 1);
			
			controller.MoveRight(false);
			AssertSelection().Both(7, 1);
			controller.MoveRight(false);
			AssertSelection().Both(0, 2);
			controller.MoveRight(false);
			AssertSelection().Both(0, 3);
			controller.MoveRight(false);
			AssertSelection().Both(1, 3);

			controller.MoveLeft(false);
			AssertSelection().Both(0, 3);
			controller.MoveLeft(false);
			AssertSelection().Both(0, 2);
			controller.MoveLeft(false);
			AssertSelection().Both(7, 1);
			controller.MoveLeft(false);
			AssertSelection().Both(6, 1);
		}
		
		[Test]
		public void MoveLeftRight_RN_Shift()
		{
			Init();
			lines.SetText(
				"Du\r\n" +
				"Du hast\n" +
				"\r" +
				"Du hast mich");
			Assert.AreEqual(4, lines.LinesCount);
			AssertSelection().Both(0, 0);
			
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(2, 0);
			controller.MoveRight(true);
			AssertSelection().Anchor(2, 0).Caret(0, 1);
			
			controller.MoveRight(false);
			AssertSelection().Both(0, 1);
			controller.MoveRight(false);
			AssertSelection().Both(1, 1);
			
			controller.MoveLeft(true);
			AssertSelection().Anchor(1, 1).Caret(0, 1);
			controller.MoveLeft(true);
			AssertSelection().Anchor(1, 1).Caret(2, 0);
			controller.MoveLeft(true);
			AssertSelection().Anchor(1, 1).Caret(1, 0);
		}
		
		[Test]
		public void MoveUpDown()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Both(0, 0);
			
			controller.MoveDown(false);
			AssertSelection().Both(0, 1);
			
			controller.MoveUp(false);
			AssertSelection().Both(0, 0);
			
			controller.MoveUp(false);
			AssertSelection().Both(0, 0);
			
			controller.MoveRight(false);
			AssertSelection().Both(1, 0);
			
			controller.MoveDown(false);
			AssertSelection().Both(1, 1);
			
			controller.MoveUp(false);
			AssertSelection().Both(1, 0);
			
			controller.MoveUp(false);
			AssertSelection().Both(1, 0);
			
			controller.MoveDown(false);
			controller.MoveDown(false);
			AssertSelection().Both(1, 2);
			
			controller.MoveUp(false);
			AssertSelection().Both(1, 1);
		}
		
		[Test]
		public void MoveUpDown_PreferredPos()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Both(0, 0);
			
			controller.MoveDown(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveUp(false);
			AssertSelection().Both(2, 0);
			controller.MoveDown(false);
			AssertSelection().Both(2, 1);
			
			controller.MoveRight(false);
			AssertSelection().Both(3, 1);
			controller.MoveUp(false);
			AssertSelection().Both(2, 0);
			controller.MoveDown(false);
			AssertSelection().Both(3, 1);
			controller.MoveDown(false);
			AssertSelection().Both(3, 2);
			
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(8, 2);
			controller.MoveUp(false);
			AssertSelection().Both(7, 1);
			controller.MoveUp(false);
			AssertSelection().Both(2, 0);
			controller.MoveDown(false);
			AssertSelection().Both(7, 1);
			controller.MoveDown(false);
			AssertSelection().Both(8, 2);
			controller.MoveUp(false);
			AssertSelection().Both(7, 1);
			controller.MoveLeft(false);
			AssertSelection().Both(6, 1);
			controller.MoveDown(false);
			AssertSelection().Both(6, 2);
			
			controller.MoveUp(false);
			AssertSelection().Both(6, 1);
			controller.MoveRight(false);
			AssertSelection().Both(7, 1);
			controller.MoveDown(false);
			AssertSelection().Both(7, 2);
			controller.MoveUp(false);
			AssertSelection().Both(7, 1);
			controller.MoveRight(false);
			AssertSelection().Both(0, 2);
			controller.MoveUp(false);
			AssertSelection().Both(0, 1);
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void MoveDown_LastLine_MastSetCursorToRight(bool fictiveWordWrap)
		{			
			Init();
			lines.SetText(SimpleText);
			lines.wordWrap = fictiveWordWrap;
			if (lines.wordWrap)
			{
				lines.wwValidator.Validate(50);
				Assert.AreEqual(50, lines.wwSizeX);
				Assert.AreEqual(3, lines.wwSizeY);
			}
			
			AssertSelection().Both(0, 0);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveDown(false);
			controller.MoveDown(false);
			AssertSelection().Both(2, 2);
			
			Assert.AreEqual(true, controller.MoveDown(false), "without selection");
			AssertSelection().Both(12, 2);
			
			Assert.AreEqual(false, controller.MoveDown(false), "without selection 2");
			AssertSelection().Both(12, 2);
			
			controller.MoveUp(false);
			AssertSelection().Both(2, 1);
			
			controller.MoveDown(false);
			AssertSelection().Both(2, 2);
			
			Assert.AreEqual(true, controller.MoveDown(true), "with selection");
			AssertSelection().Anchor(2, 2).Caret(12, 2);
		}
		
		[Test]
		public void InsertChar()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Both(0, 0);
			
			controller.InsertText("a");
			AssertSelection().Both(1, 0);
			Assert.AreEqual("aDu\n", GetLineText(0));
			
			controller.MoveDown(false);
			AssertSelection().Both(1, 1);
			controller.InsertText("b");
			AssertSelection().Both(2, 1);
			Assert.AreEqual("aDu\n", GetLineText(0));
			Assert.AreEqual("Dbu hast\n", GetLineText(1));
			for (int i = 0; i < 6; i++)
			{
				controller.MoveRight(false);
			}
			AssertSelection().Both(8, 1);
			controller.InsertText("c");
			AssertSelection().Both(9, 1);
			Assert.AreEqual("Dbu hastc\n", GetLineText(1));
			
			Assert.AreEqual(new IntSize(12, 3), lines.Size);
			
			controller.MoveDown(false);
			AssertSelection().Both(9, 2);
			controller.InsertText("d");
			AssertSelection().Both(10, 2);
			Assert.AreEqual(3, lines.LinesCount);
			Assert.AreEqual("aDu\n", GetLineText(0));
			Assert.AreEqual("Dbu hastc\n", GetLineText(1));
			Assert.AreEqual("Du hast mdich", GetLineText(2));
			
			Assert.AreEqual(new IntSize(13, 3), lines.Size);
		}
		
		[Test]
		public void InsertEnter_N()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Both(0, 0);
			
			controller.InsertText("\n");
			AssertSelection().Both(0, 1);
			Assert.AreEqual("\n", GetLineText(0));
			Assert.AreEqual("Du\n", GetLineText(1));
			Assert.AreEqual("Du hast\n", GetLineText(2));
			
			Assert.AreEqual(new IntSize(12, 4), lines.Size);
			
			controller.MoveDown(false);
			controller.MoveDown(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(3, 3);
			controller.InsertText("\n");
			AssertSelection().Both(0, 4);
			Assert.AreEqual("\n", GetLineText(0));
			Assert.AreEqual("Du\n", GetLineText(1));
			Assert.AreEqual("Du hast\n", GetLineText(2));
			Assert.AreEqual("Du \n", GetLineText(3));
			Assert.AreEqual("hast mich", GetLineText(4));
			
			Assert.AreEqual(new IntSize(9, 5), lines.Size);
			
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.InsertText("\n");
			AssertSelection().Both(0, 5);
			Assert.AreEqual("\n", GetLineText(0));
			Assert.AreEqual("Du\n", GetLineText(1));
			Assert.AreEqual("Du hast\n", GetLineText(2));
			Assert.AreEqual("Du \n", GetLineText(3));
			Assert.AreEqual("ha\n", GetLineText(4));
			Assert.AreEqual("st mich", GetLineText(5));
		}
		
		[Test]
		public void InsertEnter_R()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Both(0, 0);
			
			controller.InsertText("\r");
			AssertSelection().Both(0, 1);
			Assert.AreEqual("\r", GetLineText(0));
			Assert.AreEqual("Du\n", GetLineText(1));
			Assert.AreEqual("Du hast\n", GetLineText(2));
			
			Assert.AreEqual(new IntSize(12, 4), lines.Size);
			
			controller.MoveDown(false);
			controller.MoveDown(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(3, 3);
			controller.InsertText("\r");
			AssertSelection().Both(0, 4);
			Assert.AreEqual("\r", GetLineText(0));
			Assert.AreEqual("Du\n", GetLineText(1));
			Assert.AreEqual("Du hast\n", GetLineText(2));
			Assert.AreEqual("Du \r", GetLineText(3));
			Assert.AreEqual("hast mich", GetLineText(4));
			
			Assert.AreEqual(new IntSize(9, 5), lines.Size);
			
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.InsertText("\r");
			AssertSelection().Both(0, 5);
			Assert.AreEqual("\r", GetLineText(0));
			Assert.AreEqual("Du\n", GetLineText(1));
			Assert.AreEqual("Du hast\n", GetLineText(2));
			Assert.AreEqual("Du \r", GetLineText(3));
			Assert.AreEqual("ha\r", GetLineText(4));
			Assert.AreEqual("st mich", GetLineText(5));
		}
		
		[Test]
		public void Backspace0()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Both(0, 0);
			
			controller.MoveRight(false);
			AssertSelection().Both(1, 0);
			
			controller.Backspace();
			AssertSelection().Both(0, 0);
			Assert.AreEqual("u\n", GetLineText(0));
			
			controller.Backspace();
			AssertSelection().Both(0, 0);
			Assert.AreEqual("u\n", GetLineText(0));
			
			controller.MoveDown(false);
			AssertSelection().Both(0, 1);
			
			Assert.AreEqual(new IntSize(12, 3), lines.Size);
			
			controller.Backspace();
			AssertSelection().Both(1, 0);
			Assert.AreEqual("uDu hast\n", GetLineText(0));
			Assert.AreEqual("Du hast mich", GetLineText(1));
	
			Assert.AreEqual(new IntSize(12, 2), lines.Size);
			
			controller.Backspace();
			AssertSelection().Both(0, 0);
			Assert.AreEqual("Du hast\n", GetLineText(0));
			controller.MoveDown(false);
			AssertSelection().Both(0, 1);
			controller.Backspace();
			AssertSelection().Both(7, 0);
			Assert.AreEqual("Du hastDu hast mich", GetLineText(0));
			
			Assert.AreEqual(new IntSize(19, 1), lines.Size);
		}
		
		[Test]
		public void Backspace1()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Both(0, 0);
			
			controller.MoveDown(false);
			controller.MoveDown(false);
			AssertSelection().Both(0, 2);
			
			controller.MoveRight(false);
			AssertSelection().Both(1, 2);
			
			Assert.AreEqual(new IntSize(12, 3), lines.Size);
			
			controller.Backspace();
	
			Assert.AreEqual(new IntSize(11, 3), lines.Size);
		}
		
		[Test]
		public void Backspace_RN()
		{
			Init();
			lines.SetText("Du\r\nDu hast\r\nDu hast mich");
			AssertSelection().Both(0, 0);
			
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(0, 1);
			
			controller.Backspace();
			AssertText("DuDu hast\r\nDu hast mich");
			AssertSelection().Both(2, 0);
		}
		
		[Test]
		public void Delete0()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Both(0, 0);
			
			controller.Delete();
			AssertSelection().Both(0, 0);
			Assert.AreEqual("u\n", GetLineText(0));
			
			controller.Delete();
			AssertSelection().Both(0, 0);
			Assert.AreEqual("\n", GetLineText(0));
			
			Assert.AreEqual(new IntSize(12, 3), lines.Size);
			
			controller.Delete();
			AssertSelection().Both(0, 0);
			Assert.AreEqual("Du hast\n", GetLineText(0));
			Assert.AreEqual("Du hast mich", GetLineText(1));
			
			Assert.AreEqual(new IntSize(12, 2), lines.Size);
			
			for (int i = 0; i < 7; i++)
			{
				controller.MoveRight(false);
			}
			controller.Delete();
			AssertSelection().Both(7, 0);
			Assert.AreEqual("Du hastDu hast mich", GetLineText(0));
			
			Assert.AreEqual(new IntSize(19, 1), lines.Size);
		}
		
		[Test]
		public void Delete1()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Both(0, 0);
			
			Assert.AreEqual(new IntSize(12, 3), lines.Size);
			
			controller.MoveDown(false);
			controller.MoveDown(false);
			for (int i = 0; i < 11; i++)
			{
				controller.MoveRight(false);
			}
			controller.Delete();
			AssertSelection().Both(11, 2);
			Assert.AreEqual("Du hast mic", GetLineText(2));
			
			Assert.AreEqual(new IntSize(11, 3), lines.Size);
			
			controller.MoveUp(false);
			controller.MoveUp(false);
			AssertSelection().Both(2, 0);
			controller.MoveDown(false);
			AssertSelection().Both(7, 1);
			controller.MoveUp(false);
			AssertSelection().Both(2, 0);
			controller.Delete();
			AssertSelection().Both(2, 0);
			Assert.AreEqual("DuDu hast\n", GetLineText(0));
			Assert.AreEqual("Du hast mic", GetLineText(1));
			controller.MoveDown(false);
			AssertSelection().Both(2, 1);
		}
		
		[Test]
		public void Delete_RN()
		{
			Init();
			lines.SetText(
				"Du\r\n" +
				"Du hast\r\n" +
				"Du hast mich");
			AssertSelection().Both(0, 0);
			
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(2, 0);
			controller.Delete();
			AssertText("DuDu hast\r\nDu hast mich");
			AssertSelection().Both(2, 0);
		}
		
		[Test]
		public void HomeEnd()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Both(0, 0);
			
			controller.MoveEnd(false);
			AssertSelection().Both(2, 0);
			controller.MoveHome(false);
			AssertSelection().Both(0, 0);
			
			controller.MoveEnd(false);
			controller.MoveDown(false);
			AssertSelection().Both(2, 1);
			controller.MoveHome(false);
			controller.MoveUp(false);
			AssertSelection().Both(0, 0);
		}
		
		[Test]
		public void PlaceOfPos()
		{
			Init();
			lines.SetText(SimpleText);
			
			Assert.AreEqual(new Place(0, 0), lines.PlaceOf(new Pos(0, 0)));
			Assert.AreEqual(new Place(2, 0), lines.PlaceOf(new Pos(2, 0)));
			Assert.AreEqual(new Place(2, 1), lines.PlaceOf(new Pos(2, 1)));
			Assert.AreEqual(new Place(12, 2), lines.PlaceOf(new Pos(12, 2)));
			
			Assert.AreEqual(new Place(11, 2), lines.PlaceOf(new Pos(11, 3)));
			Assert.AreEqual(new Place(2, 0), lines.PlaceOf(new Pos(2, -2)));
			
			Assert.AreEqual(new Place(12, 2), lines.PlaceOf(new Pos(13, 2)));
			Assert.AreEqual(new Place(12, 2), lines.PlaceOf(new Pos(15, 2)));
			Assert.AreEqual(new Place(0, 2), lines.PlaceOf(new Pos(-1, 2)));
		}
		
		[Test]
		public void PutCursor()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Anchor(0, 0).Caret(0, 0);
			
			controller.PutCursor(new Pos(0, 0), false);
			AssertSelection().Anchor(0, 0).Caret(0, 0);
			
			controller.PutCursor(new Pos(12, 2), false);
			AssertSelection().Anchor(12, 2).Caret(12, 2);
			
			controller.PutCursor(new Pos(13, 2), false);
			AssertSelection().Anchor(12, 2).Caret(12, 2);
			
			controller.PutCursor(new Pos(11, 3), false);
			AssertSelection().Anchor(11, 2).Caret(11, 2);
		}
		
		[Test]
		public void PutCursor_Moving()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Anchor(0, 0).Caret(0, 0);
			
			controller.PutCursor(new Pos(0, 0), true);
			AssertSelection().Anchor(0, 0).Caret(0, 0);
			
			controller.PutCursor(new Pos(12, 2), true);
			AssertSelection().Anchor(0, 0).Caret(12, 2);
			
			controller.PutCursor(new Pos(13, 2), true);
			AssertSelection().Anchor(0, 0).Caret(12, 2);
			
			controller.PutCursor(new Pos(1, 2), false);
			AssertSelection().Anchor(1, 2).Caret(1, 2);
			controller.PutCursor(new Pos(12, 2), true);
			AssertSelection().Anchor(1, 2).Caret(12, 2);
			controller.PutCursor(new Pos(11, 2), true);
			AssertSelection().Anchor(1, 2).Caret(11, 2);
			controller.PutCursor(new Pos(4, 1), true);
			AssertSelection().Anchor(1, 2).Caret(4, 1);
		}
		
		[Test]
		public void PutCursor_PreferredPos()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Caret(0, 0);
			
			controller.PutCursor(new Pos(3, 0), false);
			AssertSelection().Caret(2, 0);
			controller.MoveDown(false);
			AssertSelection().Caret(2, 1);
			
			controller.PutCursor(new Pos(3, 1), false);
			AssertSelection().Caret(3, 1);
			controller.MoveDown(false);
			AssertSelection().Caret(3, 2);
		}
		
		[Test]
		public void MoveWordLeftRight0()
		{
			Init();
			lines.SetText("text text123 text");
			AssertSelection().Caret(0, 0).PreferredPos(0);
			
			controller.MoveWordRight(false);
			AssertSelection().Caret(5, 0).PreferredPos(5);
			controller.MoveWordLeft(false);
			AssertSelection().Caret(0, 0).PreferredPos(0);
			controller.MoveWordRight(false);
			AssertSelection().Caret(5, 0).PreferredPos(5);
			controller.MoveWordRight(false);
			AssertSelection().Caret(13, 0).PreferredPos(13);
			controller.MoveWordLeft(false);
			AssertSelection().Caret(5, 0).PreferredPos(5);
			controller.MoveWordLeft(false);
			AssertSelection().Caret(0, 0).PreferredPos(0);
		}
		
		[Test]
		public void MoveWordRightRN()
		{
			Init();
			lines.SetText(
				"text text123 text\r\n" +
				"    --text(text);\r\n" +
				"\t    text text\n"
			);
			AssertSelection().Caret(0, 0);
			
			controller.MoveWordRight(false);
			controller.MoveWordRight(false);
			controller.MoveWordRight(false);
			AssertSelection().Caret(17, 0);
			
			controller.MoveWordRight(false);
			AssertSelection().Caret(4, 1);
			controller.MoveWordRight(false);
			AssertSelection().Caret(6, 1);
			controller.MoveWordRight(false);
			AssertSelection().Caret(10, 1);
			controller.MoveWordRight(false);
			AssertSelection().Caret(11, 1);
		}
		
		[Test]
		public void MoveWordLeftRN()
		{
			Init();
			lines.SetText(
				"text text123 text\r\n" +
				"    --text(text);\r\n" +
				"\t    text text\n"
			);
			controller.PutCursor(new Place(11, 1), false);
			AssertSelection().Caret(11, 1);
			
			controller.MoveWordLeft(false);
			AssertSelection().Caret(10, 1);
			controller.MoveWordLeft(false);
			AssertSelection().Caret(6, 1);
			controller.MoveWordLeft(false);
			AssertSelection().Caret(4, 1);
			controller.MoveWordLeft(false);
			AssertSelection().Caret(17, 0);
			
			controller.MoveWordLeft(false);
			controller.MoveWordLeft(false);
			controller.MoveWordLeft(false);
			AssertSelection().Caret(0, 0);
			controller.MoveWordLeft(false);
			AssertSelection().Caret(0, 0);
		}
		
		[Test]
		public void SelectAll()
		{
			Init();
			lines.SetText(SimpleText);
			AssertSelection().Both(0, 0).NoNext();
			controller.SelectAll();			
			AssertSelection().Caret(0, 0).Anchor(12, 2).NoNext();
			
			Init();
			lines.SetText(SimpleText);
			controller.PutCursor(new Pos(1, 0), false);
			controller.PutNewCursor(new Pos(4, 1));
			AssertSelection().Both(1, 0).Next().Both(4, 1).NoNext();
			controller.SelectAll();			
			AssertSelection().Caret(0, 0).Anchor(12, 2).NoNext();
		}
		
		[Test]
		public void ShiftRight()
		{
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			controller.PutCursor(new Pos(0, 0), false);
			controller.PutCursor(new Pos(2, 0), true);
			AssertSelection().Anchor(0, 0).Caret(2, 0).NoNext();
			
			controller.ShiftRight();
			AssertText("Du\nDu hast\nDu hast mich");
			
			controller.PutCursor(new Pos(0, 1), true);
			AssertSelection().Anchor(0, 0).Caret(0, 1).NoNext();
			controller.ShiftRight();
			AssertText("\tDu\nDu hast\nDu hast mich");
		}
		
		[Test]
		public void ShiftLeft()
		{
			Init();
			lines.SetText("\tDu\nDu hast\nDu hast mich");			
			controller.PutCursor(new Pos(4, 0), false);
			controller.PutCursor(new Pos(6, 0), true);
			AssertSelection().Anchor(1, 0).Caret(3, 0).NoNext();
			
			controller.ShiftLeft();
			AssertText("\tDu\nDu hast\nDu hast mich");
			
			controller.PutCursor(new Pos(0, 1), true);
			AssertSelection().Anchor(1, 0).Caret(0, 1).NoNext();
			controller.ShiftLeft();
			AssertText("Du\nDu hast\nDu hast mich");
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
			
			controller.processor.Undo();
			AssertText("\tDu\nDu hast\n    Du hast mich");
			
			controller.processor.Redo();
			AssertText("Du\nDu hast\nDu hast mich");
			
			controller.processor.Undo();
			AssertText("\tDu\nDu hast\n    Du hast mich");
			
			controller.ShiftRight();
			AssertText("\t\tDu\n\tDu hast\n\t\tDu hast mich");
			
			controller.processor.Undo();
			AssertText("\tDu\nDu hast\n    Du hast mich");
		}
		
		[Test]
		public void ShiftLeftRight1()
		{
			Init();
			lines.SetText("\t   Du\n Du hast\n    Du hast mich");			
			controller.PutCursor(new Pos(8, 0), false);
			controller.PutCursor(new Pos(6, 2), true);
			AssertSelection().Anchor(5, 0).Caret(6, 2).NoNext();
			
			controller.ShiftLeft();
			AssertText("   Du\nDu hast\nDu hast mich");
			
			controller.processor.Undo();
			AssertText("\t   Du\n Du hast\n    Du hast mich");
			
			controller.processor.Redo();
			AssertText("   Du\nDu hast\nDu hast mich");
			
			controller.processor.Undo();
			AssertText("\t   Du\n Du hast\n    Du hast mich");
			
			controller.ShiftRight();
			AssertText("\t\t   Du\n\t Du hast\n\t\tDu hast mich");
			
			controller.processor.Undo();
			AssertText("\t   Du\n Du hast\n    Du hast mich");
		}
		
		[Test]
		public void Shift_ComplexMultilselection0()
		{
			Init();
			lines.SetText("line0\nline1\nline2\nline3\nline4");
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutCursor(new Pos(1, 1), true);			
			controller.PutNewCursor(new Pos(2, 1));
			controller.PutCursor(new Pos(1, 2), true);
			controller.PutNewCursor(new Pos(2, 2));
			controller.PutCursor(new Pos(1, 3), true);
			AssertSelection().Anchor(2, 0).Caret(1, 1).Next().Anchor(2, 1).Caret(1, 2).Next().Anchor(2, 2).Caret(1, 3).NoNext();
			
			controller.ShiftRight();
			AssertText("\tline0\n\tline1\n\tline2\n\tline3\nline4");
		}
		
		[Test]
		public void Shift_ComplexMultilselection1()
		{
			Init();
			lines.SetText("line0\nline1\nline2\nline3\nline4\nline5\nline6\nline7");
			controller.PutCursor(new Place(1, 1), false);
			controller.PutCursor(new Place(1, 2), true);			
			controller.PutNewCursor(new Place(1, 4));
			controller.PutCursor(new Place(1, 6), true);
			AssertSelection().Anchor(1, 1).Caret(1, 2).Next().Anchor(1, 4).Caret(1, 6).NoNext();
			
			controller.ShiftRight();
			AssertText("line0\n\tline1\n\tline2\nline3\n\tline4\n\tline5\n\tline6\nline7");
		}
		
		[Test]
		public void RemoveWordLeft()
		{
			Init();
			lines.SetText("line0 line1 line2 line3 line4");
			controller.PutCursor(new Place(11, 0), false);
			AssertSelection().Both(11, 0).NoNext();
			controller.RemoveWordLeft();
			AssertText("line0  line2 line3 line4");
			AssertSelection().Both(6, 0).NoNext();
			
			controller.processor.Undo();
			AssertText("line0 line1 line2 line3 line4");
			AssertSelection().Both(11, 0).NoNext();
			
			controller.processor.Redo();
			AssertText("line0  line2 line3 line4");
			AssertSelection().Both(6, 0).NoNext();
		}
		
		[Test]
		public void RemoveWordRight()
		{
			Init();
			lines.SetText("line0 line1 line2 line3 line4");
			controller.PutCursor(new Place(6, 0), false);
			AssertSelection().Both(6, 0).NoNext();
			controller.RemoveWordRight();
			AssertText("line0 line2 line3 line4");
			AssertSelection().Both(6, 0).NoNext();
			
			//Then not Npp behavour (npp moves cursor on undo), but seems to be more nice
			
			controller.processor.Undo();
			AssertSelection().Both(6, 0).NoNext();
			AssertText("line0 line1 line2 line3 line4");
			
			controller.processor.Redo();
			AssertText("line0 line2 line3 line4");
			AssertSelection().Both(6, 0).NoNext();
		}
		
		[Test]
		public void RemoveWordRight_Space()
		{
			Init();
			lines.SetText("line0 line1 line2");
			controller.PutCursor(new Place(5, 0), false);
			controller.RemoveWordRight();
			AssertText("line0line1 line2");
			AssertSelection().Both(5, 0).NoNext();
			
			lines.SetText("line0  line1 line2");
			controller.PutCursor(new Place(5, 0), false);
			controller.RemoveWordRight();
			AssertText("line0line1 line2");
			AssertSelection().Both(5, 0).NoNext();
		}
		
		[Test]
		public void MoveLineDown()
		{
			Init();
			lines.SetText("line0\nline1\nline2\nline3\nline4");
			controller.PutCursor(new Place(2, 1), false);
			AssertSelection().Both(2, 1).NoNext();
			
			controller.MoveLineDown();
			AssertText("line0\nline2\nline1\nline3\nline4");
			AssertSelection().Both(2, 2).NoNext();
			
			controller.MoveLineDown();
			AssertText("line0\nline2\nline3\nline1\nline4");
			AssertSelection().Both(2, 3).NoNext();
		}
		
		[Test]
		public void MoveLineUp()
		{
			Init();
			lines.SetText("line0\nline1\nline2\nline3\nline4");
			controller.PutCursor(new Place(2, 3), false);
			AssertSelection().Both(2, 3).NoNext();
			
			controller.MoveLineUp();
			AssertText("line0\nline1\nline3\nline2\nline4");
			AssertSelection().Both(2, 2).NoNext();
			
			controller.MoveLineUp();
			AssertText("line0\nline3\nline1\nline2\nline4");
			AssertSelection().Both(2, 1).NoNext();
		}
		
		[Test]
		public void MoveLineDown_SeveralLines()
		{
			Init();
			lines.SetText("line0\nline1\nline2\nline3\nline4");
			controller.PutCursor(new Place(2, 1), false);
			controller.PutCursor(new Place(2, 2), true);
			AssertSelection().Anchor(2, 1).Caret(2, 2).NoNext();
			
			controller.MoveLineDown();
			AssertText("line0\nline3\nline1\nline2\nline4");
			AssertSelection().Anchor(2, 2).Caret(2, 3).NoNext();
		}
		
		[Test]
		public void MoveLineDown_Constrained_SeveralLines()
		{
			Init();
			lines.SetText("line0\nline1\nline2\nline3\nline4");
			controller.PutCursor(new Place(2, 3), false);
			controller.PutCursor(new Place(2, 4), true);
			AssertSelection().Anchor(2, 3).Caret(2, 4).NoNext();
			
			controller.MoveLineDown();
			AssertText("line0\nline1\nline2\nline3\nline4");
			AssertSelection().Anchor(2, 3).Caret(2, 4).NoNext();
		}
		
		[Test]
		public void MoveLineUp_Constrained_SeveralLines()
		{
			Init();
			lines.SetText("line0\nline1\nline2\nline3\nline4");
			controller.PutCursor(new Place(2, 0), false);
			controller.PutCursor(new Place(2, 1), true);
			AssertSelection().Anchor(2, 0).Caret(2, 1).NoNext();
			
			controller.MoveLineUp();
			AssertText("line0\nline1\nline2\nline3\nline4");
			AssertSelection().Anchor(2, 0).Caret(2, 1).NoNext();
		}
		
		[Test]
		public void MoveLineDown_Multiselection()
		{
			Init();
			lines.SetText("line0\nline1\nline2\nline3\nline4\nline5\nline6\nline7\nline8\nline9\nline10");
			controller.PutCursor(new Place(2, 1), false);
			controller.PutCursor(new Place(2, 2), true);
			controller.PutNewCursor(new Place(2, 4));
			controller.PutCursor(new Place(2, 6), true);
			AssertSelection().Anchor(2, 1).Caret(2, 2).Next().Anchor(2, 4).Caret(2, 6).NoNext();
			//line0\n[line1\nline2\n]line3\n[line4\nline5\nline6]\nline7\nline8\nline9\nline10
			
			controller.MoveLineDown();
			//line0\nline3\n[line1\nline2\n]line7\n[line4\nline5\nline6\n]line8\nline9\nline10
			AssertText("line0\nline3\nline1\nline2\nline7\nline4\nline5\nline6\nline8\nline9\nline10");
			AssertSelection().Anchor(2, 2).Caret(2, 3).Next().Anchor(2, 5).Caret(2, 7).NoNext();
		}
		
		[Test]
		public void MoveLineUp_Multiselection()
		{
			Init();
			lines.SetText("line0\nline3\nline1\nline2\nline7\nline4\nline5\nline6\nline8\nline9\nline10");
			controller.PutCursor(new Place(2, 2), false);
			controller.PutCursor(new Place(2, 3), true);
			controller.PutNewCursor(new Place(2, 5));
			controller.PutCursor(new Place(2, 7), true);
			AssertSelection().Anchor(2, 2).Caret(2, 3).Next().Anchor(2, 5).Caret(2, 7).NoNext();
			
			controller.MoveLineUp();
			AssertText("line0\nline1\nline2\nline3\nline4\nline5\nline6\nline7\nline8\nline9\nline10");
			AssertSelection().Anchor(2, 1).Caret(2, 2).Next().Anchor(2, 4).Caret(2, 6).NoNext();
		}
		
		[Test]
		public void MoveLineDown_Multiselection_Undo()
		{
			Init();
			lines.SetText("line0\nline1\nline2\nline3\nline4\nline5\nline6\nline7\nline8\nline9\nline10");
			controller.PutCursor(new Place(2, 1), false);
			controller.PutCursor(new Place(2, 2), true);
			controller.PutNewCursor(new Place(2, 4));
			controller.PutCursor(new Place(2, 6), true);
			AssertSelection().Anchor(2, 1).Caret(2, 2).Next().Anchor(2, 4).Caret(2, 6).NoNext();
			
			controller.MoveLineDown();
			AssertText("line0\nline3\nline1\nline2\nline7\nline4\nline5\nline6\nline8\nline9\nline10");
			AssertSelection().Anchor(2, 2).Caret(2, 3).Next().Anchor(2, 5).Caret(2, 7).NoNext();
			
			controller.processor.Undo();
			AssertText("line0\nline1\nline2\nline3\nline4\nline5\nline6\nline7\nline8\nline9\nline10");
			AssertSelection().Anchor(2, 1).Caret(2, 2).Next().Anchor(2, 4).Caret(2, 6).NoNext();
			
			controller.processor.Redo();
			AssertText("line0\nline3\nline1\nline2\nline7\nline4\nline5\nline6\nline8\nline9\nline10");
			AssertSelection().Anchor(2, 2).Caret(2, 3).Next().Anchor(2, 5).Caret(2, 7).NoNext();
		}
		
		[Test]
		public void MoveLineUp_Multiselection_Undo()
		{
			Init();
			lines.SetText("line0\nline3\nline1\nline2\nline7\nline4\nline5\nline6\nline8\nline9\nline10");
			controller.PutCursor(new Place(2, 2), false);
			controller.PutCursor(new Place(2, 3), true);
			controller.PutNewCursor(new Place(2, 5));
			controller.PutCursor(new Place(2, 7), true);
			AssertSelection().Anchor(2, 2).Caret(2, 3).Next().Anchor(2, 5).Caret(2, 7).NoNext();
			
			controller.MoveLineUp();
			AssertText("line0\nline1\nline2\nline3\nline4\nline5\nline6\nline7\nline8\nline9\nline10");
			AssertSelection().Anchor(2, 1).Caret(2, 2).Next().Anchor(2, 4).Caret(2, 6).NoNext();
			
			controller.processor.Undo();
			AssertText("line0\nline3\nline1\nline2\nline7\nline4\nline5\nline6\nline8\nline9\nline10");
			AssertSelection().Anchor(2, 2).Caret(2, 3).Next().Anchor(2, 5).Caret(2, 7).NoNext();
			
			controller.processor.Redo();
			AssertText("line0\nline1\nline2\nline3\nline4\nline5\nline6\nline7\nline8\nline9\nline10");
			AssertSelection().Anchor(2, 1).Caret(2, 2).Next().Anchor(2, 4).Caret(2, 6).NoNext();
		}
		
		[Test]
		public void FixLineBreaks()
		{
			Init();
			lines.SetText("line0\nline3\nline1\nline2\r\nline7\nline4\nline5\nline6\nline8\nline9\nline10");
			controller.PutCursor(new Place(2, 2), false);
			controller.PutCursor(new Place(2, 3), true);
			controller.PutNewCursor(new Place(2, 5));
			controller.PutCursor(new Place(2, 7), true);
			controller.Lines.lineBreak = "\r\n";
			
			controller.FixLineBreaks();
			
			AssertText(
				"line0\r\nline3\r\nline1\r\nline2\r\nline7\r\nline4\r\nline5\r\nline6\r\nline8\r\nline9\r\nline10");
			AssertSelection().Anchor(2, 2).Caret(2, 3).Next().Anchor(2, 5).Caret(2, 7).NoNext();
			
			controller.processor.Undo();
			
			AssertText("line0\nline3\nline1\nline2\r\nline7\nline4\nline5\nline6\nline8\nline9\nline10");
			AssertSelection().Anchor(2, 2).Caret(2, 3).Next().Anchor(2, 5).Caret(2, 7).NoNext();
			
			controller.processor.Redo();
			AssertText(
				"line0\r\nline3\r\nline1\r\nline2\r\nline7\r\nline4\r\nline5\r\nline6\r\nline8\r\nline9\r\nline10");
			AssertSelection().Anchor(2, 2).Caret(2, 3).Next().Anchor(2, 5).Caret(2, 7).NoNext();
		}
		
		[Test]
		public void RemoveEmptyOrMinorSelections()
		{
			Init();
			lines.SetText("123456780123456780");
			
			controller.PutCursor(new Place(1, 0), false);
			controller.RemoveEmptyOrMinorSelections();
			AssertSelection().Both(1, 0).NoNext();
			
			controller.PutCursor(new Place(1, 0), false);
			controller.PutCursor(new Place(2, 0), true);
			controller.RemoveEmptyOrMinorSelections();
			AssertSelection().Anchor(1, 0).Caret(2, 0).NoNext();
			
			controller.PutCursor(new Place(1, 0), false);
			controller.PutCursor(new Place(2, 0), true);
			controller.PutNewCursor(new Place(3, 0));
			controller.PutCursor(new Place(4, 0), true);
			AssertSelection().Anchor(1, 0).Caret(2, 0).Next().Anchor(3, 0).Caret(4, 0).NoNext();
			controller.RemoveEmptyOrMinorSelections();
			AssertSelection().Anchor(1, 0).Caret(2, 0).Next().Anchor(3, 0).Caret(4, 0).NoNext();
			
			controller.PutNewCursor(new Place(5, 0));
			AssertSelection().Anchor(1, 0).Caret(2, 0).Next().Anchor(3, 0).Caret(4, 0).Next().Both(5, 0).NoNext();
			controller.RemoveEmptyOrMinorSelections();
			AssertSelection().Anchor(1, 0).Caret(2, 0).Next().Anchor(3, 0).Caret(4, 0).NoNext();
			
			controller.ClearMinorSelections();
			controller.PutCursor(new Place(1, 0), false);
			controller.PutNewCursor(new Place(3, 0));
			controller.PutCursor(new Place(4, 0), true);
			AssertSelection().Both(1, 0).Next().Anchor(3, 0).Caret(4, 0).NoNext();
			controller.RemoveEmptyOrMinorSelections();
			AssertSelection().Anchor(3, 0).Caret(4, 0).NoNext();
			
			controller.ClearMinorSelections();
			controller.PutCursor(new Place(1, 0), false);
			controller.PutNewCursor(new Place(2, 0));
			controller.PutNewCursor(new Place(3, 0));
			AssertSelection().Both(1, 0).Next().Both(2, 0).Next().Both(3, 0).NoNext();
			controller.RemoveEmptyOrMinorSelections();
			AssertSelection().Both(3, 0).NoNext();
		}
	}
}