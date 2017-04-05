using System;
using MulticaretEditor;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class ControllerMarkedWordTest : ControllerTestBase
	{
		private void AssertMarks(int line, int[] expectedIndices)
		{
			int[] indices;
			lines.marksByLine.TryGetValue(line, out indices);
			CollectionAssert.AreEqual(expectedIndices, indices);
		}
		
		[Test]
		public void Test1()
		{
			Init();
			lines.SetText(
				"text\n" +
				"a b text");
			controller.SelectWordAtPlace(new Place(1, 0), false);
			controller.MarkWordOnPaint(true);
			AssertMarks(0, new int[] { 0 });
			AssertMarks(1, new int[] { 4 });
		}
		
		[Test]
		public void Test2()
		{
			Init();
			lines.SetText(
				"text\n" +
				"a b  text");
			controller.SelectWordAtPlace(new Place(1, 0), false);
			controller.MarkWordOnPaint(true);
			AssertMarks(0, new int[] { 0 });
			AssertMarks(1, new int[] { 5 });
		}
	}
}
