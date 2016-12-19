using System;
using System.Xml;
using System.Drawing;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class HighlighterUtilTest
	{
		[Test]
		public void ParseColor_24Bit()
		{
			Assert.AreEqual("ff000000", HighlighterUtil.ParseColor("#000000").Value.ToArgb().ToString("x"));
			Assert.AreEqual("ff0000ff", HighlighterUtil.ParseColor("#0000ff").Value.ToArgb().ToString("x"));
			Assert.AreEqual("ffff80ff", HighlighterUtil.ParseColor("#ff80ff").Value.ToArgb().ToString("x"));
			Assert.AreEqual(Color.Red.ToArgb(), HighlighterUtil.ParseColor("#ff0000").Value.ToArgb());
			Assert.AreEqual(Color.Green.ToArgb(), HighlighterUtil.ParseColor("#008000").Value.ToArgb());
			Assert.AreEqual(Color.Blue.ToArgb(), HighlighterUtil.ParseColor("#0000ff").Value.ToArgb());
		}
		
		[Test]
		public void ParseColor_12Bit()
		{
			Assert.AreEqual("ff000000", HighlighterUtil.ParseColor("#000").Value.ToArgb().ToString("x"));
			Assert.AreEqual("ff00ffff", HighlighterUtil.ParseColor("#0ff").Value.ToArgb().ToString("x"));
			Assert.AreEqual("ffaa0000", HighlighterUtil.ParseColor("#a00").Value.ToArgb().ToString("x"));
			Assert.AreEqual("ff88aabb", HighlighterUtil.ParseColor("#8ab").Value.ToArgb().ToString("x"));
		}
		
		public class TestHighlighterSet : HighlighterSet
		{
			private Dictionary<string, Raw> dataOf = new Dictionary<string, Raw>();
			
			public void SetData(string type, Raw raw)
			{
				dataOf[type.ToLower()] = raw;
			}
			
			override protected Raw NewRaw(string type)
			{
				Raw raw;
				dataOf.TryGetValue(type.ToLower(), out raw);
				return raw;
			}
		}
		
		[Test]
		public void Parse_IncludeRulesInOtherFiles()
		{
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(@"<?xml version='1.0' encoding='UTF-8'?>
				<language name='test' extensions='*.test'>
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='a0' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
								<IncludeRules context='IncludeContext'/>
								<IncludeRules context='##Outer'/>
				            </context>
							<context attribute='a2' lineEndContext='#stay' name='IncludeContext'>
								<StringDetect attribute='a3' context='#stay' String='include'/>
							</context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				            <itemData name='a3' defStyleNum='dsChar'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			
			XmlDocument outerXml = new XmlDocument();
			outerXml.LoadXml(@"<?xml version='1.0' encoding='UTF-8'?>
				<language name='test' extensions='*.test'>
				    <highlighting>
				    	<list name='keywords2'>
				            <item>word0</item>
				        </list>
				        <contexts>
				            <context attribute='a0' lineEndContext='#stay' name='OuterNormal'>
								<IncludeRules context='OuterIncludeContext'/>
				            </context>
							<context attribute='a4' lineEndContext='#stay' name='OuterIncludeContext'>
								<StringDetect attribute='a0' context='#stay' String='include'/>
							</context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a4' defStyleNum='dsKeyword'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			
			TestHighlighterSet highlighterSet = new TestHighlighterSet();
			Raw outerRaw = Raw.Parse(outerXml);
			Raw.InlineIncludeRules(outerRaw, highlighterSet);
			highlighterSet.SetData("Outer", outerRaw);
			Raw raw = Raw.Parse(xml);
			
			Raw.InlineIncludeRules(raw, highlighterSet);
			Assert.AreEqual("<language>" +
			    "<highlighting>" +
			        "<list name='keywords0'>" +
			        	"<item>word0</item>" +
			         	"<item>word1</item>" +
			        "</list>" +
			        "<list name='keywords2'>" +
			        	"<item>word0</item>" +
			        "</list>" +
			        "<contexts>" +
			            "<context name='Normal' attribute='a0' lineEndContext='#stay'>" +
			                "<keyword attribute='a1' context='#stay' String='keywords0'/>" +
							"<StringDetect attribute='a3' context='#stay' String='include'/>" +
							"<StringDetect attribute='a0' context='#stay' String='include'/>" +
			            "</context>" +
						"<context name='IncludeContext' attribute='a2' lineEndContext='#stay'>" +
							"<StringDetect attribute='a3' context='#stay' String='include'/>" +
						"</context>" +
						
			            "<context name='OuterNormal' attribute='a0' lineEndContext='#stay'>" +
							"<StringDetect attribute='a0' context='#stay' String='include'/>" +
			            "</context>" +
						"<context name='OuterIncludeContext' attribute='a4' lineEndContext='#stay'>" +
							"<StringDetect attribute='a0' context='#stay' String='include'/>" +
						"</context>" +
						
			        "</contexts>" +
					"<itemDatas>" +
						"<itemData name='a0' defStyleNum='dsNormal'/>" +
						"<itemData name='a1' defStyleNum='dsKeyword'/>" +
						"<itemData name='a2' defStyleNum='dsDataType'/>" +
						"<itemData name='a3' defStyleNum='dsChar'/>" +
			            
						"<itemData name='a4' defStyleNum='dsKeyword'/>" +
					"</itemDatas>" +
				"</highlighting>" +
				"<general>" +
					"<keywords/>" +
				"</general>" +
			"</language>", raw.ToString());
		}
		
		private XmlDocument NewXml(string text)
		{
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(text);
			return xml;
		}
		
		[Test]
		public void Parse_SwitchingToContextsInOtherFiles()
		{
			XmlDocument xml = NewXml(@"<?xml version='1.0' encoding='UTF-8'?>
				<language name='test' extensions='*.test'>
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='a0' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				            </context>
							<context attribute='a2' lineEndContext='#stay' name='IncludeContext'>
								<StringDetect attribute='a3' context='##outer/Context' String='include'/>
							</context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				            <itemData name='a3' defStyleNum='dsChar'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			
			XmlDocument outerXml = NewXml(@"<?xml version='1.0' encoding='UTF-8'?>
				<language name='test' extensions='*.test'>
				    <highlighting>
				    	<list name='keywords2'>
				            <item>word0</item>
				        </list>
				        <contexts>
							<context attribute='a4' lineEndContext='#stay' name='OuterIncludeContext'>
								<StringDetect attribute='a0' context='#stay' String='include'/>
							</context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a4' defStyleNum='dsKeyword'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			
			TestHighlighterSet highlighterSet = new TestHighlighterSet();
			Raw outerRaw = Raw.Parse(outerXml);
			Raw.InlineIncludeRules(outerRaw, highlighterSet);
			highlighterSet.SetData("outer/context", outerRaw);
			Raw raw = Raw.Parse(xml);
			
			Raw.InlineIncludeRules(raw, highlighterSet);
			Assert.AreEqual(Raw.Parse(NewXml(@"<language name='test' extensions='*.test'>
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <list name='keywords2'>
				            <item>word0</item>
				        </list>
				        <contexts>
				            <context attribute='a0' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				            </context>
							<context attribute='a2' lineEndContext='#stay' name='IncludeContext'>
								<StringDetect attribute='a3' context='OuterIncludeContext' String='include'/>
							</context>
							<context attribute='a4' lineEndContext='#stay' name='OuterIncludeContext'>
								<StringDetect attribute='a0' context='#stay' String='include'/>
							</context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				            <itemData name='a3' defStyleNum='dsChar'/>
				            <itemData name='a4' defStyleNum='dsKeyword'/>
				        </itemDatas>
				    </highlighting>
				</language>")).ToString(), raw.ToString());
		}
		
		[Test]
		public void LazyOfRegex()
		{
			Assert.AreEqual(@"abc", HighlighterUtil.LazyOfRegex(@"abc"));
			Assert.AreEqual(@"#f+?\]f+?", HighlighterUtil.LazyOfRegex(@"#f+\]f+"));
			Assert.AreEqual(@"#f*?\]f+?", HighlighterUtil.LazyOfRegex(@"#f*\]f+"));
			Assert.AreEqual(@"#f??\]f??", HighlighterUtil.LazyOfRegex(@"#f?\]f??"));
			Assert.AreEqual(@"(a\1|(?(1)\1)){2}?", HighlighterUtil.LazyOfRegex(@"(a\1|(?(1)\1)){2}"));
			Assert.AreEqual(@"(a??)*?", HighlighterUtil.LazyOfRegex(@"(a?)*"));
		}
		
		[Test]
		public void FixUnicodeChars()
		{
			Assert.AreEqual(@"abc", HighlighterUtil.FixRegexUnicodeChars(@"abc"));
			Assert.AreEqual(
				@"[A-Z][A-Za-z\u0300-\u0326\u0330-\u0366\u0370-\u03770-9_']*",
			    HighlighterUtil.FixRegexUnicodeChars(@"[A-Z][A-Za-z\0300-\0326\0330-\0366\0370-\03770-9_']*"));
		}
		
		[Test]
		public void Parse_Normal()
		{
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(@"<?xml version='1.0' encoding='UTF-8'?>
				<language name='test' extensions='*.test'>
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item> word1</item>
				        </list>
				        <contexts>
				            <context attribute='a0' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
				            </context>
							<context attribute='a2' lineEndContext='#stay' name='IncludeContext'>
								<StringDetect attribute='a3' context='##outer/Context' String='include'/>
							</context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				            <itemData name='a3' defStyleNum='dsChar'/>
				        </itemDatas>
				    </highlighting>
				    <general>
						<keywords casesensitive='1'/>
					</general>
				</language>");
			Raw language = Raw.Parse(xml);
			Assert.AreEqual(
				"<language>" +
					"<highlighting>" +
						"<list name='keywords0'><item>word0</item><item>word1</item></list>" +
						"<contexts>" +
				            "<context name='Normal' attribute='a0' lineEndContext='#stay'>" +
				                "<keyword attribute='a1' context='#stay' String='keywords0'/>" +
				            "</context>" +
							"<context name='IncludeContext' attribute='a2' lineEndContext='#stay'>" +
								"<StringDetect attribute='a3' context='##outer/Context' String='include'/>" +
							"</context>" +
				        "</contexts>" +
				        "<itemDatas>" +
				            "<itemData name='a0' defStyleNum='dsNormal'/>" +
				            "<itemData name='a1' defStyleNum='dsKeyword'/>" +
				            "<itemData name='a2' defStyleNum='dsDataType'/>" +
				            "<itemData name='a3' defStyleNum='dsChar'/>" +
				        "</itemDatas>" +
					"</highlighting>" +
					"<general>" +
						"<keywords casesensitive='1'/>" +
					"</general>" +
				"</language>",
				language.ToString()
			);
		}
		
		[Test]
		public void PrefixContexts()
		{
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(@"<?xml version='1.0' encoding='UTF-8'?>
				<language name='test' extensions='*.test'>
				    <highlighting>
				        <list name='keywords0'>
				            <item>word0</item>
				            <item>word1</item>
				        </list>
				        <contexts>
				            <context attribute='a0' lineEndContext='#stay' name='Normal'>
				                <keyword attribute='a1' context='#stay' String='keywords0'/>
								<IncludeRules context='IncludeContext'/>
								<IncludeRules context='##Syntax file'/>
				            </context>
							<context attribute='a2' lineEndContext='#stay' name='IncludeContext'>
								<StringDetect attribute='a3' context='#pop!State name1' String='include'/>
							</context>
							<context attribute='a2' lineEndContext='State name2' name='State name1'>
								<StringDetect attribute='a3' context='#pop!State name1' String='include'/>
							</context>
				        </contexts>
				        <itemDatas>
				            <itemData name='a0' defStyleNum='dsNormal'/>
				            <itemData name='a1' defStyleNum='dsKeyword'/>
				            <itemData name='a2' defStyleNum='dsDataType'/>
				            <itemData name='a3' defStyleNum='dsChar'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			Raw raw = Raw.Parse(xml);
			Raw.PrefixContexts(raw, "prefix_");
			Assert.AreEqual("<language>" +
			    "<highlighting>" +
			        "<list name='prefix_keywords0'>" +
			        	"<item>word0</item>" +
			         	"<item>word1</item>" +
			        "</list>" +
			        "<contexts>" +
			            "<context name='prefix_Normal' attribute='prefix_a0' lineEndContext='#stay'>" +
			                "<keyword attribute='prefix_a1' context='#stay' String='prefix_keywords0'/>" +
							"<IncludeRules context='prefix_IncludeContext'/>" +
							"<IncludeRules context='##Syntax file'/>" +
			            "</context>" +
						"<context name='prefix_IncludeContext' attribute='prefix_a2' lineEndContext='#stay'>" +
							"<StringDetect attribute='prefix_a3' context='#pop!prefix_State name1' String='include'/>" +
						"</context>" +
						"<context name='prefix_State name1' attribute='prefix_a2' lineEndContext='prefix_State name2'>" +
							"<StringDetect attribute='prefix_a3' context='#pop!prefix_State name1' String='include'/>" +
						"</context>" +
			        "</contexts>" +
			        "<itemDatas>" +
			        	"<itemData name='prefix_a0' defStyleNum='dsNormal'/>" +
			            "<itemData name='prefix_a1' defStyleNum='dsKeyword'/>" +
			            "<itemData name='prefix_a2' defStyleNum='dsDataType'/>" +
			            "<itemData name='prefix_a3' defStyleNum='dsChar'/>" +
			        "</itemDatas>" +
			    "</highlighting>" +
			    "<general>" +
					"<keywords/>" +
				"</general>" +
			"</language>", raw.ToString());
		}
		
		[Test]
		public void Parse_IncludeRules()
		{
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(@"<?xml version='1.0' encoding='UTF-8'?>
				<language name='test' extensions='*.test'>
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
				            <itemData name='a3' defStyleNum='dsChar'/>
				        </itemDatas>
				    </highlighting>
				</language>");
			Raw raw = Raw.Parse(xml);
			Assert.AreEqual("<language>" +
			    "<highlighting>" +
			        "<list name='keywords0'>" +
			        	"<item>word0</item>" +
			         	"<item>word1</item>" +
			        "</list>" +
			        "<contexts>" +
			            "<context name='Normal' attribute='a0' lineEndContext='#stay'>" +
			                "<keyword attribute='a1' context='#stay' String='keywords0'/>" +
							"<IncludeRules context='IncludeContext'/>" +
			            "</context>" +
						"<context name='IncludeContext' attribute='a2' lineEndContext='#stay'>" +
							"<StringDetect attribute='a3' context='#stay' String='include'/>" +
						"</context>" +
			        "</contexts>" +
			        "<itemDatas>" +
			        	"<itemData name='a0' defStyleNum='dsNormal'/>" +
			            "<itemData name='a1' defStyleNum='dsKeyword'/>" +
			            "<itemData name='a2' defStyleNum='dsDataType'/>" +
			            "<itemData name='a3' defStyleNum='dsChar'/>" +
			        "</itemDatas>" +
			    "</highlighting>" +
			    "<general>" +
					"<keywords/>" +
				"</general>" +
			"</language>", raw.ToString());
			Raw.InlineIncludeRules(raw, new HighlighterSet());
			Assert.AreEqual("<language>" +
			    "<highlighting>" +
			        "<list name='keywords0'>" +
			        	"<item>word0</item>" +
			         	"<item>word1</item>" +
			        "</list>" +
			        "<contexts>" +
			            "<context name='Normal' attribute='a0' lineEndContext='#stay'>" +
			                "<keyword attribute='a1' context='#stay' String='keywords0'/>" +
							"<StringDetect attribute='a3' context='#stay' String='include'/>" +
			            "</context>" +
						"<context name='IncludeContext' attribute='a2' lineEndContext='#stay'>" +
							"<StringDetect attribute='a3' context='#stay' String='include'/>" +
						"</context>" +
			        "</contexts>" +
			        "<itemDatas>" +
			        	"<itemData name='a0' defStyleNum='dsNormal'/>" +
			            "<itemData name='a1' defStyleNum='dsKeyword'/>" +
			            "<itemData name='a2' defStyleNum='dsDataType'/>" +
			            "<itemData name='a3' defStyleNum='dsChar'/>" +
			        "</itemDatas>" +
			    "</highlighting>" +
			    "<general>" +
					"<keywords/>" +
				"</general>" + 
			"</language>", raw.ToString());
		}
		
		[Test]
		public void GetFilenamePatternRegex0()
		{
			Regex regex = HighlighterUtil.GetFilenamePatternRegex("file.xml");
			Assert.AreEqual(true, regex.IsMatch("file.xml"));
			Assert.AreEqual(false, regex.IsMatch("somefile.xml"));
			Assert.AreEqual(false, regex.IsMatch("file.xmls"));
		}
		
		[Test]
		public void GetFilenamePatternRegex1()
		{
			Regex regex = HighlighterUtil.GetFilenamePatternRegex("*.xml");
			Assert.AreEqual(true, regex.IsMatch("file.xml"));
			Assert.AreEqual(true, regex.IsMatch(".xml"));
			Assert.AreEqual(false, regex.IsMatch("file.xm"));
			Assert.AreEqual(false, regex.IsMatch("file.xmls"));
		}
		
		[Test]
		public void GetFilenamePatternRegex2()
		{
			Regex regex = HighlighterUtil.GetFilenamePatternRegex("*.xml*");
			Assert.AreEqual(true, regex.IsMatch("file.xml"));
			Assert.AreEqual(true, regex.IsMatch("file.xmls"));
			Assert.AreEqual(true, regex.IsMatch(".xmls"));
			Assert.AreEqual(false, regex.IsMatch("file.xm"));
		}
		
		[Test]
		public void GetFilenamePatternRegex3()
		{
			Regex regex = HighlighterUtil.GetFilenamePatternRegex("file??.xml*");
			Assert.AreEqual(true, regex.IsMatch("file01.xml"));
			Assert.AreEqual(true, regex.IsMatch("file10.xmls"));
			Assert.AreEqual(true, regex.IsMatch("fileii.xmls"));
			Assert.AreEqual(false, regex.IsMatch("file0.xml"));
			Assert.AreEqual(false, regex.IsMatch("file.xml"));
		}
		
		[Test]
		public void GetFilenamePatternRegex4()
		{
			Regex regex = HighlighterUtil.GetFilenamePatternRegex("*.c++");
			Assert.AreEqual(true, regex.IsMatch("file.c++"));
			Assert.AreEqual(false, regex.IsMatch("file.c++i"));
		}
	}
}
