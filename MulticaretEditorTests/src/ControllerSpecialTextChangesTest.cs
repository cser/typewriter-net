using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class ControllerSpecialTextChangesTest : ControllerTestBase
	{
		[Test]
		public void InsertLineBreak_Simple()
		{
			Init();
			lines.lineBreak = "\n";
			lines.SetText("line0\nline1\nline2\nline3");
			controller.PutCursor(new Place(1, 1), false);
			AssertSelection().Both(1, 1).NoNext();
			
			controller.InsertLineBreak();
			
			AssertText("line0\nl\nine1\nline2\nline3");
			AssertSelection().Both(0, 2).NoNext();
		}
		
		[Test]
		public void InsertLineBreak_Tabs()
		{
			Init();
			lines.lineBreak = "\n";
			lines.SetText("line0\n\tline1\n\tline2\n\tline3");
			controller.PutCursor(new Place(2, 1), false);
			AssertSelection().Both(2, 1).NoNext();
			
			controller.InsertLineBreak();
			
			AssertText("line0\n\tl\n\tine1\n\tline2\n\tline3");
			AssertSelection().Both(1, 2).NoNext();
		}
		
		[Test]
		public void InsertLineBreak_TabsAndSpaces()
		{
			Init();
			lines.lineBreak = "\n";
			lines.SetText("\tline0\n\t\t line1\n\tline2\n\tline3");
			controller.PutCursor(new Place(4, 1), false);
			AssertSelection().Both(4, 1).NoNext();
			
			controller.InsertLineBreak();
			
			AssertText("\tline0\n\t\t l\n\t\t ine1\n\tline2\n\tline3");
			AssertSelection().Both(3, 2).NoNext();
		}
		
		[Test]
		public void InsertLineBreak_InsideFirstSpaces0()
		{
			Init();
			lines.lineBreak = "\n";
			lines.SetText("\tline0\n\t   line1\n\tline2\n\tline3");
			controller.PutCursor(new Place(2, 1), false);
			AssertSelection().Both(2, 1).NoNext();
			
			controller.InsertLineBreak();
			
			AssertText("\tline0\n\t \n  line1\n\tline2\n\tline3");
			AssertSelection().Both(0, 2).NoNext();
		}
		
		[Test]
		public void InsertLineBreak_InsideFirstSpaces1()
		{
			Init();
			lines.lineBreak = "\n";
			lines.SetText("\tline0\n\t   line1\n\tline2\n\tline3");
			controller.PutCursor(new Place(0, 1), false);
			AssertSelection().Both(0, 1).NoNext();
			
			controller.InsertLineBreak();
			
			AssertText("\tline0\n\n\t   line1\n\tline2\n\tline3");
			AssertSelection().Both(0, 2).NoNext();
		}
		
		[Test]
		public void InsertLineBreak_InsideFirstSpaces2()
		{
			Init();
			lines.lineBreak = "\n";
			lines.SetText("\tline0\n\t   line1\n\tline2\n\tline3");
			controller.PutCursor(new Place(4, 1), false);
			AssertSelection().Both(4, 1).NoNext();
			
			controller.InsertLineBreak();
			
			AssertText("\tline0\n\t   \n\t   line1\n\tline2\n\tline3");
			AssertSelection().Both(4, 2).NoNext();
		}
	}
}
