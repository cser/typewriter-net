using System;
using System.Collections.Generic;
using NUnit.Framework;
using MulticaretEditor;
using System.Windows.Forms;

namespace UnitTests
{
	[TestFixture]
	public class ViReceiverTest : ControllerTestBase
	{
		private static Dictionary<char, char> ruMap;
		
		private static Dictionary<char, char> GetRuMap()
		{
			if (ruMap == null)
			{
				ruMap = new Dictionary<char, char>();
				string en = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQASTUVWXYZ";
				string ru = "фисвуапршолдьтщзйкыегмцчняФИСВУАПРШОЛДЬТЩЗЙФЫЕГМЦЧНЯ";
				for (int i = 0; i < en.Length; i++)
				{
					ruMap[ru[i]] = en[i];
				}
			}
			return ruMap;
		}
		
		private Receiver receiver;
		
		[SetUp]
		public void SetUp()
		{
			Init();
			lines.lineBreak = "\n";
			receiver = new Receiver(controller, ViMode.Insert, false);
		}
		
		private void EscapeNormalViMode()
		{
			string viShortcut;
			bool scrollToCursor;
			receiver.DoKeyDown(Keys.OemOpenBrackets | Keys.Control, out viShortcut, out scrollToCursor);
			Assert.AreEqual(ViMode.Normal, receiver.ViMode);
		}
		
		private void SetViMode(bool viMode)
		{
			receiver.SetViMode(viMode ? ViMode.Normal : ViMode.Insert, false);
			Assert.AreEqual(viMode ? ViMode.Normal : ViMode.Insert, receiver.ViMode);
		}
		
		private ViReceiverTest DoKeyPress(char c)
		{
			string viShortcut;
			bool scrollToCursor;
			receiver.DoKeyPress(c, out viShortcut, out scrollToCursor);
			return this;
		}
		
		private ViReceiverTest DoKeyDown(Keys keysData)
		{
			string viShortcut;
			bool scrollToCursor;
			Assert.AreEqual(true, receiver.DoKeyDown(keysData, out viShortcut, out scrollToCursor));
			return this;
		}
		
		[Test]
		public void StateEnter_Normal()
		{
			SetViMode(false);
			
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(1, 1), false);
			AssertSelection().Both(1, 1).NoNext();
			
			EscapeNormalViMode();
			AssertSelection().Both(0, 1).NoNext();
		}
		
		[Test]
		public void StateEnter_AtEndOfLine()
		{
			SetViMode(false);
			
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(1, 1), false);
			AssertSelection().Both(1, 1).NoNext();
			
			EscapeNormalViMode();
			AssertSelection().Both(0, 1).NoNext();
			
			SetViMode(false);
			AssertSelection().Both(0, 1).NoNext();
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(4, 1).NoNext();
			
			EscapeNormalViMode();
			AssertSelection().Both(3, 1).NoNext();
			
			SetViMode(false);
			AssertSelection().Both(3, 1).NoNext();
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(5, 1).NoNext();
			
			EscapeNormalViMode();
			AssertSelection().Both(4, 1).NoNext();
			SetViMode(false);
			AssertSelection().Both(4, 1).NoNext();
			controller.MoveLeft(false);
			controller.MoveLeft(false);
			controller.MoveLeft(false);
			controller.MoveLeft(false);
			AssertSelection().Both(0, 1).NoNext();
			EscapeNormalViMode();
			AssertSelection().Both(0, 1).NoNext();
		}
		
		[Test]
		public void StateEnter_AtEndOfLine_i()
		{
			SetViMode(false);
			
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(1, 1), false);
			AssertSelection().Both(1, 1).NoNext();
			
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			Assert.AreEqual(ViMode.Normal, receiver.ViMode);
			AssertSelection().Both(0, 1).NoNext();
			
			DoKeyPress('i');
			AssertSelection().Both(0, 1).NoNext();
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(4, 1).NoNext();
			
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			Assert.AreEqual(ViMode.Normal, receiver.ViMode);
			AssertSelection().Both(3, 1).NoNext();
			
			DoKeyPress('i');
			AssertSelection().Both(3, 1).NoNext();
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(5, 1).NoNext();
			
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			Assert.AreEqual(ViMode.Normal, receiver.ViMode);
			AssertSelection().Both(4, 1).NoNext();
			DoKeyPress('i');
			AssertSelection().Both(4, 1).NoNext();
			controller.MoveLeft(false);
			controller.MoveLeft(false);
			controller.MoveLeft(false);
			controller.MoveLeft(false);
			AssertSelection().Both(0, 1).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertSelection().Both(0, 1).NoNext();
		}
		
		[Test]
		public void StateEnter_hjkl()
		{
			SetViMode(false);
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(2, 1), false);
			EscapeNormalViMode();
			AssertSelection().Both(1, 1).NoNext();
			
			DoKeyPress('h');
			AssertSelection().Both(0, 1).NoNext();
			DoKeyPress('l');
			AssertSelection().Both(1, 1).NoNext();
			DoKeyPress('j');
			AssertSelection().Both(1, 2).NoNext();
			DoKeyPress('k');
			AssertSelection().Both(1, 1).NoNext();
		}
		
		[Test]
		public void StateEnter_hjkl_mapped()
		{
			ClipboardExecutor.fakeLayout = true;
			ClipboardExecutor.fakeEnLayout = false;
			receiver.viMap = GetRuMap();
			SetViMode(false);
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(2, 1), false);
			EscapeNormalViMode();
			AssertSelection().Both(1, 1).NoNext();
			
			DoKeyPress('р');
			AssertSelection().Both(0, 1).NoNext();
			DoKeyPress('д');
			AssertSelection().Both(1, 1).NoNext();
			DoKeyPress('о');
			AssertSelection().Both(1, 2).NoNext();
			DoKeyPress('л');
			AssertSelection().Both(1, 1).NoNext();
		}
		
		[Test]
		public void StateEnter_input()
		{
			SetViMode(false);
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(3, 1), false);
			EscapeNormalViMode();
			AssertSelection().Both(2, 1).NoNext();
			
			DoKeyPress('h').AssertSelection().Both(1, 1).NoNext();
			DoKeyPress('i').AssertSelection().Both(1, 1).NoNext();
			DoKeyPress('A').DoKeyPress('B').DoKeyPress('C').AssertSelection().Both(4, 1).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertText("line0\nlABCine1\nline2\nline3");
			AssertSelection().Both(3, 1).NoNext();
			DoKeyPress('j').AssertSelection().Both(3, 2).NoNext();
		}
		
		[Test]
		public void StateEnter_input_several()
		{
			SetViMode(false);
			lines.SetText("line0\nline1\nline2___________\nline3");
			controller.PutCursor(new Place(3, 1), false);
			EscapeNormalViMode();
			AssertSelection().Both(2, 1).NoNext();
			
			DoKeyPress('h').AssertSelection().Both(1, 1).NoNext();
			DoKeyPress('4').DoKeyPress('i').AssertSelection().Both(1, 1).NoNext();
			DoKeyPress('A').DoKeyPress('B').DoKeyPress('C').AssertSelection().Both(4, 1).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertText("line0\nlABCABCABCABCine1\nline2___________\nline3");
			AssertSelection().Both(12, 1).NoNext();
			DoKeyPress('j').AssertSelection().Both(12, 2).NoNext();
		}
		
		[Test]
		public void StateEnter_a()
		{
			SetViMode(false);
			lines.SetText("line0\nline1\nline2___________\nline3");
			controller.PutCursor(new Place(3, 1), false);
			EscapeNormalViMode();
			AssertSelection().Both(2, 1).NoNext();
			
			DoKeyPress('3').DoKeyPress('a').AssertSelection().Both(3, 1).NoNext();
			DoKeyPress('A').DoKeyPress('B').AssertSelection().Both(5, 1).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertText("line0\nlinABABABe1\nline2___________\nline3");
			AssertSelection().Both(8, 1).NoNext();
		}
		
		[Test]
		public void StateEnter_A()
		{
			SetViMode(true);
			lines.SetText("line0\nline1\nline2___________\nline3");
			controller.PutCursor(new Place(2, 1), false);
			AssertSelection().Both(2, 1).NoNext();
			
			DoKeyPress('3').DoKeyPress('A').AssertSelection().Both(5, 1).NoNext();
			DoKeyPress('A').DoKeyPress('B').AssertSelection().Both(7, 1).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertText("line0\nline1ABABAB\nline2___________\nline3");
			AssertSelection().Both(10, 1).NoNext();
		}
		
		[Test]
		public void StateEnter_I()
		{
			SetViMode(true);
			lines.SetText("line0\n    line1\nline2");
			controller.PutCursor(new Place(8, 1), false);
			AssertSelection().Both(8, 1).NoNext();
			
			DoKeyPress('3').DoKeyPress('I').AssertSelection().Both(4, 1).NoNext();
			DoKeyPress('A').DoKeyPress('B').AssertSelection().Both(6, 1).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertText("line0\n    ABABABline1\nline2");
			AssertSelection().Both(9, 1).NoNext();
		}
		
		[Test]
		public void StateEnter_o()
		{
			SetViMode(true);
			lines.lineBreak = "\n";
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(2, 1), false);
			AssertSelection().Both(2, 1).NoNext();
			
			DoKeyPress('o').AssertSelection().Both(0, 2).NoNext();
			DoKeyPress('A').DoKeyPress('B').DoKeyPress('C').AssertSelection().Both(3, 2).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertText("line0\nline1\nABC\nline2\nline3");
			AssertSelection().Both(2, 2).NoNext();
			DoKeyPress('j').AssertSelection().Both(2, 3).NoNext();
		}
		
		[Test]
		public void StateEnter_o_Indented()
		{
			SetViMode(true);
			lines.lineBreak = "\n";
			lines.SetText("line0\n\tline1\n\tline2\nline3");
			controller.PutCursor(new Place(2, 1), false);
			AssertSelection().Both(2, 1).NoNext();
			
			DoKeyPress('o').AssertSelection().Both(1, 2).NoNext();
			DoKeyPress('A').DoKeyPress('B').DoKeyPress('C').AssertSelection().Both(4, 2).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertText("line0\n\tline1\n\tABC\n\tline2\nline3");
			AssertSelection().Both(3, 2).NoNext();
		}
		
		[Test]
		public void StateEnter_o_Repeat()
		{
			SetViMode(true);
			lines.lineBreak = "\n";
			lines.SetText("line0\n\tline1\n\tline2\nline3");
			controller.PutCursor(new Place(2, 1), false);
			AssertSelection().Both(2, 1).NoNext();
			
			DoKeyPress('3').DoKeyPress('o').AssertSelection().Both(1, 2).NoNext();
			DoKeyPress('A').DoKeyPress('B').DoKeyPress('C').AssertSelection().Both(4, 2).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertText("line0\n\tline1\n\tABC\n\tABC\n\tABC\n\tline2\nline3");
			AssertSelection().Both(3, 4).NoNext();
		}
		
		[Test]
		public void StateEnter_O()
		{
			SetViMode(true);
			lines.lineBreak = "\n";
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(2, 1), false);
			AssertSelection().Both(2, 1).NoNext();
			
			DoKeyPress('O').AssertSelection().Both(0, 1).NoNext();
			DoKeyPress('A').DoKeyPress('B').DoKeyPress('C').AssertSelection().Both(3, 1).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertText("line0\nABC\nline1\nline2\nline3");
			AssertSelection().Both(2, 1).NoNext();
			DoKeyPress('j').AssertSelection().Both(2, 2).NoNext();
		}
		
		[Test]
		public void StateEnter_O_Indented()
		{
			SetViMode(true);
			lines.lineBreak = "\n";
			lines.SetText("line0\n\tline1\n\tline2\nline3");
			controller.PutCursor(new Place(2, 1), false);
			AssertSelection().Both(2, 1).NoNext();
			
			DoKeyPress('O').AssertSelection().Both(1, 1).NoNext();
			DoKeyPress('A').DoKeyPress('B').DoKeyPress('C').AssertSelection().Both(4, 1).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertText("line0\n\tABC\n\tline1\n\tline2\nline3");
			AssertSelection().Both(3, 1).NoNext();
		}
		
		[Test]
		public void StateEnter_O_Repeat()
		{
			SetViMode(true);
			lines.lineBreak = "\n";
			lines.SetText("line0\n\tline1\n\tline2\nline3");
			controller.PutCursor(new Place(2, 1), false);
			AssertSelection().Both(2, 1).NoNext();
			
			DoKeyPress('3').DoKeyPress('O').AssertSelection().Both(1, 1).NoNext();
			DoKeyPress('A').DoKeyPress('B').DoKeyPress('C').AssertSelection().Both(4, 1).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertText("line0\n\tABC\n\tABC\n\tABC\n\tline1\n\tline2\nline3");
			AssertSelection().Both(3, 3).NoNext();
		}
		
		[Test]
		public void StateEnter_O_Autoindent()
		{
			SetViMode(true);
			lines.autoindent = true;
			lines.lineBreak = "\n";
			lines.SetText("\tline0{\n\t\tline1\n\t}\nline3");
			controller.PutCursor(new Place(1, 2), false);
			AssertSelection().Both(1, 2).NoNext();
			
			DoKeyPress('O').AssertSelection().Both(2, 2).NoNext();
			DoKeyPress('A').DoKeyPress('B').DoKeyPress('C').AssertSelection().Both(5, 2).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertText("\tline0{\n\t\tline1\n\t\tABC\n\t}\nline3");
			AssertSelection().Both(4, 2).NoNext();
		}
		
		[Test]
		public void StateEnter_O_Autoindent_Undo()
		{
			SetViMode(true);
			lines.autoindent = true;
			lines.lineBreak = "\n";
			lines.SetText("\tline0{\n\t\tline1\n\t}\nline3");
			controller.PutCursor(new Place(1, 2), false);
			AssertSelection().Both(1, 2).NoNext();
			
			DoKeyPress('O').AssertSelection().Both(2, 2).NoNext();
			DoKeyPress('A').DoKeyPress('B').DoKeyPress('C').AssertSelection().Both(5, 2).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertText("\tline0{\n\t\tline1\n\t\tABC\n\t}\nline3");
			AssertSelection().Both(4, 2).NoNext();
			
			controller.processor.Undo();
			AssertText("\tline0{\n\t\tline1\n\t}\nline3");
		}
		
		[Test]
		public void StateEnter_o_Autoindent_Undo()
		{
			SetViMode(true);
			lines.autoindent = true;
			lines.lineBreak = "\n";
			lines.SetText("\tline0{\n\t\tline1\n\t}\nline3");
			controller.PutCursor(new Place(1, 0), false);
			AssertSelection().Both(1, 0).NoNext();
			
			DoKeyPress('o').AssertSelection("#1").Both(2, 1).NoNext();
			DoKeyPress('A').DoKeyPress('B').DoKeyPress('C').AssertSelection("#11").Both(5, 1).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertText("\tline0{\n\t\tABC\n\t\tline1\n\t}\nline3");
			AssertSelection("#12").Both(4, 1).NoNext();
			
			controller.PutCursor(new Place(1, 0), false);
			DoKeyPress('o').AssertSelection("#2").Both(2, 1).NoNext();
			DoKeyPress('A').DoKeyPress('B').DoKeyPress('C').AssertSelection("#21").Both(5, 1).NoNext();
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			AssertText("\tline0{\n\t\tABC\n\t\tABC\n\t\tline1\n\t}\nline3");
			AssertSelection("#22").Both(4, 1).NoNext();
			
			controller.processor.Undo();
			AssertText("\tline0{\n\t\tABC\n\t\tline1\n\t}\nline3");
			
			controller.processor.Undo();
			AssertText("\tline0{\n\t\tline1\n\t}\nline3");
		}
		
		[Test]
		public void fx_Ru()
		{
			SetViMode(true);
			ClipboardExecutor.fakeLayout = true;
			ClipboardExecutor.fakeEnLayout = false;
			receiver.viMap = GetRuMap();
			
			lines.autoindent = true;
			lines.lineBreak = "\n";
			lines.SetText("xxxzя");
			
			controller.PutCursor(new Place(1, 0), false);
			DoKeyPress('f').DoKeyPress('z').AssertSelection("#1").Both(3, 0).NoNext();
			
			controller.PutCursor(new Place(1, 0), false);
			DoKeyPress('а').DoKeyPress('я').AssertSelection("#2").Both(4, 0).NoNext();
		}
	}
}
