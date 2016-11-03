using System;
using System.IO;
using MulticaretEditor;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class FailOnTextEditTest
	{
		private static string textExample;
		
		private static string GetTextExample()
		{
			if (textExample == null)
			{
				textExample = File.ReadAllText("../test_code.txt");
			}
			return textExample;
		}
		
		[Test]
		public void MastNotFail()
		{
			LineArray lines = new LineArray(100);
			Controller controller = new Controller(lines);
			lines.SetText(GetTextExample());
			controller.PutCursor(new Place(0, 299), false);
			controller.PutCursor(new Place(0, 298), true);
			controller.Cut();
			controller.PutCursor(new Place(0, 305), false);
			controller.Paste();
			Assert.AreEqual(File.ReadAllText("../test_code_after.txt"), lines.GetText());
		}
	}
}