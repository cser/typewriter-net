using System;
using NUnit.Framework;
using MulticaretEditor;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;

namespace UnitTests
{
	[TestFixture]
	public class HighlightingTest
	{
		private LineArray provider;
		private Highlighter highlighting;
		
		private void Init(string xmlText)
		{
			provider = new LineArray();
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(xmlText);
			Raw raw = Raw.Parse(xml);
			Raw.PrefixContexts(raw, "prefix_");
			Raw.InlineIncludeRules(raw, new HighlighterSet());
			highlighting = new Highlighter(raw);
		}
		
		private void AssertHighlighting(string expected, Line line)
		{
			StringBuilder got = new StringBuilder();
			for (int i = 0; i < line.charsCount; i++)
			{
				got.Append(line.chars[i].style + "");
			}
			Assert.AreEqual(expected, got.ToString(), "\"" + line.Text + "\"");
		}
		
		[Test]
		public void Keywords0()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <list name='keywords1'>
				            <item>word10</item>
				            <item>word11</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='Keyword0' context='#stay' String='keywords0'/>
				                <keyword attribute='Keyword1' context='#stay' String='keywords1'/>        
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='Keyword0' defStyleNum='dsKeyword'/>
				            <itemData name='Keyword1' defStyleNum='dsDataType'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"text word0 word11 text word10\n" +
				"word1 text text");
			highlighting.Parse(provider);
			//                  text word0 word11 text word10N
			AssertHighlighting("000001111102222220000002222220", provider[0]);
			//                  word1 text text
			AssertHighlighting("111110000000000", provider[1]);
		}
		
		[Test]
		public void Keywords1()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <list name='keywords1'>
				            <item>word10</item>
				            <item>word11</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='Keyword0' context='#stay' String='keywords0'/>
				                <keyword attribute='Keyword1' context='#stay' String='keywords1'/>        
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='Keyword0' defStyleNum='dsKeyword'/>
				            <itemData name='Keyword1' defStyleNum='dsDataType'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText("text word0, word11.text word10;");
			highlighting.Parse(provider);
			//                  text word0, word11.text word10;
			AssertHighlighting("0000011111002222220000002222220", provider[0]);
		}
		
		[Test]
		public void Comments_Multiline()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='Keyword0' context='#stay' String='keywords0'/>
				                <Detect2Chars attribute='Comment' context='Comment2' char='/' char1='*'/>  
				            </context>
				            <context attribute='Comment' lineEndContext='#stay' name='Comment2'>
				                <Detect2Chars attribute='Comment' context='#pop' char='*' char1='/'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='Keyword0' defStyleNum='dsKeyword'/>
				            <itemData name='Comment' defStyleNum='dsDataType'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"text word0, text /*word10;\n" +
				"text word11*/word1 text text");
			highlighting.Parse(provider);
			//                  text word0, text /*word10;N
			AssertHighlighting("000001111100000002222222222", provider[0]);
			//                  text word11*/word1 text text
			AssertHighlighting("2222222222222111110000000000", provider[1]);
		}
		
		private void AssertParseSwitch(int expectedPops, string expectedContextName, string text)
		{
			int pops;
			string contextName;
			Highlighter.ParseSwitch(text, out pops, out contextName);
			Assert.True(
				expectedPops == pops && expectedContextName == contextName,
				string.Format("Expected: {0}, {1}, got: {2}, {3}", expectedPops, expectedContextName, pops, contextName));
		}
		
		[Test]
		public void ParseSwitch0()
		{
			AssertParseSwitch(0, null, "#stay");
			AssertParseSwitch(1, null, "#pop");
			AssertParseSwitch(0, "State name", "State name");
		}
		
		[Test]
		public void ParseSwitch1()
		{
			AssertParseSwitch(0, null, "");
			AssertParseSwitch(0, null, "#");
			AssertParseSwitch(0, null, "#stay#");
			AssertParseSwitch(0, null, null);
		}
		
		[Test]
		public void ParseSwitch2()
		{
			AssertParseSwitch(1, "State name", "#pop!State name");
			AssertParseSwitch(2, "State name", "#pop#pop!State name");
		}
		
		[Test]
		public void Comments_Singleline()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='Keyword0' context='#stay' String='keywords0'/>
				                <DetectChar attribute='Comment' context='Comment1' char='#' />
				            </context>
				            <context attribute='Comment' lineEndContext='#pop' name='Comment1'/>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='Keyword0' defStyleNum='dsKeyword'/>
				            <itemData name='Comment' defStyleNum='dsDataType'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"word0 word word1 text#word0 text\n" +
				"text text word1\n" +
				"#comment\n" +
				"text");
			highlighting.Parse(provider);
			//                  word0 word word1 text#word0 textN
			AssertHighlighting("111110000001111100000222222222222", provider[0]);
			//                  text text word1N
			AssertHighlighting("0000000000111110", provider[1]);
			//                  #commentN
			AssertHighlighting("222222222", provider[2]);
			//                  text
			AssertHighlighting("0000", provider[3]);
		}
		
		[Test]
		public void AnyChar()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='Keyword0' context='#stay' String='keywords0'/>
				                <AnyChar attribute='Symbol' context='#stay' String=':!%&amp;+,-/.*&lt;=&gt;?[]|~^&#59;'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='Keyword0' defStyleNum='dsKeyword'/>
				            <itemData name='Symbol' defStyleNum='dsDataType'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"word0 text++ word1 <text>(items) + text.value\n" +
				"--word1--");
			highlighting.Parse(provider);
			//                  word0 text++ word1 <text>(items) + text.valueN
			AssertHighlighting("1111100000220111110200002000000002000002000000", provider[0]);
			//                  --word1--
			AssertHighlighting("221111122", provider[1]);
		}
		
		[Test]
		public void StringDetect()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='Keyword0' context='#stay' String='keywords0'/>
				                <StringDetect attribute='Function1' context='#stay' String=':() - 1' />
				                <StringDetect attribute='Function0' context='#stay' String=':()' />
				                <StringDetect attribute='Function2' context='#stay' String=':() - 2' />
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='Keyword0' defStyleNum='dsKeyword'/>
				            <itemData name='Function0' defStyleNum='dsDataType'/>
				            <itemData name='Function1' defStyleNum='dsDecVal'/>
				            <itemData name='Function2' defStyleNum='dsBaseN'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"function:() - 1\n" +
				"function:()\n" +
				"function:() - 2\n" +
				"word1:() text");
			highlighting.Parse(provider);
			//                  function:() - 1N
			AssertHighlighting("0000000033333330", provider[0]);
			//                  function:()N
			AssertHighlighting("000000002220", provider[1]);
			//                  function:() - 2N
			AssertHighlighting("0000000022200000", provider[2]);
			//                  word1:() text
			AssertHighlighting("1111122200000", provider[3]);
		}
		
		[Test]
		public void StringDetect_Insencitive()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='Keyword0' context='#stay' String='keywords0'/>
				                <StringDetect attribute='string0' context='#stay' String='sensitive' />
				                <StringDetect attribute='string1' insensitive='true' context='#stay' String='insensitive' />
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='Keyword0' defStyleNum='dsKeyword'/>
				            <itemData name='string0' defStyleNum='dsDataType'/>
				            <itemData name='string1' defStyleNum='dsDecVal'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText("text word1 sensitive insensitive InSensitive Sensitive");
			highlighting.Parse(provider);
			//                  text word1 sensitive insensitive InSensitive Sensitive
			AssertHighlighting("000001111102222222220333333333330333333333330000000000", provider[0]);
		}
		
		[Test]
		public void RegExpr()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='Keyword0' context='#stay' String='keywords0'/>
				                <RegExpr attribute='RegExpr0' context='#stay' String='\d+' />
				                <RegExpr attribute='RegExpr1' context='#stay' String='[\w\d]+\send' />
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='Keyword0' defStyleNum='dsKeyword'/>
				            <itemData name='RegExpr0' defStyleNum='dsDataType'/>
				            <itemData name='RegExpr1' defStyleNum='dsDecVal'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText("word0 abc end text 123a end10 word0 11 end");
			highlighting.Parse(provider);
			//                  word0 abc end text 123a end10 word0 11 end
			AssertHighlighting("111110333333300000022233333220111110220000", provider[0]);
		}
		
		[Test]
		public void RegExpr_InsensitiveAndMinimal()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='Keyword0' context='#stay' String='keywords0'/>
				                <RegExpr attribute='RegExpr0' context='#stay' insensitive='true' String='abc\d' />
				                <RegExpr attribute='RegExpr1' context='#stay' String='def\d' />
				                <RegExpr attribute='RegExpr2' context='#stay' minimal='true' String='#f+\]f+' />
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='Keyword0' defStyleNum='dsKeyword'/>
				            <itemData name='RegExpr0' defStyleNum='dsDataType'/>
				            <itemData name='RegExpr1' defStyleNum='dsDecVal'/>
				            <itemData name='RegExpr2' defStyleNum='dsBaseN'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText("abc1 ABC1 def2 DEF2 #fff]f text #f]f word0 text #fff]ffff text");
			highlighting.Parse(provider);
			//                  abc1 ABC1 def2 DEF2 #fff]f text #f]f word0 text #fff]ffff text
			AssertHighlighting("22220222203333000000444444000000444401111100000044444400000000", provider[0]);
		}
		
		[Test]
		public void Int()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <Int attribute='a2' context='#stay'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"124123 word1 12a a12 -1.0 2.1f .1*2/3 0x10 00100\n" +
				"(1)/2/[3,4,5];6;7:8 '9' 0'' $123 123$ \\1\\ \"1\"");
			highlighting.Parse(provider);
			//                  124123 word1 12a a12 -1.0 2.1f .1*2/3 0x10 00100N
			AssertHighlighting("2222220111110220000000202020200020202020000222220", provider[0]);
			//                  (1)/2/[3,4,5];6;7:8 '9' 0'' $123 123$ \1\ "1"
			AssertHighlighting("020020020202002020200000200000000222000200000", provider[1]);

			provider.SetText("124123+1=124124");
			highlighting.Parse(provider);
			//                  124123+1=124124
			AssertHighlighting("222222020222222", provider[0]);
		}
		
		[Test]
		public void Int_StringDetect()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <Int attribute='a2' context='#stay'>
				                    <StringDetect attribute='a2' context='#stay' String='L' insensitive='true'/>
				                    <StringDetect attribute='a2' context='#stay' String='Fl'/>
				                </Int>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"123l;l 123L text word0 123L; 10 text\n" +
				"123Fl 12323L 234324F 234Fl 123FL");
			highlighting.Parse(provider);
			//                  123l;l 123L text word0 123L; 10 textN
			AssertHighlighting("2222000222200000011111022220022000000", provider[0]);
			//                  123Fl 12323L 234324F 234Fl 123FL
			AssertHighlighting("22222022222202222220022222022200", provider[1]);
		}
		
		[Test]
		public void Float()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <Float attribute='a2' context='#stay'>
									<AnyChar String='fF' attribute='a2' context='#stay'/>
				                </Float>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"1.0 1.0f 123.2F .1 .1f word0 0.1text text0.1 123 123.4567 123.4567f 123f\n" +
				"\"11.0\" 11.0\" \" '0.1' 0.1'' 0.123.45 text 0.123f.123 0.123f. 0..1 0...1");
			highlighting.Parse(provider);
			//                  1.0 1.0f 123.2F .1 .1f word0 0.1text text0.1 123 123.4567 123.4567f 123fN
			AssertHighlighting("2220222202222220220222011111022200000000000000000222222220222222222000000", provider[0]);
			//                  "11.0" 11.0" " '0.1' 0.1'' 0.123.45 text 0.123f.123 0.123f. 0..1 0...1
			AssertHighlighting("0000000222200000000002220002222200000000022222200000222222002222022022", provider[1]);
		}
		
		[Test]
		public void HlCOct()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <HlCOct attribute='a2' context='#stay'/>
				                <Int attribute='a3' context='#stay'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				            <itemData name='a3' defStyleNum='dsDecVal'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"01234 234 01334 01234.534 0123456789 word0 123 980123 text0123 0123text 07 08\n" +
				"0 00 000 01 \"01\" '01' 01\"\" 01'' :01 \"\"01;01.001");
			highlighting.Parse(provider);
			//                  01234 234 01334 01234.534 0123456789 word0 123 980123 text0123 0123text 07 08N
			AssertHighlighting("222220333022222022222033302222222200011111033303333330000000000222200000220330", provider[0]);
			//                  0 00 000 01 "01" '01' 01"" 01'' :01 ""01;01.001
			AssertHighlighting("30220222022000000000002200022000022000000220222", provider[1]);
		}
		
		[Test]
		public void HlCText()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <HlCHex attribute='a2' context='#stay'/>
				                <Int attribute='a3' context='#stay'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				            <itemData name='a3' defStyleNum='dsDecVal'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"1234 word0 0x123 0x 0x0 0xA 0xF 0xG 0x1234890ABCDEF123 0xEFG text0xA 0xAtext\n" +
				"1234 word0 0X123 0x 0x0 0xa 0Xf 0xg 0x1234890aBcdeF123 0xefg text0xa 0xatext\n" +
				"\"0x1 \" '0x1' 0x1\" \" 0x1'' :0x1 text;0x1.0xC1234F01");
			highlighting.Parse(provider);
			//                  1234 word0 0x123 0x 0x0 0xA 0xF 0xG 0x1234890ABCDEF123 0xEFG text0xA 0xAtextN
			AssertHighlighting("33330111110222220300222022202220300022222222222222222202222000000000022200000", provider[0]);
			//                  1234 word0 0X123 0x 0x0 0xa 0Xf 0xg 0x1234890aBcdeF123 0xefg text0xa 0xatextN
			AssertHighlighting("33330111110222220300222022202220300022222222222222222202222000000000022200000", provider[1]);
			//                  "0x1 " '0x1' 0x1" " 0x1'' :0x1 text;0x1.0xC1234F01
			AssertHighlighting("00000000000002220000222000022200000022202222222222", provider[2]);
		}
		
		[Test]
		public void RangeDetect()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <RangeDetect attribute='a2' context='#stay' char='(' char1=')' />
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"(text text) text(sdf(sdf) word0 (text word0 123) text(123))text ()\n" +
				"(asdf text(text word0\n" +
				"text text)");
			highlighting.Parse(provider);
			//                  (text text) text(sdf(sdf) word0 (text word0 123) text(123))text ()N
			AssertHighlighting("2222222222200000222222222011111022222222222222220000022222000000220", provider[0]);
			//                  (asdf text(text word0N
			AssertHighlighting("0000000000000000111110", provider[1]);
			//                  text text)
			AssertHighlighting("0000000000", provider[2]);
		}
		
		[Test]
		public void DetectSpaces_Normal()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <DetectSpaces attribute='a2' context='context1'/>
				            </context>
				            <context attribute='a3' lineEndContext='#stay' name='context1'>
				                <keyword attribute='a1' context='#pop' String='keywords0'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				            <itemData name='a3' defStyleNum='dsDecVal'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText("text     text 123 word0;text 123 word1:123\t123");
			highlighting.Parse(provider);
			//                  text     text 123 word0;text 123 word1:123 123
			AssertHighlighting("0000222223333333331111100000233331111100002333", provider[0]);
		}
		
		[Test]
		public void DetectSpaces_WithoutAttribute()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <DetectSpaces context='context1'/>
				            </context>
				            <context attribute='a3' lineEndContext='#stay' name='context1'>
				                <keyword attribute='a1' context='#pop' String='keywords0'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				            <itemData name='a3' defStyleNum='dsDecVal'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText("text     text 123 word0;text 123 word1:123\t123");
			highlighting.Parse(provider);
			//                  text     text 123 word0;text 123 word1:123 123
			AssertHighlighting("0000000003333333331111100000033331111100000333", provider[0]);
		}
		
		[Test]
		public void DetectSpaces_WithoutContext()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <DetectSpaces attribute='a2'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText("text     text 123 word0;text 123 word1:123\t123");
			highlighting.Parse(provider);
			//                  text     text 123 word0;text 123 word1:123 123
			AssertHighlighting("0000222220000200021111100000200021111100002000", provider[0]);
		}
		
		[Test]
		public void DetectIdentifier()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <DetectIdentifier attribute='a2' context='#stay'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"word0 text 123 text_123 1a a1 *Text,TEXT;t_e_X_t a-0 a+1 a/1 field:value _a _\n" +
				"\"text\" text\"\" 'text' text''");
			highlighting.Parse(provider);
			//                  word0 text 123 text_123 1a a1 *Text,TEXT;t_e_X_t a-0 a+1 a/1 field:value _a _N
			AssertHighlighting("111110222200000222222220000220022220222202222222020002000200022222022222022020", provider[0]);
			//                  "text" text"" 'text' text''
			AssertHighlighting("000000022220000000000222200", provider[1]);
		}
		
		[Test]
		public void HlCStringChar()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <DetectChar attribute='a2' context='String' char='&quot;'/>
				            </context>
				            <context attribute='a2' lineEndContext='#pop' name='String'>
				                <HlCStringChar attribute='a3' context='#stay'/>
				                <DetectChar attribute='a2' context='#pop' char='&quot;'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				            <itemData name='a3' defStyleNum='dsDecVal'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				@"word1 ""text"" word0 ""tex\'t text\"" text world1"" text ""text\ntext\t a\\b; \f\b\e"" text\t ""abc""" + "\n" +
				@"""_\t_\0000_\x_\x0_\x00_\x000_"" text ""\xCG_\xcf_\xcfa_\Xcf_\021_\091_\123_\137_\138_\1_\0""");
			highlighting.Parse(provider);
			//                  word1 "text" word0 "tex\'t text\" text world1" text "text\ntext\t a\\b; \f\b\e" text\t "abc"N
			AssertHighlighting("111110222222011111022223322222233222222222222200000022222332222332233222333333200000000222220", provider[0]);
			//                  "_\t_\0000_\x_\x0_\x00_\x000_" text "\xCG_\xcf_\xcfa_\Xcf_\021_\091_\123_\137_\138_\1_\0"
			AssertHighlighting("22332333322222333233332333322200000023332233332333322222223333233222333323333233322332332", provider[1]);
		}
		
		[Test]
		public void HlCChar()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <HlCChar attribute='a2' context='#stay'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				@"word1 '' word0 'a' text 'ab' text '0' '\0' text '\f' '\c' text '0'" + "\n" +
				@"''a'' text'\a';text ""'\a'"" ''' text ' '" + "\n" +
				@"'\012' '\0' '\01' '\8'; '\7'; '\78'; '\0123'; '\'" + "\n" +
				@"'\x00'; '\X00'; '\xAB'; '\xAF'; '\xAG'; '\xABC'; '\xab'; '\xA'; '\x'; '\x09';");
			highlighting.Parse(provider);
			//                  word1 '' word0 'a' text 'ab' text '0' '\0' text '\f' '\c' text '0'N
			AssertHighlighting("1111100001111102220000000000000000222022220000002222000000000002220", provider[0]);
			//                  ''a'' text'\a';text "'\a'" ''' text ' 'N
			AssertHighlighting("0222000000222200000002222000000000002220", provider[1]);
			//                  '\012' '\0' '\01' '\8'; '\7'; '\78'; '\0123'; '\'N
			AssertHighlighting("22222202222022222000000022220000000000000000000000", provider[2]);
			//                  '\x00'; '\X00'; '\xAB'; '\xAF'; '\xAG'; '\xABC'; '\xab'; '\xA'; '\x'; '\x09';
			AssertHighlighting("22222200000000002222220022222200000000000000000002222220022222000000002222220", provider[3]);
		}
		
		[Test]
		public void LineContinue()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <LineContinue attribute='a2' context='NextLine'/>
				            </context>
				            <context attribute='a3' lineEndContext='#stay' name='NextLine'>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				            <itemData name='a3' defStyleNum='dsDecVal'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				@"text \ text" + "\n" +
				@"text word0 text \ " + "\n" +
				@"text word0 text \" + "\n" +
				@"new line text");
			highlighting.Parse(provider);
			//                  text \ textN
			AssertHighlighting("000000000000", provider[0]);
			//                  text word0 text \ N
			AssertHighlighting("0000011111000000000", provider[1]);
			//                  text word0 text \N
			AssertHighlighting("000001111100000022", provider[2]);
			//                  new line text
			AssertHighlighting("3333333333333", provider[3]);
		}
		
		[Test]
		public void LineContinue2()
		{
			Init(@"<language name='test' extensions='*.test'>
				<highlighting>
					<contexts>
						<context attribute='a0' lineEndContext='#stay' name='Normal'>
							<DetectChar char='&quot;' context='String' attribute='a1'/> 
						</context>
						<context attribute='a1' lineEndContext='#pop' name='String'>
							<LineContinue context='#stay'/>
                            <DetectChar attribute='a1' context='#pop' char='&quot;'/>
						</context>
					</contexts>
					<itemDatas>
						<itemData name='a0' defStyleNum='dsNormal'/>
						<itemData name='a1' defStyleNum='dsKeyword'/>
					</itemDatas>
				</highlighting>
			</language>");
			provider.SetText("ab a \"line0\\\nline1\" cdef");
			highlighting.Parse(provider);
			//                  ab a "line0\N
			AssertHighlighting("0000011111111", provider[0]);
			//                  line1" cdef
			AssertHighlighting("11111100000", provider[1]);
		}
		
		[Test]
		public void LineContinue3()
		{
			Init(@"<language name='test' extensions='*.test'>
				<highlighting>
					<contexts>
						<context attribute='a0' lineEndContext='#stay' name='Normal'>
							<DetectChar char='&quot;' context='String' attribute='a1'/> 
						</context>
						<context attribute='a1' lineEndContext='#pop' name='String'>
							<LineContinue context='#stay'/>
                            <DetectChar attribute='a1' context='#pop' char='&quot;'/>
						</context>
					</contexts>
					<itemDatas>
						<itemData name='a0' defStyleNum='dsNormal'/>
						<itemData name='a1' defStyleNum='dsKeyword'/>
					</itemDatas>
				</highlighting>
			</language>");
			provider.SetText("ab a \"line0\\\r\nline1\" cdef");
			highlighting.Parse(provider);
			//                  ab a "line0\N
			AssertHighlighting("00000111111111", provider[0]);
			//                  line1" cdef
			AssertHighlighting("11111100000", provider[1]);
		}
		
		[Test]
		public void Fallthrough()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'
				                fallthrough='true' fallthroughContext='Context1'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <DetectSpaces attribute='a2' context='#stay'/>
				            </context>
				            <context attribute='a3' lineEndContext='#pop' name='Context1'/>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				            <itemData name='a3' defStyleNum='dsDecVal'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"word0 text\n" +
				"word0 word1 word0 word1\n" +
				"word1 word0 text word0 word1");
			highlighting.Parse(provider);
			//                  word0 textN
			AssertHighlighting("11111233333", provider[0]);
			//                  word0 word1 word0 word1N
			AssertHighlighting("111112111112111112111112", provider[1]);
			//                  word1 word0 text word0 word1
			AssertHighlighting("1111121111123333333333333333", provider[2]);
		}
		
		[Test]
		public void LookAhead()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <StringDetect attribute='a2' context='Context1' String='!#--#--' lookAhead='true' />
				            </context>
				            <context attribute='a3' lineEndContext='#pop' name='Context1'>
				                <DetectChar attribute='a4' context='#stay' char='#'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				            <itemData name='a3' defStyleNum='dsDecVal'/>
				            <itemData name='a4' defStyleNum='dsBaseN'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"text word1 !#--#-- #text #\n" +
				"text word1 !#--#- #text #");
			highlighting.Parse(provider);
			//                  text word1 !#--#-- #text #N
			AssertHighlighting("000001111103433433343333343", provider[0]);
			//                  text word1 !#--#- #text #
			AssertHighlighting("0000011111000000000000000", provider[1]);
		}
		
		[Test]
		public void FirstNonSpace()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='a0' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <StringDetect attribute='a2' context='#stay' String='string for detection' firstNonSpace='true'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"text word0\n" +
				"string for detectiontext\n" +
				"string for detection word1\n" +
				"()string for detection\n" +
				"textstring for detection word0\n" +
				"text string for detection");
			highlighting.Parse(provider);
			//                  text word0N
			AssertHighlighting("00000111110", provider[0]);
			//                  string for detectiontextN
			AssertHighlighting("2222222222222222222200000", provider[1]);
			//                  string for detection word1N
			AssertHighlighting("222222222222222222220111110", provider[2]);
			//                  ()string for detectionN
			AssertHighlighting("00000000000000000000000", provider[3]);
			//                  textstring for detection word0N
			AssertHighlighting("0000000000000000000000000111110", provider[4]);
			//                  text string for detection
			AssertHighlighting("0000000000000000000000000", provider[5]);
		}
		
		[Test]
		public void Column()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='a0' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				                <StringDetect attribute='a2' context='#stay' String='#!' column='10'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"0123456789#!123456789#!\n" +
				"#!234#!789#0123456789#!\n" +
				"#1234    #! word0\n" +
				"#   #   #\t#!# word1 text");
			highlighting.Parse(provider);
			//                  0123456789#!123456789#!N
			AssertHighlighting("000000000022000000000000", provider[0]);
			//                  #!234#!789#0123456789#!N
			AssertHighlighting("000000000000000000000000", provider[1]);
			//                  #1234    #! word0N
			AssertHighlighting("000000000000111110", provider[2]);
			//                  #   #   # #!# word1 text
			AssertHighlighting("000000000022001111100000", provider[3]);
		}
		
		[Test]
		public void IncludeRules()
		{
			Init(@"<language name='test' extensions='*.test'>
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='a0' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
								<IncludeRules context='IncludeContext'/>
				            </context>
							<context attribute='a2' lineEndContext='#stay' name='IncludeContext'>
								<StringDetect attribute='a3' context='#stay' String='include'/>
							</context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				            <itemData name='a3' defStyleNum='dsDecVal'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(  "word0 text1 include text2 include word1");
			highlighting.Parse(provider);
			AssertHighlighting("111110000000333333300000003333333011111", provider[0]);
		}
		
		[Test]
		public void KeywordsCasesensitive()
		{
			Init(@"<language name='test' extensions='*.test'>
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='a0' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				        </itemDatas>
				    </highlighting>
				    <general>
						<keywords casesensitive='1' />
					</general>
				</language>");
			provider.SetText(  "text word0 text Word1 text word1 text");
			highlighting.Parse(provider);
			AssertHighlighting("0000011111000000000000000001111100000", provider[0]);
		}
		
		[Test]
		public void KeywordsNoncasesensitive()
		{
			Init(@"<language name='test' extensions='*.test'>
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='a0' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				        </itemDatas>
				    </highlighting>
				    <general>
						<keywords casesensitive='0' />
					</general>
				</language>");
			provider.SetText(  "text word0 text WorD1 text word1 text");
			highlighting.Parse(provider);
			AssertHighlighting("0000011111000000111110000001111100000", provider[0]);
		}
		
		[Test]
		public void KeywordsUnderline()
		{
			Init(@"<language name='test' extensions='*.test'>
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='a0' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				        </itemDatas>
				    </highlighting>
				    <general>
						<keywords casesensitive='0' />
					</general>
				</language>");
			provider.SetText(  "text_word0 text word1_text word0 text");
			highlighting.Parse(provider);
			AssertHighlighting("0000000000000000000000000001111100000", provider[0]);
		}
		
		[Test]
		public void WeakDeliminator()
		{
			Init(@"<language name='test' extensions='*.test'> 
				<highlighting>
					<list name='keywords0'>
						<item>word-0</item>
						<item>word~1</item>
					</list>
					<contexts>
						<context attribute='a0' lineEndContext='#stay' name='Normal'>
							<keyword attribute='a1' context='#stay' String='keywords0'/>
						</context>
					</contexts>
					<itemDatas>
						<itemData name='a0' defStyleNum='dsNormal'/>
						<itemData name='a1' defStyleNum='dsKeyword'/>
					</itemDatas>
				</highlighting>
				<general>
					<keywords weakDeliminator='-~' />
				</general>
			</language>");
			provider.SetText(  "text word-0 text word~1");
			highlighting.Parse(provider);
			AssertHighlighting("00000111111000000111111", provider[0]);
		}
		
		[Test]
		public void AdditionalDeliminator()
		{
			Init(@"<language name='test' extensions='*.test'> 
				<highlighting>
					<list name='keywords0'>
						<item>word0</item>
						<item>word1</item>
					</list>
					<contexts>
						<context attribute='a0' lineEndContext='#stay' name='Normal'>
							<keyword attribute='a1' context='#stay' String='keywords0'/>
						</context>
					</contexts>
					<itemDatas>
						<itemData name='a0' defStyleNum='dsNormal'/>
						<itemData name='a1' defStyleNum='dsKeyword'/>
					</itemDatas>
				</highlighting>
				<general>
					<keywords additionalDeliminator='s' />
				</general>
			</language>");
			provider.SetText(  "text word0sword1 text word1");
			highlighting.Parse(provider);
			AssertHighlighting("000001111101111100000011111", provider[0]);
		}
		
		[Test]
		public void RegexTest()
		{
			Regex regex;
			
			regex = new Regex("^text$");
			Assert.AreEqual(false, regex.IsMatch("abcdtextabcd"));
			Assert.AreEqual(true, regex.IsMatch("text"));
			Assert.AreEqual(false, regex.IsMatch("abcdtext"));
			Assert.AreEqual(false, regex.IsMatch("abcdtext", 4));
			
			regex = new Regex("text$");
			Assert.AreEqual(false, regex.IsMatch("abcdtextabcd"));
			Assert.AreEqual(true, regex.IsMatch("text"));
			Assert.AreEqual(true, regex.IsMatch("abcdtext"));
			Assert.AreEqual(true, regex.IsMatch("abcdtext", 4));
			
			Assert.AreEqual(false, regex.IsMatch("abcdtexts\n"));
			Assert.AreEqual(false, regex.IsMatch("texts\n"));
			Assert.AreEqual(true, regex.IsMatch("abcdtext\n"));
		}
		
		[Test]
		public void DoubleLineEndSwitching()
		{
			Init(@"<language name='typewriter-help' version='1' kateversion='3.7' section='Markup' extensions='*.th' priority='15' author=' license='>
				<highlighting>
					<contexts>
						<context attribute='a0' lineEndContext='#stay' name='context0'>
							<DetectChar attribute='a1' context='context1' char='['/>
						</context>
						<context attribute='a1' lineEndContext='#pop' name='context1'>
							<Detect2Chars attribute='a2' context='context2' char='/' char1='/'/>
						</context>
						<context attribute='a2' lineEndContext='#pop' name='context2'/>
					</contexts>
					<itemDatas>
						<itemData name='a0' defStyleNum='dsNormal'/>
						<itemData name='a1' defStyleNum='dsKeyword'/>
						<itemData name='a2' defStyleNum='dsDataType'/>
					</itemDatas>
				</highlighting>
				<general>
				</general>
			</language>");
			provider.SetText("word0[word1//word2\nword3//word4\nword5");
			highlighting.Parse(provider);
			//                  word0[word1//word2
			AssertHighlighting("0000011111122222222", provider[0]);
			//                  word3//word4
			AssertHighlighting("0000000000000", provider[1]);
			//                  word5
			AssertHighlighting("00000", provider[2]);
		}
		
		[Test]
		public void Int_WordDetect()
		{
			Init(@"<language name='test' extensions='*.test'> 
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='Normal Text' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
								<WordDetect attribute='a3' context='#stay' String='Thing'/>
								<WordDetect attribute='a2' context='#stay' String='thinG' insensitive='true'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='Normal Text' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				            <itemData name='a3' defStyleNum='dsDecVal'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			provider.SetText(
				"123Thing;Thing text word0 Thing.thing 10 text\n" +
				"Thing 12323 word1 234(Thing) ThInG");
			highlighting.Parse(provider);
			//                  123Thing;Thing text word0 Thing.thing 10 text
			AssertHighlighting("0000000003333300000011111033333022222000000000", provider[0]);
			//                  Thing 12323 word1 234(Thing) ThInG
			AssertHighlighting("3333300000001111100000333330022222", provider[1]);
		}
		
		[Test]
		public void KeywordsCasesensitive_Complex()
		{
			Init(@"<language name='test' extensions='*.test'>
				    <highlighting>
				        <list name='keywords0'>
				            <item>abc</item>
				            <item>abcde</item>
				            <item>abcdef</item>
				            <item>abcdefg</item>
				        </list>
				        <contexts>
				            <context attribute='a0' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				        </itemDatas>
				    </highlighting>
				    <general>
						<keywords casesensitive='1' />
					</general>
				</language>");

			provider.SetText(  "ab abc abcc abcde abcdefgh abcdef abcdefg");
			highlighting.Parse(provider);
			AssertHighlighting("00011100000011111000000000011111101111111", provider[0]);
			
			provider.SetText(  "ab abe abcc abc");
			highlighting.Parse(provider);
			AssertHighlighting("000000000000111", provider[0]);
			
			provider.SetText(  "abcd");
			highlighting.Parse(provider);
			AssertHighlighting("0000", provider[0]);
		}
		
		[Test]
		public void KeywordsCasesensitive_OneChar()
		{
			Init(@"<language name='test' extensions='*.test'>
				    <highlighting>
				        <list name='keywords0'>
				            <item>a</item>
				        </list>
				        <contexts>
				            <context attribute='a0' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				        </itemDatas>
				    </highlighting>
				    <general>
						<keywords casesensitive='1' />
					</general>
				</language>");

			provider.SetText(  "ab a text");
			highlighting.Parse(provider);
			AssertHighlighting("000100000", provider[0]);
			
			provider.SetText(  "a");
			highlighting.Parse(provider);
			AssertHighlighting("1", provider[0]);
		}
		
		[Test]
		public void KeywordsCasesensitive_NoChars()
		{
			Init(@"<language name='test' extensions='*.test'>
				    <highlighting>
				        <list name='keywords0'>
				        </list>
				        <contexts>
				            <context attribute='a0' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				            </context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				        </itemDatas>
				    </highlighting>
				    <general>
						<keywords casesensitive='1' />
					</general>
				</language>");

			provider.SetText(  "ab a text");
			highlighting.Parse(provider);
			AssertHighlighting("000000000", provider[0]);
		}
		
		private Rules.Keyword NewRulesKeyword(string[] words, bool casesensitive,
			string weakDeliminator, string additionalDeliminator)
		{
			Rules.KeywordData data = new Rules.KeywordData(words, casesensitive, null);
			return new Rules.Keyword(data, weakDeliminator, additionalDeliminator);
		}
		
		[Test]
		public void Keywords()
		{
			{
				Rules.Keyword rule = NewRulesKeyword(new string[] {"word1", "Word2"}, false, "", "");
				int nextPosition = 0;
				Assert.AreEqual(true, rule.Match("word1", 0, out nextPosition));
				Assert.AreEqual(5, nextPosition);
			}
			{
				Rules.Keyword rule = NewRulesKeyword(new string[] {"word1", "Word2"}, false, "", "");
				int nextPosition = 0;
				Assert.AreEqual(true, rule.Match("word2", 0, out nextPosition));
				Assert.AreEqual(5, nextPosition);
			}
			{
				Rules.Keyword rule = NewRulesKeyword(new string[] {"word1", "Word2"}, true, "", "");
				int nextPosition = 0;
				Assert.AreEqual(false, rule.Match("word2", 0, out nextPosition));
				Assert.AreEqual(0, nextPosition);
			}
			{
				Rules.Keyword rule = NewRulesKeyword(new string[] {"a12bC", "a12"}, false, "", "");
				int nextPosition = 0;
				Assert.AreEqual(true, rule.Match("a12", 0, out nextPosition));
				Assert.AreEqual(3, nextPosition);
				Assert.AreEqual(true, rule.Match("a12bC", 0, out nextPosition));
				Assert.AreEqual(5, nextPosition);
				Assert.AreEqual(true, rule.Match("A12bC", 0, out nextPosition));
				Assert.AreEqual(5, nextPosition);
				Assert.AreEqual(false, rule.Match("A12fC", 0, out nextPosition));
				Assert.AreEqual(0, nextPosition);
			}
			{
				Rules.Keyword rule = NewRulesKeyword(new string[] {"a12bC", "a12"}, false, "", "");
				int nextPosition = 0;
				Assert.AreEqual(false, rule.Match("a1", 0, out nextPosition));
				Assert.AreEqual(0, nextPosition);
			}
			{
				Rules.Keyword rule = NewRulesKeyword(new string[] {"abABC", "abdABC", "acABC"}, true, "", "");
				int nextPosition = 0;
				Assert.AreEqual(true, rule.Match("abABC", 0, out nextPosition));
				Assert.AreEqual(true, rule.Match("abdABC", 0, out nextPosition));
				Assert.AreEqual(true, rule.Match("acABC", 0, out nextPosition));
				Assert.AreEqual(false, rule.Match("aca", 0, out nextPosition));
				Assert.AreEqual(false, rule.Match("acABD", 0, out nextPosition));
			}
			{
				Rules.Keyword rule = NewRulesKeyword(new string[] {"#define", "#elif", "#else", "#endif", "#error", "#if", "#line", "#undef", "#warning", "abstract", "as", "base", "break", "case", "catch", "checked", "class", "const", "continue", "default", "delegate", "do", "else", "enum", "event", "explicit", "extern", "finally", "fixed", "for", "foreach", "get", "goto", "if", "implicit", "in", "interface", "is", "lock", "namespace", "new", "operator", "out", "override", "params", "readonly", "ref", "return", "sealed", "set", "sizeof", "stackalloc", "static", "struct", "switch", "this", "throw", "try", "typeof", "unchecked", "unsafe", "using", "var", "virtual", "where", "while"}, true, "", "");
				int nextPosition = 0;
				Assert.AreEqual(true, rule.Match("while", 0, out nextPosition));
				Assert.AreEqual(5, nextPosition);
			}
		}
		
		[Test]
		public void Keywords_NotBug()
		{
			Rules.Keyword rule = NewRulesKeyword(new string[] {"ab", "ab_cde", "ab_cd"}, true, "", "");
			int nextPosition = 0;
			Assert.AreEqual(true, rule.Match("ab", 0, out nextPosition), "#1");
			Assert.AreEqual(2, nextPosition);
			Assert.AreEqual(true, rule.Match("ab_cde", 0, out nextPosition), "#2");
			Assert.AreEqual(6, nextPosition);
			Assert.AreEqual(false, rule.Match("ab_cdd", 0, out nextPosition), "#3");
			Assert.AreEqual(0, nextPosition);
			Assert.AreEqual(true, rule.Match("ab_cd", 0, out nextPosition), "#4");
			Assert.AreEqual(5, nextPosition);
			
			Assert.AreEqual(true, rule.Match("ab ", 0, out nextPosition), "#5");
			Assert.AreEqual(2, nextPosition);
		}
		
		[Test]
		public void Keywords_Bug()
		{
			Rules.Keyword rule = NewRulesKeyword(new string[] {"ab", "ab cde", "ab cd"}, true, "", "");
			int nextPosition = 0;
			Assert.AreEqual(true, rule.Match("ab", 0, out nextPosition), "#1");
			Assert.AreEqual(2, nextPosition);
			Assert.AreEqual(true, rule.Match("ab cde", 0, out nextPosition), "#2");
			Assert.AreEqual(6, nextPosition);
			Assert.AreEqual(false, rule.Match("ab cdd", 0, out nextPosition), "#3");
			Assert.AreEqual(0, nextPosition);
			Assert.AreEqual(true, rule.Match("ab cd", 0, out nextPosition), "#4");
			Assert.AreEqual(5, nextPosition);
			
			Assert.AreEqual(true, rule.Match("ab ", 0, out nextPosition), "#5");
			Assert.AreEqual(2, nextPosition);
		}
		
		[Test]
		public void Keywords_IgnoreCaseInvariantOptimization()
		{
			Rules.Keyword rule = NewRulesKeyword(new string[] {"2200fC", "2b23", "2456"}, false, "", "");
			int nextPosition = 0;
			Assert.AreEqual(true, rule.Match("2B23", 0, out nextPosition));
			Assert.AreEqual(4, nextPosition);
			Assert.AreEqual(true, rule.Match("2b23", 0, out nextPosition));
			Assert.AreEqual(4, nextPosition);
		}
	}
}
