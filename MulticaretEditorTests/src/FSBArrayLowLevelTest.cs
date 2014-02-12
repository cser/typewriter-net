using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class FSBArrayLowLevelTest : FSBArrayTestBase
	{
		public FSBArrayLowLevelTest()
		{
		}
		
		[Test]
		public void InsertValuesRange0()
		{
			Init(4);
			array.SetBlocks(Strings("0", "1", "2"), Strings("3", "4"), Strings("5"), Strings("6", "7", "8", "9"), Strings("10"));
			Assert.AreEqual("0:[0; 1; 2; ()]; 3:[3; 4; (); ()]; 5:[5; (); (); ()]; 6:[6; 7; 8; 9]; 10:[10; (); (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(11, array.Count);
			
			array.InsertRange(3, new string[] { "a", "b", "c" });
			Assert.AreEqual("0:[0; 1; 2; a]; 4:[b; c; 3; 4]; 8:[5; (); (); ()]; 9:[6; 7; 8; 9]; 13:[10; (); (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(14, array.Count);
			
			array.InsertRange(9, new string[] { "d", "e", "f" });
			Assert.AreEqual("0:[0; 1; 2; a]; 4:[b; c; 3; 4]; 8:[5; d; e; f]; 12:[6; 7; 8; 9]; 16:[10; (); (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(17, array.Count);
		}
		
		[Test]
		public void InsertValuesRange1()
		{
			Init(4);
			Assert.AreEqual("", array.GetBlocksInfo());
			
			array.InsertRange(0, new string[] { "a", "b", "c" });
			Assert.AreEqual("0:[a; b; c; ()]", array.GetBlocksInfo());
			Assert.AreEqual(3, array.Count);
			
			array.InsertRange(2, new string[] { "d", "e", "f" });
			Assert.AreEqual("0:[a; b; d; e]; 4:[f; c; (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(6, array.Count);
			
			array.InsertRange(3, new string[] { "g", "h", "i", "j", "k" });
			Assert.AreEqual("0:[a; b; d; g]; 4:[h; i; j; k]; 8:[e; (); (); ()]; 9:[f; c; (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(11, array.Count);
			
			array.InsertRange(3, new string[] { });
			Assert.AreEqual("0:[a; b; d; g]; 4:[h; i; j; k]; 8:[e; (); (); ()]; 9:[f; c; (); ()]", array.GetBlocksInfo());
			Assert.AreEqual(11, array.Count);
		}
		
		[Test]
		public void InsertValuesRange2()
		{
			Init(6);
			Assert.AreEqual("", array.GetBlocksInfo());
			
			array.SetBlocks(new string[] { "0", "1" }, new string[] { "2", "3", "4" }, new string[] { "5" }, new string[] { "6", "7", "8", "9", "10", "11" });
			Assert.AreEqual("0:[0; 1; (); (); (); ()]; 2:[2; 3; 4; (); (); ()]; 5:[5; (); (); (); (); ()]; 6:[6; 7; 8; 9; 10; 11]", array.GetBlocksInfo());
			Assert.AreEqual(12, array.Count);
			
			array.InsertRange(6, new string[] { "a", "b" });
			Assert.AreEqual("0:[0; 1; (); (); (); ()]; 2:[2; 3; 4; (); (); ()]; 5:[5; a; b; (); (); ()]; 8:[6; 7; 8; 9; 10; 11]", array.GetBlocksInfo());
			Assert.AreEqual(14, array.Count);
			
			array.InsertRange(8, new string[] { "c", "d", "e", "f" });
			Assert.AreEqual(
				"0:[0; 1; (); (); (); ()]; 2:[2; 3; 4; (); (); ()]; 5:[5; a; b; c; d; e]; 11:[f; 6; 7; 8; 9; 10]; 17:[11; (); (); (); (); ()]",
				array.GetBlocksInfo());
			Assert.AreEqual(18, array.Count);
		}
		
		[Test]
		public void InsertValuesRange3()
		{
			Init(6);
			Assert.AreEqual("", array.GetBlocksInfo());
			
			array.SetBlocks(new string[] { "0", "1" }, new string[] { "2", "3" }, new string[] { "4", "5", "6", "7", "8", "9" });
			Assert.AreEqual("0:[0; 1; (); (); (); ()]; 2:[2; 3; (); (); (); ()]; 4:[4; 5; 6; 7; 8; 9]", array.GetBlocksInfo());
			Assert.AreEqual(10, array.Count);
			
			array.InsertRange(2, new string[] { "a", "b", "c", "d", "e" });
			Assert.AreEqual("0:[0; 1; a; b; c; d]; 6:[e; 2; 3; (); (); ()]; 9:[4; 5; 6; 7; 8; 9]", array.GetBlocksInfo());
			Assert.AreEqual(15, array.Count);
			
			Init(6);
			Assert.AreEqual("", array.GetBlocksInfo());
			
			array.SetBlocks(new string[] { "0", "1" }, new string[] { "2", "3" }, new string[] { "4", "5", "6", "7", "8", "9" });
			Assert.AreEqual("0:[0; 1; (); (); (); ()]; 2:[2; 3; (); (); (); ()]; 4:[4; 5; 6; 7; 8; 9]", array.GetBlocksInfo());
			Assert.AreEqual(10, array.Count);
			
			array.InsertRange(3, new string[] { "a" });
			Assert.AreEqual("0:[0; 1; (); (); (); ()]; 2:[2; a; 3; (); (); ()]; 5:[4; 5; 6; 7; 8; 9]", array.GetBlocksInfo());
			Assert.AreEqual(11, array.Count);
		}
	}
}
