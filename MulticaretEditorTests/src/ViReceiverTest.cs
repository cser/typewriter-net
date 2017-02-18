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
				string en = "abcdefghijklmnopqastuvwxyzABCDEFGHIJKLMNOPQASTUVWXYZ";
				string ru = "фисвуапршолдьтщзйфыегмцчняФИСВУАПРШОЛДЬТЩЗЙФЫЕГМЦЧНЯ";
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
			receiver = new Receiver(controller, false, false);
		}
		
		private void SetViMode(bool viMode)
		{
			receiver.SetViMode(viMode);
			Assert.AreEqual(viMode, receiver.ViMode);
		}
		
		private ViReceiverTest DoKeyPress(char c)
		{
			string viShortcut;
			receiver.DoKeyPress(c, out viShortcut);
			return this;
		}
		
		private ViReceiverTest DoKeyDown(Keys keysData)
		{
			Assert.AreEqual(true, receiver.DoKeyDown(keysData));
			return this;
		}
		
		[Test]
		public void StateEnter_Normal()
		{
			SetViMode(false);
			
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(1, 1), false);
			AssertSelection().Both(1, 1).NoNext();
			
			SetViMode(true);
			AssertSelection().Both(0, 1).NoNext();
		}
		
		[Test]
		public void StateEnter_AtEndOfLine()
		{
			SetViMode(false);
			
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(1, 1), false);
			AssertSelection().Both(1, 1).NoNext();
			
			SetViMode(true);
			AssertSelection().Both(0, 1).NoNext();
			
			SetViMode(false);
			AssertSelection().Both(0, 1).NoNext();
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(4, 1).NoNext();
			
			SetViMode(true);
			AssertSelection().Both(3, 1).NoNext();
			
			SetViMode(false);
			AssertSelection().Both(3, 1).NoNext();
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(5, 1).NoNext();
			
			SetViMode(true);
			AssertSelection().Both(4, 1).NoNext();
			SetViMode(false);
			AssertSelection().Both(4, 1).NoNext();
			controller.MoveLeft(false);
			controller.MoveLeft(false);
			controller.MoveLeft(false);
			controller.MoveLeft(false);
			AssertSelection().Both(0, 1).NoNext();
			SetViMode(true);
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
			Assert.AreEqual(true, receiver.ViMode);
			AssertSelection().Both(0, 1).NoNext();
			
			DoKeyPress('i');
			AssertSelection().Both(0, 1).NoNext();
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(4, 1).NoNext();
			
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			Assert.AreEqual(true, receiver.ViMode);
			AssertSelection().Both(3, 1).NoNext();
			
			DoKeyPress('i');
			AssertSelection().Both(3, 1).NoNext();
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(5, 1).NoNext();
			
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			Assert.AreEqual(true, receiver.ViMode);
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
			SetViMode(true);
			AssertSelection().Both(1, 1).NoNext();
			
			DoKeyPress('h');
			AssertSelection().Both(0, 1).NoNext();
			DoKeyPress('l');
			AssertSelection().Both(1, 1).NoNext();
			DoKeyPress('j');
			AssertSelection().Both(1, 2).NoNext();
			DoKeyPress('k');
			AssertSelection().Both(1, 1).NoNext();
			
			DoKeyDown(Keys.Left);
			AssertSelection().Both(0, 1).NoNext();
			DoKeyDown(Keys.Right);
			AssertSelection().Both(1, 1).NoNext();
			DoKeyDown(Keys.Down);
			AssertSelection().Both(1, 2).NoNext();
			DoKeyDown(Keys.Up);
			AssertSelection().Both(1, 1).NoNext();
		}
		
		[Test]
		public void StateEnter_hjkl_mapped()
		{
			ClipboardExecuter.fakeLayout = true;
			ClipboardExecuter.fakeEnLayout = false;
			receiver.viMap = GetRuMap();
			SetViMode(false);
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(2, 1), false);
			SetViMode(true);
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
			SetViMode(true);
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
			SetViMode(true);
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
			SetViMode(true);
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
	}
}
