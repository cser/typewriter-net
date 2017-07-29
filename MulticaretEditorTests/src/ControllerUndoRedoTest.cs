using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class ControllerUndoRedoTest : ControllerTestBase
	{
		[Test]
		public void InsertText_EmptySelection()
		{
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			// Du|\n
			// D|u has|t\n
			// Du hast mich
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutNewCursor(new Pos(6, 1));
			AssertSelection().Both(2, 0).Next().Both(1, 1).Next().Both(6, 1).NoNext();
			controller.InsertText("text");
			// Dutext|\n
			// Dtext|u hastext|t\n
			// Du hast mich
			Assert.AreEqual("Dutext\nDtextu hastextt\nDu hast mich", lines.GetText());
			AssertSelection().Both(6, 0).Next().Both(5, 1).Next().Both(14, 1).NoNext();
			
			controller.processor.Undo();
			lines.SetText("Du\nDu hast\nDu hast mich");
			AssertSelection().Both(2, 0).Next().Both(1, 1).Next().Both(6, 1).NoNext();
			
			controller.processor.Redo();
			Assert.AreEqual("Dutext\nDtextu hastextt\nDu hast mich", lines.GetText());
			AssertSelection().Both(6, 0).Next().Both(5, 1).Next().Both(14, 1).NoNext();
		}
		
		[Test]
		public void InsertText_NonEmptySelection()
		{
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			// Du|\n
			// D[u ]has[t\n
			// D]u hast mich
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutCursor(new Pos(3, 1), true);
			controller.PutNewCursor(new Pos(6, 1));
			controller.PutCursor(new Pos(1, 2), true);
			AssertSelection().Both(2, 0).Next().Anchor(1, 1).Caret(3, 1).Next().Anchor(6, 1).Caret(1, 2).NoNext();
			controller.InsertText("text");
			// Dutext|\n
			// Dtext|hastext|u hast mich
			Assert.AreEqual("Dutext\nDtexthastextu hast mich", lines.GetText());
			AssertSelection().Both(6, 0).Next().Both(5, 1).Next().Both(12, 1).NoNext();
			
			controller.processor.Undo();
			// Du|\n
			// Du |hast\n
			// D|u hast mich
			lines.SetText("Du\nDu hast\nDu hast mich");
			AssertSelection().Both(2, 0).Next().Anchor(1, 1).Caret(3, 1).Next().Anchor(6, 1).Caret(1, 2).NoNext();
			
			controller.processor.Redo();
			Assert.AreEqual("Dutext\nDtexthastextu hast mich", lines.GetText());
			AssertSelection().Both(6, 0).Next().Both(5, 1).Next().Both(12, 1).NoNext();
			
			controller.ClearMinorSelections();
			controller.processor.Undo();
			lines.SetText("Du\nDu hast\nDu hast mich");
			AssertSelection().Both(2, 0).Next().Anchor(1, 1).Caret(3, 1).Next().Anchor(6, 1).Caret(1, 2).NoNext();
		}
		
		[Test]
		public void InsertText_Joining()
		{
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			// Du|\n
			// D[u ][hast\n
			// Du h]ast mich
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutCursor(new Pos(3, 1), true);
			controller.PutNewCursor(new Pos(4, 2));
			controller.PutCursor(new Pos(3, 1), true);
			AssertSelection().Both(2, 0).Next().Anchor(1, 1).Caret(3, 1).Next().Anchor(4, 2).Caret(3, 1).NoNext();
			controller.InsertText("text");
			// Dutext|\n
			// Dtext|text|ast mich
			Assert.AreEqual("Dutext\nDtexttextast mich", lines.GetText());
			AssertSelection().Both(6, 0).Next().Both(5, 1).Next().Both(9, 1).NoNext();
			
			controller.processor.Undo();
			Assert.AreEqual("Du\nDu hast\nDu hast mich", lines.GetText());
			AssertSelection().Both(2, 0).Next().Anchor(1, 1).Caret(3, 1).Next().Anchor(4, 2).Caret(3, 1).NoNext();
		}
		
		[Test]
		public void InsertText_Joining_ChangeSelection()
		{
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			// Du|\n
			// D[u ][hast\n
			// Du h]ast mich
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutCursor(new Pos(3, 1), true);
			controller.PutNewCursor(new Pos(4, 2));
			controller.PutCursor(new Pos(3, 1), true);
			AssertSelection().Both(2, 0).Next().Anchor(1, 1).Caret(3, 1).Next().Anchor(4, 2).Caret(3, 1).NoNext();
			controller.InsertText("text");
			// Dutext|\n
			// Dtext|text|ast mich
			Assert.AreEqual("Dutext\nDtexttextast mich", lines.GetText());
			AssertSelection().Both(6, 0).Next().Both(5, 1).Next().Both(9, 1).NoNext();
			
			controller.PutCursor(new Pos(7, 2), false);
			
			controller.processor.Undo();
			Assert.AreEqual("Du\nDu hast\nDu hast mich", lines.GetText());
			AssertSelection().Both(2, 0).Next().Anchor(1, 1).Caret(3, 1).Next().Anchor(4, 2).Caret(3, 1).NoNext();
			
			controller.PutCursor(new Pos(7, 2), false);
			
			controller.processor.Redo();
			Assert.AreEqual("Dutext\nDtexttextast mich", lines.GetText());
			AssertSelection().Both(6, 0).Next().Both(5, 1).Next().Both(9, 1).NoNext();
			
			controller.processor.Undo();
			Assert.AreEqual("Du\nDu hast\nDu hast mich", lines.GetText());
			AssertSelection().Both(2, 0).Next().Anchor(1, 1).Caret(3, 1).Next().Anchor(4, 2).Caret(3, 1).NoNext();
		}
		
		[Test]
		public void Delete()
		{
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			// Du|\n
			// D|u has|t\n
			// Du hast mich
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutNewCursor(new Pos(6, 1));
			AssertSelection().Both(2, 0).Next().Both(1, 1).Next().Both(6, 1).NoNext();
			controller.Delete();
			// Du|D| has|\n
			// Du hast mich
			Assert.AreEqual("DuD has\nDu hast mich", lines.GetText());
			AssertSelection().Both(2, 0).Next().Both(3, 0).Next().Both(7, 0).NoNext();
			
			controller.processor.Undo();
			Assert.AreEqual("Du\nDu hast\nDu hast mich", lines.GetText());
			AssertSelection().Both(2, 0).Next().Both(1, 1).Next().Both(6, 1).NoNext();
		}
		
		[Test]
		public void Delete_RN()
		{
			Init();
			lines.SetText("Du\r\nDu hast\r\nDu hast mich");
			controller.PutCursor(new Pos(2, 0), false);
			AssertSelection().Both(2, 0);
			controller.Delete();
			Assert.AreEqual("DuDu hast\r\nDu hast mich", lines.GetText());
			AssertSelection().Both(2, 0);
			
			controller.processor.Undo();
			Assert.AreEqual("Du\r\nDu hast\r\nDu hast mich", lines.GetText());
			AssertSelection().Both(2, 0);
		}
		
		[Test]
		public void Delete_Joining()
		{
			Init();
			lines.SetText("1234567890");
			controller.PutCursor(new Pos(1, 0), false);
			controller.PutNewCursor(new Pos(2, 0));
			controller.PutNewCursor(new Pos(8, 0));
			controller.PutNewCursor(new Pos(7, 0));
			controller.PutNewCursor(new Pos(6, 0));
			controller.PutNewCursor(new Pos(3, 0));
			controller.PutNewCursor(new Pos(4, 0));
			controller.PutNewCursor(new Pos(5, 0));
			AssertSelection().Both(1, 0).Next().Both(2, 0).Next()
				.Both(8, 0).Next().Both(7, 0).Next().Both(6, 0).Next().Both(3, 0).Next().Both(4, 0).Next().Both(5, 0);
			
			controller.Delete();
			Assert.AreEqual("10", lines.GetText());
			AssertSelection().Both(1, 0);
			
			controller.processor.Undo();
			Assert.AreEqual("1234567890", lines.GetText());
			AssertSelection().Both(1, 0).Next().Both(2, 0).Next()
				.Both(8, 0).Next().Both(7, 0).Next().Both(6, 0).Next().Both(3, 0).Next().Both(4, 0).Next().Both(5, 0);
			
			controller.processor.Redo();
			Assert.AreEqual("10", lines.GetText());
			AssertSelection().Both(1, 0);
		}
		
		[Test]
		public void EraseSelection()
		{
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			// D|u\n
			// D[u ha]st[\n
			// Du hast mi]ch
			controller.PutCursor(new Pos(1, 0), false);
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutCursor(new Pos(5, 1), true);
			controller.PutNewCursor(new Pos(7, 1));
			controller.PutCursor(new Pos(10, 2), true);
			AssertSelection().Both(1, 0).Next().Anchor(1, 1).Caret(5, 1).Next().Anchor(7, 1).Caret(10, 2).NoNext();
			controller.EraseSelection();
			// D|u\n
			// D|st|ch
			Assert.AreEqual("Du\nDstch", lines.GetText());
			AssertSelection().Both(1, 0).Next().Both(1, 1).Next().Both(3, 1).NoNext();
			
			controller.processor.Undo();
			Assert.AreEqual("Du\nDu hast\nDu hast mich", lines.GetText());
			AssertSelection().Both(1, 0).Next().Anchor(1, 1).Caret(5, 1).Next().Anchor(7, 1).Caret(10, 2).NoNext();
			
			controller.processor.Redo();
			Assert.AreEqual("Du\nDstch", lines.GetText());
			AssertSelection().Both(1, 0).Next().Both(1, 1).Next().Both(3, 1).NoNext();
		}
		
		[Test]
		public void EraseSelection_SelectionsJoined()
		{
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			// Du|\n
			// D[u ][hast\n
			// Du h]ast mich
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutCursor(new Pos(3, 1), true);
			controller.PutNewCursor(new Pos(4, 2));
			controller.PutCursor(new Pos(3, 1), true);
			AssertSelection().Both(2, 0).Next().Anchor(1, 1).Caret(3, 1).Next().Anchor(4, 2).Caret(3, 1).NoNext();
			controller.EraseSelection();
			// Du|\n
			// D|ast mich
			Assert.AreEqual("Du\nDast mich", lines.GetText());
			AssertSelection().Both(2, 0).Next().Both(1, 1).NoNext();
			
			controller.processor.Undo();
			Assert.AreEqual("Du\nDu hast\nDu hast mich", lines.GetText());
			AssertSelection().Both(2, 0).Next().Anchor(1, 1).Caret(3, 1).Next().Anchor(4, 2).Caret(3, 1).NoNext();
		}
		
		[Test]
		public void Backspace()
		{
			Init();
			lines.SetText("Du\nDu hast\nDu hast mich");
			// Du|\n
			// D|u has|t\n
			// Du hast mich
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutNewCursor(new Pos(6, 1));
			AssertSelection().Both(2, 0).Next().Both(1, 1).Next().Both(6, 1).NoNext();
			controller.Backspace();
			// D|\n
			// |u ha|t\n
			// Du hast mich
			Assert.AreEqual("D\nu hat\nDu hast mich", lines.GetText());
			AssertSelection().Both(1, 0).Next().Both(0, 1).Next().Both(4, 1).NoNext();
			
			controller.processor.Undo();
			Assert.AreEqual("Du\nDu hast\nDu hast mich", lines.GetText());
			AssertSelection().Both(2, 0).Next().Both(1, 1).Next().Both(6, 1).NoNext();
		}
		
		[Test]
		public void Backspace_RN()
		{
			Init();
			lines.SetText("Du\r\nDu hast\r\nDu hast mich");
			controller.PutCursor(new Pos(0, 1), false);
			AssertSelection().Both(0, 1);
			controller.Backspace();
			Assert.AreEqual("DuDu hast\r\nDu hast mich", lines.GetText());
			AssertSelection().Both(2, 0);
			
			controller.processor.Undo();
			Assert.AreEqual("Du\r\nDu hast\r\nDu hast mich", lines.GetText());
			AssertSelection().Both(0, 1);
		}
		
		[Test]
		public void Backspace_Joining()
		{
			Init();
			lines.SetText("1234567890");
			controller.PutCursor(new Pos(1, 0), false);
			controller.PutNewCursor(new Pos(2, 0));
			controller.PutNewCursor(new Pos(8, 0));
			controller.PutNewCursor(new Pos(7, 0));
			controller.PutNewCursor(new Pos(6, 0));
			controller.PutNewCursor(new Pos(3, 0));
			controller.PutNewCursor(new Pos(4, 0));
			controller.PutNewCursor(new Pos(5, 0));
			AssertSelection().Both(1, 0).Next().Both(2, 0).Next()
				.Both(8, 0).Next().Both(7, 0).Next().Both(6, 0).Next().Both(3, 0).Next().Both(4, 0).Next().Both(5, 0);
			
			controller.Backspace();
			Assert.AreEqual("90", lines.GetText());
			AssertSelection().Both(0, 0);
			
			controller.processor.Undo();
			Assert.AreEqual("1234567890", lines.GetText());
			AssertSelection().Both(1, 0).Next().Both(2, 0).Next()
				.Both(8, 0).Next().Both(7, 0).Next().Both(6, 0).Next().Both(3, 0).Next().Both(4, 0).Next().Both(5, 0);
			
			controller.processor.Redo();
			Assert.AreEqual("90", lines.GetText());
			AssertSelection().Both(0, 0);
		}
		
		[Test]
		public void Paste()
		{
			Init();
			
			lines.SetText("Du\nDu hast\nDu hast mich");
			// Du|\n
			// D[u ][hast\n
			// Du h]ast mich
			controller.PutCursor(new Pos(2, 0), false);
			controller.PutNewCursor(new Pos(1, 1));
			controller.PutCursor(new Pos(3, 1), true);
			controller.PutNewCursor(new Pos(4, 2));
			controller.PutCursor(new Pos(3, 1), true);
			AssertSelection().Both(2, 0).Next().Anchor(1, 1).Caret(3, 1).Next().Anchor(4, 2).Caret(3, 1).NoNext();
			
			ClipboardExecutor.PutToClipboard("a\ntext\nanother text");
			controller.Paste();
			// Dua|\n
			// Dtext|another text|ast mich
			Assert.AreEqual("Dua\nDtextanother textast mich", lines.GetText());
			AssertSelection().Both(3, 0).Next().Both(5, 1).Next().Both(17, 1).NoNext();
			
			controller.processor.Undo();
			Assert.AreEqual("Du\nDu hast\nDu hast mich", lines.GetText());
			AssertSelection().Both(2, 0).Next().Anchor(1, 1).Caret(3, 1).Next().Anchor(4, 2).Caret(3, 1).NoNext();
			
			controller.processor.Redo();
			Assert.AreEqual("Dua\nDtextanother textast mich", lines.GetText());
			AssertSelection().Both(3, 0).Next().Both(5, 1).Next().Both(17, 1).NoNext();
		}
	}
}