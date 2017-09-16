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
			Assert.AreEqual(expectedFile + ":" + expectedPosition, was != null ? was.file.path + ":" + was.position : "null",
				_executor.GetDebugText());
		}
		
		private void AssertHistory(string expected)
		{
			Assert.AreEqual(expected, _executor.GetDebugText());
		}
		
		[SetUp]
		public void SetUp()
		{
			_executor = new MacrosExecutor(null, 3);
			_executor.ViSetCurrentFile("A");
		}
		
		[Test]
		public void Simple()
		{
			_executor.ViPositionAdd(1);
			_executor.ViSetCurrentFile("B");
			_executor.ViPositionAdd(2);
			AssertPosition("A", 1, _executor.ViPositionPrev());
			AssertPosition("B", 2, _executor.ViPositionNext());
			AssertPosition("A", 1, _executor.ViPositionPrev());
		}
		
		[Test]
		public void Simple_DontFailIfNoOneElement()
		{
			Assert.AreEqual(null, _executor.ViPositionPrev());
			Assert.AreEqual(null, _executor.ViPositionNext());
		}
		
		[Test]
		public void WorksAfterMaxCountReached()
		{
			_executor.ViPositionAdd(1);
			_executor.ViPositionAdd(2);
			_executor.ViPositionAdd(3);
			_executor.ViPositionAdd(4);
			AssertPosition("A", 3, _executor.ViPositionPrev());
			AssertPosition("A", 2, _executor.ViPositionPrev());
			AssertPosition("A", 3, _executor.ViPositionNext());
			AssertPosition("A", 4, _executor.ViPositionNext());
		}
		
		[Test]
		public void NullAfterMaxCountOverflow()
		{
			_executor.ViPositionAdd(1);
			_executor.ViPositionAdd(2);
			_executor.ViPositionAdd(3);
			_executor.ViPositionAdd(4);
			AssertPosition("A", 3, _executor.ViPositionPrev());
			AssertPosition("A", 2, _executor.ViPositionPrev());
			Assert.AreEqual(null, _executor.ViPositionPrev());
			Assert.AreEqual(null, _executor.ViPositionPrev());
			AssertPosition("A", 3, _executor.ViPositionNext());
			AssertPosition("A", 4, _executor.ViPositionNext());
			Assert.AreEqual(null, _executor.ViPositionNext());
		}
		
		[Test]
		public void PositionHistory()
		{
			_executor.ViPositionAdd(1);
			_executor.ViPositionAdd(2);
			_executor.ViPositionAdd(3);
			Assert.AreEqual(3, _executor.positionHistory.Length);
			AssertHistory("[(A:1)(A:2)][(A:3)]");
			
			AssertPosition("A", 2, _executor.ViPositionPrev());
			AssertPosition("A", 1, _executor.ViPositionPrev());
			AssertHistory("[][(A:1)(A:2)(A:3)]");
			_executor.ViPositionAdd(4);
			AssertHistory("[(A:1)][(A:4)]");
		}
		
		[Test]
		public void Circle()
		{
			_executor.ViPositionAdd(1);
			_executor.ViPositionAdd(2);
			_executor.ViPositionAdd(3);
			_executor.ViPositionAdd(4);
			_executor.ViPositionAdd(5);
			_executor.ViPositionAdd(6);
			_executor.ViPositionAdd(7);
			_executor.ViPositionAdd(8);
			_executor.ViPositionAdd(9);
			AssertPosition("A", 8, _executor.ViPositionPrev());
			AssertPosition("A", 7, _executor.ViPositionPrev());
			Assert.AreEqual(null, _executor.ViPositionPrev());
			AssertPosition("A", 8, _executor.ViPositionNext());
			AssertPosition("A", 9, _executor.ViPositionNext());
			Assert.AreEqual(null, _executor.ViPositionNext());
			AssertPosition("A", 8, _executor.ViPositionPrev());
			AssertPosition("A", 7, _executor.ViPositionPrev());
			Assert.AreEqual(null, _executor.ViPositionPrev());
			AssertPosition("A", 8, _executor.ViPositionNext());
			AssertPosition("A", 9, _executor.ViPositionNext());
			AssertPosition("A", 8, _executor.ViPositionPrev());
			_executor.ViPositionAdd(19);
			Assert.AreEqual(null, _executor.ViPositionNext());
		}
		
		[Test]
		public void ViPositionSet()
		{
			_executor.ViPositionAdd(1);
			_executor.ViPositionAdd(2);
			_executor.ViPositionAdd(3);
			_executor.ViPositionAdd(4);
			_executor.ViPositionAdd(5);
			_executor.ViPositionAdd(6);
			_executor.ViPositionAdd(7);
			_executor.ViPositionAdd(8);
			_executor.ViPositionAdd(9);
			_executor.ViPositionSet(19);
			AssertPosition("A", 8, _executor.ViPositionPrev());
			AssertPosition("A", 7, _executor.ViPositionPrev());
			AssertPosition("A", 8, _executor.ViPositionNext());
			AssertPosition("A", 19, _executor.ViPositionNext());
			Assert.AreEqual(null, _executor.ViPositionNext());
			AssertPosition("A", 8, _executor.ViPositionPrev());
			AssertPosition("A", 19, _executor.ViPositionNext());
			Assert.AreEqual(null, _executor.ViPositionNext());
			_executor.ViPositionSet(29);
			_executor.ViPositionSet(39);
			AssertPosition("A", 8, _executor.ViPositionPrev());
			AssertPosition("A", 39, _executor.ViPositionNext());
			AssertPosition("A", 8, _executor.ViPositionPrev());
			_executor.ViPositionSet(18);
			AssertHistory("[(A:7)][(A:18)]");
			Assert.AreEqual(null, _executor.ViPositionNext());
			AssertPosition("A", 7, _executor.ViPositionPrev());
		}
	}
}
