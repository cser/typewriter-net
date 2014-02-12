using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class ControllerMultieditTest : ControllerTestBase
	{
		private const string SimpleText =
			"Du\n" +
			"Du hast\n" +
			"Du hast mich";
		//	0123456789012
		
		[Test]
		public void PutNewCursor()
		{
			Init();
			lines.SetText(SimpleText);
			
			controller.PutCursor(new Pos(3, 1), false);
			AssertSelection().Both(3, 1);
			controller.PutNewCursor(new Pos(7, 2));
			AssertSelection().Both(3, 1).Next().Both(7, 2).NoNext();
			
			controller.PutCursor(new Pos(12, 2), true);
			AssertSelection().Both(3, 1).Next().Anchor(7, 2).Caret(12, 2).NoNext();
		}
		
		[Test]
		public void PutNewCursor_InExistentSelection()
		{
			Init();
			lines.SetText(SimpleText);
			
			controller.PutCursor(new Pos(3, 1), false);
			AssertSelection().Both(3, 1);
			controller.PutNewCursor(new Pos(3, 1));
			AssertSelection().Both(3, 1).NoNext();
			
			controller.PutCursor(new Pos(12, 2), true);
			AssertSelection().Anchor(3, 1).Caret(12, 2).NoNext();
		}
		
		[Test]
		public void ClearMinorSelections0()
		{
			Init();
			lines.SetText(SimpleText);
			
			controller.PutCursor(new Pos(3, 1), false);
			controller.PutNewCursor(new Pos(7, 2));
			AssertSelection().Both(3, 1).Next().Both(7, 2).NoNext();
			
			controller.ClearMinorSelections();
			AssertSelection().Both(3, 1).NoNext();
		}
		
		[Test]
		public void ClearMinorSelections1()
		{
			Init();
			lines.SetText(SimpleText);
			
			controller.PutCursor(new Pos(3, 1), false);
			controller.PutNewCursor(new Pos(7, 2));
			controller.PutCursor(new Pos(12, 2), true);
			AssertSelection().Both(3, 1).Next().Anchor(7, 2).Caret(12, 2).NoNext();
			
			controller.ClearMinorSelections();
			AssertSelection().Both(3, 1).NoNext();
		}
		
		[Test]
		public void InsertChar()
		{
			Init();
			lines.SetText("1234567890");
			
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(7, 0));
			AssertSelection().Both(2, 0).Next().Both(7, 0).NoNext();
			
			controller.InsertText("?");
			AssertSelection().Both(3, 0).Next().Both(9, 0).NoNext();
			Assert.AreEqual("12?34567?890", GetLineText(0));
		}
		
		[Test]
		public void Backspace()
		{
			Init();
			lines.SetText("1234567890");
			
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(7, 0));
			AssertSelection().Both(2, 0).Next().Both(7, 0).NoNext();
			
			controller.Backspace();
			AssertSelection().Both(1, 0).Next().Both(5, 0).NoNext();
			Assert.AreEqual("13456890", GetLineText(0));
		}
		
		[Test]
		public void Delete()
		{
			Init();
			lines.SetText("1234567890");
			
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(7, 0));
			AssertSelection().Both(2, 0).Next().Both(7, 0).NoNext();
			
			controller.Delete();
			AssertSelection().Both(2, 0).Next().Both(6, 0).NoNext();
			Assert.AreEqual("12456790", GetLineText(0));
		}
		
		[Test]
		public void InsertChar_Multiline()
		{
			Init();
			lines.SetText(SimpleText);
			
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutNewCursor(new Pos(6, 1));
			AssertSelection().Both(2, 0).Next().Both(1, 1).Next().Both(6, 1).NoNext();
			
			controller.InsertText("?");
			AssertSelection().Both(3, 0).Next().Both(2, 1).Next().Both(8, 1).NoNext();
			Assert.AreEqual("Du?\n", GetLineText(0));
			Assert.AreEqual("D?u has?t\n", GetLineText(1));
			Assert.AreEqual("Du hast mich", GetLineText(2));
			
			controller.InsertText("\n");
			AssertSelection().Both(0, 1).Next().Both(0, 3).Next().Both(0, 4).NoNext();
			Assert.AreEqual("Du?\n", GetLineText(0));
			Assert.AreEqual("\n", GetLineText(1));
			Assert.AreEqual("D?\n", GetLineText(2));
			Assert.AreEqual("u has?\n", GetLineText(3));
			Assert.AreEqual("t\n", GetLineText(4));
			Assert.AreEqual("Du hast mich", GetLineText(5));
		}
		
		[Test]
		public void Backspace_Multiline0()
		{
			Init();
			lines.SetText(SimpleText);
			
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutNewCursor(new Pos(6, 1));
			AssertSelection().Both(2, 0).Next().Both(1, 1).Next().Both(6, 1).NoNext();
			
			controller.Backspace();
			AssertSelection().Both(1, 0).Next().Both(0, 1).Next().Both(4, 1).NoNext();
			Assert.AreEqual("D\n", GetLineText(0));
			Assert.AreEqual("u hat\n", GetLineText(1));
			Assert.AreEqual("Du hast mich", GetLineText(2));
		}
		
		[Test]
		public void Backspace_Multiline1()
		{
			Init();
			lines.SetText(SimpleText);
			
			controller.PutCursor(new Pos(0, 1), false);
			controller.PutNewCursor(new Pos(3, 1));
			controller.PutNewCursor(new Pos(0, 2));
			AssertSelection().Both(0, 1).Next().Both(3, 1).Next().Both(0, 2).NoNext();
			
			//Du
			//|Du |hast
			//|Du hast mich
			
			//Du|Du|hast|Du hast mich
			
			controller.Backspace();
			AssertSelection().Both(2, 0).Next().Both(4, 0).Next().Both(8, 0).NoNext();
			Assert.AreEqual("DuDuhastDu hast mich", GetLineText(0));
			Assert.AreEqual(1, lines.LinesCount);
		}
		
		[Test]
		public void Delete_Multiline0()
		{
			Init();
			lines.SetText(SimpleText);
			
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutNewCursor(new Pos(6, 1));
			AssertSelection().Both(2, 0).Next().Both(1, 1).Next().Both(6, 1).NoNext();
			
			controller.Delete();
			AssertSelection().Both(2, 0).Next().Both(3, 0).Next().Both(7, 0).NoNext();
			Assert.AreEqual("DuD has\n", GetLineText(0));
			Assert.AreEqual("Du hast mich", GetLineText(1));
		}
		
		[Test]
		public void EraseSelection_Single()
		{
			Init();
			lines.SetText("12345678");
			
			controller.PutCursor(new Pos(1, 0), false);
			controller.PutCursor(new Pos(3, 0), true);
			
			controller.PutNewCursor(new Pos(5, 0));
			controller.PutCursor(new Pos(7, 0), true);
			
			AssertSelection().Anchor(1, 0).Caret(3, 0).Next().Anchor(5, 0).Caret(7, 0).NoNext();
			
			controller.EraseSelection();
			
			Assert.AreEqual("1458", GetLineText(0));
			AssertSelection().Both(1, 0).Next().Both(3, 0).NoNext();
		}
		
		[Test]
		public void EraseSelection_Complex()
		{
			Init();
			lines.SetText(SimpleText);
			
			controller.PutCursor(new Pos(1, 1), false);
			controller.PutCursor(new Pos(5, 1), true);
			
			controller.PutNewCursor(new Pos(6, 1));
			controller.PutCursor(new Pos(7, 1), true);
			
			controller.PutNewCursor(new Pos(2, 2));
			controller.PutCursor(new Pos(4, 2), true);
			
			controller.PutNewCursor(new Pos(6, 2));
			controller.PutCursor(new Pos(5, 2), true);
			
			controller.PutNewCursor(new Pos(10, 2));
			
			AssertSelection().Anchor(1, 1).Caret(5, 1)
				.Next().Anchor(6, 1).Caret(7, 1)
				.Next().Anchor(2, 2).Caret(4, 2)
				.Next().Anchor(6, 2).Caret(5, 2)
				.Next().Both(10, 2).NoNext();
			
			//Du
			//D[u ha]|s[t]|
			//Du[ h]|a|[s]t mi|ch
			
			//Du
			//D|s|
			//Du|a|t mi|ch
			
			controller.EraseSelection();
			AssertSelection().Both(1, 1).Next().Both(2, 1).Next().Both(2, 2).Next().Both(3, 2).Next().Both(7, 2).NoNext();
			Assert.AreEqual("Du\n", GetLineText(0));
			Assert.AreEqual("Ds\n", GetLineText(1));
			Assert.AreEqual("Duat mich", GetLineText(2));
		}
		
		[Test]
		public void EraseSelection_Multiline()
		{
			Init();
			lines.SetText(SimpleText);
			
			controller.PutCursor(new Pos(1, 0), false);
			controller.PutCursor(new Pos(2, 2), true);
			
			controller.PutNewCursor(new Pos(7, 2));
			controller.PutCursor(new Pos(5, 2), true);
			
			AssertSelection().Anchor(1, 0).Caret(2, 2).Next().Anchor(7, 2).Caret(5, 2).NoNext();
			
			//D[u
			//Du hast
			//Du]| ha|[st] mich
			
			//D| ha| mich
			
			controller.EraseSelection();
			
			Assert.AreEqual("D ha mich", GetLineText(0));
			AssertSelection().Both(1, 0).Next().Both(4, 0).NoNext();
			Assert.AreEqual(1, lines.LinesCount);
		}
		
		[Test]
		public void EraseSelection_Contact()
		{
			Init();
			lines.SetText("12345678");
			
			controller.PutCursor(new Pos(3, 0), false);
			controller.PutCursor(new Pos(7, 0), true);
			
			controller.PutNewCursor(new Pos(1, 0));
			controller.PutCursor(new Pos(3, 0), true);
			
			AssertSelection().Anchor(3, 0).Caret(7, 0).Next().Anchor(1, 0).Caret(3, 0).NoNext();
			
			controller.EraseSelection();
			
			Assert.AreEqual("18", GetLineText(0));
			AssertSelection().Both(1, 0).NoNext();
		}
		
		[Test]
		public void EraseSelection_Multiline_Contact()
		{
			Init();
			lines.SetText(SimpleText);
			
			controller.PutCursor(new Pos(1, 0), false);
			controller.PutCursor(new Pos(2, 2), true);
			
			controller.PutNewCursor(new Pos(7, 2));
			controller.PutCursor(new Pos(2, 2), true);
			
			controller.PutNewCursor(new Pos(11, 2));
			
			controller.PutNewCursor(new Pos(12, 2));
			
			AssertSelection()
				.Anchor(1, 0).Caret(2, 2).Next()
				.Anchor(7, 2).Caret(2, 2).Next()
				.Both(11, 2).Next()
				.Both(12, 2).NoNext();
			
			//D[u
			//Du hast
			//Du]||[ hast] mic|h|
			
			controller.EraseSelection();
			
			Assert.AreEqual("D mich", GetLineText(0));
			AssertSelection().Both(1, 0).Next().Both(5, 0).Next().Both(6, 0).NoNext();
			Assert.AreEqual(1, lines.LinesCount);
		}
		
		[Test]
		public void InsertChar_Contact0()
		{
			Init();
			lines.SetText("12345678");
			
			controller.PutCursor(new Pos(3, 0), false);
			controller.PutCursor(new Pos(7, 0), true);
			
			controller.PutNewCursor(new Pos(1, 0));
			controller.PutCursor(new Pos(3, 0), true);
			
			AssertSelection().Anchor(3, 0).Caret(7, 0).Next().Anchor(1, 0).Caret(3, 0).NoNext();
			
			controller.InsertText("!");
			
			Assert.AreEqual("1!!8", GetLineText(0));
			AssertSelection().Count(2).HasBoth(2, 0).HasBoth(3, 0);
		}
		
		[Test]
		public void InsertChar_Contact1()
		{
			Init();
			lines.SetText("12345678");
			
			controller.PutCursor(new Pos(7, 0), false);
			controller.PutCursor(new Pos(3, 0), true);
			
			controller.PutNewCursor(new Pos(1, 0));
			controller.PutCursor(new Pos(3, 0), true);
			
			AssertSelection().Anchor(7, 0).Caret(3, 0).Next().Anchor(1, 0).Caret(3, 0).NoNext();
			
			controller.InsertText("!");
			
			Assert.AreEqual("1!!8", GetLineText(0));
			AssertSelection().Count(2).HasBoth(2, 0).HasBoth(3, 0);
		}
		
		[Test]
		public void InsertChar_Contact2()
		{
			Init();
			lines.SetText("Slkjflsjdf sldfaj lasdjf lsd fl Ssdfldsf sdfsdf sdfsdf sdfsdfsdf sdfsdfsfsdfsf");
			
			controller.PutCursor(new Pos(11, 0), false);
			controller.PutCursor(new Pos(17, 0), true);
			
			controller.PutNewCursor(new Pos(28, 0));
			controller.PutCursor(new Pos(17, 0), true);
			
			AssertSelection().Anchor(11, 0).Caret(17, 0).Next().Anchor(28, 0).Caret(17, 0).NoNext();
			
			//Slkjflsjdf [sldfaj]||[ lasdjf lsd] fl Ssdfldsf sdfsdf sdfsdf sdfsdfsdf sdfsdfsfsdfsf
			controller.InsertText("!");
			
			Assert.AreEqual("Slkjflsjdf !! fl Ssdfldsf sdfsdf sdfsdf sdfsdfsdf sdfsdfsfsdfsf", GetLineText(0));
			AssertSelection().Count(2).HasBoth(12, 0).HasBoth(13, 0);
		}
		
		[Test]
		public void InsertChar_Multiline_Contact()
		{
			Init();
			lines.SetText(SimpleText);
			
			controller.PutCursor(new Pos(1, 0), false);
			controller.PutCursor(new Pos(2, 2), true);
			
			controller.PutNewCursor(new Pos(7, 2));
			controller.PutCursor(new Pos(2, 2), true);
			
			controller.PutNewCursor(new Pos(11, 2));
			
			controller.PutNewCursor(new Pos(12, 2));
			
			AssertSelection()
				.Anchor(1, 0).Caret(2, 2).Next()
				.Anchor(7, 2).Caret(2, 2).Next()
				.Both(11, 2).Next()
				.Both(12, 2).NoNext();
			
			//D[u
			//Du hast
			//Du]||[ hast] mic|h|
			
			controller.InsertText("!");
			
			Assert.AreEqual("D!! mic!h!", GetLineText(0));
			Assert.AreEqual(1, lines.LinesCount);
			AssertSelection().Both(2, 0).Next().Both(3, 0).Next().Both(8, 0).Next().Both(10, 0).NoNext();
		}
		
		[Test]
		public void Backspace_MergeSelections0()
		{
			Init();
			lines.SetText(SimpleText);
			
			controller.PutCursor(new Pos(1, 0), false);
			controller.PutNewCursor(new Pos(0, 1));
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutNewCursor(new Pos(4, 2));
			controller.PutNewCursor(new Pos(5, 2));
			controller.PutNewCursor(new Pos(6, 2));
			controller.PutNewCursor(new Pos(11, 2));
			controller.PutNewCursor(new Pos(12, 2));
			
			//D|uN
			//|D|u hastN
			//Du h|a|s|t mic|h|
			
			//|u| hastN
			//Du |t mi|
			
			controller.Backspace();
			
			Assert.AreEqual("uu hast\n", GetLineText(0));
			Assert.AreEqual("Du t mi", GetLineText(1));
			Assert.AreEqual(2, lines.LinesCount);
			AssertSelection().Both(0, 0).Next().Both(1, 0).Next().Both(3, 1).Next().Both(7, 1).NoNext();
		}
		
		[Test]
		public void Backspace_EraseSelections_NothingHappensIfSelectionsIsNotEmpty()
		{
			Init();
			lines.SetText(SimpleText);
			
			controller.PutCursor(new Pos(0, 1), false);
			controller.PutCursor(new Pos(1, 1), true);
			
			controller.PutNewCursor(new Pos(5, 1));
			controller.PutCursor(new Pos(6, 1), true);
			
			controller.PutNewCursor(new Pos(4, 2));
			controller.PutCursor(new Pos(7, 2), true);
			
			controller.PutNewCursor(new Pos(10, 2));
			
			//Du
			//[D]u ha[s]t
			//Du h[ast] mi|ch
			
			controller.Backspace();
			
			Assert.AreEqual("Du\n", GetLineText(0));
			Assert.AreEqual("Du hast\n", GetLineText(1));
			Assert.AreEqual("Du hast mich", GetLineText(2));
			Assert.AreEqual(3, lines.LinesCount);
			AssertSelection().Anchor(0, 1).Caret(1, 1).Next().Anchor(5, 1).Caret(6, 1).Next().Anchor(4, 2).Caret(7, 2).Next().Both(10, 2).NoNext();
		}
		
		[Test]
		public void Delete_EraseSelections_NothingHappensIfSelectionsIsNotEmpty()
		{
			Init();
			lines.SetText(SimpleText);
			
			controller.PutCursor(new Pos(0, 1), false);
			controller.PutCursor(new Pos(1, 1), true);
			controller.PutNewCursor(new Pos(5, 1));
			controller.PutCursor(new Pos(6, 1), true);
			controller.PutNewCursor(new Pos(4, 2));
			controller.PutCursor(new Pos(7, 2), true);
			controller.PutNewCursor(new Pos(10, 2));
			//Du
			//[D]u ha[s]t
			//Du h[ast] mi|ch
			AssertSelection().Anchor(0, 1).Caret(1, 1).Next().Anchor(5, 1).Caret(6, 1).Next().Anchor(4, 2).Caret(7, 2).Next().Both(10, 2).NoNext();
			
			controller.Delete();
			Assert.AreEqual("Du\n", GetLineText(0));
			Assert.AreEqual("Du hast\n", GetLineText(1));
			Assert.AreEqual("Du hast mich", GetLineText(2));
			Assert.AreEqual(3, lines.LinesCount);
			AssertSelection().Anchor(0, 1).Caret(1, 1).Next().Anchor(5, 1).Caret(6, 1).Next().Anchor(4, 2).Caret(7, 2).Next().Both(10, 2).NoNext();
		}
		
		[Test]
		public void InsertText_SelectionMastBeMergedBefore()
		{
			Init();
			//                  |  |
			lines.SetText("12345678901234567890123");
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(5, 0));
			AssertSelection().Both(2, 0).Next().Both(5, 0).NoNext();
			controller.MoveRight(true);
			controller.MoveRight(true);
			controller.MoveRight(true);
			controller.MoveRight(true);
			controller.MoveRight(true);
			AssertSelection().Anchor(2, 0).Caret(7, 0).Next().Anchor(5, 0).Caret(10, 0).NoNext();
			
			controller.InsertText("text");
			
			Assert.AreEqual("12text1234567890123", lines.GetText());
			AssertSelection().Both(6, 0).NoNext();
		}
		
		[Test]
		public void EraseSelection_SelectionMastBeMergedBefore()
		{
			Init();
			//                  |  |
			lines.SetText("12345678901234567890123");
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(5, 0));
			AssertSelection().Both(2, 0).Next().Both(5, 0).NoNext();
			controller.MoveRight(true);
			controller.MoveRight(true);
			controller.MoveRight(true);
			controller.MoveRight(true);
			controller.MoveRight(true);
			AssertSelection().Anchor(2, 0).Caret(7, 0).Next().Anchor(5, 0).Caret(10, 0).NoNext();
			
			controller.EraseSelection();
			
			Assert.AreEqual("121234567890123", lines.GetText());
			AssertSelection().Both(2, 0).NoNext();
		}
		
		[Test]
		public void Delete_SelectionMastBeMergedBefore()
		{
			Init();
			//                  |  |
			lines.SetText("12345678901234567890123");
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(5, 0));
			AssertSelection().Both(2, 0).Next().Both(5, 0).NoNext();
			controller.MoveLeft(false);
			controller.MoveLeft(false);
			controller.MoveLeft(false);
			controller.MoveLeft(false);
			controller.MoveLeft(false);
			controller.MoveLeft(false);
			
			controller.Delete();
			
			Assert.AreEqual("2345678901234567890123", lines.GetText());
			AssertSelection().Both(0, 0).NoNext();
		}
		
		[Test]
		public void Backspace_SelectionMastBeMergedBefore()
		{
			Init();
			//                  |  |
			lines.SetText("1234567");
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(5, 0));
			AssertSelection().Both(2, 0).Next().Both(5, 0).NoNext();
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			
			controller.Backspace();
			
			Assert.AreEqual("123456", lines.GetText());
			AssertSelection().Both(6, 0).NoNext();
		}
	}
}