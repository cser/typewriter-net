using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class ControllerDialogExtensionTest : ControllerTestBase
	{
		[Test]
		public void FindNext()
		{
			Init(10);
			lines.SetText("aaa bbbb ccccc\neeee aaa ccccc fff");
			controller.PutCursor(new Place(6, 0), false);
			AssertSelection().Both(6, 0);
			
			controller.DialogsExtension.FindNext("ccccc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Anchor(9, 0).Caret(14, 0).NoNext();
			
			controller.DialogsExtension.FindNext("ccccc", false, false);
			Assert.IsNull(controller.DialogsExtension.NeedShowError);
			AssertSelection().Anchor(9, 0).Caret(14, 0).NoNext();
		}
	}
}