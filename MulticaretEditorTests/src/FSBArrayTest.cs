using System;
using System.Text;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class FSBArrayTest : FSBArrayTestBase
	{
		[Test]
		public void Add()
		{
			Init(3);
			
			Assert.AreEqual(0, array.Count);
			array.Add("a");
			Assert.AreEqual(1, array.Count);
			Assert.AreEqual("a", array[0]);
			
			array.Add("b");
			array.Add("c");
			array.Add("d");
			Assert.AreEqual(4, array.Count);
			CollectionAssert.AreEqual(Strings("a", "b", "c", "d"), array.ToArray());
			
			array.Add("e");
			array.Add("f");
			array.Add("g");
			Assert.AreEqual(7, array.Count);
			CollectionAssert.AreEqual(Strings("a", "b", "c", "d", "e", "f", "g"), array.ToArray());
		}
		
		[Test]
		public void BlocksAllocation_Add()
		{
			Init(3);
			
			Assert.AreEqual(0, array.BlocksCount);
			array.Add("1");
			Assert.AreEqual(1, array.BlocksCount);
			array.Add("2");
			array.Add("3");
			Assert.AreEqual(1, array.BlocksCount);
			array.Add("1");
			Assert.AreEqual(2, array.BlocksCount);
			array.Add("2");
			array.Add("3");
			array.Add("1");
			array.Add("2");
			array.Add("3");
			array.Add("1");
			array.Add("2");
			array.Add("3");		
			CollectionAssert.AreEqual(Strings("1", "2", "3", "1", "2", "3", "1", "2", "3", "1", "2", "3"), array.ToArray());
			Assert.AreEqual(4, array.BlocksCount);
			Assert.AreEqual(4, array.BlocksLength);
			
			array.Add("1");
			CollectionAssert.AreEqual(Strings("1", "2", "3", "1", "2", "3", "1", "2", "3", "1", "2", "3", "1"), array.ToArray());
			Assert.AreEqual(5, array.BlocksCount);
			Assert.AreEqual(8, array.BlocksLength);
	
			for (int i = 0; i < 4 * 3 - 1; i++)
			{
				array.Add("x");
			}
			Assert.AreEqual(8, array.BlocksCount);
			Assert.AreEqual(8, array.BlocksLength);
			array.Add("1");
			Assert.AreEqual(9, array.BlocksCount);
			Assert.AreEqual(16, array.BlocksLength);
		}
		
		[Test]
		public void BlocksAllocation_Insert()
		{
			Init(3);
			
			Assert.AreEqual(0, array.BlocksCount);
			array.Insert(0, "1");// 1
			Assert.AreEqual("0:[1; (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(1, array.BlocksCount);
			array.Add("2");// 1 2
			Assert.AreEqual("0:[1; 2; ()]", array.GetBlocksInfo());
			array.Insert(0, "3");// 3 1 2
			Assert.AreEqual("0:[3; 1; 2]", array.GetBlocksInfo());
			Assert.AreEqual(1, array.BlocksCount);
			array.Add("1");// 3 1 2 1
			Assert.AreEqual("0:[3; 1; 2]; 3:[1; (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(2, array.BlocksCount);
			array.Add("2");// 3 1 2 1 2
			Assert.AreEqual("0:[3; 1; 2]; 3:[1; 2; ()]", array.GetBlocksInfo());
			array.Add("3");// 3 1 2 1 2 3
			Assert.AreEqual("0:[3; 1; 2]; 3:[1; 2; 3]", array.GetBlocksInfo());
			array.Insert(2, "1");// 3 1 1 2 1 2 3
			Assert.AreEqual("0:[3; 1; 1]; 3:[2; (); ()]; 4:[1; 2; 3]", array.GetBlocksInfo());
			array.Add("2");// 3 1 1 2 1 2 3 2
			Assert.AreEqual("0:[3; 1; 1]; 3:[2; (); ()]; 4:[1; 2; 3]; 7:[2; (); ()]", array.GetBlocksInfo());
			array.Insert(5, "3");// 3 1 1 2 1 3 2 3 2
			Assert.AreEqual("0:[3; 1; 1]; 3:[2; (); ()]; 4:[1; 3; 2]; 7:[3; 2; ()]", array.GetBlocksInfo());
			array.Add("1");// 3 1 1 2 1 3 2 3 2 1
			Assert.AreEqual("0:[3; 1; 1]; 3:[2; (); ()]; 4:[1; 3; 2]; 7:[3; 2; 1]", array.GetBlocksInfo());
			array.Insert(0, "2");// 2 3 1 1 2 1 3 2 3 2 1
			Assert.AreEqual("0:[2; 3; 1]; 3:[1; 2; ()]; 5:[1; 3; 2]; 8:[3; 2; 1]", array.GetBlocksInfo());
			array.Insert(7, "3");// 2 3 1 1 2 1 3 3 2 3 2 1
			Assert.AreEqual("0:[2; 3; 1]; 3:[1; 2; ()]; 5:[1; 3; 3]; 8:[2; (); ()]; 9:[3; 2; 1]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "3", "1", "1", "2", "1", "3", "3", "2", "3", "2", "1"), array.ToArray());
			Assert.AreEqual(5, array.BlocksCount);
			Assert.AreEqual(8, array.BlocksLength);
			
			array.Add("1");
			Assert.AreEqual("0:[2; 3; 1]; 3:[1; 2; ()]; 5:[1; 3; 3]; 8:[2; (); ()]; 9:[3; 2; 1]; 12:[1; (); ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "3", "1", "1", "2", "1", "3", "3", "2", "3", "2", "1", "1"), array.ToArray());
			Assert.AreEqual(6, array.BlocksCount);
			Assert.AreEqual(8, array.BlocksLength);
		}
		
		[Test]
		public void Insert0()
		{
			Init(3);
			array.Add("a");
			array.Add("c");
			Assert.AreEqual("0:[a; c; ()]", array.GetBlocksInfo());
			Assert.AreEqual(2, array.Count);
			CollectionAssert.AreEqual(Strings("a", "c"), array.ToArray());
			array.Insert(1, "b");
			Assert.AreEqual("0:[a; b; c]", array.GetBlocksInfo());
			Assert.AreEqual(3, array.Count);
			CollectionAssert.AreEqual(Strings("a", "b", "c"), array.ToArray());
			
			Init(3);
			array.Add("b");
			array.Add("c");
			Assert.AreEqual("0:[b; c; ()]", array.GetBlocksInfo());
			Assert.AreEqual(2, array.Count);
			CollectionAssert.AreEqual(Strings("b", "c"), array.ToArray());		
			array.Insert(0, "a");
			Assert.AreEqual("0:[a; b; c]", array.GetBlocksInfo());
			Assert.AreEqual(3, array.Count);
			CollectionAssert.AreEqual(Strings("a", "b", "c"), array.ToArray());
			
			Init(3);
			array.Add("a");
			array.Add("b");
			Assert.AreEqual("0:[a; b; ()]", array.GetBlocksInfo());
			Assert.AreEqual(2, array.Count);
			CollectionAssert.AreEqual(Strings("a", "b"), array.ToArray());		
			array.Insert(2, "c");
			Assert.AreEqual("0:[a; b; c]", array.GetBlocksInfo());
			Assert.AreEqual(3, array.Count);
			CollectionAssert.AreEqual(Strings("a", "b", "c"), array.ToArray());
		}
		
		[Test]
		public void Insert1()
		{
			Init(3);
			array.Add("a");
			array.Add("b");
			array.Add("c");
			CollectionAssert.AreEqual(Strings("a", "b", "c"), array.ToArray());
			array.Insert(3, "d");
			CollectionAssert.AreEqual(Strings("a", "b", "c", "d"), array.ToArray());
			array.Insert(3, "e");
			CollectionAssert.AreEqual(Strings("a", "b", "c", "e", "d"), array.ToArray());
			array.Insert(4, "f");
			CollectionAssert.AreEqual(Strings("a", "b", "c", "e", "f", "d"), array.ToArray());
			
			Init(3);
			array.Add("a");
			array.Add("b");
			array.Add("c");
			array.Add("d");
			CollectionAssert.AreEqual(Strings("a", "b", "c", "d"), array.ToArray());
			array.Insert(4, "e");
			CollectionAssert.AreEqual(Strings("a", "b", "c", "d", "e"), array.ToArray());
			array.Insert(3, "f");
			CollectionAssert.AreEqual(Strings("a", "b", "c", "f", "d", "e"), array.ToArray());
			
			Init(3);
			array.Add("a");
			array.Add("b");
			array.Add("c");
			array.Add("d");
			array.Add("e");
			CollectionAssert.AreEqual(Strings("a", "b", "c", "d", "e"), array.ToArray());
			array.Insert(5, "f");
			CollectionAssert.AreEqual(Strings("a", "b", "c", "d", "e", "f"), array.ToArray());
		}
		
		[Test]
		public void Insert2()
		{
			Init(3);
			array.Add("a");
			array.Add("b");
			array.Add("c");
			CollectionAssert.AreEqual(Strings("a", "b", "c"), array.ToArray());
			array.Insert(0, "d");
			CollectionAssert.AreEqual(Strings("d", "a", "b", "c"), array.ToArray());
			array.Insert(1, "e");
			CollectionAssert.AreEqual(Strings("d", "e", "a", "b", "c"), array.ToArray());
			array.Insert(2, "f");
			CollectionAssert.AreEqual(Strings("d", "e", "f", "a", "b", "c"), array.ToArray());
		}
		
		[Test]
		public void Insert3()
		{
			Init(3);
			array.Add("1");
			array.Add("2");
			array.Add("3");
			array.Add("11");
			array.Add("12");
			array.Add("13");
			array.Add("21");
			array.Add("22");
			CollectionAssert.AreEqual(Strings("1", "2", "3", "11", "12", "13", "21", "22"), array.ToArray());
			array.Insert(0, "a");
			CollectionAssert.AreEqual(Strings("a", "1", "2", "3", "11", "12", "13", "21", "22"), array.ToArray());
		}
		
		[Test]
		public void Insert4()
		{
			Init(3);
			array.Add("1");
			array.Add("2");
			array.Add("3");
			array.Add("11");
			array.Add("12");
			array.Add("13");
			array.Add("21");
			array.Add("22");
			CollectionAssert.AreEqual(Strings("1", "2", "3", "11", "12", "13", "21", "22"), array.ToArray());
			array.Insert(0, "a");
			CollectionAssert.AreEqual(Strings("a", "1", "2", "3", "11", "12", "13", "21", "22"), array.ToArray());
			array.Insert(3, "b");
			CollectionAssert.AreEqual(Strings("a", "1", "2", "b", "3", "11", "12", "13", "21", "22"), array.ToArray());
		}
		
		[Test]
		public void Insert5()
		{
			Init(3);
			array.Add("1");
			array.Add("2");
			array.Add("3");
			array.Add("11");
			array.Add("12");
			array.Add("13");
			array.Add("21");
			array.Add("22");
			CollectionAssert.AreEqual(Strings("1", "2", "3", "11", "12", "13", "21", "22"), array.ToArray());
			array.Insert(0, "a");
			CollectionAssert.AreEqual(Strings("a", "1", "2", "3", "11", "12", "13", "21", "22"), array.ToArray());
			array.Insert(3, "b");
			CollectionAssert.AreEqual(Strings("a", "1", "2", "b", "3", "11", "12", "13", "21", "22"), array.ToArray());
			array.Insert(4, "c");
			CollectionAssert.AreEqual(Strings("a", "1", "2", "b", "c", "3", "11", "12", "13", "21", "22"), array.ToArray());
		}
		
		[Test]
		public void Remove()
		{
			Init(3);
			array.Add("1");
			array.Add("2");
			array.Add("3");
			array.Add("11");
			array.Add("12");
			array.Add("13");
			array.Add("21");
			array.Add("22");
			Assert.AreEqual("0:[1; 2; 3]; 3:[11; 12; 13]; 6:[21; 22; ()]", array.GetBlocksInfo());
			Assert.AreEqual(8, array.Count);
			CollectionAssert.AreEqual(Strings("1", "2", "3", "11", "12", "13", "21", "22"), array.ToArray());
			array.RemoveAt(0);
			Assert.AreEqual("0:[2; 3; ()]; 2:[11; 12; 13]; 5:[21; 22; ()]", array.GetBlocksInfo());
			Assert.AreEqual(7, array.Count);
			CollectionAssert.AreEqual(Strings("2", "3", "11", "12", "13", "21", "22"), array.ToArray());
			array.RemoveAt(2);
			Assert.AreEqual("0:[2; 3; ()]; 2:[12; 13; ()]; 4:[21; 22; ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "3", "12", "13", "21", "22"), array.ToArray());
			array.RemoveAt(1);
			Assert.AreEqual("0:[2; 12; 13]; 3:[21; 22; ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "12", "13", "21", "22"), array.ToArray());
			array.RemoveAt(4);
			Assert.AreEqual("0:[2; 12; 13]; 3:[21; (); ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "12", "13", "21"), array.ToArray());
			array.RemoveAt(2);
			Assert.AreEqual("0:[2; 12; 21]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "12", "21"), array.ToArray());
			array.RemoveAt(0);
			Assert.AreEqual("0:[12; 21; ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("12", "21"), array.ToArray());
			array.RemoveAt(0);
			Assert.AreEqual("0:[21; (); ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("21"), array.ToArray());
			array.RemoveAt(0);
			Assert.AreEqual("", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings(), array.ToArray());
		}
		
		[Test]
		public void RemoveAndInsert()
		{
			Init(3);
			array.Add("1");
			array.Add("2");
			array.Add("3");
			array.Add("11");
			array.Add("12");
			array.Add("13");
			array.Add("21");
			array.Add("22");
			Assert.AreEqual("0:[1; 2; 3]; 3:[11; 12; 13]; 6:[21; 22; ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("1", "2", "3", "11", "12", "13", "21", "22"), array.ToArray());
			array.RemoveAt(0);
			Assert.AreEqual("0:[2; 3; ()]; 2:[11; 12; 13]; 5:[21; 22; ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "3", "11", "12", "13", "21", "22"), array.ToArray());
			array.Insert(2, "a");
			Assert.AreEqual("0:[2; 3; a]; 3:[11; 12; 13]; 6:[21; 22; ()]", array.GetBlocksInfo());
			Assert.AreEqual(8, array.Count);
			CollectionAssert.AreEqual(Strings("2", "3", "a", "11", "12", "13", "21", "22"), array.ToArray());
			array.Insert(6, "b");
			Assert.AreEqual("0:[2; 3; a]; 3:[11; 12; 13]; 6:[b; 21; 22]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "3", "a", "11", "12", "13", "b", "21", "22"), array.ToArray());
			array.Insert(8, "c");
			Assert.AreEqual("0:[2; 3; a]; 3:[11; 12; 13]; 6:[b; 21; c]; 9:[22; (); ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "3", "a", "11", "12", "13", "b", "21", "c", "22"), array.ToArray());
			array.Insert(10, "d");
			Assert.AreEqual("0:[2; 3; a]; 3:[11; 12; 13]; 6:[b; 21; c]; 9:[22; d; ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "3", "a", "11", "12", "13", "b", "21", "c", "22", "d"), array.ToArray());
			array.RemoveAt(9);
			Assert.AreEqual("0:[2; 3; a]; 3:[11; 12; 13]; 6:[b; 21; c]; 9:[d; (); ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "3", "a", "11", "12", "13", "b", "21", "c", "d"), array.ToArray());
			array.RemoveAt(4);
			Assert.AreEqual("0:[2; 3; a]; 3:[11; 13; ()]; 5:[b; 21; c]; 8:[d; (); ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "3", "a", "11", "13", "b", "21", "c", "d"), array.ToArray());
			array.RemoveAt(3);
			Assert.AreEqual("0:[2; 3; a]; 3:[13; (); ()]; 4:[b; 21; c]; 7:[d; (); ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "3", "a", "13", "b", "21", "c", "d"), array.ToArray());
			array.RemoveAt(7);
			Assert.AreEqual("0:[2; 3; a]; 3:[13; (); ()]; 4:[b; 21; c]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "3", "a", "13", "b", "21", "c"), array.ToArray());
			array.RemoveAt(5);
			Assert.AreEqual("0:[2; 3; a]; 3:[13; b; c]", array.GetBlocksInfo());
			Assert.AreEqual(6, array.Count);
			CollectionAssert.AreEqual(Strings("2", "3", "a", "13", "b", "c"), array.ToArray());
			array.Insert(3, "f");
			Assert.AreEqual("0:[2; 3; a]; 3:[f; 13; b]; 6:[c; (); ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "3", "a", "f", "13", "b", "c"), array.ToArray());
			array.RemoveAt(1);
			Assert.AreEqual("0:[2; a; ()]; 2:[f; 13; b]; 5:[c; (); ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("2", "a", "f", "13", "b", "c"), array.ToArray());
			array.RemoveAt(0);
			Assert.AreEqual("0:[a; (); ()]; 1:[f; 13; b]; 4:[c; (); ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("a", "f", "13", "b", "c"), array.ToArray());
			array.RemoveAt(2);
			Assert.AreEqual("0:[a; f; b]; 3:[c; (); ()]", array.GetBlocksInfo());
			CollectionAssert.AreEqual(Strings("a", "f", "b", "c"), array.ToArray());
			array.RemoveAt(3);
			CollectionAssert.AreEqual(Strings("a", "f", "b"), array.ToArray());
			array.RemoveAt(2);
			CollectionAssert.AreEqual(Strings("a", "f"), array.ToArray());
			array.RemoveAt(1);
			CollectionAssert.AreEqual(Strings("a"), array.ToArray());
			array.RemoveAt(0);
			CollectionAssert.AreEqual(Strings(), array.ToArray());
		}
		
		[Test]
		public void GetIndices0()
		{
			Init(3);
			array.Add("1");
			array.Add("2");
			array.Add("3");
			array.Add("11");
			array.Add("12");
			array.Add("13");
			array.Add("21");
			array.Add("22");
			array.Add("23");
			array.Add("31");
			array.Add("32");
			array.Add("33");
			array.Add("41");
			array.Add("42");
			array.Add("43");
			array.Add("51");
			array.Add("52");
			array.Add("53");
			array.Add("61");
			array.Add("62");
			array.Add("63");
			Assert.AreEqual("1", array[0]);
			Assert.AreEqual("43", array[14]);
			Assert.AreEqual("22", array[7]);
			Assert.AreEqual("11", array[3]);
		}
		
		[Test]
		public void GetIndices1()
		{
			Init(3);
			array.Add("1");
			array.Add("2");
			array.Add("3");
			Assert.AreEqual("1", array[0]);
			Assert.AreEqual("2", array[1]);
			Assert.AreEqual("3", array[2]);
		}
		
		[Test]
		public void RemoveRange0()
		{
			Init(3);
			
			array.Add("a");
			array.Add("b");
			array.Add("c");
			Assert.AreEqual("0:[a; b; c]", array.GetBlocksInfo());
			Assert.AreEqual(3, array.Count);
			
			array.RemoveRange(1, 1);
			Assert.AreEqual("0:[a; c; ()]", array.GetBlocksInfo());
			Assert.AreEqual(2, array.Count);
			
			array.RemoveRange(1, 1);
			Assert.AreEqual("0:[a; (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(1, array.Count);
			
			array.RemoveRange(0, 1);
			Assert.AreEqual("", array.GetBlocksInfo());
			Assert.AreEqual(0, array.Count);
			
			Init(5);
			
			array.Add("a");
			array.Add("b");
			array.Add("c");
			array.Add("d");
			array.Add("e");
			Assert.AreEqual("0:[a; b; c; d; e]", array.GetBlocksInfo());
			Assert.AreEqual(5, array.Count);
			
			array.RemoveRange(1, 3);
			Assert.AreEqual("0:[a; e; (); (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(2, array.Count);
		}
		
		private void InitForRangeTests0()
		{
			Init(4);
			for (int i = 0; i < 19; i++)
			{
				array.Add(i + "");
			}
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; 10; 11]; 12:[12; 13; 14; 15]; 16:[16; 17; 18; ()]", array.GetBlocksInfo());
		}
		
		private void InitForRangeTests1()
		{
			Init(4);
			for (int i = 0; i < 15; i++)
			{
				array.Add(i + "");
			}
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; 10; 11]; 12:[12; 13; 14; ()]", array.GetBlocksInfo());
			array.RemoveAt(7);
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 9; 10; 11]; 11:[12; 13; 14; ()]", array.GetBlocksInfo());
			array.RemoveAt(8);
			array.RemoveAt(8);
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 11; (); ()]; 9:[12; 13; 14; ()]", array.GetBlocksInfo());
			array.Add("15");
			array.Add("16");
			array.Add("17");
			array.Add("18");
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 11; (); ()]; 9:[12; 13; 14; 15]; 13:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(16, array.Count);
		}
		
		[Test]
		public void RemoveRange1()
		{
			InitForRangeTests1();
			array.RemoveRange(13, 2);
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 11; (); ()]; 9:[12; 13; 14; 15]; 13:[18; (); (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(14, array.Count);
			
			InitForRangeTests1();
			array.RemoveRange(1, 2);
			Assert.AreEqual("0:[0; 3; (); ()]; 2:[4; 5; 6; ()]; 5:[8; 11; (); ()]; 7:[12; 13; 14; 15]; 11:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(14, array.Count);
			
			InitForRangeTests1();
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 11; (); ()]; 9:[12; 13; 14; 15]; 13:[16; 17; 18; ()]", array.GetBlocksInfo());
			array.Insert(9, "19");
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 11; 19; ()]; 10:[12; 13; 14; 15]; 14:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(17, array.Count);
			array.RemoveRange(8, 1);
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 19; (); ()]; 9:[12; 13; 14; 15]; 13:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(16, array.Count);
		}
		
		[Test]
		public void RemoveRange2()
		{
			InitForRangeTests0();
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; 10; 11]; 12:[12; 13; 14; 15]; 16:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(19, array.Count);
			array.RemoveRange(6, 3);
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; (); ()]; 6:[9; 10; 11; ()]; 9:[12; 13; 14; 15]; 13:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(16, array.Count);
		}
		
		[Test]
		public void RemoveRange3()
		{
			InitForRangeTests0();
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; 10; 11]; 12:[12; 13; 14; 15]; 16:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(19, array.Count);
			array.RemoveRange(6, 4);
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 10; 11]; 8:[12; 13; 14; 15]; 12:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(15, array.Count);
		}
		
		[Test]
		public void RemoveRange4()
		{
			InitForRangeTests0();
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; 10; 11]; 12:[12; 13; 14; 15]; 16:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(19, array.Count);
			array.RemoveRange(7, 9);
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(10, array.Count);
		}
		
		[Test]
		public void RemoveRange5()
		{
			InitForRangeTests0();
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; 10; 11]; 12:[12; 13; 14; 15]; 16:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(19, array.Count);
			array.RemoveRange(4, 7);
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[11; (); (); ()]; 5:[12; 13; 14; 15]; 9:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(12, array.Count);
		}
		
		[Test]
		public void RemoveRange6()
		{
			InitForRangeTests0();
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; 10; 11]; 12:[12; 13; 14; 15]; 16:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(19, array.Count);
			array.RemoveRange(4, 4);
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[8; 9; 10; 11]; 8:[12; 13; 14; 15]; 12:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(15, array.Count);
		}
		
		[Test]
		public void RemoveRange7()
		{
			InitForRangeTests0();
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; 10; 11]; 12:[12; 13; 14; 15]; 16:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(19, array.Count);
			array.RemoveRange(4, 8);
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[12; 13; 14; 15]; 8:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(11, array.Count);
		}
		
		[Test]
		public void RemoveRange8()
		{
			InitForRangeTests0();
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; 10; 11]; 12:[12; 13; 14; 15]; 16:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(19, array.Count);
			array.RemoveRange(6, 7);
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; (); ()]; 6:[13; 14; 15; ()]; 9:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(12, array.Count);
		}
		
		[Test]
		public void RemoveRange9()
		{
			InitForRangeTests0();
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; 10; 11]; 12:[12; 13; 14; 15]; 16:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(19, array.Count);
			array.RemoveRange(0, 19);
			Assert.AreEqual("", array.GetBlocksInfo());
			Assert.AreEqual(0, array.Count);
			
			InitForRangeTests0();
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; 10; 11]; 12:[12; 13; 14; 15]; 16:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(19, array.Count);
			array.RemoveRange(0, 8);
			Assert.AreEqual("0:[8; 9; 10; 11]; 4:[12; 13; 14; 15]; 8:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(11, array.Count);
			
			InitForRangeTests0();
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; 10; 11]; 12:[12; 13; 14; 15]; 16:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(19, array.Count);
			array.RemoveRange(0, 9);
			Assert.AreEqual("0:[9; 10; 11; ()]; 3:[12; 13; 14; 15]; 7:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(10, array.Count);
			array.RemoveRange(3, 3);
			Assert.AreEqual("0:[9; 10; 11; 15]; 4:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(7, array.Count);
		}
		
		[Test]
		public void RemoveRange10()
		{
			InitForRangeTests1();
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 11; (); ()]; 9:[12; 13; 14; 15]; 13:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(16, array.Count);
			
			array.RemoveRange(5, 0);
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 11; (); ()]; 9:[12; 13; 14; 15]; 13:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(16, array.Count);
			
			array.RemoveRange(15, 0);
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 11; (); ()]; 9:[12; 13; 14; 15]; 13:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(16, array.Count);
			
			array.RemoveRange(16, 0);
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 11; (); ()]; 9:[12; 13; 14; 15]; 13:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(16, array.Count);
			
			array.RemoveRange(0, 16);
			Assert.AreEqual("", array.GetBlocksInfo());
			Assert.AreEqual(0, array.Count);
		}
		
		[Test]
		public void InsertRange0()
		{
			InitForRangeTests1();
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 11; (); ()]; 9:[12; 13; 14; 15]; 13:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(16, array.Count);
			array.InsertRange(5, new string[] { "a" });
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; a; 5; 6]; 8:[8; 11; (); ()]; 10:[12; 13; 14; 15]; 14:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(17, array.Count);		
			
			InitForRangeTests1();
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 11; (); ()]; 9:[12; 13; 14; 15]; 13:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(16, array.Count);
			array.InsertRange(9, new string[] { "a" });
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 11; a; ()]; 10:[12; 13; 14; 15]; 14:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(17, array.Count);
		}
		
		[Test]
		public void InsertRange1()
		{
			InitForRangeTests1();
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 11; (); ()]; 9:[12; 13; 14; 15]; 13:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(16, array.Count);
			array.InsertRange(5, new string[] { "a" });
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; a; 5; 6]; 8:[8; 11; (); ()]; 10:[12; 13; 14; 15]; 14:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(17, array.Count);		
			array.InsertRange(8, new string[] { "b", "c" });
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; a; 5; 6]; 8:[b; c; 8; 11]; 12:[12; 13; 14; 15]; 16:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(19, array.Count);
		}
		
		[Test]
		public void InsertRange2()
		{
			InitForRangeTests1();
			CollectionAssert.AreEqual(new string[] { "0", "1", "2", "3", "4", "5", "6", "8", "11", "12", "13", "14", "15", "16", "17", "18" }, array.ToArray());
			Assert.AreEqual(16, array.Count);
			array.InsertRange(8, new string[] { "a", "b", "c", "d", "e" });
			Assert.AreEqual("0, 1, 2, 3, 4, 5, 6, 8, a, b, c, d, e, 11, 12, 13, 14, 15, 16, 17, 18", string.Join(", ", array.ToArray()));
			Assert.AreEqual(21, array.Count);
		}
		
		[Test]
		public void InsertRange3()
		{
			InitForRangeTests1();
			Assert.AreEqual("0, 1, 2, 3, 4, 5, 6, 8, 11, 12, 13, 14, 15, 16, 17, 18", string.Join(", ", array.ToArray()));
			Assert.AreEqual(16, array.Count);
			array.InsertRange(16, new string[] { "a", "b", "c", "d", "e" });
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; ()]; 7:[8; 11; (); ()]; 9:[12; 13; 14; 15]; 13:[16; 17; 18; a]; 17:[b; c; d; e]", array.GetBlocksInfo());
			Assert.AreEqual(21, array.Count);
		}
		
		[Test]
		public void Clear()
		{
			Init(4);
			for (int i = 0; i < 19; i++)
			{
				array.Add(i + "");
			}
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; 10; 11]; 12:[12; 13; 14; 15]; 16:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(19, array.Count);
			
			array.Clear();
			Assert.AreEqual("", array.GetBlocksInfo());
			Assert.AreEqual(0, array.Count);
			for (int i = 0; i < 19; i++)
			{
				array.Add(i + "");
			}
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; 10; 11]; 12:[12; 13; 14; 15]; 16:[16; 17; 18; ()]", array.GetBlocksInfo());
			Assert.AreEqual(19, array.Count);
		}
		
		[Test]
		public void OutOfRangeChecking_Single()
		{
			Init(4);
			for (int i = 0; i < 10; i++)
			{
				array.Add(i + "");
			}
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(10, array.Count);
			
			AssertIndexOutOfRangeException("index=11 is out of [0, 10]", delegate { array.Insert(11, "a"); });		
			AssertIndexOutOfRangeException("index=-1 is out of [0, 10]", delegate { array.Insert(-1, "a"); });		
			AssertIndexOutOfRangeException("index=-1 is out of [0, 10)", delegate { string value = array[-1]; });
			AssertIndexOutOfRangeException("index=10 is out of [0, 10)", delegate { string value = array[10]; });		
			AssertIndexOutOfRangeException("index=-1 is out of [0, 10)", delegate { array[-1] = "a"; });		
			AssertIndexOutOfRangeException("index=10 is out of [0, 10)", delegate { array[10] = "a"; });
			AssertIndexOutOfRangeException("index=-1 is out of [0, 10)", delegate { array.RemoveAt(-1); });
			AssertIndexOutOfRangeException("index=10 is out of [0, 10)", delegate { array.RemoveAt(10); });
			
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(10, array.Count);
	
			
			array.Insert(10, "a");// Mast not fail
		}
		
		[Test]
		public void OutOfRangeChecking_Range()
		{
			Init(4);
			for (int i = 0; i < 10; i++)
			{
				array.Add(i + "");
			}
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(10, array.Count);
			
			AssertIndexOutOfRangeException("index=-1 is out of [0, 10]", delegate { array.InsertRange(-1, new string[] { "line" }); });
			AssertIndexOutOfRangeException("index=11 is out of [0, 10]", delegate { array.InsertRange(11, new string[] { "line" }); });
			AssertIndexOutOfRangeException("index=-1, count=1 is out of [0, 10)", delegate { array.RemoveRange(-1, 1); });
			AssertIndexOutOfRangeException("index=10, count=1 is out of [0, 10)", delegate { array.RemoveRange(10, 1); });
			AssertIndexOutOfRangeException("index=-1, count=1 is out of [0, 10)", delegate { array.RemoveRange(-1, 1); });
			AssertIndexOutOfRangeException("index=9, count=2 is out of [0, 10)", delegate { array.RemoveRange(9, 2); });
			AssertIndexOutOfRangeException("index=9, count=3 is out of [0, 10)", delegate { array.RemoveRange(9, 3); });
			AssertIndexOutOfRangeException("index=0, count=11 is out of [0, 10)", delegate { array.RemoveRange(0, 11); });
			
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(10, array.Count);
			
			array.InsertRange(10, new string[] { "line" });// Mast not fail
		}
		
		[Test]
		public void OutOfRangeChecking_Range_NoFailCases()
		{
			Init(4);
			for (int i = 0; i < 10; i++)
			{
				array.Add(i + "");
			}
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(10, array.Count);
			array.InsertRange(0, new string[] { "line" });
			CollectionAssert.AreEqual(new string[] { "line", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" }, array.ToArray());
			Assert.AreEqual(11, array.Count);
		
			Init(4);
			for (int i = 0; i < 10; i++)
			{
				array.Add(i + "");
			}
			Assert.AreEqual("0:[0; 1; 2; 3]; 4:[4; 5; 6; 7]; 8:[8; 9; (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(10, array.Count);
			array.InsertRange(10, new string[] { "line" });
		}
	}
}