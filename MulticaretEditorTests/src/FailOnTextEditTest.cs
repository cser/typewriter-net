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
			LineArray lines = new LineArray(100);
			Controller controller = new Controller(lines);
			Console.WriteLine("|                      lines.SetText(GetTextExample())");
			lines.SetText(GetTextExample());
			Console.WriteLine("|                      " + lines.GetDebugText());
			Console.WriteLine("|                      " + lines.CheckConsistency());
			Console.WriteLine("|                      controller.PutCursor(new Place(0, 799), false)");
			controller.PutCursor(new Place(0, 299), false);
			Console.WriteLine("|                      " + lines.GetDebugText());
			Console.WriteLine("|                      " + lines.CheckConsistency());
			Console.WriteLine("|                      controller.PutCursor(new Place(0, 798), true)");
			controller.PutCursor(new Place(0, 298), true);
			Console.WriteLine("|                      " + lines.GetDebugText());
			Console.WriteLine("|                      " + lines.CheckConsistency());
			Console.WriteLine("|                      controller.Cut()");
			controller.Cut();
			Console.WriteLine("|                      " + lines.GetDebugText());
			Console.WriteLine("|                      " + lines.GetFullDebugText());
			Console.WriteLine("|                      " + lines.CheckConsistency());
			Console.WriteLine("|                      controller.PutCursor(new Place(0, 805), false)");
			controller.PutCursor(new Place(0, 305), false);
			Console.WriteLine("|                      " + lines.GetDebugText());
			Console.WriteLine("|                      " + lines.CheckConsistency());
			Console.WriteLine("|                      controller.Paste()");
			controller.Paste();
			Console.WriteLine("|                      " + lines.GetDebugText());
			Console.WriteLine("|                      " + lines.CheckConsistency());
			Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
			Assert.AreEqual(File.ReadAllText("../test_code.txt"), lines.GetText());
		}
	}
}