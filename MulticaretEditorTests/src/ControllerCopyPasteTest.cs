using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class ControllerCopyPasteTest : ControllerTestBase
	{
		private const string SimpleText =
			"Du\n" +
			"Du hast\n" +
			"Du hast mich";
		//	0123456789012
		
		[Test]
		public void ClipboardPutToGetFrom()
		{
			ClipboardExecuter.PutToClipboard("Test");
			Assert.AreEqual("Test", ClipboardExecuter.GetFromClipboard());
			
			ClipboardExecuter.PutToClipboard("??????? ?????");
			Assert.AreEqual("??????? ?????", ClipboardExecuter.GetFromClipboard());
			
			ClipboardExecuter.PutToClipboard("Multiline\ntext");
			Assert.AreEqual("Multiline\ntext", ClipboardExecuter.GetFromClipboard());
		}
		
		[Test]
		public void PutEmptyToClipboard()
		{
			ClipboardExecuter.PutToClipboard("-");
			Assert.AreEqual("-", ClipboardExecuter.GetFromClipboard());
			ClipboardExecuter.PutToClipboard("");
			Assert.AreEqual("-", ClipboardExecuter.GetFromClipboard());
		}
		
		[Test]
		public void Copy()
		{
			Init();
			lines.SetText(SimpleText);
			ClipboardExecuter.PutToClipboard("-");
			
			controller.PutCursor(new Pos(3, 2), false);
			controller.PutCursor(new Pos(7, 2), true);
			AssertSelection().Anchor(3, 2).Caret(7, 2);
			controller.Copy();
			Assert.AreEqual("hast", ClipboardExecuter.GetFromClipboard());
			
			controller.PutCursor(new Pos(0, 0), false);
			controller.PutCursor(new Pos(12, 2), true);
			controller.Copy();
			Assert.AreEqual(SimpleText, ClipboardExecuter.GetFromClipboard());
			
			controller.PutCursor(new Pos(0, 0), false);
			controller.PutCursor(new Pos(7, 1), true);
			controller.Copy();
			Assert.AreEqual("Du\nDu hast", ClipboardExecuter.GetFromClipboard());
			
			controller.PutCursor(new Pos(0, 0), false);
			controller.PutCursor(new Pos(0, 2), true);
			controller.Copy();
			Assert.AreEqual("Du\nDu hast\n", ClipboardExecuter.GetFromClipboard());
		}
		
		[Test]
		public void Paste()
		{
			Init();
			lines.SetText(SimpleText);
			
			ClipboardExecuter.PutToClipboard("!");
			AssertSelection().Both(0, 0);
			controller.Paste();
			Assert.AreEqual("!Du\n", GetLineText(0));
			AssertSelection().Both(1, 0);
			
			ClipboardExecuter.PutToClipboard("text");
			controller.PutCursor(new Pos(3, 1), false);
			AssertSelection().Both(3, 1);
			controller.Paste();
			Assert.AreEqual("Du texthast\n", GetLineText(1));
			AssertSelection().Both(7, 1);
		}
		
		[Test]
		public void Paste_Multiline()
		{
			Init();
			lines.SetText(SimpleText);
			
			AssertSize().XY(12, 3);
			
			ClipboardExecuter.PutToClipboard("line0\nline00\nline000");
			controller.PutCursor(new Pos(3, 1), false);
			AssertSelection().Both(3, 1);
			controller.Paste();
			/*
			Du
			Du line0
			line00
			line000hast
			Du hast mich
			*/
			AssertSelection().Both(7, 3);
			Assert.AreEqual("Du\n", GetLineText(0));
			Assert.AreEqual("Du line0\n", GetLineText(1));
			Assert.AreEqual("line00\n", GetLineText(2));
			Assert.AreEqual("line000hast\n", GetLineText(3));
			Assert.AreEqual("Du hast mich", GetLineText(4));
			
			AssertSize().XY(12, 5);
			
			ClipboardExecuter.PutToClipboard("text\n");
			controller.PutCursor(new Pos(0, 1), false);
			AssertSelection().Both(0, 1);
			controller.Paste();
			Assert.AreEqual("Du\n", GetLineText(0));
			Assert.AreEqual("text\n", GetLineText(1));
			Assert.AreEqual("Du line0\n", GetLineText(2));
			Assert.AreEqual("line00\n", GetLineText(3));
		}
		
		[Test]
		public void CopyPast_Multiline_EuqualCount()
		{
			Init();
			lines.SetText(
				"text-\n" +
				"-text1--\n" +
				"--end-"
			);
			controller.PutCursor(new Pos(0, 0), false);
			controller.PutCursor(new Pos(4, 0), true);
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutCursor(new Pos(6, 1), true);
			controller.PutNewCursor(new Pos(2, 2));
			controller.PutCursor(new Pos(5, 2), true);
			AssertSelection().Anchor(0, 0).Caret(4, 0).Next().Anchor(1, 1).Caret(6, 1).Next().Anchor(2, 2).Caret(5, 2).NoNext();
			
			controller.Copy();
			Assert.AreEqual("text\ntext1\nend", ClipboardExecuter.GetFromClipboard());
			
			//te|xt-
			//-text1[--]
			//-|-end-
			controller.ClearMinorSelections();
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(6, 1));
			controller.PutCursor(new Pos(8, 1), true);
			controller.PutNewCursor(new Pos(1, 2));
			AssertSelection().Both(2, 0).Next().Anchor(6, 1).Caret(8, 1).Next().Both(1, 2).NoNext();
			
			//tetext|xt-
			//-text1text1|
			//-end|-end-
			controller.Paste();
			Assert.AreEqual("tetextxt-\n", GetLineText(0));
			Assert.AreEqual("-text1text1\n", GetLineText(1));
			Assert.AreEqual("-end-end-", GetLineText(2));
			Assert.AreEqual(3, lines.LinesCount);
			AssertSelection().Both(6, 0).Next().Both(11, 1).Next().Both(4, 2).NoNext();
		}
		
		[Test]
		public void CopyPast_Multiline_EuqualCount_Order()
		{
			Init();
			lines.SetText(
				"text-\n" +
				"-text1--\n" +
				"--end-"
			);
			controller.PutCursor(new Pos(0, 0), false);
			controller.PutCursor(new Pos(4, 0), true);
			controller.PutNewCursor(new Pos(2, 2));
			controller.PutCursor(new Pos(5, 2), true);
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutCursor(new Pos(6, 1), true);
			AssertSelection().Anchor(0, 0).Caret(4, 0).Next().Anchor(2, 2).Caret(5, 2).Next().Anchor(1, 1).Caret(6, 1).NoNext();
			
			controller.Copy();
			Assert.AreEqual("text\ntext1\nend", ClipboardExecuter.GetFromClipboard());
			
			//te|xt-
			//-text1[--]
			//-|-end-
			controller.ClearMinorSelections();
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(1, 2));
			controller.PutNewCursor(new Pos(6, 1));
			controller.PutCursor(new Pos(8, 1), true);
			AssertSelection().Both(2, 0).Next().Both(1, 2).Next().Anchor(6, 1).Caret(8, 1).NoNext();
			
			//tetext|xt-
			//-text1text1|
			//-end|-end-
			controller.Paste();
			Assert.AreEqual("tetextxt-\n", GetLineText(0));
			Assert.AreEqual("-text1text1\n", GetLineText(1));
			Assert.AreEqual("-end-end-", GetLineText(2));
			Assert.AreEqual(3, lines.LinesCount);
			AssertSelection().Both(6, 0).Next().Both(4, 2).Next().Both(11, 1).NoNext();
		}
		
		[Test]
		public void CopyPast_Multiline_EuqualCount_PreferredPos()
		{
			Init();
			lines.SetText(
				"text-\n" +
				"-text1--\n" +
				"--end-"
			);
			controller.PutCursor(new Pos(0, 0), false);
			controller.PutCursor(new Pos(4, 0), true);
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutCursor(new Pos(6, 1), true);
			controller.PutNewCursor(new Pos(2, 2));
			controller.PutCursor(new Pos(5, 2), true);
			AssertSelection().Anchor(0, 0).Caret(4, 0).Next().Anchor(1, 1).Caret(6, 1).Next().Anchor(2, 2).Caret(5, 2).NoNext();
			
			controller.Copy();
			Assert.AreEqual("text\ntext1\nend", ClipboardExecuter.GetFromClipboard());
			
			//te|xt-
			//-text1[--]
			//-|-end-
			controller.ClearMinorSelections();
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(6, 1));
			controller.PutCursor(new Pos(8, 1), true);
			controller.PutNewCursor(new Pos(1, 2));
			AssertSelection().Both(2, 0).Next().Anchor(6, 1).Caret(8, 1).Next().Both(1, 2).NoNext();
			
			//tetext|xt-
			//-text1text1|
			//-end|-end-
			controller.Paste();
			Assert.AreEqual("tetextxt-\n", GetLineText(0));
			Assert.AreEqual("-text1text1\n", GetLineText(1));
			Assert.AreEqual("-end-end-", GetLineText(2));
			Assert.AreEqual(3, lines.LinesCount);
			AssertSelection().Both(6, 0).Next().Both(11, 1).Next().Both(4, 2).NoNext();
			controller.MoveDown(false);
			AssertSelection().Both(6, 1);
		}
		
		[Test]
		public void Past_Multiline_DifferentCount()
		{
			Init();
			lines.SetText(SimpleText);
			
			ClipboardExecuter.PutToClipboard("text\ntext1\nend");
			
			//Du
			//Du h[as]t
			//Du| hast mich
			controller.ClearMinorSelections();
			controller.PutCursor(new Pos(4, 1), false);
			controller.PutCursor(new Pos(6, 1), true);
			controller.PutNewCursor(new Pos(2, 2));
			AssertSelection().Anchor(4, 1).Caret(6, 1).Next().Both(2, 2).NoNext();
			
			controller.Paste();
			Assert.AreEqual("Du\n", GetLineText(0));
			Assert.AreEqual("Du htext\n", GetLineText(1));
			Assert.AreEqual("text1\n", GetLineText(2));
			Assert.AreEqual("endt\n", GetLineText(3));
			Assert.AreEqual("Dutext\n", GetLineText(4));
			Assert.AreEqual("text1\n", GetLineText(5));
			Assert.AreEqual("end hast mich", GetLineText(6));
			Assert.AreEqual(7, lines.LinesCount);
			AssertSelection().Both(3, 3).Next().Both(3, 6).NoNext();
		}
		
		[Test]
		public void CopyPaste_RN0()
		{
			Init();
			lines.SetText("Du\r\nDu hast\nDu hast\rDu hast mich");
			//D[u
			//Du hast
			//Du hast
			//Du ]hast mich
			controller.ClearMinorSelections();
			controller.PutCursor(new Pos(1, 0), false);
			controller.PutCursor(new Pos(3, 3), true);
			AssertSelection().Anchor(1, 0).Caret(3, 3).NoNext();
			
			controller.Copy();
			Assert.AreEqual("u\r\nDu hast\nDu hast\rDu ", ClipboardExecuter.GetFromClipboard());
			Assert.AreEqual("Du\r\nDu hast\nDu hast\rDu hast mich", lines.GetText());
			AssertSelection().Anchor(1, 0).Caret(3, 3).NoNext();
			                
			controller.EraseSelection();
			Assert.AreEqual("Dhast mich", lines.GetText());
			AssertSelection().Both(1, 0).NoNext();
			
			controller.Paste();
			Assert.AreEqual("Du\r\nDu hast\nDu hast\rDu hast mich", lines.GetText());
		}
		
		[Test]
		public void CopyPaste_RN1()
		{
			Init();
			lines.SetText("Du\r\nDu hast\nDu hast\rDu hast mich");
			//D|u
			//Du| hast
			//Du |hast
			//Du h|ast mich
			controller.PutCursor(new Pos(1, 0), false);
			controller.PutNewCursor(new Pos(2, 1));
			controller.PutNewCursor(new Pos(3, 2));
			controller.PutNewCursor(new Pos(4, 3));
			AssertSelection().Both(1, 0).Next().Both(2, 1).Next().Both(3, 2).Next().Both(4, 3).NoNext();
			
			ClipboardExecuter.PutToClipboard("text0\r\ntext1\ntext2\rtext3");
			
			controller.Paste();
			Assert.AreEqual("Dtext0u\r\nDutext1 hast\nDu text2hast\rDu htext3ast mich", lines.GetText());
		}
		
		[Test]
		public void Paste_SelectionMastBeMergedBefore()
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
			ClipboardExecuter.PutToClipboard("text");
			
			controller.Paste();
			Assert.AreEqual("12text1234567890123", lines.GetText());
			AssertSelection().Both(6, 0).NoNext();
		}
	}
}