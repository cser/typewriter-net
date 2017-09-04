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
			receiver.SetViMode(viMode ? ViMode.Normal : ViMode.Insert);
			Assert.AreEqual(viMode ? ViMode.Normal : ViMode.Insert, receiver.ViMode);
		}
		
		private void EscapeNormalViMode()
		{
			string viShortcut;
			bool scrollToCursor;
			receiver.DoKeyDown(Keys.OemOpenBrackets | Keys.Control, out viShortcut, out scrollToCursor);
			Assert.AreEqual(ViMode.Normal, receiver.ViMode);
		}
		
		private ViSimpleTest Press(string keys)
		{
			foreach (char c in keys)
			{
				string viShortcut;
				bool scrollToCursor;
				receiver.DoKeyPress(c, out viShortcut, out scrollToCursor);
			}
			return this;
		}
		
		private ViSimpleTest Press(Keys keysData)
		{
			string viShortcut;
			bool scrollToCursor;
			receiver.DoKeyDown(keysData, out viShortcut, out scrollToCursor);
			return this;
		}
		
		private ViSimpleTest PressCommandMode()
		{
			string viShortcut;
			bool scrollToCursor;
			receiver.DoKeyDown(Keys.Control | Keys.OemOpenBrackets, out viShortcut, out scrollToCursor);
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
			AssertSelection("Put").Both(iChar, iLine);
			return this;
		}
		
		private ViSimpleTest PutNew(int iChar, int iLine)
		{
			controller.PutNewCursor(new Place(iChar, iLine));
			return this;
		}
		
		[SetUp]
		public void SetUp()
		{
			ClipboardExecutor.Reset(true);
			Init();
			lines.lineBreak = "\n";
			receiver = new Receiver(controller, ViMode.Insert, false);
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
		public void fFtT_RepeatNuance2()
		{
			lines.SetText(
			//	 0123456789012345678901234
				"aaaa((cccc(dddd))eeeeeee)");
			
			Put(18, 0).Press("2F(").AssertSelection().Both(5, 0).NoNext();
			Press("F(").AssertSelection().Both(4, 0).NoNext();
			Press("2t)").AssertSelection().Both(15, 0).NoNext();
			Press("2T)").AssertSelection().Both(16, 0).NoNext();
			Put(6, 0).Press("2f)").AssertSelection().Both(16, 0).NoNext();
		}
		
		[Test]
		public void S6_0()
		{
			lines.SetText("Du hast\n   Du hast mich\n   a");
			
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
			lines.SetText("Du hast\n   \n\na");
			Put(1, 1).Press("^").AssertSelection().Both(2, 1).NoNext();
			Put(0, 2).Press("^").AssertSelection().Both(0, 2).NoNext();
		}
		
		[Test]
		public void S4()
		{
			lines.SetText("Du hast\n   Du hast mich\n   a");
			Put(4, 0).Press("$").AssertSelection().Both(6, 0).NoNext();
			Put(0, 0).Press("$").AssertSelection().Both(6, 0).NoNext();
			Put(1, 1).Press("$").AssertSelection().Both(14, 1).NoNext();
			Put(3, 2).Press("$").AssertSelection().Both(3, 2).NoNext();
		}
		
		[Test]
		public void S4_Repeat()
		{
			lines.SetText("Du hast\n   Du hast mich\n   a");
			Put(4, 0).Press("2").Press("$").AssertSelection().Both(14, 1).NoNext();
			Put(4, 0).Press("3").Press("$").AssertSelection().Both(3, 2).NoNext();
			Put(4, 0).Press("4").Press("$").AssertSelection().Both(3, 2).NoNext();
		}
		
		[Test]
		public void gg_G()
		{
			lines.SetText("Du hast\n   Du hast mich\naaaaaaa");
			Put(4, 1).Press("g").Press("g").AssertSelection().Both(0, 0).NoNext();
			Put(4, 1).Press("G").AssertSelection().Both(0, 2).NoNext();
			Put(4, 2).Press("G").AssertSelection().Both(0, 2).NoNext();
		}
		
		[Test]
		public void dgg()
		{
			lines.SetText("Du hast\n   Du hast mich\naaaaaaa");
			Put(4, 1).Press("dgg").AssertText("aaaaaaa").AssertSelection().Both(0, 0).NoNext();
			Assert.AreEqual("Du hast\n   Du hast mich\n", ClipboardExecutor.GetFromRegister(lines, '0'));
		}
		
		[Test]
		public void cgg()
		{
			lines.SetText("Du hast\n   Du hast mich\naaaaaaa");
			Put(4, 1).Press("cggx").AssertText("x\naaaaaaa").AssertSelection().Both(1, 0).NoNext();
			Assert.AreEqual("Du hast\n   Du hast mich\n", ClipboardExecutor.GetFromRegister(lines, '0'));
		}
		
		[Test]
		public void cgg2()
		{
			lines.SetText("Du hast\n   Du hast mich\naaaaaaa");
			Put(4, 2).Press("cggx").AssertText("x").AssertSelection().Both(1, 0).NoNext();
			Assert.AreEqual("Du hast\n   Du hast mich\naaaaaaa\n", ClipboardExecutor.GetFromRegister(lines, '0'));
		}
		
		[Test]
		public void ygg()
		{
			lines.SetText("Du hast\n   Du hast mich\naaaaaaa");
			Put(4, 1).Press("ygg").AssertSelection().Both(4, 1).NoNext();
			Assert.AreEqual("Du hast\n   Du hast mich\n", ClipboardExecutor.GetFromRegister(lines, '0'));
		}
		
		[Test]
		public void gg_VISUAL()
		{
			lines.SetText("  Du hast\nDu hast mich\naaaaaaa");
			Put(4, 1).Press("vgg").AssertSelection().Anchor(4, 1).Caret(2, 0).NoNext();
			Press("y");
			Assert.AreEqual("Du hast\nDu h", ClipboardExecutor.GetFromRegister(lines, '0'));
		}
		
		[Test]
		public void cG()
		{
			lines.SetText("Du hast\n   Du hast mich\naaaaaaa");
			Put(3, 1).Press("cGx").AssertText("Du hast\nx").AssertSelection().Both(1, 1);
		}
		
		[Test]
		public void dG2()
		{
			lines.SetText("Du hast\n   Du hast mich\naaaaaaa");
			Put(3, 1).Press("dG").AssertText("Du hast").AssertSelection().Both(0, 0);
			Press("u").AssertText("Du hast\n   Du hast mich\naaaaaaa");
		}
		
		[Test]
		public void cG2()
		{
			lines.SetText("Du hast\n   Du hast mich\naaaaaaa");
			Put(0, 0).Press("cG").AssertText("").AssertSelection().Both(0, 0);
			Assert.AreEqual("Du hast\n   Du hast mich\naaaaaaa\n", ClipboardExecutor.GetFromRegister(lines, '0'));
			Press(Keys.Control | Keys.OemOpenBrackets);
			Press("u").AssertText("Du hast\n   Du hast mich\naaaaaaa");
		}
		
		[Test]
		public void dG()
		{
			lines.SetText("Du hast\n   Du hast mich\naaaaaaa");
			Put(3, 0).Press("dG").AssertText("").AssertSelection().Both(0, 0);
			Press("u").AssertText("Du hast\n   Du hast mich\naaaaaaa");
		}
		
		[Test]
		public void dG3()
		{
			lines.SetText("Du hast\n   Du hast mich\naaaaaaa");
			Put(0, 0).Press("dG").AssertText("").AssertSelection().Both(0, 0);
			Assert.AreEqual("Du hast\n   Du hast mich\naaaaaaa\n", ClipboardExecutor.GetFromRegister(lines, '0'));
			Press("u").AssertText("Du hast\n   Du hast mich\naaaaaaa");
		}
		
		[Test]
		public void yG()
		{
			lines.SetText("Du hast\n   Du hast mich\naaaaaaa");
			Put(3, 0).Press("yG");
			Assert.AreEqual("Du hast\n   Du hast mich\naaaaaaa\n", ClipboardExecutor.GetFromRegister(lines, '0'));
		}
		
		[Test]
		public void yG_VISUAL()
		{
			lines.SetText("Du hast\n   Du hast mich\naaaaaaa");
			Put(3, 0).Press("vGy");
			Assert.AreEqual("hast\n   Du hast mich\n", ClipboardExecutor.GetFromRegister(lines, '0'));
		}
		
		[Test]
		public void yG2_VISUAL()
		{
			lines.SetText("Du hast\nDu hast mich\n  aaaaaaa");
			Put(3, 0).Press("vGy");
			Assert.AreEqual("hast\nDu hast mich\n  ", ClipboardExecutor.GetFromRegister(lines, '0'));
		}
		
		[Test]
		public void G_VISUAL()
		{
			lines.SetText("Du hast\nDu hast mich\n  aaaaaaa");
			Put(3, 1).Press("vG").AssertSelection().Anchor(3, 1).Caret(2, 2);
			Press(Keys.Control | Keys.OemOpenBrackets);
			Put(3, 1).Press("vkG").AssertSelection().Anchor(3, 1).Caret(2, 2);
		}
		
		[Test]
		public void hjkl()
		{
			lines.SetText(
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
		public void gjk_Simple()
		{
			lines.SetText(
			//	 012345678901234
				"Du hast\n" +
				"   Du hast mich\n" +
				"aaaaaaa");
			Put(4, 1).Press("gj").AssertSelection().Both(4, 2).NoNext();
			Put(4, 1).Press("gk").AssertSelection().Both(4, 0).NoNext();
		}
		
		[Test]
		public void gjk_Subline()
		{
			lines.SetText(
			//	 012345678901234
				"Abcd efg "/*br*/ +
				"hidklmnop\n" +
				"   qrstuw\n" +
				"aaaaaaa");
			lines.wordWrap = true;
			lines.wwValidator.Validate(10);
			Assert.AreEqual(4, lines.wwSizeY);
			Put(4, 0).Press("gj").AssertSelection().Both(13, 0).NoNext();
			Put(13, 0).Press("gk").AssertSelection().Both(4, 0).NoNext();
		}
		
		[Test]
		public void gjk_EndPosition()
		{
			lines.SetText(
			//	 012345678901234
				"Abcd efg "/*br*/ +
				"hidk\n" +
				"   qrstuw\n" +
				"aaaaaaa\n" +
				"bbbbbbbb");
			lines.wordWrap = true;
			lines.wwValidator.Validate(10);
			Assert.AreEqual(5, lines.wwSizeY);
			Put(8, 0).Press("gj").AssertSelection().Both(12, 0).NoNext();
			Put(7, 3).Press("gk").AssertSelection().Both(6, 2).NoNext();
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
			Put(3, 0).Press("3rx").AssertSelection().Both(5, 0).NoNext();
			AssertText("Du xxxt");
		}
		
		[Test]
		public void r_RepeatExtremal()
		{
			lines.SetText("Du hast");
			Put(3, 0).Press("3rx").AssertSelection().Both(5, 0).NoNext();
			AssertText("Du xxxt");
			Put(3, 0).Press("4ry").AssertSelection().Both(6, 0).NoNext();
			AssertText("Du yyyy");
			Put(3, 0).Press("5rz").AssertSelection().Both(3, 0).NoNext();
			AssertText("Du yyyy");
		}
		
		[Test]
		public void r_PreferredPos()
		{
			lines.SetText("Du hast\nDu hast mich");
			Put(3, 0).Press("rx").AssertSelection().Both(3, 0).NoNext();
			AssertText("Du xast\nDu hast mich");
			Press("j").AssertSelection().Both(3, 1);
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
		public void cw_AtTheLineEnd()
		{
			lines.SetText("Du hast\nDu hast mich");
			
			Put(3, 0).Press("cw").AssertSelection().Both(3, 0).NoNext();
			Press("x").AssertText("Du x\nDu hast mich").AssertSelection().Both(4, 0);
		}
		
		[Test]
		public void w_DocumentEnd()
		{
			lines.SetText("Abcdef");
			Put(2, 0).Press("w").AssertSelection().Both(5, 0);
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
		public void cB()
		{
			lines.SetText("Du a,hast!! mich");
			Put(11, 0).Press("cB").AssertSelection().Both(3, 0).NoNext();
			Press("BBB").PressCommandMode().AssertSelection().Both(5, 0).NoNext();
			AssertText("Du BBB mich");
			
			lines.SetText("Du a,hast!! mich");
			Put(12, 0).Press("cB").AssertSelection().Both(3, 0).NoNext();
			Press("BBB").PressCommandMode().AssertSelection().Both(5, 0).NoNext();
			AssertText("Du BBBmich");
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
			
			PutToViClipboard("");
			Put(3, 0).Press("yw");
			AssertViClipboard("hast ");
			AssertSelection().Both(3, 0).NoNext();
		}
		
		[Test]
		public void p()
		{
			lines.SetText("Du hast mich");	
			
			PutToViClipboard("AAA");
			Put(3, 0).Press("p");
			AssertText("Du hAAAast mich");
			AssertSelection().Both(6, 0).NoNext();
		}
		
		[Test]
		public void p_Undo()
		{
			lines.SetText("Du hast mich");
			
			PutToViClipboard("AAA");
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
			
			PutToViClipboard("AAA");
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
			
			PutToViClipboard("AAA");
			Put(3, 0).Press("P");
			AssertText("Du AAAhast mich");
			AssertSelection().Both(5, 0).NoNext();
		}
		
		[Test]
		public void P_Undo()
		{
			lines.SetText("Du hast mich");	
			
			PutToViClipboard("AAA");
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
		public void p_UndoRedo_Repeat()
		{
			lines.SetText("Du hast mich gefragt");
			Put(3, 0).Press("dw").AssertText("Du mich gefragt");
			Press("x").AssertText("Du ich gefragt");
			AssertSelection().Both(3, 0);
			
			Press("2u").AssertText("Du hast mich gefragt");
			AssertSelection().Both(3, 0);
			
			Press("2").Press(Keys.Control | Keys.R).AssertText("Du ich gefragt");
			AssertSelection().Both(3, 0);
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
		public void dW_cW()
		{
			lines.SetText("Du hast mich gefragt");
			Put(0, 0).Press("2dW").AssertText("mich gefragt");
			
			lines.SetText("Du,a,. hast! mich gefragt");
			Put(0, 0).Press("2dW").AssertText("mich gefragt");
			
			lines.SetText("Du hast mich gefragt");
			Put(0, 0).Press("2cW").Press("AAA").PressCommandMode().AssertText("AAA mich gefragt");
			
			lines.SetText("Du,a,. hast! mich gefragt");
			Put(0, 0).Press("2cW").Press("AAA").PressCommandMode().AssertText("AAA mich gefragt");
		}
		
		[Test]
		public void dE()
		{
			//             0123456789012345678901234567
			lines.SetText("Du hast ;. AAAA..lkd d  asdf");
			Put(3, 0).Press("dE").AssertText("Du  ;. AAAA..lkd d  asdf");
			AssertSelection().Both(3, 0);
			
			//             0123456789012345678901234567
			lines.SetText("Du hast ;. AAAA..lkd d  asdf");
			Put(11, 0).Press("dE").AssertText("Du hast ;.  d  asdf");
			AssertSelection().Both(11, 0);
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
		
		[Test]
		public void J_Indented()
		{
			lines.SetText(
			//   012345678901
				"Du hast mich\n" +
				"\taaaa\n" +
				"bbbb");
			Put(3, 0).Press("J").AssertText("Du hast mich aaaa\nbbbb");
			AssertSelection().Both(12, 0);
		}
		
		[Test]
		public void Registers()
		{
			lines.SetText(
			//   012345678901
				"Du hast mich\n" +
				"aaaa\n" +
				"bbbb");
			Put(3, 0).Press("\"ayw").AssertText("Du hast mich\naaaa\nbbbb");
			Assert.AreEqual("hast ", ClipboardExecutor.GetFromRegister(lines, 'a'));
			
			Put(8, 0).Press("\"bye").AssertText("Du hast mich\naaaa\nbbbb");
			Put(3, 0).Press("ye").AssertText("Du hast mich\naaaa\nbbbb");
			Assert.AreEqual("mich", ClipboardExecutor.GetFromRegister(lines, 'b'));
			Assert.AreEqual("hast ", ClipboardExecutor.GetFromRegister(lines, 'a'));
			Assert.AreEqual("hast", ClipboardExecutor.GetFromRegister(lines, '\0'));
			
			Put(0, 1).Press("\"ap").AssertText("Du hast mich\nahast aaa\nbbbb");
			Put(0, 2).Press("\"bp").AssertText("Du hast mich\nahast aaa\nbmichbbb");
			Put(0, 2).Press("p").AssertText("Du hast mich\nahast aaa\nbhastmichbbb");
		}
		
		[Test]
		public void d_Registers()
		{
			lines.SetText("Abcd efghij klmnop");
			Put(5, 0).Press("\"adw").AssertText("Abcd klmnop");
			Assert.AreEqual("efghij ", ClipboardExecutor.GetFromRegister(lines, 'a'));
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void iw()
		{
			lines.SetText("One two three");
			Put(5, 0).Press("diw").AssertText("One  three");
			AssertSelection().Both(4, 0);
		}
		
		[Test]
		public void aw()
		{
			lines.SetText("One two three");
			Put(5, 0).Press("daw").AssertText("One three");
			AssertSelection().Both(4, 0);
		}
		
		[TestCase("di{")]
		[TestCase("di}")]
		public void diBracket(string command)
		{
			lines.SetText("One {two three four} five");
			Put(6, 0).Press(command).AssertText("One {} five").AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void diBracket_Nested_Left()
		{
			lines.SetText("One {two {three} four} five");
			Put(6, 0).Press("di{").AssertText("One {} five").AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void diBracket_Nested_Right()
		{
			lines.SetText("One {two {three} four} five");
			Put(19, 0).Press("di{").AssertText("One {} five").AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void diBracket_NestedNear_Left()
		{
			lines.SetText("One {{two three} four} five");
			Put(4, 0).Press("di{").AssertText("One {} five").AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void diBracket_NestedNear_Right()
		{
			lines.SetText("One {two {three four}} five");
			Put(21, 0).Press("di{").AssertText("One {} five").AssertSelection().Both(5, 0);
		}
		
		[TestCase("One two three four five")]
		[TestCase("One {two three four five")]
		[TestCase("One two three four}} five")]
		public void diBracket_Empty(string text)
		{
			lines.SetText(text);
			Put(6, 0).Press("di{").AssertText(text).AssertSelection().Both(6, 0);
		}
		
		[Test]
		public void daBracket()
		{
			lines.SetText("One {two three four} five");
			Put(6, 0).Press("da{").AssertText("One  five").AssertSelection().Both(4, 0);
		}
		
		[Test]
		public void diBracket_Repeat()
		{
			lines.SetText("One {two {three} four} five");
			Put(14, 0).Press("2di{").AssertText("One {} five").AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void diBracket_Repeat_Complex()
		{
			lines.SetText("{One {two {three} four} five}");
			Put(14, 0).Press("2di{").AssertText("{One {} five}").AssertSelection().Both(6, 0);
		}
		
		[TestCase(9)]
		[TestCase(15)]
		public void diBracket_Repeat_StartsWithKet(int position)
		{
			lines.SetText("One {two {three} four} five");
			Put(position, 0).Press("2di{").AssertText("One {} five").AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void diString()
		{
			lines.SetText("One \"two three\" four \"five\"");
			Put(7, 0).Press("di\"").AssertText("One \"\" four \"five\"").AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void diString2()
		{
			lines.SetText("One 'two three' four 'five'");
			Put(7, 0).Press("di'").AssertText("One '' four 'five'").AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void daString()
		{
			lines.SetText("One \"two three\" four \"five\"");
			Put(7, 0).Press("da\"").AssertText("One  four \"five\"").AssertSelection().Both(4, 0);
		}
		
		[Test]
		public void diString_BeforeEscaped()
		{
			lines.SetText("One 'two \\'three' four 'five'");
			Put(7, 0).Press("di'").AssertText("One '' four 'five'").AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void diString_AfterEscaped()
		{
			lines.SetText("One 'two \\'three' four 'five'");
			Put(14, 0).Press("di'").AssertText("One '' four 'five'").AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void diString_BeforeEscapedSlash()
		{
			lines.SetText(@"One 'two \\'three' four 'five'");
			Put(7, 0).Press("di'").AssertText("One ''three' four 'five'").AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void diString_AfterEscapedSlash()
		{
			lines.SetText(@"One 'two \\'three' four 'five'");
			Put(14, 0).Press("di'").AssertText("One ''three' four 'five'").AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void diString_EscapedComplex()
		{
			lines.SetText("One \"two \\\"'three'\" four five");
			Put(7, 0).Press("di\"").AssertText("One \"\" four five").AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void diString_EscapedComplex2()
		{
			lines.SetText(@"One 'two \\th\\nree' four five");
			Put(7, 0).Press("di'").AssertText("One '' four five").AssertSelection().Both(5, 0);
		}
		
		[Test]
		public void Percents_ToRight()
		{
			lines.SetText("012{abc}d");
			Put(3, 0).Press("%").AssertSelection().Both(7, 0);
		}
		
		[Test]
		public void Percents_ToRight2()
		{
			lines.SetText("012(abc)d");
			Put(3, 0).Press("%").AssertSelection().Both(7, 0);
		}
		
		[Test]
		public void Percents_ToRight3()
		{
			lines.SetText("012[abc]d");
			Put(3, 0).Press("%").AssertSelection().Both(7, 0);
		}
		
		[Test]
		public void Percents_ToLeft()
		{
			lines.SetText("012{abc}d");
			Put(7, 0).Press("%").AssertSelection().Both(3, 0);
		}
		
		[Test]
		public void s()
		{
			lines.SetText("01234567");
			Put(6, 0).Press("s").AssertSelection().Both(6, 0);
			Press("AB").PressCommandMode().AssertText("012345AB7");
			AssertSelection().Both(7, 0);
		}
		
		[Test]
		public void s_Repeat()
		{
			lines.SetText("01234567");
			Put(2, 0).Press("3sAB").PressCommandMode().AssertText("01AB567");
			AssertSelection().Both(3, 0);
		}
		
		[Test]
		public void Percents_StartsWithFirstBracketOnLine1()
		{
			lines.SetText("012{abc}d");
			Put(1, 0).Press("%").AssertSelection().Both(7, 0);
		}
		
		[Test]
		public void Percents_StartsWithFirstBracketOnLine2()
		{
			lines.SetText("012\n{abc}d");
			Put(1, 0).Press("%").AssertSelection().Both(1, 0);
		}
		
		[Test]
		public void Percents_d()
		{
			lines.SetText("012{abc}d");
			Put(3, 0).Press("d%").AssertText("012d").AssertSelection().Both(3, 0);
		}
		
		[Test]
		public void Percents_Selection()
		{
			lines.SetText("012{abc}d");
			Put(3, 0).Press("v%").AssertSelection().Anchor(3, 0).Caret(8, 0);
		}
		
		[Test]
		public void dd()
		{
			lines.SetText(
				"Darf ich leben ohne Grenzen?\n" +
				"Nein, das darfst du nicht\n" +
				"Lieben trotz der Konsequenzen\n" +
				"Nein, das darfst du nicht");
			Put(2, 1).Press("dd").PressCommandMode().AssertText(
				"Darf ich leben ohne Grenzen?\n" +
				"Lieben trotz der Konsequenzen\n" +
				"Nein, das darfst du nicht");
			AssertSelection().Both(0, 1);
			Assert.AreEqual("Nein, das darfst du nicht\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
			
			Put(2, 1).Press("\"ddd").PressCommandMode().AssertText(
				"Darf ich leben ohne Grenzen?\n" +
				"Nein, das darfst du nicht");
			Assert.AreEqual("Lieben trotz der Konsequenzen\n", ClipboardExecutor.GetFromRegister(lines, 'd'));
		}
		
		[Test]
		public void dd_Repeat()
		{
			lines.SetText(
				"Darf ich leben ohne Grenzen?\n" +
				"Nein, das darfst du nicht\n" +
				"Lieben trotz der Konsequenzen\n" +
				"Nein, das darfst du nicht");
			Put(2, 1).Press("2dd").PressCommandMode().AssertText(
				"Darf ich leben ohne Grenzen?\n" +
				"Nein, das darfst du nicht");
			AssertSelection().Both(0, 1);
			Assert.AreEqual(
				"Nein, das darfst du nicht\n" +
				"Lieben trotz der Konsequenzen\n",
				ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void dd_Repeat_Overflow()
		{
			lines.SetText(
				"Darf ich leben ohne Grenzen?\n" +
				"Nein, das darfst du nicht\n" +
				"Lieben trotz der Konsequenzen\n" +
				"Nein, das darfst du nicht");
			Put(2, 2).Press("3dd").PressCommandMode().AssertText(
				"Darf ich leben ohne Grenzen?\n" +
				"Nein, das darfst du nicht");
			AssertSelection().Both(0, 1);
			Assert.AreEqual(
				"Lieben trotz der Konsequenzen\n" +
				"Nein, das darfst du nicht\n",
				ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void dd_Indented()
		{
			lines.SetText(
				"Darf ich leben ohne Grenzen?\n" +
				"Nein, das darfst du nicht\n" +
				"    Lieben trotz der Konsequenzen\n" +
				"    Nein, das darfst du nicht");
			Put(2, 1).Press("dd").PressCommandMode().AssertText(
				"Darf ich leben ohne Grenzen?\n" +
				"    Lieben trotz der Konsequenzen\n" +
				"    Nein, das darfst du nicht");
			AssertSelection().Both(4, 1);
		}
		
		[Test]
		public void dd_EndLine_SeveralLines()
		{
			lines.SetText(
				"Darf ich leben ohne Grenzen?\n" +
				"Nein, das darfst du nicht\n" +
				"Lieben trotz der Konsequenzen\n" +
				"Nein, das darfst du nicht");
			Put(3, 2).Press("2dd").PressCommandMode().AssertText(
				"Darf ich leben ohne Grenzen?\n" +
				"Nein, das darfst du nicht");
			AssertSelection().Both(0, 1);
			Assert.AreEqual(
				"Lieben trotz der Konsequenzen\n" +
				"Nein, das darfst du nicht\n",
				ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void dd_EndLine_SingleLine()
		{
			lines.SetText("Darf ich leben ohne Grenzen?");
			Put(3, 0).Press("2dd").PressCommandMode().AssertText("");
			AssertSelection().Both(0, 0);
			Assert.AreEqual("Darf ich leben ohne Grenzen?\n",
				ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void p_Lines()
		{
			lines.SetText(
				"Mein Leben könnte gar nicht besser sein\n" +
				"    Und plötzlich läufst du mir ins Messer rein\n" +
				"    Du sagst zum Leben fehlt mir der Mut");
			ClipboardExecutor.PutToRegister('\0', " aaaaaa\n");
			Put(10, 1).Press("p").AssertText(
				"Mein Leben könnte gar nicht besser sein\n" +
				"    Und plötzlich läufst du mir ins Messer rein\n" +
				" aaaaaa\n" +
				"    Du sagst zum Leben fehlt mir der Mut");
			AssertSelection().Both(1, 2);
		}
		
		[Test]
		public void p_Lines_Multiline()
		{
			lines.SetText(
				"Mein Leben könnte gar nicht besser sein\n" +
				"    Und plötzlich läufst du mir ins Messer rein\n" +
				"    Du sagst zum Leben fehlt mir der Mut");
			ClipboardExecutor.PutToRegister('\0', " aaaaaa\nbbbbbbbb\n");
			Put(10, 1).Press("p").AssertText(
				"Mein Leben könnte gar nicht besser sein\n" +
				"    Und plötzlich läufst du mir ins Messer rein\n" +
				" aaaaaa\n" +
				"bbbbbbbb\n" +
				"    Du sagst zum Leben fehlt mir der Mut");
			AssertSelection().Both(1, 2);
		}
		
		[Test]
		public void P_Lines()
		{
			lines.SetText(
				"Mein Leben könnte gar nicht besser sein\n" +
				"    Und plötzlich läufst du mir ins Messer rein\n" +
				"    Du sagst zum Leben fehlt mir der Mut");
			ClipboardExecutor.PutToRegister('\0', " aaaaaa\nbbbbbbbb\n");
			Put(10, 1).Press("P").AssertText(
				"Mein Leben könnte gar nicht besser sein\n" +
				" aaaaaa\n" +
				"bbbbbbbb\n" +
				"    Und plötzlich läufst du mir ins Messer rein\n" +
				"    Du sagst zum Leben fehlt mir der Mut");
			AssertSelection().Both(1, 1);
		}
		
		[Test]
		public void p_Lines_Repeat()
		{
			lines.SetText(
				"11111\n" +
				"222");
			ClipboardExecutor.PutToRegister('\0', "aa\nbb\n");
			Put(1, 0).Press("pp").AssertText(
				"11111\n" +
				"aa\n" +
				"aa\n" +
				"bb\n" +
				"bb\n" +
				"222");
			AssertSelection().Both(0, 2);
			
			lines.SetText(
				"11111\n" +
				"222");
			ClipboardExecutor.PutToRegister('\0', "aa\nbb\n");
			Put(1, 0).Press("2p").AssertText(
				"11111\n" +
				"aa\n" +
				"bb\n" +
				"aa\n" +
				"bb\n" +
				"222");
			AssertSelection().Both(0, 1);
		}
		
		[Test]
		public void Dot_dw()
		{
			lines.SetText("In meiner Hand ein Bild von dir");
			Put(3, 0).Press("2dw").AssertText("In ein Bild von dir");
			AssertSelection().Both(3, 0);
			Press(".").AssertText("In von dir");
			AssertSelection().Both(3, 0);
		}
		
		[Test]
		public void Dot_cw()
		{
			lines.SetText("In meiner Hand ein Bild von dir");
			Put(3, 0).Press("2cw[_]").AssertText("In [_] ein Bild von dir");
			EscapeNormalViMode();
			AssertSelection().Both(5, 0);
			Press(".").AssertText("In [_[_] Bild von dir");
			AssertSelection().Both(7, 0);
		}
		
		[Test]
		public void Dot_s()
		{
			lines.SetText("In meiner Hand ein Bild von dir");
			Put(3, 0).Press("2sX").AssertText("In Xiner Hand ein Bild von dir");
			EscapeNormalViMode();
			AssertSelection().Both(3, 0);
			Put(14, 0).Press(".").AssertText("In Xiner Hand Xn Bild von dir");
			AssertSelection().Both(14, 0);
		}
		
		[Test]
		public void Dot_df_Repeat()
		{
			lines.SetText("In meiner Hand ein Bild von dir");
			Put(3, 0).Press("dfi").AssertText("In ner Hand ein Bild von dir");
			AssertSelection().Both(3, 0);
			Press("2.").AssertText("In ld von dir");
			AssertSelection().Both(3, 0);
		}
		
		[Test]
		public void Dot_dw_MovesIgnoredByDot()
		{
			lines.SetText("In meiner Hand ein Bild von dir");
			Put(3, 0).Press("dw").AssertText("In Hand ein Bild von dir");
			AssertSelection().Both(3, 0);
			Press("e");
			AssertSelection().Both(6, 0);
			Press(".").AssertText("In Hanein Bild von dir");
		}
		
		[Test]
		public void yy()
		{
			lines.SetText("Oooo\naaaa\nccc\ndddddddd");
			
			Put(1, 1).Press("yy");
			Assert.AreEqual("aaaa\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
			AssertSelection().Both(1, 1);
			
			Put(1, 1).Press("2yy");
			Assert.AreEqual("aaaa\nccc\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
			AssertSelection().Both(1, 1);
		}
		
		[Test]
		public void Y()
		{
			lines.SetText("Oooo\naaaa\nccc\ndddddddd");
			
			Put(1, 1).Press("Y");
			Assert.AreEqual("aaaa\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
			AssertSelection().Both(1, 1);
			
			Put(1, 1).Press("2Y");
			Assert.AreEqual("aaaa\nccc\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
			AssertSelection().Both(1, 1);
		}
		
		[Test]
		public void Y_Selection()
		{
			lines.SetText("Oooo\naaaa\nccc\ndddddddd");
			
			Put(3, 0).Press("vj").Press("Y");
			Assert.AreEqual("Oooo\naaaa\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void xp()
		{
			lines.SetText("abcd");
			
			Put(1, 0).Press("xp");
			AssertText("acbd");
			AssertSelection().Both(2, 0);
		}
		
		[Test]
		public void RemoveLines()
		{
			lines.SetText("Oooo\naaaa\nccc\ndddddddd");
			Put(1, 1).Press("V").Press("j").Press("d");
			AssertText("Oooo\ndddddddd");
		}
		
		[Test]
		public void Shift()
		{
			lines.SetText("Oooo\naaaa\n\tccc\ndddddddd");
			Put(1, 1).Press(">>");
			AssertText("Oooo\n\taaaa\n\tccc\ndddddddd");
			Press("<<");
			AssertText("Oooo\naaaa\n\tccc\ndddddddd");
			Press("2<<");
			AssertText("Oooo\naaaa\nccc\ndddddddd");
		}
		
		[Test]
		public void Shift_UndoRedo()
		{
			lines.SetText("Oooo\naaaa\n\tccc\ndddddddd");
			Put(1, 1).Press(">>");
			AssertText("Oooo\n\taaaa\n\tccc\ndddddddd");
			controller.processor.Undo();
			lines.SetText("Oooo\naaaa\n\tccc\ndddddddd");
			controller.processor.Redo();
			AssertText("Oooo\n\taaaa\n\tccc\ndddddddd");
			Press("<<");
			AssertText("Oooo\naaaa\n\tccc\ndddddddd");
			controller.processor.Undo();
			AssertText("Oooo\n\taaaa\n\tccc\ndddddddd");
			controller.processor.Redo();
			AssertText("Oooo\naaaa\n\tccc\ndddddddd");
		}
		
		[Test]
		public void Shift_VISUAL()
		{
			lines.SetText("Oooo\naaaa\n\tccc\ndddddddd");
			Put(1, 1).Press("v").Press("2>");
			AssertText("Oooo\n\t\taaaa\n\tccc\ndddddddd");
		}
		
		[Test]
		public void C()
		{
			lines.SetText("Oooo\naaaa\n\tccc\ndddddddd");
			Put(1, 1).Press("C");
			AssertText("Oooo\na\n\tccc\ndddddddd");
			AssertViClipboard("aaa");
			AssertSelection().Both(1, 1).NoNext();
		}
		
		[Test]
		public void C_Repeat()
		{
			lines.SetText("Oooo\naaaa\n\tccc\ndddddddd");
			Put(1, 1).Press("2").Press("C");
			AssertText("Oooo\na\ndddddddd");
			AssertViClipboard("aaa\n\tccc");
			AssertSelection().Both(1, 1).NoNext();
		}
		
		[Test]
		public void D()
		{
			lines.SetText("Oooo\naaaa\n\tccc\ndddddddd");
			Put(2, 1).Press("D");
			AssertText("Oooo\naa\n\tccc\ndddddddd");
			AssertViClipboard("aa");
			AssertSelection().Both(1, 1).NoNext();
		}
		
		[Test]
		public void D_Repeat()
		{
			lines.SetText("Oooo\naaaa\n\tccc\ndddddddd");
			Put(1, 1).Press("2").Press("D");
			AssertText("Oooo\na\ndddddddd");
			AssertViClipboard("aaa\n\tccc");
			AssertSelection().Both(0, 1).NoNext();
		}
		
		[Test]
		public void cc()
		{
			lines.SetText("Oooo\naaaa\n\tccc\ndddddddd");
			Put(2, 1).Press("cc");
			AssertText("Oooo\n\n\tccc\ndddddddd");
			AssertViClipboard("aaaa\n");
			AssertSelection().Both(0, 1).NoNext();
		}
		
		[Test]
		public void cc_FirstLine()
		{
			lines.SetText("Oooo\naaaa\n\tccc\ndddddddd");
			Put(2, 0).Press("cc");
			AssertText("\naaaa\n\tccc\ndddddddd");
			AssertViClipboard("Oooo\n");
			AssertSelection().Both(0, 0).NoNext();
		}
		
		[Test]
		public void cc_LastLine()
		{
			lines.SetText("Oooo\naaaa\nccc\ndddddddd");
			Put(2, 3).Press("cc");
			AssertText("Oooo\naaaa\nccc\n");
			AssertViClipboard("dddddddd\n");
			AssertSelection().Both(0, 3).NoNext();
		}
		
		[Test]
		public void InsideFileBookmarks_JumpToPosition()
		{
			lines.SetText("Oooo\naaaa\nccc\ndddddddd");
			Put(2, 3).Press("ma");
			Put(3, 1).Press("mz");
			Put(1, 0).AssertSelection().Both(1, 0);
			Press("`a").AssertSelection().Both(2, 3);
			Press("`z").AssertSelection().Both(3, 1);
			Press("`a").AssertSelection().Both(2, 3);
		}
		
		[Test]
		public void InsideFileBookmarks_JumpToLine()
		{
			lines.SetText("Oooo\naaaa\nccc\n    dddddddd");
			Put(7, 3).Press("ma");
			Put(1, 0).Press("`a").AssertSelection().Both(7, 3);
			Put(1, 0).Press("'a").AssertSelection().Both(4, 3);
		}
		
		[Test]
		public void InsideFileBookmarks_JumpToPosition_VISUAL()
		{
			lines.SetText("Oooo\naaaa\n\tccc\n    dddddddd");
			Put(7, 3).Press("ma");
			Put(1, 0).Press("v");
			Press("`a").AssertSelection().Anchor(1, 0).Caret(7, 3);
		}
		
		[Test]
		public void InsideFileBookmarks_JumpToLine_VISUAL()
		{
			lines.SetText("Oooo\naaaa\n\tccc\n    dddddddd");
			Put(7, 3).Press("ma");
			Put(1, 0).Press("v");
			Press("'a").AssertSelection().Anchor(1, 0).Caret(4, 3);
		}
		
		[Test]
		public void InsideFileBookmarks_JumpToPosition_Delete()
		{
			lines.SetText("Oooo\naaaa\n\tccc\n    dddddddd");
			Put(7, 3).Press("ma");
			Put(2, 1).Press("d`a").AssertText("Oooo\naaddddd").AssertSelection().Both(2, 1);
		}
		
		[Test]
		public void InsideFileBookmarks_JumpToLine_Delete()
		{
			lines.SetText("Oooo\naaaa\n\tccc\n    dddddddd\neeee");
			Put(7, 3).Press("ma");
			Put(2, 1).Press("d'a").AssertText("Oooo\neeee").AssertSelection().Both(0, 1);
		}
		
		[Test]
		public void InsideFileBookmarks_JumpToLine_Delete2()
		{
			lines.SetText("Oooo\naaaa\n\tccc\n    dddddddd\neeee");
			Put(7, 3).Press("ma");
			Put(2, 0).Press("d'a").AssertText("eeee").AssertSelection().Both(0, 0);
		}
		
		[Test]
		public void InsideFileBookmarks_JumpToLine_Delete3()
		{
			lines.SetText("Oooo\naaaa\n\tccc\n    dddddddd\neeee");
			Put(3, 1).Press("ma");
			Put(2, 3).Press("d'a").AssertText("Oooo\neeee").AssertSelection().Both(0, 1);
		}
		
		[Test]
		public void InsideFileBookmarks_JumpToLine_Clipboard()
		{
			lines.SetText("Oooo\naaaa\n\tccc\n    dddddddd\neeee");
			Put(7, 3).Press("ma");
			Put(2, 1).Press("d'a").AssertText("Oooo\neeee").AssertSelection().Both(0, 1);
			Assert.AreEqual("aaaa\n\tccc\n    dddddddd\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void InsideFileBookmarks_JumpToLine_Clipboard2()
		{
			lines.SetText("Oooo\naaaa\n\tccc\n    dddddddd\neeee");
			Put(2, 4).Press("ma");
			Put(2, 1).Press("d'a").AssertText("Oooo").AssertSelection().Both(0, 0);
			Assert.AreEqual("aaaa\n\tccc\n    dddddddd\neeee\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void InsideFileBookmarks_CopyToLine()
		{
			lines.SetText("Oooo\naaaa\n\tccc\n    dddddddd\neeee");
			Put(7, 3).Press("ma");
			Put(2, 1).Press("y'a").AssertSelection().Both(2, 1);
			Assert.AreEqual("aaaa\n\tccc\n    dddddddd\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void InsideFileBookmarks_CopyToLine_Last()
		{
			lines.SetText("Oooo\naaaa\n\tccc\n    dddddddd\neeee");
			Put(1, 4).Press("ma");
			Put(2, 1).Press("y'a").AssertSelection().Both(2, 1);
			Assert.AreEqual("aaaa\n\tccc\n    dddddddd\neeee\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void InsideFileBookmarks_ChangeToLine()
		{
			lines.SetText("Oooo\naaaa\n\tccc\n    dddddddd\neeee");
			Put(7, 3).Press("ma");
			Put(2, 1).Press("c'a").AssertText("Oooo\n\neeee").AssertSelection().Both(0, 1);
		}
		
		[Test]
		public void UpperLower_VISUAL()
		{
			lines.SetText("Abcdefghij");
			Put(2, 0).Press("vlll").AssertSelection().Anchor(2, 0).Caret(5, 0);
			Press("U").AssertText("AbCDEfghij").AssertSelection().Both(2, 0);
			Put(0, 0).Press("vl").AssertSelection().Anchor(0, 0).Caret(1, 0);
			Press("u").AssertText("abCDEfghij").AssertSelection().Both(0, 0);
		}
		
		[Test]
		public void UpperLower()
		{
			lines.SetText("abcdef");
			Put(1, 0).Press("~").AssertText("aBcdef").AssertSelection().Both(2, 0);
			Put(1, 0).Press("~").AssertText("abcdef").AssertSelection().Both(2, 0);
			Put(1, 0).Press("2~").AssertText("aBCdef").AssertSelection().Both(3, 0);
		}
		
		[Test]
		public void LastInput()
		{
			lines.SetText("abcdef");
			Put(2, 0).Press("iABC").AssertText("abABCcdef");
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(lines, '.'));
			Press(Keys.OemOpenBrackets | Keys.Control);
			Assert.AreEqual("ABC", ClipboardExecutor.GetFromRegister(lines, '.'));
		}
		
		[Test]
		public void PasteLineAtTheEnd()
		{
			lines.SetText("line0\nline1\nline2");
			Put(2, 1).Press("yyjp").AssertText("line0\nline1\nline2\nline1").AssertSelection().Both(0, 3);
		}
		
		[Test]
		public void PasteLineAtTheEnd2()
		{
			lines.lineBreak = "\r\n";
			lines.SetText("line0\r\nline1\r\nline2");
			Put(2, 1).Press("yyjp").AssertText("line0\r\nline1\r\nline2\r\nline1");
		}
		
		[Test]
		public void PutCursorAtStartAfterCopy()
		{
			lines.SetText("Abcd efgh ijk");
			Put(5, 0).Press("vllll").AssertSelection().Anchor(5, 0).Caret(9, 0).NoNext();
			Press("y");
			Assert.AreEqual("efgh", ClipboardExecutor.GetFromRegister(lines, '\0'));
			AssertSelection().Both(5, 0).NoNext();
		}
		
		[Test]
		public void PutCursorAtStartAfterCopy_LINES()
		{
			lines.SetText("line0\n  line1\nline2\nline3");
			Put(3, 1).Press("Vjy");
			Assert.AreEqual("  line1\nline2\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
			AssertSelection().Both(3, 1).NoNext();
		}
		
		[Test]
		public void DotLineInsertion()
		{
			lines.SetText("line0\n  line1\nline2\nline3");
			Put(3, 1).Press("oABC").Press(Keys.Control | Keys.OemOpenBrackets);
			AssertText("line0\n  line1\n  ABC\nline2\nline3");
			Put(1, 3).Press(".");
			AssertText("line0\n  line1\n  ABC\nline2\nABC\nline3");
		}
		
		[Test]
		public void DotIndent()
		{
			lines.SetText("line0\nline1\nline2\nline3");
			Put(3, 1).Press(">>.");
			AssertText("line0\n\t\tline1\nline2\nline3");
			Put(3, 1).Press("<<.");
			AssertText("line0\nline1\nline2\nline3");
		}
		
		[Test]
		public void DotIndentRight_VISUAL()
		{
			lines.SetText("line0\nline1\nline2\nline3");
			Put(3, 1).Press("vj").AssertSelection().Anchor(3, 1).Caret(3, 2);
			Press(">.");
			AssertText("line0\n\t\tline1\n\t\tline2\nline3");
		}
		
		[Test]
		public void DotIndentLeft_VISUAL()
		{
			lines.SetText("line0\n\t\tline1\n\t\tline2\nline3");
			Put(3, 1).Press("vj").AssertSelection().Anchor(3, 1).Caret(3, 2);
			Press("<.");
			AssertText("line0\nline1\nline2\nline3");
		}
		
		[Test]
		public void IndentRightPosition_VISUAL()
		{
			lines.SetText("line0\nline1\nline2\nline3");
			Put(3, 1).Press("vj").AssertSelection().Anchor(3, 1).Caret(3, 2);
			Press(">");
			AssertText("line0\n\tline1\n\tline2\nline3").AssertSelection().Both(1, 1);
		}
		
		[Test]
		public void IndentLeftPosition_VISUAL()
		{
			lines.SetText("line0\n\tline1\n\tline2\nline3");
			Put(3, 1).Press("vj").AssertSelection().Anchor(3, 1).Caret(3, 2);
			Press("<");
			AssertText("line0\nline1\nline2\nline3").AssertSelection().Both(0, 1);
		}
		
		[Test]
		public void DotRemoving_VISUAL()
		{
			lines.SetText("line0\nabcd\nefghij\nline3");
			Put(3, 1).Press("vj").AssertSelection().Anchor(3, 1).Caret(3, 2);
			Press("x");
			AssertText("line0\nabchij\nline3").AssertSelection().Both(3, 1);
			Press(".");
			AssertText("line0\nabce3").AssertSelection().Both(3, 1);
		}
		
		[Test]
		public void w_ToEnd()
		{
			lines.SetText("Abcd\nEfgh ijkl");
			Put(7, 1).Press("w").AssertSelection().Both(8, 1);
		}
		
		[Test]
		public void w_ToEnd2()
		{
			lines.SetText("Abcd\nEfgh ijkl ");
			Put(7, 1).Press("w").AssertSelection().Both(9, 1);
		}
		
		[Test]
		public void w_ToEnd3()
		{
			lines.SetText("Abcd\nEfgh ijkl a");
			Put(7, 1).Press("w").AssertSelection().Both(10, 1);
		}
		
		[Test]
		public void w_ToEnd_VISUAL()
		{
			lines.SetText("Abcd\nEfgh ijkl");
			Put(7, 1).Press("vw").AssertSelection().Anchor(7, 1).Caret(9, 1);
		}
		
		[Test]
		public void dw_ToEnd()
		{
			lines.SetText("Abcd\nEfgh ijkl");
			Put(7, 1).Press("dw").AssertText("Abcd\nEfgh ij").AssertSelection().Both(6, 1);
		}
		
		[Test]
		public void cw_ToEnd()
		{
			lines.SetText("Abcd\nEfgh ijkl");
			Put(7, 1).Press("cw").AssertText("Abcd\nEfgh ij").AssertSelection().Both(7, 1);
		}
		
		[Test]
		public void dw_AtEnd()
		{
			lines.SetText("Abcd efg\nhij");
			Put(5, 0).Press("dw").AssertText("Abcd \nhij").AssertSelection().Both(4, 0);
		}
		
		[Test]
		public void dw_AtEnd2()
		{
			lines.SetText("Abcd efg\n\rhij");
			Put(5, 0).Press("dw").AssertText("Abcd \n\rhij").AssertSelection().Both(4, 0);
		}
		
		[Test]
		public void dw_AtEnd3()
		{
			lines.SetText("Abcd efg\rhij");
			Put(5, 0).Press("dw").AssertText("Abcd \rhij").AssertSelection().Both(4, 0);
		}
		
		[Test]
		public void w_AtEnd_VISUAL()
		{
			lines.SetText("Abcd efg\nhij");
			Put(5, 0).Press("vw").AssertSelection().Anchor(5, 0).Caret(0, 1);
		}
		
		[Test]
		public void yl_AtEnd()
		{
			lines.SetText("Abcd");
			Put(3, 0).Press("yl").AssertSelection().Both(3, 0);
			Assert.AreEqual("d", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void c_LINES()
		{
			lines.SetText("Oooo\naaaa\n\tccc\ndddddddd");
			Put(2, 1).Press("Vc");
			AssertText("Oooo\n\n\tccc\ndddddddd");
			AssertViClipboard("aaaa\n");
			AssertSelection().Both(0, 1).NoNext();
		}
		
		[Test]
		public void c2_LINES()
		{
			lines.SetText("Oooo\naaaa\n\tccc\ndddddddd");
			Put(2, 1).Press("Vjc");
			AssertText("Oooo\n\ndddddddd");
			AssertViClipboard("aaaa\n\tccc\n");
			AssertSelection().Both(0, 1).NoNext();
		}
		
		[Test]
		public void c_Indentation_LINES()
		{
			lines.SetText("Oooo\n\taaaa\n\t\tccc\ndddddddd");
			Put(2, 1).Press("Vjc");
			AssertText("Oooo\n\t\ndddddddd");
			AssertViClipboard("\taaaa\n\t\tccc\n");
			AssertSelection().Both(1, 1).NoNext();
		}
		
		[Test]
		public void c_Indentation_LINES_Undo()
		{
			lines.SetText("Oooo\n\taaaa\n\t\tccc\ndddddddd");
			Put(2, 1).Press("Vjc");
			AssertText("Oooo\n\t\ndddddddd");
			AssertViClipboard("\taaaa\n\t\tccc\n");
			AssertSelection().Both(1, 1).NoNext();
			
			Press(Keys.Control | Keys.OemOpenBrackets).Press("u");
			AssertText("Oooo\n\taaaa\n\t\tccc\ndddddddd");
		}
		
		[Test]
		public void UpperLower_Multicaret()
		{
			lines.SetText("Oooo\naaaa\nccc\ndddddddd");
			Put(2, 1).PutNew(2, 2).Press("~");
			AssertText("Oooo\naaAa\nccC\ndddddddd").AssertSelection("#1").Both(3, 1).Next().Both(3, 2).NoNext();
			Press(Keys.Control | Keys.OemOpenBrackets);
			Put(2, 1).PutNew(2, 2).Press("~");
			AssertText("Oooo\naaaa\nccc\ndddddddd").AssertSelection("#2").Both(3, 1).Next().Both(3, 2).NoNext();
		}
		
		[Test]
		public void gv_DontFail()
		{
			lines.SetText("Abcdefg");
			Put(2, 0).Press("v5lyD").AssertText("Ab");
			Press("gv").AssertSelection("#1").Both(2, 0);
		}
		
		[Test]
		public void di_Brackets()
		{
			lines.SetText("\taaa{\n\t\tbbb\n\t\tccc\n\t}\n\tddd");
			Put(3, 1).Press("di{").AssertText("\taaa{\n\t}\n\tddd");
			Assert.AreEqual("\t\tbbb\n\t\tccc\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void ci_Brackets()
		{
			lines.SetText("\taaa{\n\t\tbbb\n\t\tccc\n\t}\n\tddd");
			Put(3, 1).Press("ci{x").AssertText("\taaa{\n\t\tx\n\t}\n\tddd");
			Assert.AreEqual("\t\tbbb\n\t\tccc\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void yi_Brackets()
		{
			lines.SetText("\taaa{\n\t\tbbb\n\t\tccc\n\t}\n\tddd");
			Put(3, 1).Press("yi{");
			Assert.AreEqual("\t\tbbb\n\t\tccc\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void da_Brackets()
		{
			lines.SetText("\taaa{\n\t\tbbb\n\t\tccc\n\t}\n\tddd");
			Put(3, 1).Press("da{").AssertText("\taaa\n\tddd");
			Assert.AreEqual("{\n\t\tbbb\n\t\tccc\n\t}", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void da_Brackets2()
		{
			lines.SetText("\taaa\n\t{\n\t\tbbb\n\t\tccc\n\t}f\n\tddd");
			Put(3, 2).Press("da{").AssertText("\taaa\n\tf\n\tddd");
			Assert.AreEqual("{\n\t\tbbb\n\t\tccc\n\t}", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void da_Brackets3()
		{
			lines.SetText("\taaa\n\t{\n\t\tbbb\n\t\tccc\n\t}\n\tddd");
			Put(3, 2).Press("da{").AssertText("\taaa\n\tddd");
			Assert.AreEqual("\t{\n\t\tbbb\n\t\tccc\n\t}\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void ca_Brackets()
		{
			lines.SetText("\taaa{\n\t\tbbb\n\t\tccc\n\t}\n\tddd");
			Put(3, 1).Press("ca{x").AssertText("\taaax\n\tddd");
			Assert.AreEqual("{\n\t\tbbb\n\t\tccc\n\t}", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Ignore]
		[Test]
		public void y_Brackets_VISUAL()
		{
			lines.SetText("\taaa{\n\t\tbbb\n\t\tccc\n\t}\n\tddd");
			Put(3, 1).Press("vi{y");
			Assert.AreEqual("\t\tbbb\n\t\tccc\n", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void NestedBrackets()
		{
			lines.SetText("a{b{c{d}}}");
			Put(6, 0).Press("2yi{");
			Assert.AreEqual("c{d}", ClipboardExecutor.GetFromRegister(lines, '\0'));
			Put(6, 0).Press("2ya{");
			Assert.AreEqual("{c{d}}", ClipboardExecutor.GetFromRegister(lines, '\0'));
			Put(6, 0).Press("3yi{");
			Assert.AreEqual("b{c{d}}", ClipboardExecutor.GetFromRegister(lines, '\0'));
			Put(6, 0).Press("3ya{");
			Assert.AreEqual("{b{c{d}}}", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void BracketsInsideString()
		{
			lines.SetText("a{b{\"c{\"d}e}");
			Put(8, 0).Press("yi{");
			Assert.AreEqual("\"c{\"d", ClipboardExecutor.GetFromRegister(lines, '\0'));
			
			lines.SetText("a{b{'c{'d}e}");
			Put(8, 0).Press("yi{");
			Assert.AreEqual("'c{'d", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void BracketsInsideString_Escape()
		{
			lines.SetText("a{b{\"c\\\"{\"d}e}");
			Put(10, 0).Press("yi{");
			Assert.AreEqual("\"c\\\"{\"d", ClipboardExecutor.GetFromRegister(lines, '\0'));
			
			lines.SetText(@"a{b{'c{\''d}e}");
			Put(10, 0).Press("yi{");
			Assert.AreEqual(@"'c{\''d", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void BracketsInsideString_EscapeComplex()
		{
			lines.SetText("a{b{@\"c{\"\"\"d}e}");
			Put(10, 0).Press("yi{");
			Assert.AreEqual("@\"c{\"\"\"d", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void BracketsInsideString_EscapeComplex2()
		{
			lines.SetText("\"a{b{@\\\"c{\\\"\\\"\\\"d}e}\"}");
			Put(6, 0).Press("yi{");
			Assert.AreEqual("@\\\"c{\\\"\\\"\\\"d}e", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
		
		[Test]
		public void BracketsInsideString_EscapeComplex3()
		{
			lines.SetText(@"a{b'dir_name\\'}f");
			Put(2, 0).Press("yi{");
			Assert.AreEqual(@"b'dir_name\\'", ClipboardExecutor.GetFromRegister(lines, '\0'));
		}
	}
}