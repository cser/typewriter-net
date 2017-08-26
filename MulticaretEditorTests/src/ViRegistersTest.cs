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
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(null, 'a'));
			
			ClipboardExecutor.PutToRegister('a', "XXX");
			ClipboardExecutor.PutToRegister('z', "YYY");
			
			Assert.AreEqual("XXX", ClipboardExecutor.GetFromRegister(null, 'a'));
			Assert.AreEqual("YYY", ClipboardExecutor.GetFromRegister(null, 'z'));
		}
		
		[Test]
		public void AlphabeticalAccumulation()
		{
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(null, 'a'));
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(null, 'A'));
			
			ClipboardExecutor.PutToRegister('a', "one");
			ClipboardExecutor.PutToRegister('A', "two");
			ClipboardExecutor.PutToRegister('A', "three");
			
			ClipboardExecutor.PutToRegister('z', "four");
			ClipboardExecutor.PutToRegister('Z', "five");
			
			Assert.AreEqual("onetwothree", ClipboardExecutor.GetFromRegister(null, 'a'));
			Assert.AreEqual("onetwothree", ClipboardExecutor.GetFromRegister(null, 'A'));
			Assert.AreEqual("fourfive", ClipboardExecutor.GetFromRegister(null, 'z'));
			Assert.AreEqual("fourfive", ClipboardExecutor.GetFromRegister(null, 'Z'));
			
			ClipboardExecutor.PutToRegister('a', "XXX");
			
			Assert.AreEqual("XXX", ClipboardExecutor.GetFromRegister(null, 'a'));
			Assert.AreEqual("XXX", ClipboardExecutor.GetFromRegister(null, 'A'));
		}
		
		[Test]
		public void AlphabeticalAccumulation_FromEmpty()
		{
			ClipboardExecutor.PutToRegister('A', "abcd");
			Assert.AreEqual("abcd", ClipboardExecutor.GetFromRegister(null, 'a'));
			Assert.AreEqual("abcd", ClipboardExecutor.GetFromRegister(null, 'A'));
		}
		
		[Test]
		public void UnusedRegisterAlwaysEmpty()
		{
			ClipboardExecutor.PutToRegister('`', "ZZZ");
			ClipboardExecutor.PutToRegister('[', "ZZZ");
			
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(null, '`'));
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(null, '['));
		}
		
		[TestCase('*')]
		[TestCase('-')]
		public void Clipboard(char register)
		{
			ClipboardExecutor.PutToRegister(register, "AAAA");
			Assert.AreEqual("AAAA", ClipboardExecutor.GetFromRegister(null, register));
			Assert.AreEqual("AAAA", ClipboardExecutor.GetFromClipboard());
			ClipboardExecutor.PutToClipboard("BBBB");
			Assert.AreEqual("BBBB", ClipboardExecutor.GetFromRegister(null, register));
			Assert.AreEqual("BBBB", ClipboardExecutor.GetFromClipboard());
		}
		
		[Test]
		public void LastSearch()
		{
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(null, '/'));
			ClipboardExecutor.PutToSearch(new Pattern("pattern", false, false));
			Assert.AreEqual("pattern", ClipboardExecutor.GetFromRegister(null, '/'));
		}
		
		[Test]
		public void LastSearch_Readonly()
		{
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(null, '/'));
			ClipboardExecutor.PutToRegister('/', "ABC");
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(null, '/'));
		}
		
		[Test]
		public void Readonly()
		{
			LineArray lines = new LineArray();
			
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(lines, ':'));
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(lines, '.'));
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(lines, '%'));
			
			ClipboardExecutor.viLastCommand = "command";
			lines.viFullPath = "file name";
			
			Assert.AreEqual("command", ClipboardExecutor.GetFromRegister(lines, ':'));
			Assert.AreEqual("file name", ClipboardExecutor.GetFromRegister(lines, '%'));
			
			ClipboardExecutor.PutToRegister(':', "A");
			ClipboardExecutor.PutToRegister('.', "B");
			ClipboardExecutor.PutToRegister('%', "C");
			
			Assert.AreEqual("command", ClipboardExecutor.GetFromRegister(lines, ':'));
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(lines, '.'));
			Assert.AreEqual("file name", ClipboardExecutor.GetFromRegister(lines, '%'));
		}
		
		[Test]
		public void Default()
		{
			Assert.AreEqual("", ClipboardExecutor.GetFromRegister(null, '0'));
			ClipboardExecutor.PutToRegister('0', "AAAAAAAAAA");
			Assert.AreEqual("AAAAAAAAAA", ClipboardExecutor.GetFromRegister(null, '0'));
			
			ClipboardExecutor.PutToRegister('\0', "BBB");
			Assert.AreEqual("BBB", ClipboardExecutor.GetFromRegister(null, '\0'));
			Assert.AreEqual("BBB", ClipboardExecutor.GetFromRegister(null, '0'));
		}
	}
}