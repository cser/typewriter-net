using System;
using System.Text;
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
			
			public List<Snippet.Part> TestParseText(string text)
			{
				return base.ParseText(text);
			}
			
			public string TestParseEntry(string text, int i,
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
		
		private string StringOf(SnippetRange range)
		{
			return "-" + range.order + ":" + range.index + "/" + range.count + ":" + range.defaultValue;
		}
		
		private void AssertRanges(string expected, List<SnippetRange> ranges)
		{
			StringBuilder builder = new StringBuilder();
			foreach (SnippetRange range in ranges)
			{
				builder.Append(StringOf(range) + "\n");
				builder.Append("  next:\n");
				for (SnippetRange subrange = range.next; subrange != null; subrange = subrange.next)
				{
					builder.Append(StringOf(subrange) + "\n");
				}
				builder.Append("  subrange:\n");
				for (SnippetRange subrange = range.subrange; subrange != null; subrange = subrange.next)
				{
					builder.Append(StringOf(subrange) + "\n");
				}
				builder.Append("  nested:\n");
				for (SnippetRange subrange = range.nested; subrange != null; subrange = subrange.next)
				{
					builder.Append(StringOf(subrange) + "\n");
				}
			}
			Assert.AreEqual(expected, builder.ToString());
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
				ListUtil.ToString(snippet.TestParseText("text text ${1:pattern} text"), StringOf));
		}
		
		[Test]
		public void Test1()
		{
			Assert.AreEqual(
				"[(TEXT'text text '), (1:ENTRY'pattern'), (2:ENTRY'pattern2'), (TEXT' text')]",
				ListUtil.ToString(snippet.TestParseText("text text ${1:pattern}${2:pattern2} text"), StringOf));
		}
		
		[Test]
		public void Test2()
		{
			Assert.AreEqual(
				"[(TEXT'text text '), (1:ENTRY'p1'), (2:ENTRY'p2${3:p3}${4:p4}'), (TEXT' text')]",
				ListUtil.ToString(snippet.TestParseText("text text ${1:p1}${2:p2${3:p3}${4:p4}} text"), StringOf));
		}
		
		[Test]
		public void Test3()
		{
			Assert.AreEqual(
				"[(TEXT'for('), (1:ENTRY'i'), (2:ENTRY',${4:len}=${5:item}.length'), (TEXT';'), (1:ENTRY_S''), (TEXT'<'), (3:ENTRY'count'), (TEXT';'), (1:ENTRY_S''), (TEXT'++){'), (0:ENTRY''), (TEXT'}')]",
				ListUtil.ToString(snippet.TestParseText("for(${1:i}${2:,${4:len}=${5:item}.length};$1<${3:count};$1++){${0}}"), StringOf));
		}
		
		[Test]
		public void ParseEntry()
		{
			string order;
			string defaultValue;
			bool secondary;
			Assert.AreEqual(
				"${1:pattern}",
				snippet.TestParseEntry("text text ${1:pattern} text", 10, out order, out defaultValue, out secondary));
		}
		
		[Test]
		public void ParseEntry_Nested()
		{
			string order;
			string defaultValue;
			bool secondary;
			Assert.AreEqual(
				"${1:when ${2:pattern} : T}",
				snippet.TestParseEntry("text text ${1:when ${2:pattern} : T} text", 10, out order, out defaultValue, out secondary));
			Assert.AreEqual("when ${2:pattern} : T", defaultValue);
		}
		
		[Test]
		public void Ranges()
		{
			Snippet snippet = new Snippet("for(${1:i}${2:,${4:len}=${5:item}.length};$1<${3:count};$1++){${0}}",
				new Settings(null), null);
			AssertRanges(
				"-1:5,1:i\n" +
				"-2:6,1:,len=item\n" +
				"  -4:1,2:len\n" +
				"  -5:1,3:item\n" +
				"-3:1,1:count\n" +
				"-0:1,4:", snippet.ranges);
		}
	}
}