using System;
using System.Text;
using System.Collections.Generic;
using MulticaretEditor;
using NUnit.Framework;

namespace SnippetTest
{
	[TestFixture]
	public class StringListTest
	{
		private StringList list;
		
		[SetUp]
		public void SetUp()
		{
			list = new StringList();
		}
		
		[Test]
		public void Start()
		{
			Assert.AreEqual("a", list.Get("a", true));
		}
	}
}