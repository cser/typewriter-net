using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	public class RegexTest
	{
		[Test]
		public void MatchLength()
		{
			Assert.AreEqual(2, new RERegex(new REChar('a', new REChar('b', null))).MatchLength("ab"));
			Assert.AreEqual(-1, new RERegex(new REChar('a', new REChar('b', null))).MatchLength("aс"));
			Assert.AreEqual(2, new RERegex(new REChar('a',
				new REAlternate(new REChar('b', null), new REChar('c', null), null)
			)).MatchLength("ab"));
			Assert.AreEqual(2, new RERegex(new REChar('a',
				new REAlternate(new REChar('b', null), new REChar('c', null), null)
			)).MatchLength("ac"));
			Assert.AreEqual(-1, new RERegex(new REChar('a',
				new REAlternate(new REChar('b', null), new REChar('c', null), null)
			)).MatchLength("ae"));
		}
			
		[Test]
		public void Parsing_Chars()
		{
			Assert.AreEqual("('a'('b'))", RERegex.Parse(@"ab").ToString());
			Assert.AreEqual("('a'('b'('c')))", RERegex.Parse(@"abc").ToString());
			Assert.AreEqual("('a')", RERegex.Parse(@"a").ToString());
		}
		
		[Test]
		public void Parsing_Alternate()
		{
			Assert.AreEqual("(('a')|('b'))", RERegex.Parse(@"a\|b").ToString());
			Assert.AreEqual("(('a'('b'))|('b'('c')))", RERegex.Parse(@"ab\|bc").ToString());
			Assert.AreEqual("(('a'('b'))|('b'('c')))", RERegex.Parse(@"ab\|bc").ToString());
			Assert.AreEqual("((('f')|('a'('b')))|('b'('c')))", RERegex.Parse(@"f\|ab\|bc").ToString());
		}
		
		[Test]
		public void Parsing_AlternateBrackets()
		{
			Assert.AreEqual("('a'('('('b'('|'('c'(')'('d')))))))", RERegex.Parse(@"a(b|c)d").ToString());
			Assert.AreEqual("('a'(('b')|('c')`('d')))", RERegex.Parse(@"a\(b\|c\)d").ToString());
		}
		
		[Test]
		public void Parsing_wWsS()
		{
			Assert.AreEqual("('a'(w('b')))", RERegex.Parse(@"a\wb").ToString());
			Assert.AreEqual("(w(w('b')))", RERegex.Parse(@"\w\wb").ToString());
			Assert.AreEqual("('a'(W('b')))", RERegex.Parse(@"a\Wb").ToString());
			Assert.AreEqual("(W(w('b')))", RERegex.Parse(@"\W\wb").ToString());
			
			Assert.AreEqual("('a'(s))", RERegex.Parse(@"a\s").ToString());
			Assert.AreEqual("('a'(S))", RERegex.Parse(@"a\S").ToString());
		}
		
		[Test]
		public void MatchLength2()
		{
			Assert.AreEqual(2, new RERegex("ab").MatchLength("ab"));
			Assert.AreEqual(-1, new RERegex("ab").MatchLength("aс"));
			Assert.AreEqual(2, new RERegex("a\\(b\\|c\\)").MatchLength("ab"));
			Assert.AreEqual(2, new RERegex("a\\(b\\|c\\)").MatchLength("ac"));
			Assert.AreEqual(-1, new RERegex("a\\(b\\|c\\)").MatchLength("ae"));
			
			Assert.AreEqual(3, new RERegex("a\\(b\\|cf\\)").MatchLength("acf"));
			Assert.AreEqual(2, new RERegex("a\\(bf\\|c\\)").MatchLength("acf"));
			Assert.AreEqual(-1, new RERegex("a\\(bf\\|c\\)").MatchLength("dbf"));
		}
		
		[Test]
		public void MatchLength_wW()
		{
			Assert.AreEqual(1, new RERegex("\\w").MatchLength("ab"));
			Assert.AreEqual(2, new RERegex("\\w\\w").MatchLength("ab"));
			Assert.AreEqual(2, new RERegex("\\w\\w").MatchLength("1a"));
			Assert.AreEqual(1, new RERegex("\\w").MatchLength("_a"));
			Assert.AreEqual(-1, new RERegex("\\w").MatchLength("-a"));
			Assert.AreEqual(-1, new RERegex("\\w").MatchLength(" a"));
			
			Assert.AreEqual(1, new RERegex("\\W").MatchLength("+ab"));
			Assert.AreEqual(1, new RERegex("\\W").MatchLength(" ab"));
			Assert.AreEqual(2, new RERegex("\\W\\W").MatchLength("+ "));
			Assert.AreEqual(-1, new RERegex("\\W\\W").MatchLength("+a"));
		}
		
		[Test]
		public void MatchLength_sS()
		{
			Assert.AreEqual(1, new RERegex("\\s").MatchLength(" a"));
			Assert.AreEqual(1, new RERegex("\\s").MatchLength("\tb"));
			Assert.AreEqual(-1, new RERegex("\\s").MatchLength("ab"));
			Assert.AreEqual(1, new RERegex("\\S").MatchLength("ab"));
			Assert.AreEqual(-1, new RERegex("\\S").MatchLength(" b"));
		}
		
		[Test]
		public void Parsing_dDxXoOhHpPaAlLuU()
		{
			Assert.AreEqual("(d(D(x(X(o(O(h(H(p(P(a(A(l(L(u(U))))))))))))))))",
				RERegex.Parse(@"\d\D\x\X\o\O\h\H\p\P\a\A\l\L\u\U").ToString());
		}
		
		[Test]
		public void MatchLength_dD()
		{
			Assert.AreEqual(1, new RERegex(@"\d").MatchLength("1"));
			Assert.AreEqual(-1, new RERegex(@"\d").MatchLength("a"));
			Assert.AreEqual(1, new RERegex(@"\D").MatchLength("a"));
			Assert.AreEqual(-1, new RERegex(@"\D").MatchLength("1"));
		}
		
		[Test]
		public void MatchLength_aA()
		{
			Assert.AreEqual(4, new RERegex(@"\a\a\a\a").MatchLength("abcd"));
			Assert.AreEqual(3, new RERegex(@"\A\A\A").MatchLength(" 2_"));
			Assert.AreEqual(-1, new RERegex(@"\a").MatchLength("1"));
			Assert.AreEqual(-1, new RERegex(@"\A").MatchLength("a"));
		}
		
		[Test]
		public void MatchLength_hH()
		{
			Assert.AreEqual(4, new RERegex(@"\h\h\h\h").MatchLength("A_bz"));
			Assert.AreEqual(5, new RERegex(@"\H\H\H\H\H").MatchLength("-2! ."));
			Assert.AreEqual(-1, new RERegex(@"\h").MatchLength("-"));
			Assert.AreEqual(-1, new RERegex(@"\h").MatchLength(" "));
			Assert.AreEqual(-1, new RERegex(@"\H").MatchLength("a"));
		}
		
		[Test]
		public void MatchLength_lL()
		{
			Assert.AreEqual(4, new RERegex(@"\l\l\l\l").MatchLength("abcd"));
			Assert.AreEqual(3, new RERegex(@"\L\L\L").MatchLength("A2_"));
			Assert.AreEqual(-1, new RERegex(@"\l").MatchLength("A"));
			Assert.AreEqual(-1, new RERegex(@"\l").MatchLength("1"));
			Assert.AreEqual(-1, new RERegex(@"\L").MatchLength("a"));
		}
		
		[Test]
		public void MatchLength_oO()
		{
			Assert.AreEqual(9, new RERegex(@"\o\o\o\o\oa\o\o\o").MatchLength("01234a567"));
			Assert.AreEqual(3, new RERegex(@"\O\O\O").MatchLength("89a"));
			Assert.AreEqual(-1, new RERegex(@"\o").MatchLength("8"));
			Assert.AreEqual(-1, new RERegex(@"\o").MatchLength("a"));
			Assert.AreEqual(-1, new RERegex(@"\O").MatchLength("7"));
		}
		
		[Test]
		public void MatchLength_pP()
		{
			Assert.AreEqual(6, new RERegex(@"\p\p\p\p\p\p").MatchLength("a -?12"));
			Assert.AreEqual(4, new RERegex(@"\P\P\P\P").MatchLength("a -?"));
			Assert.AreEqual(-1, new RERegex(@"\p").MatchLength("\t"));
			Assert.AreEqual(-1, new RERegex(@"\P").MatchLength("\t"));
			Assert.AreEqual(-1, new RERegex(@"\p").MatchLength("\n"));
			Assert.AreEqual(-1, new RERegex(@"\P").MatchLength("\n"));
			Assert.AreEqual(-1, new RERegex(@"\P").MatchLength("\r"));
			Assert.AreEqual(-1, new RERegex(@"\P").MatchLength("\r"));
			Assert.AreEqual(-1, new RERegex(@"\P").MatchLength("2"));
		}
		
		[Test]
		public void MatchLength_uU()
		{
			Assert.AreEqual(4, new RERegex(@"\u\u\u\u").MatchLength("ABCD"));
			Assert.AreEqual(3, new RERegex(@"\U\U\U").MatchLength("a2_"));
			Assert.AreEqual(-1, new RERegex(@"\u").MatchLength("a"));
			Assert.AreEqual(-1, new RERegex(@"\u").MatchLength("1"));
			Assert.AreEqual(-1, new RERegex(@"\U").MatchLength("A"));
		}
		
		[Test]
		public void MatchLength_xX()
		{
			Assert.AreEqual(19, new RERegex(@"\x\x\x\xa\x\x\x\x\xb\x\x\x\x\x\x\x\x").MatchLength("0123a45678b90aBcdeF"));
			Assert.AreEqual(17, new RERegex(@"\x\x\x\x\x\x\x\x\x\x\x\x\x\x\x\x\x").MatchLength("0123a45678b90abcdef"));
			Assert.AreEqual(17, new RERegex(@"\x\x\x\x\x\x\x\x\x\x\x\x\x\x\x\x\x").MatchLength("0123a45678b90ABCDEF"));
			Assert.AreEqual(8, new RERegex(@"\X\X\X\X\X\X\X\X").MatchLength("ghiGH .z"));
			Assert.AreEqual(-1, new RERegex(@"\x").MatchLength("g"));
			Assert.AreEqual(-1, new RERegex(@"\x").MatchLength("-"));
			Assert.AreEqual(-1, new RERegex(@"\X").MatchLength("a"));
		}
		
		[Test]
		public void MatchLength_SDXOHWALU_NotLineBreak()
		{
			Assert.AreEqual(1, new RERegex(@"\S\|\D").MatchLength(" "));
			Assert.AreEqual(1, new RERegex(@"\S\|\D").MatchLength("1"));
			
			Assert.AreEqual(-1, new RERegex(@"\s").MatchLength("\n"));
			
			Assert.AreEqual(-1, new RERegex(@"\S\|\D").MatchLength("\n"));
			Assert.AreEqual(-1, new RERegex(@"\S\|\D").MatchLength("\r"));
			
			Assert.AreEqual(-1, new RERegex(@"\A\|\H\|\L\|\O\|\U\|\W\|\X").MatchLength("\n"));
			Assert.AreEqual(-1, new RERegex(@"\A\|\H\|\L\|\O\|\U\|\W\|\X").MatchLength("\r"));
		}
		
		[Test]
		public void Parsing_Dot()
		{
			Assert.AreEqual("(.('a'))", RERegex.Parse(@".a").ToString());
		}
		
		[Test]
		public void MatchLength_Dot()
		{
			Assert.AreEqual(4, new RERegex(@"....").MatchLength("a- 2"));
			Assert.AreEqual(-1, new RERegex(@".").MatchLength("\n"));
			Assert.AreEqual(-1, new RERegex(@".").MatchLength("\r"));
		}
	}
}
