using MulticaretEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;

namespace UnitTests
{
	[TestFixture]
	public class LineCharsTest
	{
		private LineArray lines;
		private Line line;

		private void Init(string text)
		{
			lines = new LineArray(10);
			lines.tabSize = 4;
			lines.SetText(text);
			line = lines[0];
		}
		
		private void AssertChars(string expected)
		{
			AssertChars(expected, line);
		}
		
		private void AssertChars(string expected, Line line)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < line.charsCount; ++i)
			{
				builder.Append(line.chars[i].c);
			}
			Assert.AreEqual(expected, builder.ToString(), "chars");
		}
		
		private void AssertStyles(string expected)
		{
			AssertStyles(expected, line);
		}
		
		private void AssertStyles(string expected, Line line)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < line.charsCount; ++i)
			{
				short style = line.chars[i].style;
				Assert.True(style >= 0 && style < 10, " uncorrect: 0 <= " + style + " < 10");
				builder.Append(style);
			}
			Assert.AreEqual(expected, builder.ToString(), "styles");
		}
		
		private void AssertCharsBuffer(string expected)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < line.chars.Length; ++i)
			{
				builder.Append(line.chars[i].c);
			}
			Assert.AreEqual(expected, builder.ToString(), "chars");
		}

		[Test]
		public void EmptyString()
		{
			Init("");
			AssertChars("");
			AssertStyles("");
			AssertCharsBuffer("\0\0");
		}
		
		[Test]
		public void Chars_Add()
		{
			Init("");
			line.Chars_Add(new Char('1'));
			line.Chars_Add(new Char('2'));
			AssertChars("12");
			AssertStyles("00");
			AssertCharsBuffer("12");
			
			line.Chars_Add(new Char('3'));
			AssertChars("123");
			AssertStyles("000");
			AssertCharsBuffer("123\0");
		}
		
		[Test]
		public void Chars_AddRange()
		{
			Init("abc\ndef");
			lines[0].SetRangeStyle(1, 2, 9);
			lines[1].SetRangeStyle(1, 2, 7);
			
			line.Chars_AddRange(lines[1]);
			AssertChars("abc\ndef");
			AssertStyles("0990077");
			AssertCharsBuffer("abc\ndef\0");
			
			line.Chars_AddRange(lines[1], 1, 2);
			AssertChars("abc\ndefef");
			AssertStyles("099007777");
			AssertCharsBuffer("abc\ndefef\0\0\0\0\0\0\0");
			
			line.Chars_AddRange(lines[1], 1, 1);
			AssertChars("abc\ndefefe");
			AssertStyles("0990077777");
			AssertCharsBuffer("abc\ndefefe\0\0\0\0\0\0");
			
			line.Chars_AddRange(lines[1], 0, 2);
			AssertChars("abc\ndefefede");
			AssertStyles("099007777707");
			AssertCharsBuffer("abc\ndefefede\0\0\0\0");
		}
		
		[Test]
		public void Chars_AddRange_JumpOverX2()
		{
			Init("abc\nDEFGHIDKLMNOPQRSTUVWXYZ");
			
			line.Chars_AddRange(lines[1]);
			AssertChars("abc\nDEFGHIDKLMNOPQRSTUVWXYZ");//can't be in real, but chars mast works so
			AssertCharsBuffer("abc\nDEFGHIDKLMNOPQRSTUVWXYZ\0\0\0\0\0");
		}
		
		[Test]
		public void Chars_RemoveAt_0()
		{
			Init("abcde");
			lines[0].SetRangeStyle(1, 2, 9);
			lines[0].SetRangeStyle(3, 2, 7);
			AssertChars("abcde");
			AssertStyles("09977");
			AssertCharsBuffer("abcde");
			
			line.Chars_RemoveAt(0);
			AssertChars("bcde");
			AssertStyles("9977");
			AssertCharsBuffer("bcdee");//optionally
		}
		
		[Test]
		public void Chars_RemoveAt_1()
		{
			Init("abcde");
			lines[0].SetRangeStyle(1, 2, 9);
			lines[0].SetRangeStyle(3, 2, 7);
			AssertChars("abcde");
			AssertStyles("09977");
			AssertCharsBuffer("abcde");
			
			line.Chars_RemoveAt(1);
			AssertChars("acde");
			AssertStyles("0977");
			AssertCharsBuffer("acdee");//optionally
		}
		
		[Test]
		public void Chars_RemoveAt_End()
		{
			Init("abcde");
			lines[0].SetRangeStyle(1, 2, 9);
			lines[0].SetRangeStyle(3, 2, 7);
			AssertChars("abcde");
			AssertStyles("09977");
			AssertCharsBuffer("abcde");
			
			line.Chars_RemoveAt(4);
			AssertChars("abcd");
			AssertStyles("0997");
			AssertCharsBuffer("abcde");//optionally
		}
		
		[Test]
		public void Chars_RemoveRange_0()
		{
			Init("abcde");
			lines[0].SetRangeStyle(1, 2, 9);
			lines[0].SetRangeStyle(3, 2, 7);
			AssertChars("abcde");
			AssertStyles("09977");
			AssertCharsBuffer("abcde");
			
			line.Chars_RemoveRange(0, 3);
			AssertChars("de");
			AssertStyles("77");
			AssertCharsBuffer("decde");//optionally
		}
		
		[Test]
		public void Chars_RemoveRange_1()
		{
			Init("abcde");
			lines[0].SetRangeStyle(1, 2, 9);
			lines[0].SetRangeStyle(3, 2, 7);
			AssertChars("abcde");
			AssertStyles("09977");
			AssertCharsBuffer("abcde");
			
			line.Chars_RemoveRange(1, 2);
			AssertChars("ade");
			AssertStyles("077");
			AssertCharsBuffer("adede");//optionally
		}
		
		[Test]
		public void Chars_RemoveRange_End()
		{
			Init("abcde");
			lines[0].SetRangeStyle(1, 2, 9);
			lines[0].SetRangeStyle(3, 2, 7);
			AssertChars("abcde");
			AssertStyles("09977");
			AssertCharsBuffer("abcde");
			
			line.Chars_RemoveRange(2, 3);
			AssertChars("ab");
			AssertStyles("09");
			AssertCharsBuffer("abcde");//optionally
		}
		
		[Test]
		public void Chars_InsertRange1()
		{
			Init("abcdef");
			lines[0].SetRangeStyle(1, 2, 9);
			lines[0].SetRangeStyle(3, 2, 7);
			AssertChars("abcdef");
			AssertStyles("099770");
			AssertCharsBuffer("abcdef");
			
			line.Chars_InsertRange(2, new List<char>(new char[] { 'A', 'B', 'C' }));
			AssertChars("abABCcdef");
			AssertStyles("090009770");
			AssertCharsBuffer("abABCcdef\0\0\0");//optionally
		}
		
		[Test]
		public void Chars_InsertRange2()
		{
			Init("abcdef");
			lines[0].SetRangeStyle(1, 2, 9);
			lines[0].SetRangeStyle(3, 2, 7);
			AssertChars("abcdef");
			AssertStyles("099770");
			AssertCharsBuffer("abcdef");
			
			line.Chars_InsertRange(2, "ABC");
			AssertChars("abABCcdef");
			AssertStyles("090009770");
			AssertCharsBuffer("abABCcdef\0\0\0");//optionally
		}
		
		[Test]
		public void Chars_InsertRange3()
		{
			Init("abcde\nFGHIGKLMNO");
			lines[0].SetRangeStyle(1, 2, 9);
			lines[0].SetRangeStyle(3, 2, 7);
			AssertChars("abcde\n");
			AssertStyles("099770");
			AssertCharsBuffer("abcde\n");
			
			line.Chars_InsertRange(2, lines[1], 3, 4);
			AssertChars("abIGKLcde\n");
			AssertStyles("0900009770");
			AssertCharsBuffer("abIGKLcde\n\0\0");//optionally
		}
		
		[Test]
		public void Chars_InsertRange31()
		{
			Init("abcde\nFGHIGKLMNO");
			lines[0].SetRangeStyle(1, 2, 9);
			lines[0].SetRangeStyle(3, 2, 7);
			AssertChars("abcde\n");
			AssertStyles("099770");
			AssertCharsBuffer("abcde\n");
			
			line.Chars_InsertRange(0, lines[1], 3, 4);
			AssertChars("IGKLabcde\n");
			AssertStyles("0000099770");
			AssertCharsBuffer("IGKLabcde\n\0\0");//optionally
		}
		
		[Test]
		public void Chars_InsertRange32()
		{
			Init("abcde\nFGHIGKLMNO");
			lines[0].SetRangeStyle(1, 2, 9);
			lines[0].SetRangeStyle(3, 2, 7);
			AssertChars("abcde\n");
			AssertStyles("099770");
			AssertCharsBuffer("abcde\n");
			
			lines[1].Chars_InsertRange(10, lines[0], 2, 3);
			AssertChars("FGHIGKLMNOcde", lines[1]);
			AssertStyles("0000000000977", lines[1]);
		}
		
		[Test]
		public void Chars_InsertRange_JumpOverX2()
		{
			Init("abc\nDEFGHIDKLMNOPQRSTUVWXYZ");
			AssertChars("abc\n");
			AssertCharsBuffer("abc\n");
			
			line.Chars_InsertRange(3, lines[1], 0, 23);
			AssertChars("abcDEFGHIDKLMNOPQRSTUVWXYZ\n");
			AssertCharsBuffer("abcDEFGHIDKLMNOPQRSTUVWXYZ\n\0\0\0\0\0");//optionally
		}
		
		[Test]
		public void Chars_ReduceBuffer()
		{
			Init(
				"123456789_123456789_123456789_123456789_123456789_" +
				"123456789_123456789_123456789_123456789_123456789_" +
				"123456789_123456789_123456789_123456789_123456789_" +
				"123456789_123456789_123456789_123456789_123456789_"
			);
			Assert.AreEqual(200, line.charsCount);
			Assert.AreEqual(200, line.chars.Length);
			
			line.Chars_RemoveAt(199);
			line.Chars_ReduceBuffer();
			Assert.AreEqual(199, line.charsCount);
			Assert.AreEqual(200, line.chars.Length);
			
			line.Chars_RemoveRange(100, 99);
			line.Chars_ReduceBuffer();
			Assert.AreEqual(100, line.charsCount);
			Assert.AreEqual(200, line.chars.Length);
			
			line.Chars_RemoveRange(51, 49);
			line.Chars_ReduceBuffer();
			Assert.AreEqual(51, line.charsCount);
			AssertCharsBuffer(
				"123456789_123456789_123456789_123456789_123456789_" +
				"123456789_123456789_123456789_123456789_123456789_" +
				"123456789_123456789_123456789_123456789_123456789_" +
				"123456789_123456789_123456789_123456789_123456789_"
			);
			
			line.Chars_RemoveRange(49, 2);
			Assert.AreEqual(49, line.charsCount);
			Assert.AreEqual(200, line.chars.Length);
			line.Chars_ReduceBuffer();
			Assert.AreEqual(49, line.charsCount);
			Assert.AreEqual(100, line.chars.Length);
			AssertCharsBuffer(
				"123456789_123456789_123456789_123456789_123456789_" +
				"123456789_123456789_123456789_123456789_123456789_"
			);
			
			line.Chars_RemoveRange(24, 25);
			Assert.AreEqual(24, line.charsCount);
			Assert.AreEqual(100, line.chars.Length);
			line.Chars_ReduceBuffer();
			Assert.AreEqual(24, line.charsCount);
			Assert.AreEqual(50, line.chars.Length);
			AssertCharsBuffer("123456789_123456789_123456789_123456789_123456789_");
		}
		
		[Test]
		public void Chars_ReduceBuffer_CantBeLess32()
		{
			Init(
				"123456789_123456789_123456789_123456789_123456789_" +
				"123456789_123456789_123456789_123456789_123456789_" +
				"123456789_123456789_123456789_123456789_123456789_" +
				"123456789_123456789_123456789_123456789_123456789_"
			);
			Assert.AreEqual(200, line.charsCount);
			Assert.AreEqual(200, line.chars.Length);
			
			line.Chars_RemoveRange(1, 199);
			Assert.AreEqual(1, line.charsCount);
			Assert.AreEqual(200, line.chars.Length);
			line.Chars_ReduceBuffer();
			Assert.AreEqual(1, line.charsCount);
			Assert.AreEqual(50, line.chars.Length);
			AssertCharsBuffer("123456789_123456789_123456789_123456789_123456789_");
		}
	}
}