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
	}
}
