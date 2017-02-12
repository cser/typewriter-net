using System;
using NUnit.Framework;
using MulticaretEditor;
using System.Windows.Forms;

namespace UnitTests
{
	[TestFixture]
	public class ViRegistersTest
	{
		[SetUp]
		public void SetUp()
		{
			ClipboardExecuter.Reset(true);
		}
		
		[Test]
		public void AlphabeticalRegisters()
		{
			ClipboardExecuter.PutToRegister('a', "XXX");
			ClipboardExecuter.PutToRegister('z', "YYY");
			
			Assert.AreEqual("XXX", ClipboardExecuter.GetFromRegister('a'));
			Assert.AreEqual("YYY", ClipboardExecuter.GetFromRegister('z'));
		}
		
		[Test]
		public void UnusedRegisterAlwaysEmpty()
		{
			ClipboardExecuter.PutToRegister('`', "ZZZ");
			ClipboardExecuter.PutToRegister('[', "ZZZ");
			
			Assert.AreEqual("", ClipboardExecuter.GetFromRegister('`'));
			Assert.AreEqual("", ClipboardExecuter.GetFromRegister('['));
		}
	}
}