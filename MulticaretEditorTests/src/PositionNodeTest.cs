using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class PositionNodeTest
	{
		private MacrosExecutor _executor;
		
		[SetUp]
		public void SetUp()
		{
			_executor = new MacrosExecutor(null);
			_executor.maxViPositions = 3;
		}
		
		private readonly PositionNode Node0 = new PositionNode("Node0", 0);
		private readonly PositionNode Node1 = new PositionNode("Node1", 1);
		private readonly PositionNode Node2 = new PositionNode("Node2", 2);
		
		[Test]
		public void SingleNode()
		{
			Assert.AreEqual(new PositionNode(), _executor.ViPositionNext());
			Assert.AreEqual(new PositionNode(), _executor.ViPositionPrev());
			_executor.ViPositionAdd(Node0);
			Assert.AreEqual(new PositionNode(), _executor.ViPositionPrev());
			Assert.AreEqual(Node0, _executor.ViPositionNext());
			Assert.AreEqual(new PositionNode(), _executor.ViPositionPrev());
		}
		
		[Test]
		public void SingleNode_ReturnsNoneIfMissing()
		{
			_executor.ViPositionAdd(Node0);
			Assert.AreEqual(Node0, _executor.ViPositionNext());
			Assert.AreEqual(new PositionNode(), _executor.ViPositionNext());
		}
		
		[Test]
		public void Next()
		{
			Assert.AreEqual(new PositionNode(), _executor.ViPositionNext());
			Assert.AreEqual(new PositionNode(), _executor.ViPositionPrev());
			_executor.ViPositionAdd(Node0);
			_executor.ViPositionAdd(Node1);
			Assert.AreEqual(Node0, _executor.ViPositionNext());
			Assert.AreEqual(Node1, _executor.ViPositionNext());
			Assert.AreEqual(Node0, _executor.ViPositionPrev());
		}
		
		[Test]
		public void Next_ClearNextIfNewPosition()
		{
			Assert.AreEqual(new PositionNode(), _executor.ViPositionNext());
			Assert.AreEqual(new PositionNode(), _executor.ViPositionPrev());
			_executor.ViPositionAdd(Node0);
			_executor.ViPositionAdd(Node1);
			Assert.AreEqual(Node0, _executor.ViPositionNext());
			Assert.AreEqual(Node1, _executor.ViPositionNext());
			_executor.ViPositionAdd(Node2);
			Assert.AreEqual(Node2, _executor.ViPositionPrev());
			Assert.AreEqual(new PositionNode(), _executor.ViPositionNext());
		}
	}
}
