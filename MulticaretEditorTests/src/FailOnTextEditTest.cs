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
			Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
			LineArray lines = new LineArray(200);
			Controller controller = new Controller(lines);
			lines.SetText(GetTextExample());
			controller.PutCursor(new Place(0, 799), false);
			controller.PutCursor(new Place(0, 798), true);
			controller.Cut();
			controller.PutCursor(new Place(0, 805), false);
			controller.Paste();
			Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
		}
	}
}