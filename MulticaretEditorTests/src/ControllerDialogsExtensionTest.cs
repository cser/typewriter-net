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
			Init(10);
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
			Init(10);
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
			Init(10);
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
			Init(10);
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
			Init(10);
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
			Init(10);
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
			Init(10);
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
			Init(10);
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
			Init(10);
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
			Init(10);
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
			Init(10);
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
			Init(10);
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
	}
}