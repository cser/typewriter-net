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
		
		[Test]
		public void InsertLineBreak_AfterBra()
		{
			Init();
			lines.lineBreak = "\n";
			lines.autoindent = false;
			lines.SetText("\tline0\n\tline1() {\n\tline2\n\tline3");
			controller.PutCursor(new Place(10, 1), false);
			AssertSelection().Both(10, 1).NoNext();
			controller.InsertLineBreak();
			AssertText("\tline0\n\tline1() {\n\t\n\tline2\n\tline3");
			controller.PutCursor(new Place(1, 2), false);
		}
		
		[Test]
		public void InsertLineBreak_AfterBra_Autoident()
		{
			Init();
			lines.lineBreak = "\n";
			lines.autoindent = true;
			lines.SetText("\tline0\n\tline1() {\n\tline2\n\tline3");
			controller.PutCursor(new Place(10, 1), false);
			AssertSelection().Both(10, 1).NoNext();
			
			controller.InsertLineBreak();
			AssertText("\tline0\n\tline1() {\n\t\t\n\tline2\n\tline3");
			controller.PutCursor(new Place(2, 2), false);
			
			controller.InsertLineBreak();
			AssertText("\tline0\n\tline1() {\n\t\t\n\t\t\n\tline2\n\tline3");
			controller.PutCursor(new Place(2, 3), false);
		}
		
		[Test]
		public void InsertLineBreak_AfterCket()
		{
			Init();
			lines.lineBreak = "\n";
			lines.autoindent = false;
			lines.SetText(
				"\tline0() {\n" +
				"\t\tline1\n" +
				"\t\tline2\n" +
				"\t\t\n" +
				"\tline3");
			controller.PutCursor(new Place(2, 3), false);
			AssertSelection().Both(2, 3).NoNext();
			controller.InsertText("}");
			AssertText(
				"\tline0() {\n" +
				"\t\tline1\n" +
				"\t\tline2\n" +
				"\t\t}\n" +
				"\tline3");
			controller.PutCursor(new Place(3, 3), false);
		}
		
		[Test]
		public void InsertLineBreak_AfterCket_Autoindent()
		{
			Init();
			lines.lineBreak = "\n";
			lines.autoindent = true;
			lines.SetText(
				"\tline0() {\n" +
				"\t\tline1\n" +
				"\t\tline2\n" +
				"\t\t\n" +
				"\tline3");
			controller.PutCursor(new Place(2, 3), false);
			AssertSelection().Both(2, 3).NoNext();
			
			controller.InsertText("}");
			AssertText(
				"\tline0() {\n" +
				"\t\tline1\n" +
				"\t\tline2\n" +
				"\t}\n" +
				"\tline3");
			controller.PutCursor(new Place(2, 3), false);
			
			controller.processor.Undo();
			AssertText(
				"\tline0() {\n" +
				"\t\tline1\n" +
				"\t\tline2\n" +
				"\t\t\n" +
				"\tline3");
		}
		
		[Test]
		public void InsertLineBreak_AfterCket_Autoindent2()
		{
			Init();
			lines.lineBreak = "\n";
			lines.autoindent = true;
			lines.SetText(
				"\tline0()\n" +
				"\t{\n" +
				"\t\t\n" +
				"\tline3");
			controller.PutCursor(new Place(2, 2), false);
			AssertSelection().Both(2, 2).NoNext();
			
			controller.InsertText("}");
			AssertText(
				"\tline0()\n" +
				"\t{\n" +
				"\t}\n" +
				"\tline3");
			controller.PutCursor(new Place(2, 2), false);
			
			controller.processor.Undo();
			AssertText(
				"\tline0()\n" +
				"\t{\n" +
				"\t\t\n" +
				"\tline3");
		}
		
		[Test]
		public void InsertLineBreak_AfterCket_Autoindent3()
		{
			Init();
			lines.lineBreak = "\n";
			lines.autoindent = true;
			lines.SetText(
				"line0()\n" +
				"{\n" +
				"\t\n" +
				"line3");
			controller.PutCursor(new Place(1, 2), false);
			AssertSelection().Both(1, 2).NoNext();
			
			controller.InsertText("}");
			AssertText(
				"line0()\n" +
				"{\n" +
				"}\n" +
				"line3");
			controller.PutCursor(new Place(1, 2), false);
			
			controller.processor.Undo();
			AssertText(
				"line0()\n" +
				"{\n" +
				"\t\n" +
				"line3");
		}
		
		[Test]
		public void InsertLineBreak_BeforeCket_Autoindent()
		{
			Init();
			lines.lineBreak = "\n";
			lines.autoindent = true;
			lines.SetText(
				"\tline0() {\n" +
				"\t\tline1\n" +
				"\t\tline2}\n" +
				"\tline3");
			controller.PutCursor(new Place(7, 2), false);
			AssertSelection().Both(7, 2).NoNext();
			controller.InsertLineBreak();
			AssertText(
				"\tline0() {\n" +
				"\t\tline1\n" +
				"\t\tline2\n" +
				"\t}\n" +
				"\tline3");
			controller.PutCursor(new Place(2, 3), false);
			
			controller.processor.Undo();
			AssertText(
				"\tline0() {\n" +
				"\t\tline1\n" +
				"\t\tline2}\n" +
				"\tline3");
		}
		
		[Test]
		public void InsertLineBreak_BeforeCket_Autoindent2()
		{
			Init();
			lines.lineBreak = "\n";
			lines.autoindent = true;
			lines.SetText(
				"\tline0() {\n" +
				"\t\tline1\n" +
				"\t\tline2}ABCDE\n" +
				"\tline3");
			controller.PutCursor(new Place(7, 2), false);
			AssertSelection().Both(7, 2).NoNext();
			controller.InsertLineBreak();
			AssertText(
				"\tline0() {\n" +
				"\t\tline1\n" +
				"\t\tline2\n" +
				"\t}ABCDE\n" +
				"\tline3");
			controller.PutCursor(new Place(2, 3), false);
			
			controller.processor.Undo();
			AssertText(
				"\tline0() {\n" +
				"\t\tline1\n" +
				"\t\tline2}ABCDE\n" +
				"\tline3");
		}
		
		[Test]
		public void InsertLineBreak_BeforeCket_Autoindent3()
		{
			Init();
			lines.lineBreak = "\n";
			lines.autoindent = true;
			lines.SetText("line0() {\n\tline2}ABCDE");
			controller.PutCursor(new Place(6, 2), false);
			controller.InsertLineBreak();
			AssertText("line0() {\n\tline2\n}ABCDE");
			controller.processor.Undo();
			AssertText("line0() {\n\tline2}ABCDE");
		}
		
		[Test]
		public void InsertLineBreak_BeforeCket_Autoindent4()
		{
			Init();
			lines.lineBreak = "\n";
			lines.autoindent = true;
			lines.SetText("line0() {\nline2}ABCDE");
			controller.PutCursor(new Place(5, 2), false);
			controller.InsertLineBreak();
			AssertText("line0() {\nline2\n}ABCDE");
			controller.processor.Undo();
			AssertText("line0() {\nline2}ABCDE");
		}
		
		[Test]
		public void InsertLineBreak_AfterColon()
		{
			Init();
			lines.lineBreak = "\n";
			lines.autoindent = true;
			lines.SetText("\tline0()\n\tline1:ABCDE\n\tline2");
			controller.PutCursor(new Place(7, 1), false);
			controller.InsertLineBreak();
			AssertText("\tline0()\n\tline1:\n\t\tABCDE\n\tline2").AssertSelection().Both(1, 2);
			controller.processor.Undo();
			AssertText("\tline0()\n\tline1:ABCDE\n\tline2");
		}
	}
}
