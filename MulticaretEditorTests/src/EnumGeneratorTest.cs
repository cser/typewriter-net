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
			CollectionAssert.AreEqual(new string[] { "1", "2", "3" }, new EnumGenerator("1", 3).texts);
		}
		
		[Test]
		public void SimpleStep()
		{
			CollectionAssert.AreEqual(new string[] { "40", "50", "60" }, new EnumGenerator("40 10", 3).texts);
		}
	}
}