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
		
		private StringListTest SwitchPrev()
		{
			list.Switch(true);
			return this;
		}
		
		private StringListTest SwitchNext()
		{
			list.Switch(false);
			return this;
		}
		
		private StringListTest SetCurrent(string text)
		{
			list.SetCurrent(text);
			return this;
		}
		
		private StringListTest AssertCurrent(string expected, string desc)
		{
			Assert.AreEqual(expected, list.Current, "Current" + (!string.IsNullOrEmpty(desc) ? "/" + desc : ""));
			return this;
		}
		
		private StringListTest AssertCurrent(string expected)
		{
			return AssertCurrent(expected, "");
		}
		
		private StringListTest Add(string text)
		{
			list.Add(text);
			return this;
		}
		
		[Test]
		public void Start()
		{
			AssertCurrent("").SetCurrent("a").AssertCurrent("a");
			SwitchPrev().AssertCurrent("a");
			SwitchNext().AssertCurrent("a");
		}
		
		[Test]
		public void Simple()
		{
			Add("a").Add("b").Add("c").AssertCurrent("");
			SwitchPrev().AssertCurrent("c");
			SwitchPrev().AssertCurrent("b");
			SwitchPrev().AssertCurrent("a");
			SwitchNext().AssertCurrent("b");
			SwitchNext().AssertCurrent("c");
		}
		
		[Test]
		public void AddCurrentInList()
		{
			//abc[d]
			Add("a").Add("b").Add("c").SetCurrent("d").AssertCurrent("d", "#1");
			SwitchPrev().AssertCurrent("c", "#2");
			SwitchNext().AssertCurrent("d", "#3");
			SwitchPrev().AssertCurrent("c", "#4");
			SwitchPrev().AssertCurrent("b", "#5");
			SwitchPrev().AssertCurrent("a", "#6");
			SwitchPrev().AssertCurrent("a", "#7");
		}
		
		[Test]
		public void AddCurrentInList_Exists()
		{
			Add("a").Add("b").Add("c").SetCurrent("b").AssertCurrent("b", "#1");
			SwitchPrev().AssertCurrent("c", "#2");
			SwitchPrev().AssertCurrent("a", "#3");
			SwitchNext().AssertCurrent("c", "#4");
			SwitchNext().AssertCurrent("b", "#5");
		}
		
		[Test]
		public void AddCurrentInList_Empty()
		{
			Add("a").Add("b").Add("c").SetCurrent("").AssertCurrent("", "#1");
			SwitchPrev().AssertCurrent("c", "#2");
			SwitchPrev().AssertCurrent("b", "#4");
			SwitchPrev().AssertCurrent("a", "#5");
			SwitchNext().AssertCurrent("b", "#6");
			SwitchNext().AssertCurrent("c", "#7");
			SwitchNext().AssertCurrent("", "#8");
		}
		
		[Test]
		public void AddCurrentInList_EmptyRemoving()
		{
			Add("a").Add("b").Add("c").SetCurrent("").AssertCurrent("", "#1");
			SwitchPrev().AssertCurrent("c", "#2").SetCurrent("d");
			SwitchPrev().AssertCurrent("c", "#4");
			SwitchNext().AssertCurrent("d", "#5");
			SwitchNext().AssertCurrent("d", "#6");
		}
		
		[Test]
		public void Overflow()
		{
			list.MaxCount = 3;
			Add("a").Add("b").Add("c").Add("d").AssertCurrent("", "#1");
			SwitchPrev().AssertCurrent("d", "#2");
			SwitchPrev().AssertCurrent("c", "#3");
			SwitchPrev().AssertCurrent("b", "#4");
			SwitchPrev().AssertCurrent("b", "#5");
		}
		
		[Test]
		public void Simple0()
		{
			Add("a").Add("b").Add("c").AssertCurrent("").SetCurrent("");
			SwitchPrev().AssertCurrent("c").SetCurrent("c");
			SwitchNext().AssertCurrent("");
		}
		
		[Test]
		public void Simple1()
		{
			Add("a").Add("b").Add("c").AssertCurrent("").SetCurrent("");
			SwitchPrev().AssertCurrent("c").SetCurrent("c");
			SwitchNext().AssertCurrent("").SetCurrent("");
			SwitchPrev().AssertCurrent("c").SetCurrent("c");
			SwitchPrev().AssertCurrent("b").SetCurrent("b");
			SwitchPrev().AssertCurrent("a").SetCurrent("a");
			SwitchPrev().AssertCurrent("a").SetCurrent("a");
			SwitchNext().AssertCurrent("b").SetCurrent("b");
			SwitchNext().AssertCurrent("c").SetCurrent("c");
			SwitchNext().AssertCurrent("").SetCurrent("");
			SwitchNext().AssertCurrent("");
		}
		
		[Test]
		public void Simple2()
		{
			Add("a").Add("$").Add("c").AssertCurrent("").SetCurrent("$");
			SwitchPrev().AssertCurrent("c").SetCurrent("c");
			SwitchNext().AssertCurrent("$").SetCurrent("$");
			SwitchPrev().AssertCurrent("c").SetCurrent("c");
			SwitchPrev().AssertCurrent("a").SetCurrent("a");
			SwitchPrev().AssertCurrent("a").SetCurrent("a");
			SwitchNext().AssertCurrent("c").SetCurrent("c");
			SwitchNext().AssertCurrent("$").SetCurrent("$");
			SwitchNext().AssertCurrent("$").SetCurrent("$");
			SwitchPrev().AssertCurrent("c");
		}
	}
}