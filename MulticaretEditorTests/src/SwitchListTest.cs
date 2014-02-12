using System;
using MulticaretEditor;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class SwitchListTest
	{
		private SwitchList<string> list;
		private string log;
		
		private void AssertLog(string expected)
		{
			Assert.AreEqual(expected, log);
			log = "";
		}
		
		[SetUp]
		public void SetUp()
		{
			list = new SwitchList<string>();
			log = "";
		}
		
		[Test]
		public void Test0()
		{
			list.Add("1").Add("2").Add("3").Add("4");
			
			list.Selected = "4";
			list.Selected = "3";
			list.Selected = "2";
			list.Selected = "1";
			Assert.AreEqual("1", list.Selected);			
			list.ModeOn().Down().Down().ModeOff();
			Assert.AreEqual("3", list.Selected);
			list.ModeOn().Down().ModeOff();
			Assert.AreEqual("1", list.Selected);
			list.ModeOn().Down().Down().ModeOff();
			Assert.AreEqual("2", list.Selected);
		}
		
		[Test]
		public void Test1()
		{
			list.Add("1").Add("2").Add("3").Add("4");
			Assert.AreEqual("4", list.Selected);
			list.ModeOn().Down().ModeOff();
			Assert.AreEqual("3", list.Selected);
			list.ModeOn().Down().ModeOff();
			Assert.AreEqual("4", list.Selected);
			list.ModeOn().Down().Down().Down().ModeOff();
			Assert.AreEqual("1", list.Selected);
			list.ModeOn().Down().Down().Down().Down().Down().Down().ModeOff();
			Assert.AreEqual("3", list.Selected);
			list.ModeOn().Down().Down().ModeOff();
			Assert.AreEqual("4", list.Selected);
			list.ModeOn().Down().Down().Down().Down().Down().Down().ModeOff();
			Assert.AreEqual("1", list.Selected);
		}
		
		[Test]
		public void Test2()
		{
			list.Add("1").Add("2").Add("3").Add("4");
			Assert.AreEqual("4", list.Selected);
			list.Remove("4");
			Assert.AreEqual("3", list.Selected);
			list.Add("4");
			Assert.AreEqual("4", list.Selected);
			list.Selected = "2";
			list.Remove(list.Selected);
			Assert.AreEqual("3", list.Selected);
			list.ModeOn().Down().ModeOff();
			Assert.AreEqual("4", list.Selected);
		}
		
		[Test]
		public void Test3()
		{
			list.Add("1").Add("2").Add("3").Add("4");
			Assert.AreEqual("4", list.Selected);
			list.Remove("4");
			list.Remove("3");
			list.Remove("2");
			list.Remove("1");
			Assert.AreEqual(null, list.Selected);
		}
		
		[Test]
		public void Test4()
		{
			list.Add("1").Add("2").Add("3").Add("4");
			
			list.Selected = "4";
			list.Selected = "3";
			list.Selected = "2";
			list.Selected = "1";
			Assert.AreEqual("1", list.Selected);
			list.ModeOn().ModeOff();
			Assert.AreEqual("1", list.Selected);
			list.ModeOn().Down().Down().ModeOff();
			Assert.AreEqual("3", list.Selected);
			list.ModeOn().ModeOff();
			Assert.AreEqual("3", list.Selected);
			list.ModeOn().Down().ModeOff();
			Assert.AreEqual("1", list.Selected);
			list.ModeOn().ModeOff();
			Assert.AreEqual("1", list.Selected);
			list.ModeOn().Down().Down().ModeOff();
			Assert.AreEqual("2", list.Selected);
		}
		
		[Test]
		public void Constraints()
		{
			list.Add("1").Add("2").Add("3").Add("4");
			list.Add("3");
			Assert.AreEqual("3", list.Selected);
			CollectionAssert.AreEqual(new string[] { "1", "2", "3", "4" }, list);
			
			list.Remove("5");
			list.Selected = "6";
			Assert.AreEqual("3", list.Selected);
		}
		
		private void OnSelectedChange()
		{
			log += "changed;";
		}
		
		[Test]
		public void SelectedChangeDispatching()
		{
			list.SelectedChange += OnSelectedChange;
			Assert.AreEqual(null, list.Selected);
			AssertLog("");
			
			list.Add("1");
			Assert.AreEqual("1", list.Selected);
			AssertLog("changed;");
			
			list.Add("2").Add("3").Add("4");
			log = "";
			list.Add("3");
			Assert.AreEqual("3", list.Selected);
			AssertLog("changed;");
			
			list.Add("3");
			Assert.AreEqual("3", list.Selected);
			AssertLog("");
			
			CollectionAssert.AreEqual(new string[] { "1", "2", "3", "4" }, list);
			list.Remove("5");
			AssertLog("");
			
			list.Selected = "6";
			Assert.AreEqual("3", list.Selected);
			AssertLog("");
			
			list.Remove("3");
			Assert.AreEqual("4", list.Selected);
			AssertLog("changed;");
			
			list.Remove("4");
			Assert.AreEqual("2", list.Selected);
			AssertLog("changed;");
			
			list.Remove("2");
			Assert.AreEqual("1", list.Selected);
			AssertLog("changed;");
			
			list.Remove("1");
			Assert.AreEqual(null, list.Selected);
			AssertLog("changed;");
		}
		
		[Test]
		public void Oldest()
		{
			list.SelectedChange += OnSelectedChange;
			Assert.AreEqual(null, list.Selected);
			Assert.AreEqual(null, list.Oldest);
			
			list.Add("1");
			Assert.AreEqual("1", list.Selected);
			Assert.AreEqual("1", list.Oldest);
			
			list.Add("2").Add("3").Add("4");
			Assert.AreEqual("4", list.Selected);
			Assert.AreEqual("1", list.Oldest);
			
			list.Remove("1");
			Assert.AreEqual("2", list.Oldest);
		}
	}
}
