using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class ControllerUndoRedoTimeTest : ControllerTestBase
	{
		private void AssertWithSelection(string text)
		{
			Assert.AreEqual(text.Replace("|", ""), lines.GetText());
			AssertSelection().Both(text.IndexOf("|"), 0).NoNext();
		}
		
		[Test]
		public void Test0()
		{
			Init();
			lines.SetText("01234567890123456789");
			controller.PutCursor(new Pos(2, 0), false);
			controller.processor.debugNowMilliseconds = 1000;
			controller.InsertText("t");
			controller.processor.debugNowMilliseconds = 1010;
			controller.InsertText("e");
			controller.processor.debugNowMilliseconds = 1020;
			controller.InsertText("x");
			controller.processor.debugNowMilliseconds = 1030;
			controller.InsertText("t");
			controller.processor.debugNowMilliseconds = 1040;
			AssertWithSelection("01text|234567890123456789");
			controller.processor.Undo();
			AssertWithSelection("01|234567890123456789");
		}
		
		[Test]
		public void Test1()
		{
			Init();
			lines.SetText("01234567890123456789");
			controller.PutCursor(new Pos(2, 0), false);
			controller.processor.debugNowMilliseconds = 1000;
			controller.InsertText("t");
			controller.processor.debugNowMilliseconds = 1010;
			controller.InsertText("e");
			controller.processor.debugNowMilliseconds = 2020;
			controller.InsertText("x");
			controller.processor.debugNowMilliseconds = 2030;
			controller.InsertText("t");
			controller.processor.debugNowMilliseconds = 2040;
			AssertWithSelection("01text|234567890123456789");
			controller.processor.Undo();
			controller.processor.debugNowMilliseconds = 2050;
			AssertWithSelection("01te|234567890123456789");
			controller.processor.Undo();
			controller.processor.debugNowMilliseconds = 2060;
			AssertWithSelection("01|234567890123456789");
			controller.processor.Redo();
			controller.processor.debugNowMilliseconds = 2070;
			AssertWithSelection("01te|234567890123456789");
			controller.processor.Redo();
			controller.processor.debugNowMilliseconds = 2080;
			AssertWithSelection("01text|234567890123456789");
		}
		
		[Test]
		public void Test2()
		{
			Init();
			lines.SetText("01234567890123456789");
			controller.PutCursor(new Pos(2, 0), false);
			controller.processor.debugNowMilliseconds = 1000;
			controller.InsertText("t");
			controller.processor.debugNowMilliseconds = 1010;
			controller.InsertText("e");
			controller.processor.debugNowMilliseconds = 2020;
			controller.InsertText("x");
			controller.processor.debugNowMilliseconds = 2030;
			controller.InsertText("t");
			controller.processor.debugNowMilliseconds = 2040;
			controller.PutCursor(new Pos(11, 0), false);
			AssertWithSelection("01text23456|7890123456789");
			controller.InsertText("a");
			controller.processor.debugNowMilliseconds = 2050;
			controller.InsertText("a");
			controller.processor.debugNowMilliseconds = 2060;
			controller.InsertText("a");
			AssertWithSelection("01text23456aaa|7890123456789");
			controller.processor.debugNowMilliseconds = 2070;
			controller.processor.Undo();
			AssertWithSelection("01text23456|7890123456789");
			controller.processor.Undo();
			controller.processor.debugNowMilliseconds = 2080;
			AssertWithSelection("01te|234567890123456789");
			controller.processor.Undo();
			controller.processor.debugNowMilliseconds = 2090;
			AssertWithSelection("01|234567890123456789");
			controller.processor.Redo();
			controller.processor.debugNowMilliseconds = 2100;
			AssertWithSelection("01te|234567890123456789");
			controller.processor.Redo();
			controller.processor.debugNowMilliseconds = 2110;
			AssertWithSelection("01text|234567890123456789");
			controller.processor.Redo();
			controller.processor.debugNowMilliseconds = 2110;
			AssertWithSelection("01text23456aaa|7890123456789");
		}
		
		[Test]
		public void Test3()
		{
			Init();
			lines.SetText("01234567890123456789");
			controller.PutCursor(new Pos(2, 0), false);
			controller.processor.debugNowMilliseconds = 1000;
			controller.InsertText("t");
			controller.processor.debugNowMilliseconds = 1010;
			controller.InsertText("e");
			controller.processor.debugNowMilliseconds = 2020;
			controller.InsertText("x");
			controller.processor.debugNowMilliseconds = 2030;
			controller.InsertText("t");
			controller.processor.debugNowMilliseconds = 2040;
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertWithSelection("01text23456|7890123456789");
			controller.InsertText("a");
			controller.processor.debugNowMilliseconds = 2050;
			controller.InsertText("a");
			controller.processor.debugNowMilliseconds = 2060;
			controller.InsertText("a");
			AssertWithSelection("01text23456aaa|7890123456789");
			controller.processor.debugNowMilliseconds = 2070;
			controller.processor.Undo();
			AssertWithSelection("01text23456|7890123456789");
			controller.processor.Undo();
			controller.processor.debugNowMilliseconds = 2080;
			AssertWithSelection("01te|234567890123456789");
			controller.processor.Undo();
			controller.processor.debugNowMilliseconds = 2090;
			AssertWithSelection("01|234567890123456789");
			controller.processor.Redo();
			controller.processor.debugNowMilliseconds = 2100;
			AssertWithSelection("01te|234567890123456789");
			controller.processor.Redo();
			controller.processor.debugNowMilliseconds = 2110;
			AssertWithSelection("01text|234567890123456789");
			controller.processor.Redo();
			controller.processor.debugNowMilliseconds = 2110;
			AssertWithSelection("01text23456aaa|7890123456789");
		}
		
		[Test]
		public void Test4()
		{
			Init();
			lines.SetText("01234567890123456789");
			controller.PutCursor(new Pos(2, 0), false);
			controller.processor.debugNowMilliseconds = 1000;
			controller.InsertText("t");
			controller.processor.debugNowMilliseconds = 1010;
			controller.MoveRight(false);
			controller.processor.debugNowMilliseconds = 1020;
			controller.InsertText("e");
			controller.processor.debugNowMilliseconds = 2030;
			AssertWithSelection("01t2e|34567890123456789");
			controller.processor.debugNowMilliseconds = 2040;
			controller.processor.Undo();
			AssertWithSelection("01t2|34567890123456789");
			controller.processor.debugNowMilliseconds = 2050;
			controller.processor.Undo();
			AssertWithSelection("01|234567890123456789");
			controller.processor.debugNowMilliseconds = 2060;
			controller.processor.Redo();
			AssertWithSelection("01t|234567890123456789");
			controller.processor.debugNowMilliseconds = 2070;
			controller.processor.Redo();
			AssertWithSelection("01t2e|34567890123456789");
		}
	}
}