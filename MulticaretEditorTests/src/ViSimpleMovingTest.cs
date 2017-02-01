using System;
using NUnit.Framework;
using MulticaretEditor;
using System.Windows.Forms;

namespace UnitTests
{
	[TestFixture]
	public class ViSimpleMovingTest : ControllerTestBase
	{
		private Receiver receiver;
		
		private void SetViMode(bool viMode)
		{
			receiver.SetViMode(viMode);
			Assert.AreEqual(viMode, receiver.viMode);
		}
		
		private ViSimpleMovingTest Press(string keys)
		{
			foreach (char c in keys)
			{
				receiver.DoKeyPress(c);
			}
			return this;
		}
		
		private ViSimpleMovingTest Press(Keys keysData)
		{
			receiver.DoKeyDown(keysData);
			return this;
		}
		
		private ViSimpleMovingTest Put(int iChar, int iLine, bool shift)
		{
			controller.PutCursor(new Place(iChar, iLine), shift);
			return this;
		}
		
		private ViSimpleMovingTest Put(int iChar, int iLine)
		{
			controller.PutCursor(new Place(iChar, iLine), false);
			AssertSelection().Both(iChar, iLine);
			return this;
		}
		
		[SetUp]
		public void SetUp()
		{
			Init();
			lines.lineBreak = "\n";
			receiver = new Receiver(controller);
			SetViMode(true);
		}
		
		[Test]
		public void fF()
		{
			lines.SetText(
			//	 0123456789012345678901
				"Du\n" +
				"Du hast\n" +
				"Du hast mich\n" +
				"abcd,.;.;.asdf234234\n" +
				"abcd ,.;.;.asdf234234\n" +
				"abcd  ,.;.;.asdf234234");
			
			Put(3, 2).Press("fa").AssertSelection().Both(4, 2).NoNext();
			Put(3, 2).Press("fs").AssertSelection().Both(5, 2).NoNext();
		}
	}
}