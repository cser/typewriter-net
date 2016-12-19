using System;
using NUnit.Framework;
using MulticaretEditor;
using System.Windows.Forms;

namespace UnitTests
{
	[TestFixture]
	public class ReceiverTest : ControllerTestBase
	{
		private Receiver receiver;
		
		[SetUp]
		public void SetUp()
		{
			Init();
			lines.lineBreak = "\n";
			receiver = new Receiver(controller);
		}
		
		private void SetViMode(bool viMode)
		{
			receiver.SetViMode(viMode);
			Assert.AreEqual(viMode, receiver.viMode);
		}
		
		private void DoKeyPress(char c)
		{
			receiver.DoKeyPress(c);
		}
		
		private void DoKeyDown(Keys keysData)
		{
			Assert.AreEqual(true, receiver.DoKeyDown(keysData));
		}
		
		[Test]
		public void StateEnter_Normal()
		{
			SetViMode(false);
			
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(1, 1), false);
			AssertSelection().Both(1, 1).NoNext();
			
			SetViMode(true);
			AssertSelection().Both(1, 1).NoNext();
		}
		
		[Test]
		public void StateEnter_AtEndOfLine()
		{
			SetViMode(false);
			
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(1, 1), false);
			AssertSelection().Both(1, 1).NoNext();
			
			SetViMode(true);
			AssertSelection().Both(1, 1).NoNext();
			
			SetViMode(false);
			AssertSelection().Both(1, 1).NoNext();
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(4, 1).NoNext();
			
			SetViMode(true);
			AssertSelection().Both(4, 1).NoNext();
			
			SetViMode(false);
			AssertSelection().Both(4, 1).NoNext();
			controller.MoveRight(false);
			AssertSelection().Both(5, 1).NoNext();
			
			SetViMode(true);
			AssertSelection().Both(4, 1).NoNext();
			SetViMode(false);
			AssertSelection().Both(4, 1).NoNext();
		}
		
		[Test]
		public void StateEnter_AtEndOfLine_i()
		{
			SetViMode(false);
			
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(1, 1), false);
			AssertSelection().Both(1, 1).NoNext();
			
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			Assert.AreEqual(true, receiver.viMode);
			AssertSelection().Both(1, 1).NoNext();
			
			DoKeyPress('i');
			AssertSelection().Both(1, 1).NoNext();
			controller.MoveRight(false);
			controller.MoveRight(false);
			controller.MoveRight(false);
			AssertSelection().Both(4, 1).NoNext();
			
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			Assert.AreEqual(true, receiver.viMode);
			AssertSelection().Both(4, 1).NoNext();
			
			DoKeyPress('i');
			AssertSelection().Both(4, 1).NoNext();
			controller.MoveRight(false);
			AssertSelection().Both(5, 1).NoNext();
			
			DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			Assert.AreEqual(true, receiver.viMode);
			AssertSelection().Both(4, 1).NoNext();
			DoKeyPress('i');
			AssertSelection().Both(4, 1).NoNext();
		}
		
		[Test]
		public void StateEnter_hjkl()
		{
			SetViMode(false);
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(1, 1), false);
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
	}
}
