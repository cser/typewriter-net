using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class ControllerDialogExtensionTest : ControllerTestBase
	{
		[Test]
		public void FindNext1()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.PutCursor(new Place(6, 0), false);
			AssertSelection().Both(6, 0).NoNext();
			
			controller.DialogsExtension.FindNext("ccccc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Anchor(9, 0).Caret(14, 0).NoNext();
			
			controller.DialogsExtension.FindNext("ccccc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Anchor(9, 1).Caret(14, 1).NoNext();
			
			controller.DialogsExtension.FindNext("ccccc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Anchor(9, 0).Caret(14, 0).NoNext();
		}
		
		[Test]
		public void FindNext1_NoMatches()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.PutCursor(new Place(6, 0), false);
			AssertSelection().Both(6, 0).NoNext();
			
			controller.DialogsExtension.FindNext("ccccce", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Both(6, 0).NoNext();
		}
		
		[Test]
		public void FindNext1_CloseMatches()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			
			controller.PutCursor(new Place(8, 0), false);			
			AssertSelection().Both(8, 0).NoNext();
			controller.DialogsExtension.FindNext("cc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Anchor(9, 0).Caret(11, 0).NoNext();
			
			controller.PutCursor(new Place(9, 0), false);			
			AssertSelection().Both(9, 0).NoNext();
			controller.DialogsExtension.FindNext("cc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Anchor(9, 0).Caret(11, 0).NoNext();
			
			controller.PutCursor(new Place(10, 0), false);			
			AssertSelection().Both(10, 0).NoNext();
			controller.DialogsExtension.FindNext("cc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Anchor(10, 0).Caret(12, 0).NoNext();
			controller.DialogsExtension.FindNext("cc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Anchor(12, 0).Caret(14, 0).NoNext();
		}
		
		[Test]
		public void FindNext2()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.PutCursor(new Place(6, 0), false);
			AssertSelection().Both(6, 0).NoNext();
			
			controller.DialogsExtension.FindNext("cc\neee", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Anchor(12, 0).Caret(3, 1).NoNext();
			
			controller.DialogsExtension.FindNext("cc\neee", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Anchor(12, 0).Caret(3, 1).NoNext();
		}
		
		[Test]
		public void SelectNextFound1()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.PutCursor(new Place(6, 0), false);
			AssertSelection().Both(6, 0).NoNext();
			
			controller.DialogsExtension.SelectNextFound("ccccc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Both(6, 0).Next()
				.Anchor(9, 0).Caret(14, 0).NoNext();
			
			controller.DialogsExtension.SelectNextFound("ccccc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Both(6, 0).Next()
				.Anchor(9, 0).Caret(14, 0).Next()
				.Anchor(9, 1).Caret(14, 1).NoNext();
			
			//TODO How mast it work?
			//controller.DialogsExtension.SelectNextFound("ccccc", false, false);
			//Assert.IsNull(controller.DialogsExtension.NeedShowError);
			//AssertSelection().Anchor(9, 0).Caret(14, 0).NoNext();
		}
		
		[Test]
		public void SelectNextFound1_NoMatches()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.PutCursor(new Place(6, 0), false);
			AssertSelection().Both(6, 0).NoNext();
			
			controller.DialogsExtension.SelectNextFound("ccccce", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Both(6, 0).NoNext();
		}
		
		[Test]
		public void SelectNextFound1_CloseMatches()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			
			controller.PutCursor(new Place(10, 0), false);			
			AssertSelection().Both(10, 0).NoNext();
			controller.DialogsExtension.SelectNextFound("cc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Both(10, 0).Next()
				.Anchor(10, 0).Caret(12, 0).NoNext();
			controller.DialogsExtension.SelectNextFound("cc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Both(10, 0).Next()
				.Anchor(10, 0).Caret(12, 0).Next()
				.Anchor(12, 0).Caret(14, 0).NoNext();
		}
		
		[Test]
		public void SelectAllFound()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.PutCursor(new Place(6, 0), false);
			AssertSelection().Both(6, 0).NoNext();
			
			controller.DialogsExtension.SelectAllFound("ccccc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection()
				.Anchor(9, 0).Caret(14, 0).Next()
				.Anchor(9, 1).Caret(14, 1).NoNext();
		}
		
		[Test]
		public void SelectAllFound_NoMatches()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.PutCursor(new Place(6, 0), false);
			AssertSelection().Both(6, 0).NoNext();
			
			controller.DialogsExtension.SelectAllFound("eeeee", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Both(6, 0).NoNext();
		}
		
		[Test]
		public void SelectAllFound_InsideSelection()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			
			controller.PutCursor(new Place(14, 0), false);
			controller.PutCursor(new Place(16, 1), true);
			AssertSelection().Anchor(14, 0).Caret(16, 1).NoNext();
			
			controller.DialogsExtension.SelectAllFound("ccccc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Anchor(9, 1).Caret(14, 1).NoNext();
			
			controller.PutCursor(new Place(9, 0), false);
			controller.PutCursor(new Place(16, 1), true);
			AssertSelection().Anchor(9, 0).Caret(16, 1).NoNext();
			
			controller.DialogsExtension.SelectAllFound("ccccc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection()
				.Anchor(9, 0).Caret(14, 0).Next()
				.Anchor(9, 1).Caret(14, 1).NoNext();
		}
		
		[Test]
		public void SelectAllFound_InsideSelection_NotMatched()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			
			controller.PutCursor(new Place(9, 1), false);
			controller.PutCursor(new Place(13, 1), true);
			AssertSelection().Anchor(9, 1).Caret(13, 1).NoNext();
			
			controller.DialogsExtension.SelectAllFound("ccccc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Anchor(9, 1).Caret(13, 1).NoNext();
		}
		
		[Test]
		public void SelectAllFound_IfEqualSelectionThenMatchesAsNotInside()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			
			controller.PutCursor(new Place(9, 1), false);
			controller.PutCursor(new Place(14, 1), true);
			AssertSelection().Anchor(9, 1).Caret(14, 1).NoNext();
			
			controller.DialogsExtension.SelectAllFound("ccccc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection()
				.Anchor(9, 0).Caret(14, 0).Next()
				.Anchor(9, 1).Caret(14, 1).NoNext();
		}
		
		[Test]
		public void Replace()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.PutCursor(new Place(5, 0), false);
			controller.DialogsExtension.FindNext("ccccc", false, false);
			AssertSelection().Anchor(9, 0).Caret(14, 0).NoNext();
			
			controller.DialogsExtension.Replace("ccccc", "CC", false, false, false);
			AssertText("aaa bbbb CC\neeee aaa ccccc fff");
			AssertSelection().Anchor(9, 1).Caret(14, 1).NoNext();
			
			controller.DialogsExtension.Replace("ccccc", "CC", false, false, false);
			AssertText("aaa bbbb CC\neeee aaa CC fff");
			AssertSelection().Both(11, 1).NoNext();
			
			controller.DialogsExtension.Replace("ccccc", "CC", false, false, false);
			AssertText("aaa bbbb CC\neeee aaa CC fff");
			AssertSelection().Both(11, 1).NoNext();
		}
		
		[Test]
		public void Replace_Cyclic()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.PutCursor(new Place(5, 1), false);
			controller.DialogsExtension.FindNext("ccccc", false, false);
			AssertSelection().Anchor(9, 1).Caret(14, 1).NoNext();
			
			controller.DialogsExtension.Replace("ccccc", "CC", false, false, false);
			AssertText("aaa bbbb ccccc\neeee aaa CC fff");
			AssertSelection().Anchor(9, 0).Caret(14, 0).NoNext();
			
			controller.DialogsExtension.Replace("ccccc", "CC", false, false, false);
			AssertText("aaa bbbb CC\neeee aaa CC fff");
			AssertSelection().Both(11, 0).NoNext();
		}
		
		[Test]
		public void Replace_IfSelectionIsEmpty_JustSelectNext()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.PutCursor(new Place(5, 0), false);
			
			controller.DialogsExtension.Replace("ccccc", "CC", false, false, false);
			AssertText("aaa bbbb ccccc\neeee aaa ccccc fff");
			
			AssertSelection().Anchor(9, 0).Caret(14, 0).NoNext();
		}
		
		[Test]
		public void ReplaceAll()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.PutCursor(new Place(5, 0), false);
			
			controller.DialogsExtension.ReplaceAll("ccccc", "CC", false, false, false);
			AssertText("aaa bbbb CC\neeee aaa CC fff");
			
			AssertSelection().Both(11, 1).NoNext();
		}
		
		[Test]
		public void ReplaceAll_Undo()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.PutCursor(new Place(5, 0), false);
			
			controller.DialogsExtension.ReplaceAll("ccccc", "CC", false, false, false);
			AssertText("aaa bbbb CC\neeee aaa CC fff");
			AssertSelection().Both(11, 1).NoNext();
			
			controller.processor.Undo();
			AssertText("aaa bbbb ccccc\neeee aaa ccccc fff");
			AssertSelection().Both(5, 0).NoNext();
			
			controller.processor.Redo();
			AssertText("aaa bbbb CC\neeee aaa CC fff");
			AssertSelection().Both(11, 1).NoNext();
		}
		
		[Test]
		public void ReplaceAll_DontFreezeOnEqualReplace()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.PutCursor(new Place(5, 0), false);
			
			controller.DialogsExtension.ReplaceAll("ccccc", "ccccc", false, false, false);
			AssertText("aaa bbbb ccccc\neeee aaa ccccc fff");
		}
		
		[Test]
		public void ReplaceAll_ToEmpty()
		{
			Init();
			lines.SetText("aaaaaaa");
			controller.PutCursor(new Place(5, 0), false);
			
			controller.DialogsExtension.ReplaceAll("a", "", false, false, false);
			AssertText("");
			AssertSelection().Both(0, 0).NoNext();
		}
		
		[Test]
		public void ReplaceAll_InsideSelection()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			
			controller.PutCursor(new Place(14, 0), false);
			controller.PutCursor(new Place(16, 1), true);
			AssertSelection().Anchor(14, 0).Caret(16, 1).NoNext();
			
			controller.DialogsExtension.ReplaceAll("ccccc", "CC", false, false, false);
			AssertText("aaa bbbb ccccc\neeee aaa CC fff");
			
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			
			controller.PutCursor(new Place(9, 0), false);
			controller.PutCursor(new Place(16, 1), true);
			AssertSelection().Anchor(9, 0).Caret(16, 1).NoNext();
			
			controller.DialogsExtension.ReplaceAll("ccccc", "CC", false, false, false);
			AssertText("aaa bbbb CC\neeee aaa CC fff");
		}
		
		[Test]
		public void ReplaceAll_Escape()
		{
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.DialogsExtension.ReplaceAll("ccccc", "C\\tC", false, false, true);
			AssertText("aaa bbbb C\tC\neeee aaa C\tC fff");
			
			Init();
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.DialogsExtension.ReplaceAll("ccccc", "C\\tC", false, false, false);
			AssertText("aaa bbbb C\\tC\neeee aaa C\\tC fff");
		}
		
		[Test]
		public void Issue_with_replace_text_N6()
		{
			Init();
			lines.SetText(
				"test\n" +
				"test\n" +
				"test\n" +
				"test\n" +
				"test\n" +
				"test\n" +
				"test\n" +
				"test\n" +
				"test");
			controller.PutCursor(new Place(0, 4), false);
			controller.SelectNextText();
			controller.SelectNextText();
			controller.SelectNextText();
			AssertSelection()
				.Anchor(0, 4).Caret(4, 4).Next()
				.Anchor(0, 5).Caret(4, 5).Next()
				.Anchor(0, 6).Caret(4, 6).NoNext();
			controller.DialogsExtension.Replace("test", "pass", false, false, false);
			controller.DialogsExtension.Replace("test", "pass", false, false, false);
			controller.DialogsExtension.Replace("test", "pass", false, false, false);
			controller.DialogsExtension.Replace("test", "pass", false, false, false);
			controller.DialogsExtension.Replace("test", "pass", false, false, false);
			controller.DialogsExtension.Replace("test", "pass", false, false, false);
			controller.DialogsExtension.Replace("test", "pass", false, false, false);
			AssertText(
				"pass\n" +
				"pass\n" +
				"pass\n" +
				"pass\n" +
				"pass\n" +
				"pass\n" +
				"pass\n" +
				"pass\n" +
				"pass");
			//Made despite the fact that Sublime Text replace only last selection in contract to this behaviour
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void Replace_InsideLineBreak1(bool isIgnoreCase)
		{
			Init();
			lines.SetText("abc\r\ndef");
			Assert.AreEqual("abc\r\n", lines[0].Text);
			Assert.AreEqual("def", lines[1].Text);
			controller.PutCursor(new Place(1, 0), false);
			
			controller.DialogsExtension.FindNext("\r\n", false, isIgnoreCase);
			AssertSelection().Anchor(3, 0).Caret(0, 1).NoNext();
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void Replace_InsideLineBreak2(bool isIgnoreCase)
		{
			Init();
			lines.SetText("abc\r\ndef");
			Assert.AreEqual("abc\r\n", lines[0].Text);
			Assert.AreEqual("def", lines[1].Text);
			controller.PutCursor(new Place(1, 0), false);
			controller.DialogsExtension.FindNext("\r", false, isIgnoreCase);
			AssertSelection().Anchor(3, 0).Caret(4, 0).NoNext();
			
			controller.DialogsExtension.Replace("\r", "\n", false, isIgnoreCase, false);
			AssertText("abc\n\ndef");
			Assert.AreEqual("abc\n", lines[0].Text);
			Assert.AreEqual("\n", lines[1].Text);
			Assert.AreEqual("def", lines[2].Text);
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void SelectAllFound_InsideLineBreak(bool isIgnoreCase)
		{
			Init();
			//             012345 6789 0 12345678 901
			lines.SetText("aaa aa\njaa\r\njabj aa\njj");
			Assert.AreEqual("aaa aa\n", lines[0].Text);
			Assert.AreEqual("jaa\r\n", lines[1].Text);
			Assert.AreEqual("jabj aa\n", lines[2].Text);
			controller.PutCursor(new Place(1, 0), false);
			AssertSelection().Both(1, 0).NoNext();
			Assert.AreEqual("jj", lines[3].Text);
			
			controller.DialogsExtension.SelectAllFound("aa\nj", false, isIgnoreCase);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().AbsoluteAnchorCaret(4, 8).Next().AbsoluteAnchorCaret(17, 21).NoNext();
			
			controller.ClearMinorSelections();
			controller.PutCursor(new Place(1, 0), false);
			controller.DialogsExtension.SelectAllFound("\n", false, isIgnoreCase);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection()
				.AbsoluteAnchorCaret(6, 7).Next()
				.AbsoluteAnchorCaret(11, 12).Next()
				.AbsoluteAnchorCaret(19, 20).NoNext();
			
			controller.ClearMinorSelections();
			controller.PutCursor(new Place(1, 0), false);
			controller.DialogsExtension.SelectAllFound("\nj", false, isIgnoreCase);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection()
				.AbsoluteAnchorCaret(6, 8).Next()
				.AbsoluteAnchorCaret(11, 13).Next()
				.AbsoluteAnchorCaret(19, 21).NoNext();
		}
		
		[Test]
		public void EnterText_InsideBreak_Undo()
		{
			Init();
			lines.SetText("rh\r\nsf");
			controller.PutCursor(new Place(1, 0), false);
			controller.DialogsExtension.FindNext("\n", false, false);
			AssertSelection().AbsoluteAnchorCaret(3, 4).NoNext();
			controller.InsertText("GS");
			AssertText("rh\rGSsf");
			Assert.AreEqual("rh\r", lines[0].Text);
			Assert.AreEqual("GSsf", lines[1].Text);
			
			controller.processor.Undo();
			AssertText("rh\r\nsf");
			Assert.AreEqual("rh\r\n", lines[0].Text);
			Assert.AreEqual("sf", lines[1].Text);
		}
	}
}