using System;
using System.Collections.Generic;
using NUnit.Framework;
using MulticaretEditor;
using System.Windows.Forms;

namespace UnitTests
{
	[TestFixture]
	public class ViCommandParserTest
	{
		private ViCommandParser parser;
		
		[SetUp]
		public void SetUp()
		{
			parser = new ViCommandParser();
		}
		
		private bool AddKey(char c)
		{
			return parser.AddKey(new ViChar(c, false));
		}
		
		private bool AddKey(char c, bool control)
		{
			return parser.AddKey(new ViChar(c, control));
		}
		
		private void AssertParsed(string expected)
		{
			Assert.AreEqual(expected,
				parser.count + ":action:" + parser.action + ";move:" + parser.move + ";moveChar:" + parser.moveChar);
		}
		
		[Test]
		public void Move_hjkl()
		{
			Assert.AreEqual(true, AddKey('h'));
			AssertParsed("1:action:\\0;move:h;moveChar:\\0");
			
			Assert.AreEqual(true, AddKey('j'));
			AssertParsed("1:action:\\0;move:j;moveChar:\\0");
			
			Assert.AreEqual(true, AddKey('k'));
			AssertParsed("1:action:\\0;move:k;moveChar:\\0");
			
			Assert.AreEqual(true, AddKey('l'));
			AssertParsed("1:action:\\0;move:l;moveChar:\\0");
		}
		
		[Test]
		public void Move_repeat_hj()
		{
			Assert.AreEqual(false, AddKey('2'));
			Assert.AreEqual(true, AddKey('h'));
			AssertParsed("2:action:\\0;move:h;moveChar:\\0");
			
			Assert.AreEqual(false, AddKey('5'));
			Assert.AreEqual(false, AddKey('0'));
			Assert.AreEqual(true, AddKey('j'));
			AssertParsed("50:action:\\0;move:j;moveChar:\\0");
			
			Assert.AreEqual(false, AddKey('2'));
			Assert.AreEqual(false, AddKey('3'));
			Assert.AreEqual(false, AddKey('5'));
			Assert.AreEqual(true, AddKey('j'));
			AssertParsed("235:action:\\0;move:j;moveChar:\\0");
		}
		
		[Test]
		public void Repeat_delete_hjkl()
		{
			Assert.AreEqual(false, AddKey('d'));
			Assert.AreEqual(true, AddKey('h'));
			AssertParsed("1:action:d;move:h;moveChar:\\0");
			
			Assert.AreEqual(false, AddKey('7'));
			Assert.AreEqual(false, AddKey('d'));
			Assert.AreEqual(true, AddKey('j'));
			AssertParsed("7:action:d;move:j;moveChar:\\0");
			
			Assert.AreEqual(false, AddKey('3'));
			Assert.AreEqual(false, AddKey('5'));
			Assert.AreEqual(false, AddKey('d'));
			Assert.AreEqual(true, AddKey('k'));
			AssertParsed("35:action:d;move:k;moveChar:\\0");
			
			Assert.AreEqual(false, AddKey('d'));
			Assert.AreEqual(true, AddKey('l'));
			AssertParsed("1:action:d;move:l;moveChar:\\0");
		}
		
		[Test]
		public void Repeat_delete_h_inversed()
		{
			Assert.AreEqual(false, AddKey('d'));
			Assert.AreEqual(true, AddKey('h'));
			AssertParsed("1:action:d;move:h;moveChar:\\0");
			
			Assert.AreEqual(false, AddKey('d'));
			Assert.AreEqual(false, AddKey('7'));
			Assert.AreEqual(true, AddKey('h'));
			AssertParsed("7:action:d;move:h;moveChar:\\0");
			
			Assert.AreEqual(false, AddKey('3'));
			Assert.AreEqual(false, AddKey('d'));
			Assert.AreEqual(false, AddKey('5'));
			Assert.AreEqual(true, AddKey('h'));
			AssertParsed("5:action:d;move:h;moveChar:\\0");
		}
		
		[Test]
		public void UndoRedo()
		{
			Assert.AreEqual(true, AddKey('u'));
			AssertParsed("1:action:u;move:\\0;moveChar:\\0");
			
			Assert.AreEqual(false, AddKey('7'));
			Assert.AreEqual(true, AddKey('u'));
			AssertParsed("7:action:u;move:\\0;moveChar:\\0");
			
			Assert.AreEqual(true, AddKey('r', true));
			AssertParsed("1:action:<C-r>;move:\\0;moveChar:\\0");
			
			Assert.AreEqual(false, AddKey('2'));
			Assert.AreEqual(false, AddKey('0'));
			Assert.AreEqual(true, AddKey('r', true));
			AssertParsed("20:action:<C-r>;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void Move_toChar()
		{
			Assert.AreEqual(false, AddKey('f'));
			Assert.AreEqual(true, AddKey('a'));
			AssertParsed("1:action:\\0;move:f;moveChar:a");
			
			Assert.AreEqual(false, AddKey('f'));
			Assert.AreEqual(true, AddKey('2'));
			AssertParsed("1:action:\\0;move:f;moveChar:2");
			
			Assert.AreEqual(false, AddKey('f'));
			Assert.AreEqual(true, AddKey('0'));
			AssertParsed("1:action:\\0;move:f;moveChar:0");
		}
		
		[Test]
		public void Repeat_delete_toChar()
		{
			Assert.AreEqual(false, AddKey('2'));
			Assert.AreEqual(false, AddKey('f'));
			Assert.AreEqual(true, AddKey('a'));
			AssertParsed("2:action:\\0;move:f;moveChar:a");
			
			Assert.AreEqual(false, AddKey('3'));
			Assert.AreEqual(false, AddKey('4'));
			Assert.AreEqual(false, AddKey('f'));
			Assert.AreEqual(true, AddKey('a'));
			AssertParsed("34:action:\\0;move:f;moveChar:a");
			
			Assert.AreEqual(false, AddKey('4'));
			Assert.AreEqual(false, AddKey('9'));
			Assert.AreEqual(false, AddKey('f'));
			Assert.AreEqual(true, AddKey('2'));
			AssertParsed("49:action:\\0;move:f;moveChar:2");
			
			Assert.AreEqual(false, AddKey('5'));
			Assert.AreEqual(false, AddKey('1'));
			Assert.AreEqual(false, AddKey('d'));
			Assert.AreEqual(false, AddKey('f'));
			Assert.AreEqual(true, AddKey('2'));
			AssertParsed("51:action:d;move:f;moveChar:2");
		}
		
		[Test]
		public void Repeat_delete_toChar_reversed()
		{
			Assert.AreEqual(false, AddKey('3'));
			Assert.AreEqual(false, AddKey('d'));
			Assert.AreEqual(false, AddKey('5'));
			Assert.AreEqual(false, AddKey('1'));
			Assert.AreEqual(false, AddKey('f'));
			Assert.AreEqual(true, AddKey('2'));
			AssertParsed("51:action:d;move:f;moveChar:2");
		}
		
		[Test]
		public void Move_word()
		{
			Assert.AreEqual(true, AddKey('w'));
			AssertParsed("1:action:\\0;move:w;moveChar:\\0");
			
			Assert.AreEqual(false, AddKey('2'));
			Assert.AreEqual(false, AddKey('d'));
			Assert.AreEqual(true, AddKey('w'));
			AssertParsed("2:action:d;move:w;moveChar:\\0");
			
			Assert.AreEqual(false, AddKey('d'));
			Assert.AreEqual(true, AddKey('b'));
			AssertParsed("1:action:d;move:b;moveChar:\\0");
		}
		
		private ViCommandParserTest Add(char c)
		{
			Assert.AreEqual(false, AddKey(c), "key:" + c);
			return this;
		}
		
		private ViCommandParserTest AddLast(char c)
		{
			Assert.AreEqual(true, AddKey(c), "key:" + c);
			return this;
		}
		
		private ViCommandParserTest AddControl(char c)
		{
			Assert.AreEqual(false, parser.AddKey(new ViChar(c, true)), "key:<C-" + c + ">");
			return this;
		}
		
		private ViCommandParserTest AddControlLast(char c)
		{
			Assert.AreEqual(true, parser.AddKey(new ViChar(c, true)), "key:<C-" + c + ">");
			return this;
		}
		
		[Test]
		public void fFtT()
		{
			Add('f').AddLast('a').AssertParsed("1:action:\\0;move:f;moveChar:a");
			Add('F').AddLast('a').AssertParsed("1:action:\\0;move:F;moveChar:a");
			Add('t').AddLast('a').AssertParsed("1:action:\\0;move:t;moveChar:a");
			Add('T').AddLast('a').AssertParsed("1:action:\\0;move:T;moveChar:a");
			Add('2').Add('t').AddLast('a').AssertParsed("2:action:\\0;move:t;moveChar:a");
			Add('3').Add('T').AddLast('a').AssertParsed("3:action:\\0;move:T;moveChar:a");
		}
		
		[Test]
		public void S6_S4_0()
		{
			AddLast('0').AssertParsed("1:action:\\0;move:0;moveChar:\\0");
			Add('1').Add('0').AddLast('j').AssertParsed("10:action:\\0;move:j;moveChar:\\0");
			Add('d').AddLast('0').AssertParsed("1:action:d;move:0;moveChar:\\0");
			Add('d').Add('2').Add('0').AddLast('j').AssertParsed("20:action:d;move:j;moveChar:\\0");
			
			AddLast('^').AssertParsed("1:action:\\0;move:^;moveChar:\\0");
			
			AddLast('$').AssertParsed("1:action:\\0;move:$;moveChar:\\0");
			Add('2').AddLast('$').AssertParsed("2:action:\\0;move:$;moveChar:\\0");
		}
		
		[Test]
		public void gg_G()
		{
			Add('g').AddLast('g').AssertParsed("1:action:\\0;move:g;moveChar:g");
			AddLast('G').AssertParsed("1:action:\\0;move:G;moveChar:\\0");
		}
		
		[Test]
		public void pageUpDown()
		{
			AddControlLast('f').AssertParsed("1:action:\\0;move:<C-f>;moveChar:\\0");
			Add('2').AddControlLast('f').AssertParsed("2:action:\\0;move:<C-f>;moveChar:\\0");
			AddControlLast('b').AssertParsed("1:action:\\0;move:<C-b>;moveChar:\\0");
			Add('2').AddControlLast('b').AssertParsed("2:action:\\0;move:<C-b>;moveChar:\\0");
		}
		
		[Test]
		public void Count_iaIA()
		{
			Add('1').Add('0').AddLast('i').AssertParsed("10:action:i;move:\\0;moveChar:\\0");
			Add('1').Add('0').AddLast('a').AssertParsed("10:action:a;move:\\0;moveChar:\\0");
			Add('1').Add('0').AddLast('I').AssertParsed("10:action:I;move:\\0;moveChar:\\0");
			Add('1').Add('0').AddLast('A').AssertParsed("10:action:A;move:\\0;moveChar:\\0");
		}
	}
}