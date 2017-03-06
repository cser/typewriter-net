using System;
using System.Text;
using MulticaretEditor;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class DequeTest
	{
		[Test]
		public void TestPutPop()
		{
			Deque<int> deque = new Deque<int>();
			Assert.AreEqual(true, deque.IsEmpty);
			deque.Put(1);
			Assert.AreEqual(false, deque.IsEmpty);
			deque.Put(2);
			Assert.AreEqual(false, deque.IsEmpty);
			Assert.AreEqual(1, deque.Pop());
			Assert.AreEqual(false, deque.IsEmpty);
			Assert.AreEqual(2, deque.Pop());
			Assert.AreEqual(true, deque.IsEmpty);
		}
		
		[Test]
		public void TestPushPutPop()
		{
			Deque<int> deque = new Deque<int>();
			Assert.AreEqual(true, deque.IsEmpty);
			deque.Push(1);
			Assert.AreEqual(false, deque.IsEmpty);
			deque.Push(2);
			Assert.AreEqual(false, deque.IsEmpty);
			deque.Put(3);
			Assert.AreEqual(false, deque.IsEmpty);
			Assert.AreEqual(false, deque.IsEmpty);
			Assert.AreEqual(2, deque.Pop());
			Assert.AreEqual(false, deque.IsEmpty);
			Assert.AreEqual(1, deque.Pop());
			Assert.AreEqual(false, deque.IsEmpty);
			Assert.AreEqual(3, deque.Pop());
			Assert.AreEqual(true, deque.IsEmpty);
		}
	}
}
