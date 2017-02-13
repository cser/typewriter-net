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
		public void Alphabetical()
		{
			Assert.AreEqual("", ClipboardExecuter.GetFromRegister('a'));
			
			ClipboardExecuter.PutToRegister('a', "XXX");
			ClipboardExecuter.PutToRegister('z', "YYY");
			
			Assert.AreEqual("XXX", ClipboardExecuter.GetFromRegister('a'));
			Assert.AreEqual("YYY", ClipboardExecuter.GetFromRegister('z'));
		}
		
		[Test]
		public void AlphabeticalAccumulation()
		{
			Assert.AreEqual("", ClipboardExecuter.GetFromRegister('a'));
			Assert.AreEqual("", ClipboardExecuter.GetFromRegister('A'));
			
			ClipboardExecuter.PutToRegister('a', "one");
			ClipboardExecuter.PutToRegister('A', "two");
			ClipboardExecuter.PutToRegister('A', "three");
			
			ClipboardExecuter.PutToRegister('z', "four");
			ClipboardExecuter.PutToRegister('Z', "five");
			
			Assert.AreEqual("onetwothree", ClipboardExecuter.GetFromRegister('a'));
			Assert.AreEqual("onetwothree", ClipboardExecuter.GetFromRegister('A'));
			Assert.AreEqual("fourfive", ClipboardExecuter.GetFromRegister('z'));
			Assert.AreEqual("fourfive", ClipboardExecuter.GetFromRegister('Z'));
			
			ClipboardExecuter.PutToRegister('a', "XXX");
			
			Assert.AreEqual("XXX", ClipboardExecuter.GetFromRegister('a'));
			Assert.AreEqual("XXX", ClipboardExecuter.GetFromRegister('A'));
		}
		
		[Test]
		public void AlphabeticalAccumulation_FromEmpty()
		{
			ClipboardExecuter.PutToRegister('A', "abcd");
			Assert.AreEqual("abcd", ClipboardExecuter.GetFromRegister('a'));
			Assert.AreEqual("abcd", ClipboardExecuter.GetFromRegister('A'));
		}
		
		[Test]
		public void UnusedRegisterAlwaysEmpty()
		{
			ClipboardExecuter.PutToRegister('`', "ZZZ");
			ClipboardExecuter.PutToRegister('[', "ZZZ");
			
			Assert.AreEqual("", ClipboardExecuter.GetFromRegister('`'));
			Assert.AreEqual("", ClipboardExecuter.GetFromRegister('['));
		}
		
		[TestCase('*')]
		[TestCase('-')]
		public void Clipboard(char register)
		{
			ClipboardExecuter.PutToRegister(register, "AAAA");
			Assert.AreEqual("AAAA", ClipboardExecuter.GetFromRegister(register));
			Assert.AreEqual("AAAA", ClipboardExecuter.GetFromClipboard());
			ClipboardExecuter.PutToClipboard("BBBB");
			Assert.AreEqual("BBBB", ClipboardExecuter.GetFromRegister(register));
			Assert.AreEqual("BBBB", ClipboardExecuter.GetFromClipboard());
		}
		
		[Test]
		public void LastSearch()
		{
			Assert.AreEqual("", ClipboardExecuter.GetFromRegister('/'));
			ClipboardExecuter.PutToRegister('/', "ABC");
			Assert.AreEqual("ABC", ClipboardExecuter.GetFromRegister('/'));
		}
		
		[Test]
		public void Readonly()
		{
			Assert.AreEqual("", ClipboardExecuter.GetFromRegister(':'));
			Assert.AreEqual("", ClipboardExecuter.GetFromRegister('.'));
			Assert.AreEqual("", ClipboardExecuter.GetFromRegister('%'));
			
			ClipboardExecuter.viLastCommand = "command";
			ClipboardExecuter.viLastInsertText = "last insert text";
			ClipboardExecuter.viFileName = "file name";
			
			Assert.AreEqual("command", ClipboardExecuter.GetFromRegister(':'));
			Assert.AreEqual("last insert text", ClipboardExecuter.GetFromRegister('.'));
			Assert.AreEqual("file name", ClipboardExecuter.GetFromRegister('%'));
			
			ClipboardExecuter.PutToRegister(':', "A");
			ClipboardExecuter.PutToRegister('.', "B");
			ClipboardExecuter.PutToRegister('%', "C");
			
			Assert.AreEqual("command", ClipboardExecuter.GetFromRegister(':'));
			Assert.AreEqual("last insert text", ClipboardExecuter.GetFromRegister('.'));
			Assert.AreEqual("file name", ClipboardExecuter.GetFromRegister('%'));
		}
		
		[Test]
		public void Default()
		{
			Assert.AreEqual("", ClipboardExecuter.GetFromRegister('0'));
			ClipboardExecuter.PutToRegister('0', "AAAAAAAAAA");
			Assert.AreEqual("AAAAAAAAAA", ClipboardExecuter.GetFromRegister('0'));
			
			ClipboardExecuter.PutToRegister('\0', "BBB");
			Assert.AreEqual("BBB", ClipboardExecuter.GetFromRegister('\0'));
			Assert.AreEqual("BBB", ClipboardExecuter.GetFromRegister('0'));
		}
	}
}