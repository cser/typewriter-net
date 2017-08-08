using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class PositionNodeTest
	{
		private MacrosExecutor _executor;
		
		private void AssertPosition(string expectedFile, int expectedPosition, PositionNode was)
		{
			Assert.AreEqual(expectedFile + ":" + expectedPosition, was != null ? was.file + ":" + was.position : "null");
		}
		
		[SetUp]
		public void SetUp()
		{
			_executor = new MacrosExecutor(null);
			_executor.maxViPositions = 3;
		}
		
		[Test]
		public void Simple()
		{
			_executor.ViPositionAdd("File0", 1);
			_executor.ViPositionAdd("File1", 2);
			AssertPosition("File0", 1, _executor.ViPositionPrev());
			AssertPosition("File1", 2, _executor.ViPositionNext());
			AssertPosition("File0", 1, _executor.ViPositionPrev());
		}
		
		[Test]
		public void WorksAfterMaxCountReached()
		{
			_executor.ViPositionAdd("File0", 1);
			_executor.ViPositionAdd("File0", 2);
			_executor.ViPositionAdd("File0", 3);
			_executor.ViPositionAdd("File0", 4);
			AssertPosition("File0", 3, _executor.ViPositionPrev());
			AssertPosition("File0", 2, _executor.ViPositionPrev());
			AssertPosition("File0", 3, _executor.ViPositionNext());
			AssertPosition("File0", 4, _executor.ViPositionNext());
		}
		
		[Test]
		public void NullAfterMaxCountOverflow()
		{
			_executor.ViPositionAdd("File0", 1);
			_executor.ViPositionAdd("File0", 2);
			_executor.ViPositionAdd("File0", 3);
			_executor.ViPositionAdd("File0", 4);
			AssertPosition("File0", 3, _executor.ViPositionPrev());
			AssertPosition("File0", 2, _executor.ViPositionPrev());
			Assert.AreEqual(null, _executor.ViPositionPrev());
			Assert.AreEqual(null, _executor.ViPositionPrev());
			AssertPosition("File0", 3, _executor.ViPositionNext());
			AssertPosition("File0", 4, _executor.ViPositionNext());
			Assert.AreEqual(null, _executor.ViPositionNext());
		}
	}
}
