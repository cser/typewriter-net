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
		public void ControlBra()
		{
			CommandData data = new CommandData("name", "[C-[]");
			List<MacrosExecutor.Action> actions = data.GetActions(new StringBuilder());
			Assert.AreEqual(Keys.Control | Keys.OemOpenBrackets, actions[0].keys);
		}
	}
}
