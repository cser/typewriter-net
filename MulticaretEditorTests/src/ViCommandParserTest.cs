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
		
		private void Init(bool lineMode)
		{
			parser = new ViCommandParser(lineMode);
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
			Assert.AreEqual(expected, StringOf(parser, false));
		}
		
		private void AssertParsedRawCount(string expected)
		{
			Assert.AreEqual(expected, StringOf(parser, true));
		}
		
		private static string StringOf(ViCommandParser parser, bool raw)
		{
			string text = (raw ? parser.rawCount : parser.FictiveCount) + "";
			text += ":action:" + parser.action + ";move:" + parser.move + ";moveChar:" + parser.moveChar;
			if (parser.register != '\0')
			{
				text += ";register:" + parser.register;
			}
			return text;
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void Move_hjkl(bool lineMode)
		{
			Init(lineMode);
			
			Assert.AreEqual(true, AddKey('h'));
			AssertParsed("1:action:\\0;move:h;moveChar:\\0");
			
			Assert.AreEqual(true, AddKey('j'));
			AssertParsed("1:action:\\0;move:j;moveChar:\\0");
			
			Assert.AreEqual(true, AddKey('k'));
			AssertParsed("1:action:\\0;move:k;moveChar:\\0");
			
			Assert.AreEqual(true, AddKey('l'));
			AssertParsed("1:action:\\0;move:l;moveChar:\\0");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void Move_repeat_hj(bool lineMode)
		{
			Init(lineMode);
			
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
			Init(false);
			
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
			Init(false);
			
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
			Init(false);
			
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
		public void UpperLower_VISUAL()
		{
			Init(true);
			
			AddLast('u');
			AssertParsed("1:action:u;move:\\0;moveChar:\\0");
			AddLast('U');
			AssertParsed("1:action:U;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void UpperLower()
		{
			Init(false);
			
			AddLast('~');
			AssertParsed("1:action:~;move:\\0;moveChar:\\0");
			
			Add('1').Add('4').AddLast('~');
			AssertParsed("14:action:~;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void Move_toChar()
		{
			Init(false);
			
			Add('f').AddLast('a').AssertParsed("1:action:\\0;move:f;moveChar:a");
			Add('f').AddLast('2').AssertParsed("1:action:\\0;move:f;moveChar:2");
			Add('f').AddLast('0').AssertParsed("1:action:\\0;move:f;moveChar:0");
		}
		
		[Test]
		public void Repeat_delete_toChar()
		{
			Init(false);
			
			Add('2').Add('f').AddLast('a').AssertParsed("2:action:\\0;move:f;moveChar:a");
			Add('3').Add('4').Add('f').AddLast('a').AssertParsed("34:action:\\0;move:f;moveChar:a");
			Add('4').Add('9').Add('f').AddLast('2').AssertParsed("49:action:\\0;move:f;moveChar:2");
			Add('5').Add('1').Add('d').Add('f').AddLast('2').AssertParsed("51:action:d;move:f;moveChar:2");
		}
		
		[Test]
		public void RepeatMoveChar_LINES()
		{
			Init(true);
			
			Add('2').Add('f').AddLast('a').AssertParsed("2:action:\\0;move:f;moveChar:a");
			Add('3').Add('4').Add('f').AddLast('a').AssertParsed("34:action:\\0;move:f;moveChar:a");
			Add('4').Add('9').Add('f').AddLast('2').AssertParsed("49:action:\\0;move:f;moveChar:2");
		}
		
		[Test]
		public void Repeat_delete_toChar_reversed()
		{
			Init(false);
			
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
			Init(false);
			
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
		
		[Test]
		public void Move_word_LINES()
		{
			Init(true);
			
			Assert.AreEqual(true, AddKey('w'));
			AssertParsed("1:action:\\0;move:w;moveChar:\\0");
			
			Assert.AreEqual(false, AddKey('2'));
			Assert.AreEqual(true, AddKey('w'));
			AssertParsed("2:action:\\0;move:w;moveChar:\\0");
		}
		
		private ViCommandParserTest Add(char c)
		{
			Assert.AreEqual(false, AddKey(c), "key:" + c + " - expected not last");
			return this;
		}
		
		private ViCommandParserTest AddLast(char c)
		{
			Assert.AreEqual(true, AddKey(c), "last key:" + c + " - expected last");
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
		
		[TestCase(false)]
		[TestCase(true)]
		public void fFtT(bool lineMode)
		{
			Init(lineMode);
			
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
			Init(false);
			
			AddLast('0').AssertParsed("1:action:\\0;move:0;moveChar:\\0");
			Add('1').Add('0').AddLast('j').AssertParsed("10:action:\\0;move:j;moveChar:\\0");
			Add('d').AddLast('0').AssertParsed("1:action:d;move:0;moveChar:\\0");
			Add('d').Add('2').Add('0').AddLast('j').AssertParsed("20:action:d;move:j;moveChar:\\0");
			
			AddLast('^').AssertParsed("1:action:\\0;move:^;moveChar:\\0");
			
			AddLast('$').AssertParsed("1:action:\\0;move:$;moveChar:\\0");
			Add('2').AddLast('$').AssertParsed("2:action:\\0;move:$;moveChar:\\0");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void gg_G(bool lineMode)
		{
			Init(lineMode);
			
			Add('g').AddLast('g').AssertParsed("1:action:\\0;move:g;moveChar:g");
			AddLast('G').AssertParsed("1:action:\\0;move:G;moveChar:\\0");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void NumberG(bool lineMode)
		{
			Init(lineMode);
			
			AddLast('G').AssertParsedRawCount("-1:action:\\0;move:G;moveChar:\\0");
			Add('1').AddLast('G').AssertParsedRawCount("1:action:\\0;move:G;moveChar:\\0");
			Add('1').Add('0').AddLast('G').AssertParsedRawCount("10:action:\\0;move:G;moveChar:\\0");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void pageUpDown(bool lineMode)
		{
			Init(lineMode);
			
			AddControlLast('f').AssertParsed("1:action:\\0;move:<C-f>;moveChar:\\0");
			Add('2').AddControlLast('f').AssertParsed("2:action:\\0;move:<C-f>;moveChar:\\0");
			AddControlLast('b').AssertParsed("1:action:\\0;move:<C-b>;moveChar:\\0");
			Add('2').AddControlLast('b').AssertParsed("2:action:\\0;move:<C-b>;moveChar:\\0");
		}
		
		[Test]
		public void Count_iaIA()
		{
			Init(false);
			
			Add('1').Add('0').AddLast('i').AssertParsed("10:action:i;move:\\0;moveChar:\\0");
			Add('1').Add('0').AddLast('a').AssertParsed("10:action:a;move:\\0;moveChar:\\0");
			Add('1').Add('0').AddLast('I').AssertParsed("10:action:I;move:\\0;moveChar:\\0");
			Add('1').Add('0').AddLast('A').AssertParsed("10:action:A;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void r()
		{
			Init(false);
			
			Add('r').AddLast('x').AssertParsed("1:action:r;move:\\0;moveChar:x");
			Add('5').Add('r').AddLast('x').AssertParsed("5:action:r;move:\\0;moveChar:x");
		}
		
		[Test]
		public void r_VISUAL()
		{
			Init(true);
			
			Add('r').AddLast('x').AssertParsed("1:action:r;move:\\0;moveChar:x");
			Add('5').Add('r').AddLast('x').AssertParsed("5:action:r;move:\\0;moveChar:x");
		}
		
		[Test]
		public void Jump()
		{
			Init(false);
			
			Add(' ').AddLast('x').AssertParsed("1:action: ;move:\\0;moveChar:x");
		}
		
		[Test]
		public void Jump_VISUAL()
		{
			Init(true);
			
			Add(' ').AddLast('x').AssertParsed("1:action: ;move:\\0;moveChar:x");
		}
		
		[Test]
		public void Jump_NewCursor()
		{
			Init(false);
			
			Add(',').Add(' ').AddLast('x').AssertParsed("1:action:,;move: ;moveChar:x");
			Add('\\').Add(' ').AddLast('x').AssertParsed("1:action:,;move: ;moveChar:x");
		}
		
		[Test]
		public void c()
		{
			Init(false);
			
			Add('c').AddLast('w').AssertParsed("1:action:c;move:w;moveChar:\\0");
			Add('5').Add('c').AddLast('w').AssertParsed("5:action:c;move:w;moveChar:\\0");
		}
		
		[Test]
		public void c_LINES()
		{
			Init(true);
			
			AddLast('c').AssertParsed("1:action:c;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void x()
		{
			Init(false);
			
			AddLast('x').AssertParsed("1:action:x;move:\\0;moveChar:\\0");
			Add('5').AddLast('x').AssertParsed("5:action:x;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void x_LINES()
		{
			Init(true);
			
			AddLast('x').AssertParsed("1:action:x;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void y()
		{
			Init(false);
			
			Add('y').AddLast('w').AssertParsed("1:action:y;move:w;moveChar:\\0");
			Add('8').Add('y').AddLast('w').AssertParsed("8:action:y;move:w;moveChar:\\0");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void P(bool lineMode)
		{
			Init(lineMode);
			
			AddLast('p').AssertParsed("1:action:p;move:\\0;moveChar:\\0");
			Add('8').AddLast('p').AssertParsed("8:action:p;move:\\0;moveChar:\\0");
			
			Init(lineMode);
			
			AddLast('P').AssertParsed("1:action:P;move:\\0;moveChar:\\0");
			Add('8').AddLast('P').AssertParsed("8:action:P;move:\\0;moveChar:\\0");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void e(bool lineMode)
		{
			Init(lineMode);
			
			AddLast('e').AssertParsed("1:action:\\0;move:e;moveChar:\\0");
		}
		
		[Test]
		public void J()
		{
			Init(false);
			
			AddLast('J').AssertParsed("1:action:J;move:\\0;moveChar:\\0");
			Add('2').AddLast('J').AssertParsed("2:action:J;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void J_LINES()
		{
			Init(true);
			
			AddLast('J').AssertParsed("1:action:J;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void Registers()
		{
			Init(false);
			
			/*
			"0 - default
			"a-z - simple
			"A-Z - accumulating
			"*,"- - clipboard
			"/ - last search
			
			readonly:
			": - last command
			". - last insert text
			"% - file name
			*/
			
			Add('"').Add('*').Add('y').AddLast('w').AssertParsed("1:action:y;move:w;moveChar:\\0;register:*");
		}
		
		[Test]
		public void Registers_LINES()
		{
			Init(true);
			Add('"').Add('*').AddLast('y').AssertParsed("1:action:y;move:\\0;moveChar:\\0;register:*");
		}
		
		[Test]
		public void TextObjects()
		{
			Init(false);
			
			Add('d').Add('i').AddLast('w').AssertParsed("1:action:d;move:i;moveChar:w");
			Add('1').Add('0').Add('d').Add('i').AddLast('w').AssertParsed("10:action:d;move:i;moveChar:w");
			Add('c').Add('a').AddLast('w').AssertParsed("1:action:c;move:a;moveChar:w");
			Add('d').Add('i').AddLast('"').AssertParsed("1:action:d;move:i;moveChar:\"");
			Add('d').Add('i').AddLast('\'').AssertParsed("1:action:d;move:i;moveChar:'");
			Add('d').Add('i').AddLast('`').AssertParsed("1:action:d;move:i;moveChar:`");
			Add('c').Add('i').AddLast('{').AssertParsed("1:action:c;move:i;moveChar:{");
			Add('d').Add('i').AddLast('}').AssertParsed("1:action:d;move:i;moveChar:}");
			
			Add('c').Add('i').AddLast('(').AssertParsed("1:action:c;move:i;moveChar:(");
			Add('c').Add('i').AddLast(')').AssertParsed("1:action:c;move:i;moveChar:)");
			Add('c').Add('i').AddLast('b').AssertParsed("1:action:c;move:i;moveChar:b");
			
			Add('d').Add('i').AddLast('[').AssertParsed("1:action:d;move:i;moveChar:[");
			Add('c').Add('i').AddLast('p').AssertParsed("1:action:c;move:i;moveChar:p");//paragraph
			Add('d').Add('i').AddLast('<').AssertParsed("1:action:d;move:i;moveChar:<");
			Add('d').Add('i').AddLast('>').AssertParsed("1:action:d;move:i;moveChar:>");
			Add('d').Add('i').AddLast('t').AssertParsed("1:action:d;move:i;moveChar:t");
			
			Add('c').Add('i').AddLast('s').AssertParsed("1:action:c;move:i;moveChar:s");//sentence
		}
		
		[Test]
		public void TextObjects_LINES()
		{
			Init(true);
			
			Add('i').AddLast('w').AssertParsed("1:action:\\0;move:i;moveChar:w");
			Add('1').Add('0').Add('i').AddLast('w').AssertParsed("10:action:\\0;move:i;moveChar:w");
			Add('a').AddLast('w').AssertParsed("1:action:\\0;move:a;moveChar:w");
			Add('i').AddLast('"').AssertParsed("1:action:\\0;move:i;moveChar:\"");
			Add('i').AddLast('\'').AssertParsed("1:action:\\0;move:i;moveChar:'");
			Add('i').AddLast('`').AssertParsed("1:action:\\0;move:i;moveChar:`");
			Add('i').AddLast('{').AssertParsed("1:action:\\0;move:i;moveChar:{");
			Add('i').AddLast('}').AssertParsed("1:action:\\0;move:i;moveChar:}");
			
			Add('i').AddLast('(').AssertParsed("1:action:\\0;move:i;moveChar:(");
			Add('i').AddLast(')').AssertParsed("1:action:\\0;move:i;moveChar:)");
			Add('i').AddLast('b').AssertParsed("1:action:\\0;move:i;moveChar:b");
			
			Add('i').AddLast('[').AssertParsed("1:action:\\0;move:i;moveChar:[");
			Add('i').AddLast('p').AssertParsed("1:action:\\0;move:i;moveChar:p");//paragraph
			Add('i').AddLast('<').AssertParsed("1:action:\\0;move:i;moveChar:<");
			Add('i').AddLast('>').AssertParsed("1:action:\\0;move:i;moveChar:>");
			Add('i').AddLast('t').AssertParsed("1:action:\\0;move:i;moveChar:t");
			
			Add('i').AddLast('s').AssertParsed("1:action:\\0;move:i;moveChar:s");//sentence
		}
		
		[Test]
		public void s()
		{
			Init(false);
			
			AddLast('s').AssertParsed("1:action:s;move:\\0;moveChar:\\0");
			Add('4').AddLast('s').AssertParsed("4:action:s;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void s_LINES()
		{
			Init(true);
			
			AddLast('s').AssertParsed("1:action:s;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void dd()
		{
			Init(false);
			
			Add('d').AddLast('d').AssertParsed("1:action:d;move:d;moveChar:\\0");
			Add('2').Add('d').AddLast('d').AssertParsed("2:action:d;move:d;moveChar:\\0");
		}
		
		[Test]
		public void d_LINES()
		{
			Init(true);
			
			AddLast('d').AssertParsed("1:action:d;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void Dot()
		{
			Init(false);
			
			AddLast('.').AssertParsed("1:action:.;move:\\0;moveChar:\\0");
			Add('2').AddLast('.').AssertParsed("2:action:.;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void yy()
		{
			Init(false);
			
			Add('y').AddLast('y').AssertParsed("1:action:y;move:y;moveChar:\\0");
			Add('2').Add('y').AddLast('y').AssertParsed("2:action:y;move:y;moveChar:\\0");
		}
		
		[Test]
		public void Y()
		{
			Init(false);
			
			AddLast('Y').AssertParsed("1:action:Y;move:\\0;moveChar:\\0");
			Add('2').AddLast('Y').AssertParsed("2:action:Y;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void y_LINES()
		{
			Init(true);
			
			AddLast('y').AssertParsed("1:action:y;move:\\0;moveChar:\\0");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void Shortcuts(bool lineMode)
		{
			Init(lineMode);
			
			AddLast('/');
			Assert.AreEqual("/", parser.shortcut);
			
			AddLast('?');
			Assert.AreEqual("?", parser.shortcut);
			
			AddLast(':');
			Assert.AreEqual(":", parser.shortcut);
			
			AddControlLast('/');
			Assert.AreEqual("C/", parser.shortcut);
			
			AddControlLast('?');
			Assert.AreEqual("C?", parser.shortcut);
		}
		
		[Test]
		public void Leader_b()
		{
			Init(false);
			
			Add('\\').AddLast('b');
			Assert.AreEqual("\\b", parser.shortcut);
			
			Add(',').AddLast('b');
			Assert.AreEqual("\\b", parser.shortcut);
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void ScrollUpDown(bool lineMode)
		{
			Init(lineMode);
			
			AddControlLast('k').AssertParsed("1:action:<C-k>;move:\\0;moveChar:\\0");
			AddControlLast('j').AssertParsed("1:action:<C-j>;move:\\0;moveChar:\\0");
			
			Add('2').AddControlLast('k').AssertParsed("2:action:<C-k>;move:\\0;moveChar:\\0");
			Add('1').Add('0').AddControlLast('j').AssertParsed("10:action:<C-j>;move:\\0;moveChar:\\0");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void FindNextPrevious(bool lineMode)
		{
			Init(lineMode);
			
			AddLast('n').AssertParsed("1:action:\\0;move:n;moveChar:\\0");
			AddLast('N').AssertParsed("1:action:\\0;move:N;moveChar:\\0");
			
			Add('2').AddLast('n').AssertParsed("2:action:\\0;move:n;moveChar:\\0");
			Add('1').Add('0').AddLast('N').AssertParsed("10:action:\\0;move:N;moveChar:\\0");
		}
		
		[Test]
		public void o()
		{
			Init(false);
			
			AddLast('o').AssertParsed("1:action:o;move:\\0;moveChar:\\0");
			Add('2').AddLast('o').AssertParsed("2:action:o;move:\\0;moveChar:\\0");
			
			AddLast('O').AssertParsed("1:action:O;move:\\0;moveChar:\\0");
			Add('2').AddLast('O').AssertParsed("2:action:O;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void o_LINES()
		{
			Init(true);
			
			AddLast('o').AssertParsed("1:action:o;move:\\0;moveChar:\\0");			
			AddLast('O').AssertParsed("1:action:O;move:\\0;moveChar:\\0");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void gij(bool lineMode)
		{
			Init(lineMode);
			
			Add('g').AddLast('j').AssertParsed("1:action:\\0;move:j;moveChar:g");
			Add('2').Add('g').AddLast('k').AssertParsed("2:action:\\0;move:k;moveChar:g");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void Star(bool lineMode)
		{
			Init(lineMode);
			
			AddLast('*').AssertParsed("1:action:*;move:\\0;moveChar:\\0");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void Sharp(bool lineMode)
		{
			Init(lineMode);
			
			AddLast('#').AssertParsed("1:action:#;move:\\0;moveChar:\\0");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void vV(bool lineMode)
		{
			Init(lineMode);
			
			AddLast('v').AssertParsed("1:action:v;move:\\0;moveChar:\\0");
			AddLast('V').AssertParsed("1:action:V;move:\\0;moveChar:\\0");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void Ctrl_d_Ctrl_D(bool lineMode)
		{
			Init(lineMode);
			
			Assert.AreEqual(true, AddKey('d', true));
			AssertParsed("1:action:<C-d>;move:\\0;moveChar:\\0");
			Assert.AreEqual(true, AddKey('D', true));
			AssertParsed("1:action:<C-D>;move:\\0;moveChar:\\0");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void CtrlShiftJK(bool lineMode)
		{
			Init(lineMode);
			
			Assert.AreEqual(true, AddKey('J', true));
			AssertParsed("1:action:<C-J>;move:\\0;moveChar:\\0");
			Add('1').Add('6');
			Assert.AreEqual(true, AddKey('K', true));
			AssertParsed("16:action:<C-K>;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void Shift()
		{
			Init(false);
			
			Add('>').AddLast('>').AssertParsed("1:action:>;move:>;moveChar:\\0");
			Add('2').Add('>').AddLast('>').AssertParsed("2:action:>;move:>;moveChar:\\0");
			Add('<').AddLast('<').AssertParsed("1:action:<;move:<;moveChar:\\0");
			Add('1').Add('4').Add('<').AddLast('<').AssertParsed("14:action:<;move:<;moveChar:\\0");
		}
		
		[Test]
		public void Shift_LINES()
		{
			Init(true);
			
			AddLast('>').AssertParsed("1:action:>;move:\\0;moveChar:\\0");
			Add('2').AddLast('>').AssertParsed("2:action:>;move:\\0;moveChar:\\0");
			
			AddLast('<').AssertParsed("1:action:<;move:\\0;moveChar:\\0");
			Add('2').AddLast('<').AssertParsed("2:action:<;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void CD()
		{
			Init(false);
			AddLast('C').AssertParsed("1:action:C;move:\\0;moveChar:\\0");
			AddLast('D').AssertParsed("1:action:D;move:\\0;moveChar:\\0");
			Add('2').AddLast('C').AssertParsed("2:action:C;move:\\0;moveChar:\\0");
			Add('1').Add('0').AddLast('D').AssertParsed("10:action:D;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void cc()
		{
			Init(false);
			Add('c').AddLast('c').AssertParsed("1:action:c;move:c;moveChar:\\0");
			Add('2').Add('c').AddLast('c').AssertParsed("2:action:c;move:c;moveChar:\\0");
		}
		
		[Test]
		public void Backspace()
		{
			Init(false);
			AddLast('\b').AssertParsed("1:action:\b;move:\\0;moveChar:\\0");
			Add('2').AddLast('\b').AssertParsed("2:action:\b;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void Enter()
		{
			Init(false);
			AddLast('\r').AssertParsed("1:action:\r;move:\\0;moveChar:\\0");
			Add('2').AddLast('\r').AssertParsed("2:action:\r;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void gv()
		{
			Init(false);
			Add('g').AddLast('v').AssertParsed("1:action:\\0;move:g;moveChar:v");
		}
		
		[Test]
		public void W_E_B()
		{
			Init(false);
			AddLast('W').AssertParsed("1:action:\\0;move:W;moveChar:\\0");
			AddLast('E').AssertParsed("1:action:\\0;move:E;moveChar:\\0");
			AddLast('B').AssertParsed("1:action:\\0;move:B;moveChar:\\0");
			Add('2').AddLast('W').AssertParsed("2:action:\\0;move:W;moveChar:\\0");
			Add('2').AddLast('E').AssertParsed("2:action:\\0;move:E;moveChar:\\0");
			Add('2').AddLast('B').AssertParsed("2:action:\\0;move:B;moveChar:\\0");
			Add('d').AddLast('W').AssertParsed("1:action:d;move:W;moveChar:\\0");
			Add('d').AddLast('E').AssertParsed("1:action:d;move:E;moveChar:\\0");
			Add('d').AddLast('B').AssertParsed("1:action:d;move:B;moveChar:\\0");
		}
		
		[Test]
		public void Percents()
		{
			Init(false);
			Add('d').AddLast('%').AssertParsed("1:action:d;move:%;moveChar:\\0");
			AddLast('%').AssertParsed("1:action:\\0;move:%;moveChar:\\0");
		}
		
		[Test]
		public void Percents_VISUAL()
		{
			Init(true);
			AddLast('%').AssertParsed("1:action:\\0;move:%;moveChar:\\0");
		}
		
		[Test]
		public void FileTree()
		{
			Init(true);
			
			Add(',').AddLast('n');
			Assert.AreEqual("\\n", parser.shortcut);
			
			Add(',').AddLast('N');
			Assert.AreEqual("\\N", parser.shortcut);
		}
		
		[Test]
		public void PrevNextPosition()
		{
			Init(false);
			
			Assert.AreEqual(true, AddKey('o', true));
			AssertParsed("1:action:<C-o>;move:\\0;moveChar:\\0");
			
			Assert.AreEqual(true, AddKey('i', true));
			AssertParsed("1:action:<C-i>;move:\\0;moveChar:\\0");
		}
		
		[Test]
		public void Leader_AdditionShortcuts()
		{
			Init(false);
			
			Add(',').AddLast('s');
			Assert.AreEqual("\\s", parser.shortcut);
			
			Add('\\').AddLast('s');
			Assert.AreEqual("\\s", parser.shortcut);
			
			Add(',').AddLast('r');
			Assert.AreEqual("\\r", parser.shortcut);
			
			Add('\\').AddLast('r');
			Assert.AreEqual("\\r", parser.shortcut);
			
			Add(',').AddLast('g');
			Assert.AreEqual("\\g", parser.shortcut);
			
			Add('\\').AddLast('g');
			Assert.AreEqual("\\g", parser.shortcut);
		}
		
		[Test]
		public void Bookmarks()
		{
			Init(false);
			
			Add('m').AddLast('a');
			AssertParsed("1:action:m;move:\\0;moveChar:a");
			Add('m').AddLast('z');
			AssertParsed("1:action:m;move:\\0;moveChar:z");
			
			Add('\'').AddLast('a');
			AssertParsed("1:action:\\0;move:';moveChar:a");
			Add('`').AddLast('a');
			AssertParsed("1:action:\\0;move:`;moveChar:a");
		}
		
		[Test]
		public void BookmarksUppercase()
		{
			Init(false);
			
			Add('m').AddLast('A');
			AssertParsed("1:action:m;move:\\0;moveChar:A");
			Add('m').AddLast('Z');
			AssertParsed("1:action:m;move:\\0;moveChar:Z");
			
			Add('\'').AddLast('A');
			AssertParsed("1:action:\\0;move:';moveChar:A");
			Add('`').AddLast('A');
			AssertParsed("1:action:\\0;move:`;moveChar:A");
		}
		
		[Test]
		public void Console()
		{
			Init(false);
			
			Add(',').AddLast('c');
			Assert.AreEqual("\\c", parser.shortcut);
			
			Add(',').AddLast('f');
			Assert.AreEqual("\\f", parser.shortcut);
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void UnselectPrevText(bool lineMode)
		{
			Init(lineMode);
			Add('g').AddLast('K');
			AssertParsed("1:action:\\0;move:g;moveChar:K");
		}
		
		[TestCase(false)]
		[TestCase(true)]
		public void UnselectPrevText_Repeat(bool lineMode)
		{
			Init(lineMode);
			Add('2').Add('g').AddLast('K');
			AssertParsed("2:action:\\0;move:g;moveChar:K");
		}
	}
}
