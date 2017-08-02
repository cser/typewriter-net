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
		public void GetKey()
		{
			int count = 3;
			Assert.AreEqual("0", ViJumpReceiver.GetKey(symbols, 0, 3));
			Assert.AreEqual("1", ViJumpReceiver.GetKey(symbols, 1, 3));
			Assert.AreEqual("2", ViJumpReceiver.GetKey(symbols, 2, 3));
		}
	}
}
