using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class WordWrapValidatorTest : ControllerTestBase
	{
		override protected void Init()
		{
			Init(4);
			lines.tabSize = 4;
			lines.wordWrap = true;
		}
		
		[Test]
		public void WWPosOfIndex_Simple()
		{
			Init();
			
			lines.SetText("line0\nline1 text\r\nline2");
			lines.wwValidator.Validate(6);
			
			// line0N
			// line1 textRN
			// line2
			
			Assert.AreEqual(new Pos(0, 0), lines[0].WWPosOfIndex(0));
			Assert.AreEqual(new Pos(1, 0), lines[0].WWPosOfIndex(1));
			
			Assert.AreEqual(new Pos(5, 0), lines[1].WWPosOfIndex(5));
			Assert.AreEqual(new Pos(0, 1), lines[1].WWPosOfIndex(6));
			Assert.AreEqual(new Pos(1, 1), lines[1].WWPosOfIndex(7));
		}
		
		[Test]
		public void WWPosOfIndex_Tabbed()
		{
			Init();
			
			lines.SetText("    line1 text\r\n\tline2 text");
			lines.wwValidator.Validate(10);
			
			//     line1 |
			//     textRN
			//     line2 |
			//     text
			
			Assert.AreEqual(new Pos(0, 0), lines[0].WWPosOfIndex(0));
			Assert.AreEqual(new Pos(1, 0), lines[0].WWPosOfIndex(1));
			Assert.AreEqual(new Pos(9, 0), lines[0].WWPosOfIndex(9));
			Assert.AreEqual(new Pos(4, 1), lines[0].WWPosOfIndex(10));
			Assert.AreEqual(new Pos(5, 1), lines[0].WWPosOfIndex(11));
			
			Assert.AreEqual(new Pos(0, 0), lines[1].WWPosOfIndex(0));
			Assert.AreEqual(new Pos(4, 0), lines[1].WWPosOfIndex(1));
			Assert.AreEqual(new Pos(4, 1), lines[1].WWPosOfIndex(7));
		}
		
		[Test]
		public void GetLineIndexOfWW_Simple()
		{
			Init();
			
			lines.SetText("    line1 text\r\n\tline2 text");
			lines.wwValidator.Validate(10);
			
			//     line1 |
			//     textRN
			//     line2 |
			//     text
			
			Assert.AreEqual(10, lines.wwSizeX);
			Assert.AreEqual(4, lines.wwSizeY);
			
			Assert.AreEqual(new LineIndex(0, 0), lines.wwValidator.GetLineIndexOfWW(0));
			Assert.AreEqual(new LineIndex(0, 1), lines.wwValidator.GetLineIndexOfWW(1));
			Assert.AreEqual(new LineIndex(1, 0), lines.wwValidator.GetLineIndexOfWW(2));
			Assert.AreEqual(new LineIndex(1, 1), lines.wwValidator.GetLineIndexOfWW(3));
		}
		
		[Test]
		public void GetLineIndexOfWW_SeveralBlocks()
		{
			Init();
			
			lines.SetText("t0\nt1\nt2\n    line1 text\r\n\tline2 text");
			lines.wwValidator.Validate(10);
			Assert.AreEqual(10, lines.wwSizeX);
			Assert.AreEqual(7, lines.wwSizeY);
			
			// t0
			// t1
			// t2
			//     line1 |
			//     textRN
			//     line2 |
			//     text
			
			Assert.AreEqual(new LineIndex(0, 0), lines.wwValidator.GetLineIndexOfWW(0));
			Assert.AreEqual(new LineIndex(1, 0), lines.wwValidator.GetLineIndexOfWW(1));
			Assert.AreEqual(new LineIndex(2, 0), lines.wwValidator.GetLineIndexOfWW(2));
			Assert.AreEqual(new LineIndex(3, 0), lines.wwValidator.GetLineIndexOfWW(3));
			Assert.AreEqual(new LineIndex(3, 1), lines.wwValidator.GetLineIndexOfWW(4));
			Assert.AreEqual(new LineIndex(4, 0), lines.wwValidator.GetLineIndexOfWW(5));
			Assert.AreEqual(new LineIndex(4, 1), lines.wwValidator.GetLineIndexOfWW(6));
		}
		
		[Test]
		public void MoveUpDown_SeveralCarets()
		{
			Init();
			
			lines.SetText("aaaaaaaa\nbbbbb\ncccccccc\n    line1 text\r\n\tline2 text");
			lines.wwValidator.Validate(10);
			Assert.AreEqual(10, lines.wwSizeX);
			Assert.AreEqual(7, lines.wwSizeY);
			
			// aaaaaaaa
			// bbbbb
			// cccccccc
			//     line1 |
			//     textRN
			//     line2 |
			//     text
			
			controller.PutCursor(new Place(1, 2), false);
			controller.PutNewCursor(new Place(5, 3));
			AssertSelection().Both(1, 2).Next().Both(5, 3).NoNext();
			
			controller.MoveUp(false);
			AssertSelection().Both(1, 1).Next().Both(5, 2).NoNext();
			
			controller.MoveDown(false);
			AssertSelection().Both(1, 2).Next().Both(5, 3).NoNext();
			
			controller.MoveDown(false);
			AssertSelection().Both(1, 3).Next().Both(2, 4).NoNext();			
		}
		
		[Test]
		public void MoveUpDown_OneCaret()
		{
			Init();
			
			lines.SetText("aaaaaaaa\nbbbbb\ncccccccc\n    line1 text\r\n\tline2 text");
			lines.wwValidator.Validate(10);
			Assert.AreEqual(10, lines.wwSizeX);
			Assert.AreEqual(7, lines.wwSizeY);
			
			// aaaaaaaa
			// bbbbb
			// cccccccc
			//     line1 |
			//     textRN
			//     line2 |
			//     text
			
			controller.PutCursor(new Place(5, 3), false);
			AssertSelection().Both(5, 3).NoNext();
			
			controller.MoveUp(false);
			AssertSelection().Both(5, 2).NoNext();
			
			controller.MoveDown(false);
			AssertSelection().Both(5, 3).NoNext();
			
			controller.MoveDown(false);
			AssertSelection().Both(11, 3).NoNext();
			
			controller.MoveUp(false);
			AssertSelection().Both(5, 3).NoNext();
		}
		
		[Test]
		public void MoveUpDown_PreferredPos()
		{
			Init();
			
			lines.SetText("aaaaaaaa\nbbbbb\ncccccccc\n    line1 text\r\n\tline2 text");
			lines.wwValidator.Validate(10);
			Assert.AreEqual(10, lines.wwSizeX);
			Assert.AreEqual(7, lines.wwSizeY);
			
			// aaaaaaaa
			// bbbbb
			// cccccccc
			//     line1 |
			//     textRN
			//     line2 |
			//     text
			
			controller.PutCursor(new Place(6, 3), false);
			AssertSelection().Both(6, 3).NoNext();
			controller.MoveUp(false);
			AssertSelection().Both(6, 2).NoNext();			
			controller.MoveUp(false);
			AssertSelection().Both(5, 1).NoNext();
			controller.MoveDown(false);
			AssertSelection().Both(6, 2).NoNext();
			controller.MoveDown(false);
			AssertSelection().Both(6, 3).NoNext();
		}
		
		[Test]
		public void LineIndexOfWW_WithoutValidation()
		{
			Init();
			
			lines.SetText("aaaaaaaa\nbbbbb\ncccccccc\n    line1 text\r\n\tline2 text");
			Assert.AreEqual(new LineIndex(0, 0), lines.wwValidator.GetLineIndexOfWW(0));
		}
	}
}
