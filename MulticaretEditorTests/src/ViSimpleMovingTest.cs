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
				"Du hast\n" +
				"Du hast mich\n" +
				"abcd  ,.;.;.asdf234234");
			
			Put(3, 1).Press("fa").AssertSelection().Both(4, 1).NoNext();
			Put(3, 1).Press("fs").AssertSelection().Both(5, 1).NoNext();
			Put(4, 2).Press("2f;").AssertSelection().Both(10, 2).NoNext();
			
			Put(3, 1).Press("Fu").AssertSelection().Both(1, 1).NoNext();
			Put(3, 1).Press("F ").AssertSelection().Both(2, 1).NoNext();
			Put(12, 2).Press("2F;").AssertSelection().Both(8, 2).NoNext();
		}
		
		[Test]
		public void tT()
		{
			lines.SetText(
			//	 0123456789012345678901
				"Du hast\n" +
				"Du hast mich\n" +
				"abcd  ,.;.;.asdf234234");
			
			Put(3, 1).Press("ti").AssertSelection().Both(8, 1).NoNext();
			Put(3, 1).Press("ta").AssertSelection().Both(3, 1).NoNext();			
			Put(3, 1).Press("Tu").AssertSelection().Both(2, 1).NoNext();
		}
		
		[Test]
		public void tT_RepeatNuance()
		{
			lines.SetText(
			//	 0123456789012345678901
				"Du hast\n" +
				"Du hast mich\n" +
				"abcd  ,.;.;.asdf234234");
			
			Put(4, 2).Press("2t;").AssertSelection().Both(9, 2).NoNext();
			Put(7, 2).Press("2t;").AssertSelection().Both(9, 2).NoNext();
			Put(12, 2).Press("2T;").AssertSelection().Both(9, 2).NoNext();
			Put(11, 2).Press("2T;").AssertSelection().Both(9, 2).NoNext();
			Put(7, 2).Press("t;").AssertSelection().Both(7, 2).NoNext();
			Put(11, 2).Press("T;").AssertSelection().Both(11, 2).NoNext();
		}
		
		[Test]
		public void S6_0()
		{
			lines.SetText(
			//	 012345678901234
				"Du hast\n" +
				"   Du hast mich\n" +
				"   a");
			
			Put(4, 0).Press("0").AssertSelection().Both(0, 0).NoNext();
			Put(4, 1).Press("0").AssertSelection().Both(0, 1).NoNext();
			
			Put(4, 0).Press("^").AssertSelection().Both(0, 0).NoNext();
			
			Put(14, 1).Press("^").AssertSelection().Both(3, 1).NoNext();
			Put(4, 1).Press("^").AssertSelection().Both(3, 1).NoNext();
			Put(3, 1).Press("^").AssertSelection().Both(3, 1).NoNext();
			Put(2, 1).Press("^").AssertSelection().Both(3, 1).NoNext();
			Put(1, 1).Press("^").AssertSelection().Both(3, 1).NoNext();
			Put(0, 1).Press("^").AssertSelection().Both(3, 1).NoNext();
		}
		
		[Test]
		public void S6_OnlySpacesNuance()
		{
			lines.SetText(
			//	 012345678901234
				"Du hast\n" +
				"   \n" +
				"\n" +
				"a");
			
			Put(1, 1).Press("^").AssertSelection().Both(2, 1).NoNext();
			Put(0, 2).Press("^").AssertSelection().Both(0, 2).NoNext();
		}
		
		[Test]
		public void S4()
		{
			lines.SetText(
			//	 012345678901234
				"Du hast\n" +
				"   Du hast mich\n" +
				"   a");
			
			Put(4, 0).Press("$").AssertSelection().Both(6, 0).NoNext();
			Put(0, 0).Press("$").AssertSelection().Both(6, 0).NoNext();
			Put(1, 1).Press("$").AssertSelection().Both(14, 1).NoNext();
			Put(3, 2).Press("$").AssertSelection().Both(3, 2).NoNext();
		}
		
		[Test]
		public void S4_Repeat()
		{
			lines.SetText(
			//	 012345678901234
				"Du hast\n" +
				"   Du hast mich\n" +
				"   a");
			
			Put(4, 0).Press("2").Press("$").AssertSelection().Both(14, 1).NoNext();
			Put(4, 0).Press("3").Press("$").AssertSelection().Both(3, 2).NoNext();
			Put(4, 0).Press("4").Press("$").AssertSelection().Both(3, 2).NoNext();
		}
		
		[Test]
		public void gg_G()
		{
			lines.SetText(
			//	 012345678901234
				"Du hast\n" +
				"   Du hast mich\n" +
				"aaaaaaa");
			Put(4, 1).Press("g").Press("g").AssertSelection().Both(0, 0).NoNext();
			Put(4, 1).Press("G").AssertSelection().Both(0, 2).NoNext();
			Put(4, 2).Press("G").AssertSelection().Both(0, 2).NoNext();
		}
	}
}