using System;
using System.IO;
using MulticaretEditor;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class EnumGeneratorTest
	{
		[Test]
		public void Simple()
		{
			CollectionAssert.AreEqual(new string[] { "2", "3", "4" }, new EnumGenerator("2", 3).texts);
		}
		
		[Test]
		public void NoParameters()
		{
			CollectionAssert.AreEqual(new string[] { "1", "2", "3" }, new EnumGenerator("", 3).texts);
		}
		
		[Test]
		public void SimpleStep()
		{
			CollectionAssert.AreEqual(new string[] { "40", "50", "60" }, new EnumGenerator("40 10", 3).texts);
		}
		
		[Test]
		public void SimpleStepBack()
		{
			CollectionAssert.AreEqual(new string[] { "40", "30", "20" }, new EnumGenerator("40 -10", 3).texts);
		}
		
		[Test]
		public void Repeat()
		{
			CollectionAssert.AreEqual(
				new string[] { "20 21 22 23", "24 25 26 27", "28 29 30 31" },
				new EnumGenerator("20 1 4", 3).texts);
		}
	}
}
/*
TODO
- Errors
*/