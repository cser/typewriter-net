using MulticaretEditor;
using NUnit.Framework;
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
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < line.charsCount; ++i)
			{
				builder.Append(line.chars[i].c);
			}
			Assert.AreEqual(expected, builder.ToString(), "chars");
		}
		
		private void AssertStyles(string expected)
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
		
		private void AssertStylesBuffer(string expected)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < line.chars.Length; ++i)
			{
				short style = line.chars[i].style;
				Assert.True(style >= 0 && style < 10, " uncorrect: 0 <= " + style + " < 10");
				builder.Append(line.chars[i].style);
			}
			Assert.AreEqual(expected, builder.ToString(), "styles");
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
			Init("abc\ndefghidklmnopqrstuvwxyz");
			
			line.Chars_AddRange(lines[1]);
			AssertChars("abc\ndefghidklmnopqrstuvwxyz");
			AssertCharsBuffer("abc\ndefghidklmnopqrstuvwxyz\0\0\0\0\0");
		}
	}
}
