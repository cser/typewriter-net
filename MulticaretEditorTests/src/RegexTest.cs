using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	public class RegexTest
	{
		/*[Test]
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
		}*/
			
		[Test]
		public void Parsing_Chars()
		{
			Assert.AreEqual("(0'a':1)(1'b')", new RERegex(@"ab").ToGraphString());
			Assert.AreEqual("(0'a':1)(1'b':2)(2'c')", new RERegex(@"abc").ToGraphString());
			Assert.AreEqual("(0'a')", new RERegex(@"a").ToGraphString());
		}
		
		[Test]
		public void Parsing_Alternate()
		{
			Assert.AreEqual("(0o:1|2)(1'a':3)(2'b':3)(3~)",
				new RERegex(@"a\|b").ToGraphString());
			Assert.AreEqual("(0o:1|2)(1'a':3)(2'b':4)(3'b':5)(4'c':5)(5~)",
				new RERegex(@"ab\|bc").ToGraphString());
			Assert.AreEqual("(0o:1|2)(1'f':3)(2o:4|5)(3~)(4'a':6)(5'b':7)(6'b':3)(7'c':3)",
				new RERegex(@"f\|ab\|bc").ToGraphString());
		}
		/*
		[Test]
		public void Parsing_AlternateBrackets()
		{
			Assert.AreEqual("('a'('('('b'('|'('c'(')'('d')))))))", new RERegex(@"a(b|c)d").ToGraphString());
			Assert.AreEqual("('a'(('b')|('c')`('d')))", new RERegex(@"a\(b\|c\)d").ToGraphString());
		}
		
		[Test]
		public void Parsing_wWsS()
		{
			Assert.AreEqual("('a'(w('b')))", new RERegex(@"a\wb").ToGraphString());
			Assert.AreEqual("(w(w('b')))", new RERegex(@"\w\wb").ToGraphString());
			Assert.AreEqual("('a'(W('b')))", new RERegex(@"a\Wb").ToGraphString());
			Assert.AreEqual("(W(w('b')))", new RERegex(@"\W\wb").ToGraphString());
			
			Assert.AreEqual("('a'(s))", new RERegex(@"a\s").ToGraphString());
			Assert.AreEqual("('a'(S))", new RERegex(@"a\S").ToGraphString());
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
				new RERegex(@"\d\D\x\X\o\O\h\H\p\P\a\A\l\L\u\U").ToGraphString());
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
			Assert.AreEqual("(.('a'))", new RERegex(@".a").ToGraphString());
			Assert.AreEqual("('.'('a'))", new RERegex(@"\.a").ToGraphString());
		}
		
		[Test]
		public void MatchLength_Dot()
		{
			Assert.AreEqual(4, new RERegex(@"....").MatchLength("a- 2"));
			Assert.AreEqual(-1, new RERegex(@".").MatchLength("\n"));
			Assert.AreEqual(-1, new RERegex(@".").MatchLength("\r"));
		}
		
		[Test]
		public void Parsing_Star()
		{
			Assert.AreEqual("('a'('*'))", new RERegex(@"a\*").ToGraphString());
			Assert.AreEqual("(('a')*)", new RERegex(@"a*").ToGraphString());
			Assert.AreEqual("((('a'('b'))|('c'))*)", new RERegex(@"\(ab\|c\)*").ToGraphString());
		}
		
		[Test]
		public void Parsing_Star2()
		{
			Assert.AreEqual("('a'(('b')*))", new RERegex(@"ab*").ToGraphString());
		}
		
		[Test]
		public void MatchLength_NotFull()
		{
			Assert.AreEqual(2, new RERegex(@"xy").MatchLength("xyz123"));
			Assert.AreEqual(2, new RERegex(@"\w\w").MatchLength("xyz123"));
		}
		
		[Test]
		public void MatchLength_Alternate_NotFull()
		{
			Assert.AreEqual(2, new RERegex(@"xy\|abc").MatchLength("xyabcdef"));
 			Assert.AreEqual(3, new RERegex(@"xy\|abc").MatchLength("abcdef"));
		}
		
		[Test]
		public void MatchLength_Alternate_NotFull2()
		{
 			Assert.AreEqual(3, new RERegex(@"xy\|bcd\|abc").MatchLength("abcdef"));
		}
		
		[Test]
		public void MatchLength_Alternate_NotFull3()
		{
 			Assert.AreEqual(3, new RERegex(@"xy\|abd\|abc").MatchLength("abcdef"));
		}
		
		[Test]
		public void MatchLength_Star()
		{
			Assert.AreEqual(3, new RERegex(@"a*").MatchLength("aaa"));
		}
		
		[Test]
		public void MatchLength_Star2()
		{
			Assert.AreEqual(3, new RERegex(@"a*").MatchLength("aaabacdef"));
		}
		
		[Test]
		public void MatchRegExpr()
		{
			Assert.AreEqual(2, new RegExpr("a*").Search("bbaaa"));
		}*/
	}
}
