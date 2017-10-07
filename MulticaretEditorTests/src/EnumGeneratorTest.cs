using System;
using System.IO;
using MulticaretEditor;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class EnumGeneratorTest
	{
		private void AssertNumber(string[] expected, string args, int count)
		{
			CollectionAssert.AreEqual(
				expected, new EnumGenerator(args, count, EnumGenerator.Mode.Number).texts);
		}
		
		private void AssertZeroBeforeNumber(string[] expected, string args, int count)
		{
			CollectionAssert.AreEqual(
				expected, new EnumGenerator(args, count, EnumGenerator.Mode.ZeroBeforeNumber).texts);
		}
		
		[Test]
		public void Simple()
		{
			AssertNumber(new string[] { "2", "3", "4" }, "2", 3);
		}
		
		[Test]
		public void Simple_Decrease()
		{
			AssertNumber(new string[] { "-1", "0", "1" }, "-1", 3);
		}
		
		[Test]
		public void NoParameters()
		{
			AssertNumber(new string[] { "1", "2", "3" }, "", 3);
		}
		
		[Test]
		public void SimpleStep()
		{
			AssertNumber(new string[] { "40", "50", "60" }, "40 10", 3);
		}
		
		[Test]
		public void SimpleStepBack()
		{
			AssertNumber(new string[] { "40", "30", "20" }, "40 -10", 3);
		}
		
		[Test]
		public void Repeat()
		{
			AssertNumber(new string[] { "20 21 22 23", "24 25 26 27", "28 29 30 31" }, "20 1 4", 3);
		}
		
		[Test]
		public void ZerosBefore_NoZeros()
		{
			AssertNumber(
				new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" }, "", 11);
		}
		
		[Test]
		public void ZerosBefore_Zeros()
		{
			AssertZeroBeforeNumber(
				new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11" }, "", 11);
		}
		
		[Test]
		public void ZerosBefore_Zeros2()
		{
			AssertZeroBeforeNumber(new string[] { "98", "99" }, "98", 2);
			AssertZeroBeforeNumber(new string[] { "098", "099", "100" }, "98", 3);
		}
		
		[Test]
		public void ZerosBefore_Zeros3()
		{
			AssertZeroBeforeNumber(new string[] { "-2", "-1", " 0", " 1" }, "-2", 4);
			AssertZeroBeforeNumber(new string[] { "-02", " 08", " 18" }, "-2 10", 3);
		}
		
		[Test]
		public void SimpleChars()
		{
			AssertZeroBeforeNumber(new string[] { "a", "b", "c" }, "a", 3);
			AssertZeroBeforeNumber(new string[] { "a", "c", "e" }, "a 2", 3);
			AssertZeroBeforeNumber(new string[] { "a c e", "g i k", "m o q" }, "a 2 3", 3);
		}
	}
}
/*
TODO
- Errors
*/