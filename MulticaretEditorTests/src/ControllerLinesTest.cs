using System;
using NUnit.Framework;
using MulticaretEditor;
using MulticaretEditor.Commands;

namespace UnitTests
{
	[TestFixture]
	public class ControllerLinesTest : ControllerTestBase
	{
		[Test]
		public void Copy()
		{
			Init();
			lines.lineBreak = "\n";
			lines.SetText("abcd\n  EFGHI\r\n1234");
			ClipboardExecuter.PutToClipboard("-");
			
			controller.PutCursor(new Place(1, 0), false);
			controller.PutNewCursor(new Place(3, 0));
			controller.PutNewCursor(new Place(2, 1));
			AssertSelection().Both(1, 0).Next().Both(3, 0).Next().Both(2, 1).NoNext();
			controller.Copy();
			Assert.AreEqual("abcd\n  EFGHI\r\n", ClipboardExecuter.GetFromClipboard());
			AssertSelection().Both(1, 0).Next().Both(3, 0).Next().Both(2, 1).NoNext();
		}
		
		[Test]
		public void Copy_LastLine()
		{
			Init();
			lines.lineBreak = "\r";
			lines.SetText("abcd\n  EFGHI\r\n1234");
			ClipboardExecuter.PutToClipboard("-");
			
			controller.PutCursor(new Place(1, 2), false);
			AssertSelection().Both(1, 2).NoNext();
			controller.Copy();
			Assert.AreEqual("1234\r", ClipboardExecuter.GetFromClipboard());
			AssertSelection().Both(1, 2).NoNext();
		}
		
		[Test]
		public void Copy_WordWrap()
		{
			Init();
			lines.SetText("abcd\n  word1 word2 word3\r\n1234");
			lines.wordWrap = true;
			lines.wwValidator.Validate(10);
			Assert.AreEqual(5, lines.wwSizeY);
			ClipboardExecuter.PutToClipboard("-");
			
			controller.PutCursor(new Place(4, 1), false);
			AssertSelection().Both(4, 1).NoNext();
			controller.Copy();
			Assert.AreEqual("  word1 word2 word3\r\n", ClipboardExecuter.GetFromClipboard());
			AssertSelection().Both(4, 1).NoNext();
		}
		
		[Test]
		public void Cut()
		{
			Init();
			lines.lineBreak = "\n";
			lines.SetText("abcd\n  EFGHI\r\n1234");
			ClipboardExecuter.PutToClipboard("-");
			
			controller.PutCursor(new Place(1, 0), false);
			controller.PutNewCursor(new Place(3, 0));
			controller.PutNewCursor(new Place(2, 1));
			AssertSelection().Both(1, 0).Next().Both(3, 0).Next().Both(2, 1).NoNext();
			controller.Cut();
			Assert.AreEqual("abcd\n  EFGHI\r\n", ClipboardExecuter.GetFromClipboard());
			AssertText("1234");
			AssertSelection().Both(0, 0).NoNext();
			
			controller.Undo();
			AssertText("abcd\n  EFGHI\r\n1234");
			AssertSelection().Both(1, 0).Next().Both(3, 0).Next().Both(2, 1).NoNext();
			
			controller.Redo();
			AssertText("1234");
			AssertSelection().Both(0, 0).NoNext();
		}
		
		[Test]
		public void CONTROVERSIAL_CuttingOfLastLineDoesNotChangeLinesCount_AsInSublime()
		{
			Init();
			lines.lineBreak = "\r";
			lines.SetText("abcd\n  ABCDEF\r\n1234");
			controller.PutCursor(new Place(1, 2), false);
			AssertSelection().Both(1, 2).NoNext();
			
			controller.Cut();
			Assert.AreEqual("1234\r", ClipboardExecuter.GetFromClipboard());
			AssertText("abcd\n  ABCDEF\r\n");
			AssertSelection().Both(0, 2).NoNext();
			
			controller.Undo();
			AssertText("abcd\n  ABCDEF\r\n1234");
			AssertSelection().Both(1, 2).NoNext();
		}
		
		public class TestCommand : Command
		{
			public TestCommand() : base(new CommandType(false))
			{
			}
			
			public System.Collections.Generic.List<SimpleRange> TestGet()
			{
				return GetLineRangesByCarets(GetSelectionMementos());
			}
			
			public override void Redo()
			{
			}
			
			public override void Undo()
			{
			}
		}
		
		private void AssertCommandRanges(string expected)
		{
			TestCommand command = new TestCommand();
			command.lines = lines;
			command.selections = lines.selections;
			CollectionAssert.AreEqual(
				expected,
				ListUtil.ToString(command.TestGet(), StringOfSimpleRange));
		}
		
		private static string StringOfSimpleRange(SimpleRange range)
		{
			return range.index + "/" + range.count;
		}
		
		[Test]
		public void Cut_CommandRanges1()
		{
			Init();
			lines.SetText("abcd\n  EFGHI\r\n1234");
			controller.PutCursor(new Place(1, 0), false);
			AssertCommandRanges("[0/1]");
		}
		
		[Test]
		public void Cut_CommandRanges2()
		{
			Init();
			lines.SetText("abcd\n  EFGHI\r\n1234");
			controller.PutCursor(new Place(1, 0), false);
			controller.PutNewCursor(new Place(1, 1));
			AssertCommandRanges("[0/2]");
		}
		
		[Test]
		public void Cut_CommandRanges3()
		{
			Init();
			lines.SetText("abcd\n  EFGHI\r\n1234");
			controller.PutCursor(new Place(1, 0), false);
			controller.PutNewCursor(new Place(3, 0));
			controller.PutNewCursor(new Place(2, 1));
			AssertSelection().Both(1, 0).Next().Both(3, 0).Next().Both(2, 1).NoNext();
			AssertCommandRanges("[0/2]");
		}
		
		[Test]
		public void Cut_CommandRanges4()
		{
			Init();
			lines.SetText("line0\nline1\nline2\nline3\nline4\nline5");
			controller.PutCursor(new Place(1, 0), false);
			controller.PutNewCursor(new Place(1, 1));
			controller.PutNewCursor(new Place(1, 3));
			controller.PutNewCursor(new Place(1, 5));
			AssertSelection().Both(1, 0).Next().Both(1, 1).Next().Both(1, 3).Next().Both(1, 5).NoNext();
			AssertCommandRanges("[0/2, 3/1, 5/1]");
		}
	}
}