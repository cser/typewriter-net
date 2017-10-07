using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class CommandDataTest
	{
		private string StringOf(MacrosExecutor.Action action)
		{
			string text = "";
			if (action.code != '\0')
			{
				text += action.code;
			}
			if (action.keys != Keys.None)
			{
				text += "(" + action.keys + ")";
			}
			return text;
		}
		
		private void AssertActions(string expected, CommandData data)
		{
			Assert.AreEqual(expected, ListUtil.ToString(data.GetActions(new StringBuilder()), StringOf));
		}
		
		[Test]
		public void Simple()
		{
			CommandData data = new CommandData("name", "Abc");
			AssertActions("[A, b, c]", data);
		}
		
		[Test]
		public void Control()
		{
			CommandData data = new CommandData("name", "[C-A][C-b]c");
			AssertActions("[(A, Shift, Control), (B, Control), c]", data);
		}
		
		[Test]
		public void ControlComplex()
		{
			CommandData data = new CommandData("name", "a[C-A]bc [C-b]d");
			AssertActions("[a, (A, Shift, Control), b, c,  , (B, Control), d]", data);
		}
		
		[Test]
		public void ControlBra()
		{
			CommandData data = new CommandData("name", "[C-[]");
			List<MacrosExecutor.Action> actions = data.GetActions(new StringBuilder());
			Assert.AreEqual(Keys.Control | Keys.OemOpenBrackets, actions[0].keys);
		}
		
		[Test]
		public void ControlKet()
		{
			CommandData data = new CommandData("name", "[C-]]");
			List<MacrosExecutor.Action> actions = data.GetActions(new StringBuilder());
			Assert.AreEqual(Keys.Control | Keys.OemCloseBrackets, actions[0].keys);
		}
		
		[Test]
		public void ControlShift_CommandDialog()
		{
			CommandData data = new CommandData("name", "[C-S-;]");
			List<MacrosExecutor.Action> actions = data.GetActions(new StringBuilder());
			Assert.AreEqual(Keys.Control | Keys.Shift | Keys.OemSemicolon, actions[0].keys);
		}
		
		[Test]
		public void ControlShift_CommandDialog_Alternative()
		{
			CommandData data = new CommandData("name", "[C-:]");
			List<MacrosExecutor.Action> actions = data.GetActions(new StringBuilder());
			Assert.AreEqual(Keys.Control | Keys.Shift | Keys.OemSemicolon, actions[0].keys);
		}
		
		[Test]
		public void Control_Alternative()
		{
			CommandData data = new CommandData("name", "[C-S-a]");
			AssertActions("[(A, Shift, Control)]", data);
		}
		
		[Test]
		public void Control_Alternative2()
		{
			CommandData data = new CommandData("name", "[S-C-a]");
			AssertActions("[(A, Shift, Control)]", data);
		}
		
		[Test]
		public void Shift_Alternative()
		{
			CommandData data = new CommandData("name", "[S-a]");
			AssertActions("[(A, Shift)]", data);
		}
		
		[Test]
		public void BraKet()
		{
			CommandData data = new CommandData("name", "[bra][ket]");
			AssertActions("[[, ]]", data);
		}
		
		[Test]
		public void F1_12()
		{
			CommandData data = new CommandData("name", "[F1][C-F2][S-F11][C-S-F12]");
			AssertActions("[(F1), (F2, Control), (F11, Shift), (F12, Shift, Control)]", data);
		}
		
		[Test]
		public void CtrlBra()
		{
			CommandData data = new CommandData("name", "[C-bra]");
			AssertActions("[[(Control)]", data);
		}
	}
}
