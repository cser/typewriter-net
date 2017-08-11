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
		public void Prev()
		{
			_executor.ViPosition_AddPrev(1);
			AssertHistory("[(A:1)][]");
			_executor.ViSetCurrentFile("B");
			_executor.ViPosition_AddPrev(2);
			AssertHistory("[(A:1)(B:2)][]");
			AssertPosition("B", 2, _executor.ViPosition_Prev(3));
			AssertHistory("[(A:1)][(B:2)(B:3)]");
			AssertPosition("A", 1, _executor.ViPosition_Prev(2));
			AssertHistory("[][(A:1)(B:2)(B:3)]");
		}
		
		[Test]
		public void PrevNext()
		{
			_executor.ViPosition_AddPrev(1);
			_executor.ViPosition_AddPrev(2);
			AssertHistory("[(A:1)(A:2)][]");
			AssertPosition("A", 2, _executor.ViPosition_Prev(3));
			AssertHistory("[(A:1)][(A:2)(A:3)]");
			AssertPosition("A", 3, _executor.ViPosition_Next(2));
			AssertHistory("[(A:1)(A:2)][(A:3)]");
		}
		
		[Test]
		public void PrevNextPrev()
		{
			_executor.ViPosition_AddPrev(1);
			_executor.ViPosition_AddPrev(2);
			AssertHistory("[(A:1)(A:2)][]");
			AssertPosition("A", 2, _executor.ViPosition_Prev(3));
			AssertHistory("[(A:1)][(A:2)(A:3)]");
			AssertPosition("A", 3, _executor.ViPosition_Next(2));
			AssertHistory("[(A:1)(A:2)][(A:3)]");
			AssertPosition("A", 2, _executor.ViPosition_Prev(3));
			AssertHistory("[(A:1)][(A:2)(A:3)]");
		}
		
		[Test]
		public void Simple_DontFailIfNoOneElement()
		{
			Assert.AreEqual(null, _executor.ViPosition_Prev(1));
			AssertHistory("[][]");
			Assert.AreEqual(null, _executor.ViPosition_Next(1));
			AssertHistory("[][]");
		}
		
		[Test]
		public void WorksAfterMaxCountReached()
		{
			_executor.ViPosition_AddPrev(1);
			_executor.ViPosition_AddPrev(2);
			_executor.ViPosition_AddPrev(3);
			_executor.ViPosition_AddPrev(4);
			AssertHistory("[(A:2)(A:3)(A:4)][]");
			AssertPosition("A", 4, _executor.ViPosition_Prev(5));
			AssertHistory("[(A:3)][(A:4)(A:5)]");
			AssertPosition("A", 3, _executor.ViPosition_Prev(4));
			AssertHistory("[][(A:3)(A:4)(A:5)]");
			AssertPosition("A", 4, _executor.ViPosition_Next(3));
			AssertHistory("[(A:3)][(A:4)(A:5)]");
			AssertPosition("A", 5, _executor.ViPosition_Next(4));
			AssertHistory("[(A:3)(A:4)][(A:5)]");
		}
		
		[Test]
		public void NullAfterMaxCountOverflow()
		{
			_executor.ViPosition_AddPrev(1);
			_executor.ViPosition_AddPrev(2);
			_executor.ViPosition_AddPrev(3);
			_executor.ViPosition_AddPrev(4);
			AssertPosition("A", 4, _executor.ViPosition_Prev(5));
			AssertPosition("A", 3, _executor.ViPosition_Prev(4));
			AssertHistory("[][(A:3)(A:4)(A:5)]");
			Assert.AreEqual(null, _executor.ViPosition_Prev(2));
			Assert.AreEqual(null, _executor.ViPosition_Prev(2));
			AssertHistory("[][(A:3)(A:4)(A:5)]");
			AssertPosition("A", 4, _executor.ViPosition_Next(3));
			AssertHistory("[(A:3)][(A:4)(A:5)]");
			AssertPosition("A", 5, _executor.ViPosition_Next(4));
			AssertHistory("[(A:3)(A:4)][(A:5)]");
			Assert.AreEqual(null, _executor.ViPosition_Next(5));
			AssertHistory("[(A:3)(A:4)][(A:5)]");
		}
		
		[Test]
		public void Circle()
		{
			_executor.ViPosition_AddPrev(1);
			_executor.ViPosition_AddPrev(2);
			_executor.ViPosition_AddPrev(3);
			_executor.ViPosition_AddPrev(4);
			_executor.ViPosition_AddPrev(5);
			_executor.ViPosition_AddPrev(6);
			_executor.ViPosition_AddPrev(7);
			_executor.ViPosition_AddPrev(8);
			_executor.ViPosition_AddPrev(9);
			AssertPosition("A", 9, _executor.ViPosition_Prev(10));
			AssertPosition("A", 8, _executor.ViPosition_Prev(9));
			Assert.AreEqual(null, _executor.ViPosition_Prev(8));
			AssertPosition("A", 9, _executor.ViPosition_Next(8));
			AssertPosition("A", 10, _executor.ViPosition_Next(9));
			Assert.AreEqual(null, _executor.ViPosition_Next(10));
			AssertPosition("A", 9, _executor.ViPosition_Prev(10));
			AssertPosition("A", 8, _executor.ViPosition_Prev(9));
			Assert.AreEqual(null, _executor.ViPosition_Prev(8));
			AssertPosition("A", 9, _executor.ViPosition_Next(8));
			AssertPosition("A", 10, _executor.ViPosition_Next(9));
			AssertPosition("A", 9, _executor.ViPosition_Prev(10));
			_executor.ViPosition_AddPrev(19);
			Assert.AreEqual(null, _executor.ViPosition_Next(20));
		}
		
		//[Test]
		//public void ViPositionSet()
		//{
			//_executor.ViPosition_AddPrev(1);
			//_executor.ViPosition_AddPrev(2);
			//_executor.ViPosition_AddPrev(3);
			//_executor.ViPosition_AddPrev(4);
			//_executor.ViPosition_AddPrev(5);
			//_executor.ViPosition_AddPrev(6);
			//_executor.ViPosition_AddPrev(7);
			//_executor.ViPosition_AddPrev(8);
			//_executor.ViPosition_AddPrev(9);
			//_executor.ViPositionSet(19);
			//AssertPosition("A", 8, _executor.ViPosition_Prev());
			//AssertPosition("A", 7, _executor.ViPosition_Prev());
			//AssertPosition("A", 8, _executor.ViPosition_Next());
			//AssertPosition("A", 19, _executor.ViPosition_Next());
			//Assert.AreEqual(null, _executor.ViPosition_Next());
			//AssertPosition("A", 8, _executor.ViPosition_Prev());
			//AssertPosition("A", 19, _executor.ViPosition_Next());
			//Assert.AreEqual(null, _executor.ViPosition_Next());
			//_executor.ViPositionSet(29);
			//_executor.ViPositionSet(39);
			//AssertPosition("A", 8, _executor.ViPosition_Prev());
			//AssertPosition("A", 39, _executor.ViPosition_Next());
			//AssertPosition("A", 8, _executor.ViPosition_Prev());
			//_executor.ViPositionSet(18);
			//AssertPosition("A", 7, _executor.positionHistory[0]);
			//AssertPosition("A", 18, _executor.positionHistory[1]);
			//Assert.AreEqual(null, _executor.positionHistory[2]);
			//Assert.AreEqual(null, _executor.ViPosition_Next());
			//AssertPosition("A", 7, _executor.ViPosition_Prev());
		//}
	}
}
