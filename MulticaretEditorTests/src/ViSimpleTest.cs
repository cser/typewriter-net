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
		public void cw()
		{
			lines.SetText("Du hast\nDu hast mich");
			
			Put(3, 1).Press("cw").AssertSelection().Both(3, 1).NoNext();
			Press("NEW_WORD").PressCommandMode().AssertSelection().Both(10, 1).NoNext();
			AssertText("Du hast\nDu NEW_WORD mich");
			
			Put(0, 1).Press("2cw").AssertSelection().Both(0, 1).NoNext();
			Press("AAA").PressCommandMode().AssertSelection().Both(2, 1).NoNext();
			AssertText("Du hast\nAAA mich");
		}
		
		[Test]
		public void cb()
		{
			lines.SetText("Du hast\nDu hast mich");
			
			Put(8, 1).Press("cb").AssertSelection().Both(3, 1).NoNext();
			Press("BBB").PressCommandMode().AssertSelection().Both(5, 1).NoNext();
			AssertText("Du hast\nDu BBBmich");
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
		
		[Test]
		public void NumberG()
		{
			lines.SetText("Du hast\nDu hast mich\nDu hast mich");
			
			Put(3, 1).Press("1G").AssertSelection().Both(0, 0).NoNext();
			Put(3, 1).Press("2G").AssertSelection().Both(0, 1).NoNext();
			Put(3, 1).Press("3G").AssertSelection().Both(0, 2).NoNext();
			Put(3, 1).Press("4G").AssertSelection().Both(0, 2).NoNext();
		}
		
		[Test]
		public void NumberG_Tabbed()
		{
			lines.SetText("Du hast\n    Du hast mich\n\t\tDu hast mich");
			
			Put(3, 1).Press("1G").AssertSelection().Both(0, 0).NoNext();
			Put(3, 1).Press("2G").AssertSelection().Both(4, 1).NoNext();
			Put(3, 1).Press("3G").AssertSelection().Both(2, 2).NoNext();
			Put(3, 1).Press("4G").AssertSelection().Both(2, 2).NoNext();
		}
		
		[Test]
		public void y()
		{
			lines.SetText("Du hast mich");
			
			ClipboardExecuter.PutToClipboard("");
			Put(3, 0).Press("yw");
			Assert.AreEqual("hast ", ClipboardExecuter.GetFromClipboard());
			AssertSelection().Both(3, 0).NoNext();
		}
		
		[Test]
		public void p()
		{
			lines.SetText("Du hast mich");	
			
			ClipboardExecuter.PutToClipboard("AAA");
			Put(3, 0).Press("p");
			AssertText("Du hAAAast mich");
			AssertSelection().Both(6, 0).NoNext();
		}
		
		[Test]
		public void p_Undo()
		{
			lines.SetText("Du hast mich");
			
			ClipboardExecuter.PutToClipboard("AAA");
			Put(3, 0).Press("p");
			AssertText("Du hAAAast mich");
			AssertSelection("#1").Both(6, 0).NoNext();
			Press("u");
			AssertText("Du hast mich");
			AssertSelection("#2").Both(3, 0);
		}
		
		[Test]
		public void p_Redo_Controvertial()
		{
			lines.SetText("Du hast mich");
			
			ClipboardExecuter.PutToClipboard("AAA");
			Put(3, 0).Press("p");
			AssertText("Du hAAAast mich");
			AssertSelection("#1").Both(6, 0).NoNext();
			Press("u");
			AssertText("Du hast mich");
			AssertSelection("#2").Both(3, 0);
			Press(Keys.Control | Keys.R);
			AssertText("Du hAAAast mich");
			AssertSelection("#3").Both(6, 0);
		}
		
		[Test]
		public void P()
		{
			lines.SetText("Du hast mich");	
			
			ClipboardExecuter.PutToClipboard("AAA");
			Put(3, 0).Press("P");
			AssertText("Du AAAhast mich");
			AssertSelection().Both(5, 0).NoNext();
		}
		
		[Test]
		public void P_Undo()
		{
			lines.SetText("Du hast mich");	
			
			ClipboardExecuter.PutToClipboard("AAA");
			Put(3, 0).Press("P");
			AssertText("Du AAAhast mich");
			AssertSelection("#1").Both(5, 0).NoNext();
			
			Press("u");
			AssertText("Du hast mich");
			AssertSelection("#2").Both(3, 0);
			Press(Keys.Control | Keys.R);
			AssertText("Du AAAhast mich");
			AssertSelection("#3").Both(5, 0);
		}
		
		[Test]
		public void dw_cw()
		{
			lines.SetText("Du hast mich gefragt");
			Put(0, 0).Press("2dw").AssertText("mich gefragt");
			
			lines.SetText("Du hast mich gefragt");
			Put(0, 0).Press("2cw").Press("AAA").PressCommandMode().AssertText("AAA mich gefragt");
		}
		
		[Test]
		public void e()
		{
			//             0123456789012345678901234567
			lines.SetText("Du hast ;. AAAA..lkd d  asdf");
			Put(3, 0).Press("e").AssertSelection().Both(6, 0);
			Press("e").AssertSelection().Both(9, 0);
			Put(8, 0).Press("e").AssertSelection().Both(9, 0);
			Press("e").AssertSelection().Both(14, 0);
			Press("e").AssertSelection().Both(16, 0);
			Press("e").AssertSelection().Both(19, 0);
			Press("e").AssertSelection().Both(21, 0);
			Press("e").AssertSelection().Both(27, 0);
			Put(20, 0).Press("e").AssertSelection().Both(21, 0);
			Put(22, 0).Press("e").AssertSelection().Both(27, 0);
			Put(23, 0).Press("e").AssertSelection().Both(27, 0);
		}
		
		[Test]
		public void Number_e_de()
		{
			//             0123456789012345678901234567
			lines.SetText("Du hast ;. AAAA..lkd d  asdf");
			Put(0, 0).Press("2e").AssertSelection().Both(6, 0);
			Put(8, 0).Press("5e").AssertSelection().Both(21, 0);
			Put(8, 0).Press("d2e").AssertSelection().Both(8, 0);
			AssertText("Du hast ..lkd d  asdf");
		}
		
		[Test]
		public void ce()
		{
			lines.SetText("Du hast mich");
			Put(3, 0).Press("ce").AssertSelection().Both(3, 0);
			Press("AAA").PressCommandMode().AssertText("Du AAA mich");
			AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void de()
		{
			//             0123456789012345678901234567
			lines.SetText("Du hast ;. AAAA..lkd d  asdf");
			Put(3, 0).Press("de").AssertText("Du  ;. AAAA..lkd d  asdf");
			AssertSelection().Both(3, 0);
			
			//             0123456789012345678901234567
			lines.SetText("Du hast ;. AAAA..lkd d  asdf");
			Put(15, 0).Press("de").AssertText("Du hast ;. AAAAlkd d  asdf");
			AssertSelection().Both(15, 0);
		}
		
		[Test]
		public void de_AtEnd()
		{
			lines.SetText(
			//   012345678901
				"Du hast mich\n" +
				"aaaa");
			Put(6, 0).Press("de").AssertText(
				"Du has\n" +
				"aaaa");
			AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void J()
		{
			lines.SetText(
			//   012345678901
				"Du hast mich\n" +
				"aaaa\n" +
				"bbbb");
			Put(3, 0).Press("J").AssertText("Du hast mich aaaa\nbbbb");
			AssertSelection().Both(12, 0);
		}
	}
}