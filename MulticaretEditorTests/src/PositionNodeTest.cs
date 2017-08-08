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
			Assert.AreEqual(file + ":" + position, was != null ? was.file + ":" + was.position : "null");
		}
		
		[SetUp]
		public void SetUp()
		{
			_executor = new MacrosExecutor(null);
			_executor.maxViPositions = 3;
		}
		
		[Test]
		public void SingleNode()
		{
			_executor.ViPositionAdd("File0", 1, true);
			_executor.ViPositionAdd("File1", 2, true);
			AssertPosition("File0", 1, _executor.ViPositionPrev());
			AssertPosition("File1", 2, _executor.ViPositionNext());
			AssertPosition("File0", 1, _executor.ViPositionPrev());
		}
	}
}
