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