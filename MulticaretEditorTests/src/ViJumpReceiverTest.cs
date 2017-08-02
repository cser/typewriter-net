using System;
using System.Collections.Generic;
using NUnit.Framework;
using MulticaretEditor;
using System.Windows.Forms;

namespace UnitTests
{
	[TestFixture]
	public class ViJumpReceiverTest : ControllerTest
	{
		private const string symbols = "012";
		
		[Test]
		public void GetKey1()
		{
			int count = 2;
			Assert.AreEqual("0", ViJumpReceiver.GetKey(symbols, 0, count));
			Assert.AreEqual("1", ViJumpReceiver.GetKey(symbols, 1, count));
		}
		
		[Test]
		public void GetKey2()
		{
			int count = 3;
			Assert.AreEqual("0", ViJumpReceiver.GetKey(symbols, 0, count));
			Assert.AreEqual("1", ViJumpReceiver.GetKey(symbols, 1, count));
			Assert.AreEqual("2", ViJumpReceiver.GetKey(symbols, 2, count));
		}
		
		[Test]
		public void GetKey3()
		{
			int count = 9;
			Assert.AreEqual("00", ViJumpReceiver.GetKey(symbols, 0, count));
			Assert.AreEqual("10", ViJumpReceiver.GetKey(symbols, 1, count));
			Assert.AreEqual("20", ViJumpReceiver.GetKey(symbols, 2, count));
			Assert.AreEqual("01", ViJumpReceiver.GetKey(symbols, 3, count));
			Assert.AreEqual("11", ViJumpReceiver.GetKey(symbols, 4, count));
			Assert.AreEqual("21", ViJumpReceiver.GetKey(symbols, 5, count));
			Assert.AreEqual("02", ViJumpReceiver.GetKey(symbols, 6, count));
			Assert.AreEqual("12", ViJumpReceiver.GetKey(symbols, 7, count));
			Assert.AreEqual("22", ViJumpReceiver.GetKey(symbols, 8, count));
		}
		
		[Test]
		public void GetKey4()
		{
			int count = 10;
			Assert.AreEqual("000", ViJumpReceiver.GetKey(symbols, 0, count));
			Assert.AreEqual("100", ViJumpReceiver.GetKey(symbols, 1, count));
			Assert.AreEqual("200", ViJumpReceiver.GetKey(symbols, 2, count));
			Assert.AreEqual("010", ViJumpReceiver.GetKey(symbols, 3, count));
			Assert.AreEqual("110", ViJumpReceiver.GetKey(symbols, 4, count));
			Assert.AreEqual("210", ViJumpReceiver.GetKey(symbols, 5, count));
			Assert.AreEqual("020", ViJumpReceiver.GetKey(symbols, 6, count));
			Assert.AreEqual("120", ViJumpReceiver.GetKey(symbols, 7, count));
			Assert.AreEqual("220", ViJumpReceiver.GetKey(symbols, 8, count));
			Assert.AreEqual("001", ViJumpReceiver.GetKey(symbols, 9, count));
		}
	}
}
