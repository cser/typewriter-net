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
			Assert.AreEqual(expectedFile + ":" + expectedPosition, was != null ? was.file.path + ":" + was.position : "null");
		}
		
		[SetUp]
		public void SetUp()
		{
			_executor = new MacrosExecutor(null, 3);
			_executor.ViSetCurrentFile("A");
		}
		
		[Test]
		public void Prev()
		{
			_executor.ViPosition_AddPrev(1);
			Assert.AreEqual("[(A:1)][]", _executor.GetDebugText());
			_executor.ViSetCurrentFile("B");
			_executor.ViPosition_AddPrev(2);
			Assert.AreEqual("[(A:1)(B:2)][]", _executor.GetDebugText());
			AssertPosition("B", 2, _executor.ViPosition_Prev(3));
			Assert.AreEqual("[(A:1)][(B:2)(B:3)]", _executor.GetDebugText());
			AssertPosition("A", 1, _executor.ViPosition_Prev(2));
			Assert.AreEqual("[][(A:1)(B:2)(B:3)]", _executor.GetDebugText());
		}
		
		/*[Test]
		public void PrevNext()
		{
			_executor.ViPosition_AddPrev(1);
			_executor.ViPosition_AddPrev(2);
			Assert.AreEqual("[(A:1)(A:2)][]", _executor.GetDebugText());
			AssertPosition("A", 2, _executor.ViPosition_Prev(3));
			Assert.AreEqual("[(A:1)(A:2)][(A:3)]", _executor.GetDebugText());
			AssertPosition("A", 3, _executor.ViPosition_Next(2));
			Console.Write("!" + _executor.GetDebugText());
		}*/
		
		[Test]
		public void Simple_DontFailIfNoOneElement()
		{
			Assert.AreEqual(null, _executor.ViPosition_Prev(1));
			//Assert.AreEqual(null, _executor.ViPosition_Next(1));
		}
		/*
		[Test]
		public void WorksAfterMaxCountReached()
		{
			_executor.ViPositionAdd(1);
			_executor.ViPositionAdd(2);
			_executor.ViPositionAdd(3);
			_executor.ViPositionAdd(4);
			AssertPosition("File0", 3, _executor.ViPositionPrev());
			AssertPosition("File0", 2, _executor.ViPositionPrev());
			AssertPosition("File0", 3, _executor.ViPositionNext());
			AssertPosition("File0", 4, _executor.ViPositionNext());
		}
		
		[Test]
		public void NullAfterMaxCountOverflow()
		{
			_executor.ViPositionAdd(1);
			_executor.ViPositionAdd(2);
			_executor.ViPositionAdd(3);
			_executor.ViPositionAdd(4);
			AssertPosition("File0", 3, _executor.ViPositionPrev());
			AssertPosition("File0", 2, _executor.ViPositionPrev());
			Assert.AreEqual(null, _executor.ViPositionPrev());
			Assert.AreEqual(null, _executor.ViPositionPrev());
			AssertPosition("File0", 3, _executor.ViPositionNext());
			AssertPosition("File0", 4, _executor.ViPositionNext());
			Assert.AreEqual(null, _executor.ViPositionNext());
		}
		
		[Test]
		public void PositionHistory()
		{
			_executor.ViPositionAdd(1);
			_executor.ViPositionAdd(2);
			_executor.ViPositionAdd(3);
			Assert.AreEqual(3, _executor.positionHistory.Length);
			AssertPosition("File0", 1, _executor.positionHistory[0]);
			AssertPosition("File0", 2, _executor.positionHistory[1]);
			AssertPosition("File0", 3, _executor.positionHistory[2]);
			
			AssertPosition("File0", 2, _executor.ViPositionPrev());
			AssertPosition("File0", 1, _executor.ViPositionPrev());
			AssertPosition("File0", 1, _executor.positionHistory[0]);
			AssertPosition("File0", 2, _executor.positionHistory[1]);
			AssertPosition("File0", 3, _executor.positionHistory[2]);
			_executor.ViPositionAdd(4);
			AssertPosition("File0", 1, _executor.positionHistory[0]);
			AssertPosition("File0", 4, _executor.positionHistory[1]);
			Assert.AreEqual(null, _executor.positionHistory[2]);
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
			AssertPosition("File0", 8, _executor.ViPositionPrev());
			AssertPosition("File0", 7, _executor.ViPositionPrev());
			Assert.AreEqual(null, _executor.ViPositionPrev());
			AssertPosition("File0", 8, _executor.ViPositionNext());
			AssertPosition("File0", 9, _executor.ViPositionNext());
			Assert.AreEqual(null, _executor.ViPositionNext());
			AssertPosition("File0", 8, _executor.ViPositionPrev());
			AssertPosition("File0", 7, _executor.ViPositionPrev());
			Assert.AreEqual(null, _executor.ViPositionPrev());
			AssertPosition("File0", 8, _executor.ViPositionNext());
			AssertPosition("File0", 9, _executor.ViPositionNext());
			AssertPosition("File0", 8, _executor.ViPositionPrev());
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
			AssertPosition("File0", 8, _executor.ViPositionPrev());
			AssertPosition("File0", 7, _executor.ViPositionPrev());
			AssertPosition("File0", 8, _executor.ViPositionNext());
			AssertPosition("File0", 19, _executor.ViPositionNext());
			Assert.AreEqual(null, _executor.ViPositionNext());
			AssertPosition("File0", 8, _executor.ViPositionPrev());
			AssertPosition("File0", 19, _executor.ViPositionNext());
			Assert.AreEqual(null, _executor.ViPositionNext());
			_executor.ViPositionSet(29);
			_executor.ViPositionSet(39);
			AssertPosition("File0", 8, _executor.ViPositionPrev());
			AssertPosition("File0", 39, _executor.ViPositionNext());
			AssertPosition("File0", 8, _executor.ViPositionPrev());
			_executor.ViPositionSet(18);
			AssertPosition("File0", 7, _executor.positionHistory[0]);
			AssertPosition("File0", 18, _executor.positionHistory[1]);
			Assert.AreEqual(null, _executor.positionHistory[2]);
			Assert.AreEqual(null, _executor.ViPositionNext());
			AssertPosition("File0", 7, _executor.ViPositionPrev());
		}
		*/
	}
}
