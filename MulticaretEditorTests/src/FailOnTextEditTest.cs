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
		public void MastNotFail_1()
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
		
		[Test]
		public void MastNotFail_2()
		{
			LineArray lines = new LineArray(10);
			Controller controller = new Controller(lines);
			lines.SetText(File.ReadAllText("../test_code_2.txt"));
			controller.PutCursor(new Place(0, 9), false);
			controller.PutCursor(new Place(0, 20), true);
			controller.Cut();
			Assert.AreEqual(File.ReadAllText("../test_code_2_after.txt"), lines.GetText());
		}
		
		[Test]
		public void MastNotFail_3()
		{
			LineArray lines = new LineArray(3);
			Controller controller = new Controller(lines);
			lines.SetText(File.ReadAllText("../test_code_3.txt"));
			controller.PutCursor(new Place(1, 45), false);
			controller.PutCursor(new Place(1, 44), true);
			controller.EraseSelection();
			Assert.AreEqual(File.ReadAllText("../test_code_3_after.txt"), lines.GetText());
		}
		
		[Test]
		public void MastNotFail_4()
		{
			LineArray lines = new LineArray(4);
			Controller controller = new Controller(lines);
			lines.SetText(File.ReadAllText("../test_code_4.txt"));
			
			controller.PutCursor(new Place(1, 316 - 1), false);
			controller.PutCursor(new Place(1, 316 - 0), true);
			
			controller.PutNewCursor(new Place(1, 321 - 1));
			controller.PutCursor(new Place(1, 321 - 0), true);
			
			controller.EraseSelection();
			
			Assert.AreEqual(File.ReadAllText("../test_code_4_after.txt"), lines.GetText());
		}
		
		[Test]
		public void MastNotFail_4_1()
		{
			LineArray lines = new LineArray(4);
			Controller controller = new Controller(lines);
			lines.SetText(File.ReadAllText("../test_code_4.txt"));
			
			controller.PutCursor(new Place(1, 56 - 1), false);
			controller.PutNewCursor(new Place(1, 59 - 1));
			controller.PutNewCursor(new Place(1, 61 - 1));
			controller.PutNewCursor(new Place(1, 67 - 1));
			controller.PutNewCursor(new Place(1, 69 - 1));
			controller.PutNewCursor(new Place(1, 79 - 1));
			controller.PutNewCursor(new Place(1, 145 - 1));
			controller.PutNewCursor(new Place(1, 168 - 1));
			controller.PutNewCursor(new Place(1, 171 - 1));
			controller.PutNewCursor(new Place(1, 316 - 1));
			controller.PutNewCursor(new Place(1, 321 - 1));
			controller.PutNewCursor(new Place(1, 326 - 1));
			controller.PutNewCursor(new Place(1, 342 - 1));
			controller.MoveDown(true);
			controller.MoveDown(true);
			
			controller.EraseSelection();
			
			Assert.AreEqual(File.ReadAllText("../test_code_4_1_after.txt"), lines.GetText());
		}
		
		[Test]
		public void MastNotFail_4_2()
		{
			LineArray lines = new LineArray(4);
			Controller controller = new Controller(lines);
			lines.SetText(File.ReadAllText("../test_code_4.txt"));
			Debug.Log("<<<");
			controller.PutCursor(new Place(1, 56 - 1), false);
			controller.PutNewCursor(new Place(1, 59 - 1));
			controller.MoveDown(true);
			controller.MoveDown(true);
			
			controller.EraseSelection();
			Debug.Log(">>>");
		}
	}
}