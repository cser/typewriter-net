using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class PredictableListTest
	{
		[Test]
		public void Normal()
		{
			PredictableList<int> list = new PredictableList<int>(2);
			Assert.AreEqual(0, list.count);
			
			list.Add(3);
			Assert.AreEqual(1, list.count);
			list.Add(8);
			list.Add(5);
			Assert.AreEqual(3, list.count);
			
			Assert.AreEqual(3, list.buffer[0]);
			Assert.AreEqual(8, list.buffer[1]);
			Assert.AreEqual(5, list.buffer[2]);
			
			list.Clear();
			Assert.AreEqual(0, list.count);
		}
		
		[Test]
		public void Alocation()
		{
			PredictableList<int> list = new PredictableList<int>(2);
			Assert.AreEqual(0, list.count);
			Assert.AreEqual(2, list.buffer.Length);
			
			list.Add(3);
			Assert.AreEqual(1, list.count);
			CollectionAssert.AreEqual(new int[] { 3, 0 }, list.buffer);
			
			list.Add(8);
			Assert.AreEqual(2, list.count);
			CollectionAssert.AreEqual(new int[] { 3, 8 }, list.buffer);
			
			list.Add(5);
			Assert.AreEqual(3, list.count);
			CollectionAssert.AreEqual(new int[] { 3, 8, 5, 0 }, list.buffer);
			
			list.Clear();
			Assert.AreEqual(0, list.count);
			CollectionAssert.AreEqual(new int[] { 0, 0, 0, 0 }, list.buffer);
		}
		
		[Test]
		public void Pop_Normal()
		{
			PredictableList<int> list = new PredictableList<int>(2);
			list.Add(2);
			list.Add(8);
			list.Add(6);
			Assert.AreEqual(3, list.count);
			Assert.AreEqual(2, list.buffer[0]);
			Assert.AreEqual(8, list.buffer[1]);
			Assert.AreEqual(6, list.buffer[2]);
			
			Assert.AreEqual(6, list.Pop());
			Assert.AreEqual(2, list.count);
			Assert.AreEqual(2, list.buffer[0]);
			Assert.AreEqual(8, list.buffer[1]);
			Assert.AreEqual(0, list.buffer[2]);
			
			Assert.AreEqual(8, list.Pop());
			Assert.AreEqual(1, list.count);
			Assert.AreEqual(2, list.buffer[0]);
			Assert.AreEqual(0, list.buffer[1]);
			Assert.AreEqual(0, list.buffer[2]);
			
			Assert.AreEqual(2, list.Pop());
			Assert.AreEqual(0, list.count);
			Assert.AreEqual(0, list.buffer[0]);
			Assert.AreEqual(0, list.buffer[1]);
			Assert.AreEqual(0, list.buffer[2]);
			
			Assert.AreEqual(0, list.Pop());
			Assert.AreEqual(0, list.buffer[0]);
			Assert.AreEqual(0, list.buffer[1]);
			Assert.AreEqual(0, list.buffer[2]);
		}
		
		[Test]
		public void Realocate0()
		{
			PredictableList<string> list = new PredictableList<string>(2);
			list.Add("a");//2
			list.Add("b");//2
			list.Add("c");//4
			list.Add("d");//4
			list.Add("e");//8
			list.Add("f");//8
			list.Add("g");//8
			list.Add("h");//8
			list.Add("i");//16
			list.Add("j");//16
			list.Add("k");//16
			list.Add("l");//16
			list.Add("m");//16
			list.Add("n");//16
			Assert.AreEqual(14, list.count);
			Assert.AreEqual("a", list.buffer[0]);
			Assert.AreEqual("i", list.buffer[8]);
			Assert.AreEqual(16, list.buffer.Length);
			
			list.Realocate();
			Assert.AreEqual(16, list.buffer.Length);
			
			list.Pop();
			list.Pop();
			list.Pop();
			list.Pop();
			list.Pop();
			list.Pop();
			list.Realocate();
			Assert.AreEqual(8, list.count);
			Assert.AreEqual(16, list.buffer.Length);
			
			list.Pop();
			list.Pop();
			list.Pop();
			list.Pop();
			list.Realocate();
			Assert.AreEqual(4, list.count);
			Assert.AreEqual(8, list.buffer.Length);
			
			list.Pop();
			list.Pop();
			list.Realocate();
			Assert.AreEqual(2, list.count);
			Assert.AreEqual(4, list.buffer.Length);
			
			list.Pop();
			list.Realocate();
			Assert.AreEqual(1, list.count);
			Assert.AreEqual(2, list.buffer.Length);
			
			list.Pop();
			list.Realocate();
			Assert.AreEqual(0, list.count);
			Assert.AreEqual(2, list.buffer.Length);
		}
		
		[Test]
		public void Realocate1()
		{
			PredictableList<string> list = new PredictableList<string>(2);
			list.Add("a");//2
			list.Add("b");//2
			list.Add("c");//4
			list.Add("d");//4
			list.Add("e");//8
			list.Add("f");//8
			list.Add("g");//8
			list.Add("h");//8
			list.Add("i");//16
			list.Add("j");//16
			list.Add("k");//16
			list.Add("l");//16
			list.Add("m");//16
			list.Add("n");//16
			Assert.AreEqual(14, list.count);
			Assert.AreEqual("a", list.buffer[0]);
			Assert.AreEqual("i", list.buffer[8]);
			Assert.AreEqual(16, list.buffer.Length);
			
			list.Realocate();
			Assert.AreEqual(16, list.buffer.Length);
			
			list.Pop();
			list.Pop();
			list.Pop();
			list.Pop();
			list.Pop();
			list.Pop();
			Assert.AreEqual(8, list.count);
			Assert.AreEqual(16, list.buffer.Length);
			
			list.Pop();
			list.Pop();
			list.Pop();
			list.Pop();
			Assert.AreEqual(4, list.count);
			Assert.AreEqual(16, list.buffer.Length);
			
			list.Pop();
			list.Pop();
			list.Realocate();
			Assert.AreEqual(2, list.count);
			Assert.AreEqual(4, list.buffer.Length);
			
			list.Pop();
			Assert.AreEqual(1, list.count);
			Assert.AreEqual(4, list.buffer.Length);
			
			list.Pop();
			list.Realocate();
			Assert.AreEqual(0, list.count);
			Assert.AreEqual(2, list.buffer.Length);
		}
		
		[Test]
		public void Resize_Expand0()
		{
			PredictableList<string> list = new PredictableList<string>(2);
			list.Add("a");
			list.Add("b");
			list.Add("c");
			Assert.AreEqual(3, list.count);
			Assert.AreEqual(4, list.buffer.Length);
			
			list.Resize(4);
			Assert.AreEqual(4, list.count);
			Assert.AreEqual(4, list.buffer.Length);
			Assert.AreEqual(null, list.buffer[3]);
		}
		
		[Test]
		public void Resize_Expand1()
		{
			PredictableList<string> list = new PredictableList<string>(2);
			list.Add("a");
			list.Add("b");
			list.Add("c");
			Assert.AreEqual(3, list.count);
			Assert.AreEqual(4, list.buffer.Length);
			
			list.Resize(5);
			Assert.AreEqual(5, list.count);
			Assert.AreEqual(8, list.buffer.Length);
			Assert.AreEqual("a", list.buffer[0]);
			Assert.AreEqual("b", list.buffer[1]);
			Assert.AreEqual("c", list.buffer[2]);
			Assert.AreEqual(null, list.buffer[3]);
			Assert.AreEqual(null, list.buffer[4]);
			Assert.AreEqual(null, list.buffer[5]);
			Assert.AreEqual(null, list.buffer[6]);
			Assert.AreEqual(null, list.buffer[7]);
			
			list.Resize(8);
			Assert.AreEqual(8, list.count);
			Assert.AreEqual(8, list.buffer.Length);
			Assert.AreEqual("a", list.buffer[0]);
			Assert.AreEqual("b", list.buffer[1]);
			Assert.AreEqual("c", list.buffer[2]);
			Assert.AreEqual(null, list.buffer[3]);
			Assert.AreEqual(null, list.buffer[4]);
			Assert.AreEqual(null, list.buffer[5]);
			Assert.AreEqual(null, list.buffer[6]);
			Assert.AreEqual(null, list.buffer[7]);
		}
		
		[Test]
		public void Resize_Expand2()
		{
			PredictableList<string> list = new PredictableList<string>(2);
			list.Add("a");
			list.Add("b");
			list.Add("c");
			Assert.AreEqual(3, list.count);
			Assert.AreEqual(4, list.buffer.Length);
			
			list.Resize(9);
			Assert.AreEqual(9, list.count);
			Assert.AreEqual(16, list.buffer.Length);
			Assert.AreEqual("a", list.buffer[0]);
			Assert.AreEqual("b", list.buffer[1]);
			Assert.AreEqual("c", list.buffer[2]);
			Assert.AreEqual(null, list.buffer[3]);
			Assert.AreEqual(null, list.buffer[4]);
			Assert.AreEqual(null, list.buffer[5]);
			Assert.AreEqual(null, list.buffer[6]);
			Assert.AreEqual(null, list.buffer[7]);
			Assert.AreEqual(null, list.buffer[15]);
		}
		
		[Test]
		public void Resize_Shrink0()
		{
			PredictableList<string> list = new PredictableList<string>(2);
			list.Add("a");
			list.Add("b");
			list.Add("c");
			Assert.AreEqual(3, list.count);
			Assert.AreEqual(4, list.buffer.Length);
			
			list.Resize(2);
			Assert.AreEqual(2, list.count);
			Assert.AreEqual(4, list.buffer.Length);
			Assert.AreEqual("a", list.buffer[0]);
			Assert.AreEqual("b", list.buffer[1]);
			Assert.AreEqual(null, list.buffer[2]);
			Assert.AreEqual(null, list.buffer[3]);
		}
		
		[Test]
		public void Resize_Shrink1()
		{
			PredictableList<string> list = new PredictableList<string>(2);
			list.Add("a");
			list.Add("b");
			list.Add("c");
			list.Add("d");
			list.Add("e");
			list.Add("f");
			list.Add("g");
			list.Add("h");
			list.Add("i");
			list.Add("j");
			list.Add("k");
			list.Add("l");
			list.Add("m");
			list.Add("n");
			Assert.AreEqual(14, list.count);
			Assert.AreEqual("a", list.buffer[0]);
			Assert.AreEqual("i", list.buffer[8]);
			Assert.AreEqual(16, list.buffer.Length);
			
			list.Resize(2);
			Assert.AreEqual(2, list.count);
			Assert.AreEqual(16, list.buffer.Length);
			Assert.AreEqual("a", list.buffer[0]);
			Assert.AreEqual("b", list.buffer[1]);
			Assert.AreEqual(null, list.buffer[2]);
			Assert.AreEqual(null, list.buffer[3]);
			Assert.AreEqual(null, list.buffer[15]);
		}
		
		[Test]
		public void ToArray()
		{
			PredictableList<string> list = new PredictableList<string>(2);
			list.Add("a");
			list.Add("b");
			list.Add("c");
			CollectionAssert.AreEqual(new string[] { "a", "b", "c" }, list.ToArray());
		}
		
		[Test]
		public void Peek_Normal()
		{
			PredictableList<string> list = new PredictableList<string>(2);
			list.Add("a");
			list.Add("b");
			list.Add("c");
			CollectionAssert.AreEqual(new string[] { "a", "b", "c" }, list.ToArray());
			
			Assert.AreEqual("c", list.Peek());
			CollectionAssert.AreEqual(new string[] { "a", "b", "c" }, list.ToArray());
		}
		
		[Test]
		public void Peek_ThrowsExceptionOnEmptyList()
		{
			PredictableList<string> list = new PredictableList<string>(2);
			list.Add("a");
			list.Add("b");
			CollectionAssert.AreEqual(new string[] { "a", "b" }, list.ToArray());
			list.Pop();
			list.Pop();
			CollectionAssert.AreEqual(new string[] { }, list.ToArray());
			
			try
			{
				Assert.AreEqual("c", list.Peek());
				Assert.Fail("Mast throws exception");
			}
			catch (IndexOutOfRangeException)
			{
			}
		}
	}
}
