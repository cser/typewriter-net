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
			ClipboardExecutor.Reset(true);
		}
		
		[Test]
		public void Alphabetical()
		{
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister('a'));
			
			ClipboardExecutor.PutToRegister('a', "XXX");
			ClipboardExecutor.PutToRegister('z', "YYY");
			
			Assert.AreEqual("XXX", ClipboardExecutor.GetFromRegister('a'));
			Assert.AreEqual("YYY", ClipboardExecutor.GetFromRegister('z'));
		}
		
		[Test]
		public void AlphabeticalAccumulation()
		{
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister('a'));
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister('A'));
			
			ClipboardExecutor.PutToRegister('a', "one");
			ClipboardExecutor.PutToRegister('A', "two");
			ClipboardExecutor.PutToRegister('A', "three");
			
			ClipboardExecutor.PutToRegister('z', "four");
			ClipboardExecutor.PutToRegister('Z', "five");
			
			Assert.AreEqual("onetwothree", ClipboardExecutor.GetFromRegister('a'));
			Assert.AreEqual("onetwothree", ClipboardExecutor.GetFromRegister('A'));
			Assert.AreEqual("fourfive", ClipboardExecutor.GetFromRegister('z'));
			Assert.AreEqual("fourfive", ClipboardExecutor.GetFromRegister('Z'));
			
			ClipboardExecutor.PutToRegister('a', "XXX");
			
			Assert.AreEqual("XXX", ClipboardExecutor.GetFromRegister('a'));
			Assert.AreEqual("XXX", ClipboardExecutor.GetFromRegister('A'));
		}
		
		[Test]
		public void AlphabeticalAccumulation_FromEmpty()
		{
			ClipboardExecutor.PutToRegister('A', "abcd");
			Assert.AreEqual("abcd", ClipboardExecutor.GetFromRegister('a'));
			Assert.AreEqual("abcd", ClipboardExecutor.GetFromRegister('A'));
		}
		
		[Test]
		public void UnusedRegisterAlwaysEmpty()
		{
			ClipboardExecutor.PutToRegister('`', "ZZZ");
			ClipboardExecutor.PutToRegister('[', "ZZZ");
			
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister('`'));
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister('['));
		}
		
		[TestCase('*')]
		[TestCase('-')]
		public void Clipboard(char register)
		{
			ClipboardExecutor.PutToRegister(register, "AAAA");
			Assert.AreEqual("AAAA", ClipboardExecutor.GetFromRegister(register));
			Assert.AreEqual("AAAA", ClipboardExecutor.GetFromClipboard());
			ClipboardExecutor.PutToClipboard("BBBB");
			Assert.AreEqual("BBBB", ClipboardExecutor.GetFromRegister(register));
			Assert.AreEqual("BBBB", ClipboardExecutor.GetFromClipboard());
		}
		
		[Test]
		public void LastSearch()
		{
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister('/'));
			ClipboardExecutor.PutToRegister('/', "ABC");
			Assert.AreEqual("ABC", ClipboardExecutor.GetFromRegister('/'));
		}
		
		[Test]
		public void Readonly()
		{
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(':'));
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister('.'));
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister('%'));
			
			ClipboardExecutor.viLastCommand = "command";
			ClipboardExecutor.viLastInsertText = "last insert text";
			ClipboardExecutor.viFileName = "file name";
			
			Assert.AreEqual("command", ClipboardExecutor.GetFromRegister(':'));
			Assert.AreEqual("last insert text", ClipboardExecutor.GetFromRegister('.'));
			Assert.AreEqual("file name", ClipboardExecutor.GetFromRegister('%'));
			
			ClipboardExecutor.PutToRegister(':', "A");
			ClipboardExecutor.PutToRegister('.', "B");
			ClipboardExecutor.PutToRegister('%', "C");
			
			Assert.AreEqual("command", ClipboardExecutor.GetFromRegister(':'));
			Assert.AreEqual("last insert text", ClipboardExecutor.GetFromRegister('.'));
			Assert.AreEqual("file name", ClipboardExecutor.GetFromRegister('%'));
		}
		
		[Test]
		public void Default()
		{
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister('0'));
			ClipboardExecutor.PutToRegister('0', "AAAAAAAAAA");
			Assert.AreEqual("AAAAAAAAAA", ClipboardExecutor.GetFromRegister('0'));
			
			ClipboardExecutor.PutToRegister('\0', "BBB");
			Assert.AreEqual("BBB", ClipboardExecutor.GetFromRegister('\0'));
			Assert.AreEqual("BBB", ClipboardExecutor.GetFromRegister('0'));
		}
	}
}