using System;
using System.Collections.Generic;
using MulticaretEditor;
using NUnit.Framework;

namespace SnippetTest
{
	[TestFixture]
	public class SwitchListTest
	{
		public class ExtendedSnippet : Snippet
		{
			public ExtendedSnippet() : base()
			{
			}
			
			public List<Snippet.Part> ParseText(string text)
			{
				return base.ParseText(text);
			}
			
			public string ParseEntry(string text, int i,
				out string order, out string defaultValue, out bool secondary)
			{
				return base.ParseEntry(text, i, out order, out defaultValue, out secondary);
			}
		}
		
		private string StringOf(Snippet.Part part)
		{
			string text = "(";
			if (part.entry_order != null)
			{
				text += part.entry_order + ":";
			}
			if (part.entry_value != null)
			{
				text += "ENTRY" + (part.entry_secondary ? "_S" : "") + "'" + part.entry_value + "'";
			}
			if (part.text_value != null)
			{
				text += "TEXT'" + part.text_value + "'";
			}
			return text + ")";
		}
		
		private ExtendedSnippet snippet;
		
		[SetUp]
		public void SetUp()
		{
			snippet = new ExtendedSnippet();
		}
		
		[Test]
		public void Test0()
		{
			Assert.AreEqual(
				"[(TEXT'text text '), (1:ENTRY'pattern'), (TEXT' text')]",
				ListUtil.ToString(snippet.ParseText("text text ${1:pattern} text"), StringOf));
		}
		
		[Test]
		public void Test1()
		{
			Assert.AreEqual(
				"[(TEXT'text text '), (1:ENTRY'pattern'), (2:ENTRY'pattern2'), (TEXT' text')]",
				ListUtil.ToString(snippet.ParseText("text text ${1:pattern}${2:pattern2} text"), StringOf));
		}
		
		[Test]
		public void Test2()
		{
			Assert.AreEqual(
				"[(TEXT'text text '), (1:ENTRY'p1'), (2:ENTRY'p2${3:p3}${4:p4}'), (TEXT' text')]",
				ListUtil.ToString(snippet.ParseText("text text ${1:p1}${2:p2${3:p3}${4:p4}} text"), StringOf));
		}
		
		[Test]
		public void Test3()
		{
			Assert.AreEqual(
				"[(TEXT'for('), (1:ENTRY'i'), (2:ENTRY',${4:len}=${5:item}.length'), (TEXT';'), (1:ENTRY_S''), (TEXT'<'), (3:ENTRY'count'), (TEXT';'), (1:ENTRY_S''), (TEXT'++){'), (0:ENTRY''), (TEXT'}')]",
				ListUtil.ToString(snippet.ParseText("for(${1:i}${2:,${4:len}=${5:item}.length};$1<${3:count};$1++){${0}}"), StringOf));
		}
		
		[Test]
		public void ParseEntry()
		{
			string order;
			string defaultValue;
			bool secondary;
			Assert.AreEqual(
				"${1:pattern}",
				snippet.ParseEntry("text text ${1:pattern} text", 10, out order, out defaultValue, out secondary));
		}
		
		[Test]
		public void ParseEntry_Nested()
		{
			string order;
			string defaultValue;
			bool secondary;
			Assert.AreEqual(
				"${1:when ${2:pattern} : T}",
				snippet.ParseEntry("text text ${1:when ${2:pattern} : T} text", 10, out order, out defaultValue, out secondary));
			Assert.AreEqual("when ${2:pattern} : T", defaultValue);
		}
	}
}