using System;
using NUnit.Framework;
using MulticaretEditor;
using System.Windows.Forms;

namespace UnitTests
{
	[TestFixture]
	public class ViSimpleTest : ControllerTestBase
	{
		private Receiver receiver;
		
		private void SetViMode(bool viMode)
		{
			receiver.SetViMode(viMode);
			Assert.AreEqual(viMode, receiver.viMode);
		}
		
		private ViSimpleTest Press(string keys)
		{
			foreach (char c in keys)
			{
				receiver.DoKeyPress(c);
			}
			return this;
		}
		
		private ViSimpleTest Press(Keys keysData)
		{
			receiver.DoKeyDown(keysData);
			return this;
		}
		
		private ViSimpleTest PressCommandMode()
		{
			receiver.DoKeyDown(Keys.Control | Keys.OemOpenBrackets);
			return this;
		}
		
		private ViSimpleTest Put(int iChar, int iLine, bool shift)
		{
			controller.PutCursor(new Place(iChar, iLine), shift);
			return this;
		}
		
		private ViSimpleTest Put(int iChar, int iLine)
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
			receiver = new Receiver(controller, false);
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
		
		[Test]
		public void hjkl()
		{
			lines.SetText(
			//	 012345678901234
				"Du hast\n" +
				"   Du hast mich\n" +
				"aaaaaaa");
			Put(4, 1).Press("j").AssertSelection().Both(4, 2).NoNext();
			Put(4, 1).Press("k").AssertSelection().Both(4, 0).NoNext();
			Put(4, 1).Press("h").AssertSelection().Both(3, 1).NoNext();
			Put(4, 1).Press("l").AssertSelection().Both(5, 1).NoNext();
			
			Put(4, 1).Press("hhh").AssertSelection().Both(1, 1).NoNext();
			Press("h").AssertSelection().Both(0, 1).NoNext();
			Press("h").AssertSelection().Both(0, 1).NoNext();
			
			Put(4, 1).Press("9l").AssertSelection().Both(13, 1).NoNext();
			Press("l").AssertSelection().Both(14, 1).NoNext();
			Press("l").AssertSelection().Both(14, 1).NoNext();
			
			Put(14, 1).Press("j").AssertSelection().Both(6, 2).NoNext();
			Press("j").AssertSelection().Both(6, 2).NoNext();
			
			Put(14, 1).Press("k").AssertSelection().Both(6, 0).NoNext();
			Press("k").AssertSelection().Both(6, 0).NoNext();
		}
		
		[Test]
		public void hjkl_preferredPosition()
		{
			lines.SetText(
			//	 012345678901234
				"Du hast\n" +
				"   Du hast mich\n" +
				"aaaaaaa");

			Put(14, 1);
			Press("jk").AssertSelection().Both(14, 1).NoNext();
			Press("kj").AssertSelection().Both(14, 1).NoNext();
		}
		
		[Test]
		public void hjkl_preferredPosition_withTab()
		{
			lines.SetText(
			//	 012345678901234
				"Du hast\n" +
			    "\tDu hast mich\n" +
				"aaaaaaa");

			Put(10, 1);
			Press("jk").AssertSelection().Both(10, 1).NoNext();
			Press("kj").AssertSelection().Both(10, 1).NoNext();
		}
		
		[Test]
		public void hjkl_DocumentEnd()
		{
			lines.SetText(
				"aaa\n" +
				"");

			Put(2, 0).Press("j").AssertSelection().Both(0, 1).NoNext();
		}
		
		[Test]
		public void S4_preferredPos()
		{
			lines.SetText(
			//	 012345678901234
				"Du hast\n" +
				  "\tDu hast mich\n" +
				"   a");
			
			Put(4, 0).Press("$").AssertSelection().Both(6, 0).NoNext();
			Press("j").AssertSelection().Both(3, 1).NoNext();
		}
		
		[Test]
		public void r()
		{
			lines.SetText("Du hast");
			Put(3, 0).Press("rx").AssertSelection().Both(3, 0).NoNext();
			AssertText("Du xast");
		}
		
		[Test]
		public void r_Repeat()
		{
			lines.SetText("Du hast");
			Put(3, 0).Press("3rx").AssertSelection().Both(3, 0).NoNext();
			AssertText("Du xxxt");
			Put(3, 0).Press("4ry").AssertSelection().Both(3, 0).NoNext();
			AssertText("Du yyyy");
			Put(3, 0).Press("5rz").AssertSelection().Both(3, 0).NoNext();
			AssertText("Du yyyy");
		}
		
		[Test]
		public void r_Selection()
		{
			lines.SetText("Du hast\nDu hast mich");
			Put(3, 0).Put(2, 1, true).Press("rx").AssertSelection().Both(3, 0).NoNext();
			AssertText("Du xxxx\nxx hast mich");
		}
		
		[Test]
		public void c()
		{
			lines.SetText("Du hast\nDu hast mich");
			
			Put(3, 1).Press("cw").AssertSelection().Both(3, 1).NoNext();
			Press("NEW_WORD").PressCommandMode().AssertSelection().Both(10, 1).NoNext();
			AssertText("Du hast\nDu NEW_WORD mich");
			
			Put(0, 1).Press("2cw").AssertSelection().Both(0, 1).NoNext();
			Press("AAA").PressCommandMode().AssertSelection().Both(5, 1).NoNext();
			AssertText("Du hast\nAAAAAA mich");
			
			Put(7, 1).Press("cb").AssertSelection().Both(0, 1).NoNext();
			Press("BBB").PressCommandMode().AssertSelection().Both(2, 1).NoNext();
			AssertText("Du hast\nBBBmich");
		}
		
		[Test]
		public void x()
		{
			lines.SetText("Du hast\nDu hast mich");
			
			Put(3, 0).Press("x").AssertSelection().Both(3, 0).NoNext();
			AssertText("Du ast\nDu hast mich");
			
			Put(3, 0).Press("2x").AssertSelection().Both(3, 0).NoNext();
			AssertText("Du t\nDu hast mich");
			
			Put(8, 1).Press("5x").AssertSelection().Both(7, 1).NoNext();
			AssertText("Du t\nDu hast ");
		}
	}
}