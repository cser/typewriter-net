using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class ViControllerTest : ControllerTestBase
	{
		[Test]
		public void MoveWordRight()
		{
			Init();
			lines.SetText(
			//	 0123456789012345678901
				"Du\n" +
				"Du hast\n" +
				"Du hast mich\n" +
				"abcd,.;.;.asdf234234\n" +
				"abcd ,.;.;.asdf234234\n" +
				"abcd  ,.;.;.asdf234234");
			
			controller.PutCursor(new Place(3, 2), false);
			controller.ViMove_w(false, false, true);
			AssertSelection().Both(8, 2);
			
			controller.PutCursor(new Place(4, 2), false);
			controller.ViMove_w(false, false, true);
			AssertSelection().Both(8, 2);
			
			controller.PutCursor(new Place(1, 3), false);
			controller.ViMove_w(false, false, true);
			AssertSelection().Both(4, 3);
			controller.ViMove_w(false, false, true);
			AssertSelection().Both(10, 3);
			
			controller.PutCursor(new Place(4, 5), false);
			AssertSelection().Both(4, 5);
			controller.ViMove_w(false, false, true);
			AssertSelection().Both(6, 5);
			
			controller.PutCursor(new Place(5, 5), false);
			controller.ViMove_w(false, false, true);
			AssertSelection().Both(6, 5);
			
			controller.PutCursor(new Place(10, 5), false);
			controller.ViMove_w(false, false, true);
			AssertSelection().Both(12, 5);
		}
		
		[Test]
		public void MoveWordLeft()
		{
			Init();
			lines.SetText(
			//	 0123456789012345678901
				"Du\n" +
				"Du hast\n" +
				"Du hast mich\n" +
				"abcd,.;.;.asdf234234\n" +
				"abcd ,.;.;.asdf234234\n" +
				"abcd  ,.;.;.asdf234234");
			
			controller.PutCursor(new Place(8, 2), false);
			controller.ViMove_b(false);
			AssertSelection().Both(3, 2);
			
			controller.PutCursor(new Place(9, 2), false);
			controller.ViMove_b(false);
			AssertSelection().Both(8, 2);
			
			controller.PutCursor(new Place(10, 3), false);
			controller.ViMove_b(false);
			AssertSelection().Both(4, 3);

			controller.PutCursor(new Place(6, 3), false);
			controller.ViMove_b(false);
			AssertSelection().Both(4, 3);
			
			controller.PutCursor(new Place(6, 5), false);
			controller.ViMove_b(false);
			AssertSelection().Both(0, 5);
			
			controller.PutCursor(new Place(5, 5), false);
			controller.ViMove_b(false);
			AssertSelection().Both(0, 5);
			
			controller.PutCursor(new Place(5, 11), false);
			controller.ViMove_b(false);
			AssertSelection().Both(0, 5);
			
			controller.PutCursor(new Place(12, 4), false);
			controller.ViMove_b(false);
			AssertSelection().Both(11, 4);
		}
		
		[Test]
		public void MoveWordLeftRightNewLine()
		{
			Init();
			lines.SetText(
			//	 0123456789012345678901
				"Du hast,;\n" +
				"Du hast mich\n" +
				"abcd  ,.;.;.asd aaaaaa");
			
			controller.PutCursor(new Place(8, 1), false);
			controller.ViMove_w(false, false, true);
			AssertSelection().Both(0, 2);
			
			controller.PutCursor(new Place(7, 0), false);
			controller.ViMove_w(false, false, true);
			AssertSelection().Both(0, 1);
			
			controller.PutCursor(new Place(8, 0), false);
			controller.ViMove_w(false, false, true);
			AssertSelection().Both(0, 1);
			
			controller.PutCursor(new Place(9, 0), false);
			controller.ViMove_w(false, false, true);
			AssertSelection().Both(0, 1);
			
			controller.PutCursor(new Place(0, 1), false);
			controller.ViMove_b(false);
			AssertSelection().Both(7, 0);
			
			controller.PutCursor(new Place(0, 2), false);
			controller.ViMove_b(false);
			AssertSelection().Both(8, 1);
		}
	}
}