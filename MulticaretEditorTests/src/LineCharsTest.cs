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
			AssertCharsBuffer("\0\0");
		}
		
		[Test]
		public void Chars_Add()
		{
			Init("");
			line.Chars_Add(new Char('1'));
			line.Chars_Add(new Char('2'));
			AssertChars("12");
			AssertCharsBuffer("12");
			
			line.Chars_Add(new Char('3'));
			AssertChars("123");
			AssertCharsBuffer("123\0");
		}
	}
}
