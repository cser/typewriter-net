using System;
using System.Collections.Generic;
using NUnit.Framework;
using MulticaretEditor;
using System.Windows.Forms;

namespace UnitTests
{
	[TestFixture]
	public class ViLinesTest : ControllerTest
	{
		private void AssertCommandRanges(string expected, int count)
		{
			CollectionAssert.AreEqual(
				expected,
				ListUtil.ToString(controller.ViGetLineRanges(count), StringOfSimpleRange));
		}
		
		private static string StringOfSimpleRange(SimpleRange range)
		{
			return range.index + "/" + range.count;
		}
		
		[Test]
		public void CommandRanges1()
		{
			Init();
			
			lines.SetText("line0\nline1\r\nline2");
			controller.PutCursor(new Place(1, 0), false);
			AssertCommandRanges("[0/1]", 1);
			
			lines.SetText("line0\nline1\r\nline2");
			controller.PutCursor(new Place(1, 0), false);
			AssertCommandRanges("[0/2]", 2);
		}
	}
}
