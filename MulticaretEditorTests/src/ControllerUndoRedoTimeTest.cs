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
			controller.debugNowMilliseconds = 1000;
			controller.InsertText("t");
			controller.debugNowMilliseconds = 1010;
			controller.InsertText("e");
			controller.debugNowMilliseconds = 1020;
			controller.InsertText("x");
			controller.debugNowMilliseconds = 1030;
			controller.InsertText("t");
			controller.debugNowMilliseconds = 1040;
			AssertWithSelection("01text|234567890123456789");
			controller.Undo();
			AssertWithSelection("01|234567890123456789");
		}
		
		[Test]
		public void Test1()
		{
			Init();
			lines.SetText("01234567890123456789");
			controller.PutCursor(new Pos(2, 0), false);
			controller.debugNowMilliseconds = 1000;
			controller.InsertText("t");
			controller.debugNowMilliseconds = 1010;
			controller.InsertText("e");
			controller.debugNowMilliseconds = 2020;
			controller.InsertText("x");
			controller.debugNowMilliseconds = 2030;
			controller.InsertText("t");
			controller.debugNowMilliseconds = 2040;
			AssertWithSelection("01text|234567890123456789");
			controller.Undo();
			controller.debugNowMilliseconds = 2050;
			AssertWithSelection("01te|234567890123456789");
			controller.Undo();
			controller.debugNowMilliseconds = 2060;
			AssertWithSelection("01|234567890123456789");
			controller.Redo();
			controller.debugNowMilliseconds = 2070;
			AssertWithSelection("01te|234567890123456789");
			controller.Redo();
			controller.debugNowMilliseconds = 2080;
			AssertWithSelection("01text|234567890123456789");
		}
		
		[Test]
		public void Test2()
		{
			Init();
			lines.SetText("01234567890123456789");
			controller.PutCursor(new Pos(2, 0), false);
			controller.debugNowMilliseconds = 1000;
			controller.InsertText("t");
			controller.debugNowMilliseconds = 1010;
			controller.InsertText("e");
			controller.debugNowMilliseconds = 2020;
			controller.InsertText("x");
			controller.debugNowMilliseconds = 2030;
			controller.InsertText("t");
			controller.debugNowMilliseconds = 2040;
			controller.PutCursor(new Pos(11, 0), false);
			AssertWithSelection("01text23456|7890123456789");
			controller.InsertText("a");
			controller.debugNowMilliseconds = 2050;
			controller.InsertText("a");
			controller.debugNowMilliseconds = 2060;
			controller.InsertText("a");
			AssertWithSelection("01text23456aaa|7890123456789");
			controller.debugNowMilliseconds = 2070;
			controller.Undo();
			AssertWithSelection("01text23456|7890123456789");
			controller.Undo();
			controller.debugNowMilliseconds = 2080;
			AssertWithSelection("01te|234567890123456789");
			controller.Undo();
			controller.debugNowMilliseconds = 2090;
			AssertWithSelection("01|234567890123456789");
			controller.Redo();
			controller.debugNowMilliseconds = 2100;
			AssertWithSelection("01te|234567890123456789");
			controller.Redo();
			controller.debugNowMilliseconds = 2110;
			AssertWithSelection("01text|234567890123456789");
			controller.Redo();
			controller.debugNowMilliseconds = 2110;
			AssertWithSelection("01text23456aaa|7890123456789");
		}
		
		[Test]
		public void Test3()
		{
			Init();
			lines.SetText("01234567890123456789");
			controller.PutCursor(new Pos(2, 0), false);
			controller.debugNowMilliseconds = 1000;
			controller.InsertText("t");
			controller.debugNowMilliseconds = 1010;
			controller.InsertText("e");
			controller.debugNowMilliseconds = 2020;
			controller.InsertText("x");
			controller.debugNowMilliseconds = 2030;
			controller.InsertText("t");
			controller.debugNowMilliseconds = 2040;
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertWithSelection("01text23456|7890123456789");
			controller.InsertText("a");
			controller.debugNowMilliseconds = 2050;
			controller.InsertText("a");
			controller.debugNowMilliseconds = 2060;
			controller.InsertText("a");
			AssertWithSelection("01text23456aaa|7890123456789");
			controller.debugNowMilliseconds = 2070;
			controller.Undo();
			AssertWithSelection("01text23456|7890123456789");
			controller.Undo();
			controller.debugNowMilliseconds = 2080;
			AssertWithSelection("01te|234567890123456789");
			controller.Undo();
			controller.debugNowMilliseconds = 2090;
			AssertWithSelection("01|234567890123456789");
			controller.Redo();
			controller.debugNowMilliseconds = 2100;
			AssertWithSelection("01te|234567890123456789");
			controller.Redo();
			controller.debugNowMilliseconds = 2110;
			AssertWithSelection("01text|234567890123456789");
			controller.Redo();
			controller.debugNowMilliseconds = 2110;
			AssertWithSelection("01text23456aaa|7890123456789");
		}
	}
}