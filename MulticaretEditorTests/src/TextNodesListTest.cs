using System;
using System.Collections.Generic;
using MulticaretEditor;
using NUnit.Framework;
using System.Text;
using TinyJSON;

namespace TextNodesListTest
{
	[TestFixture]
	public class TextNodesListTest
	{
		[Test]
		public void SimpleIntegration()
		{
			Buffer buffer = new Buffer("Directory/Test.cs", "Test.cs", SettingsMode.Normal);
			buffer.Controller.InitText(@"public class Test
			{
				public void Method0()
				{
				}
				
				public void Method1()
				{
				}
			}");
			TextNodesList list = new TextNodesList(buffer, new Settings(null));
			string error;
			string shellError;
			Properties.CommandInfo commandInfo = new Properties.CommandInfo();
			commandInfo.command = "buildin-cs:c#";
			list.Build(commandInfo, Encoding.UTF8, out error, out shellError);
			Assert.AreEqual(
				"Test.cs\n" +
				"class Test (1)\n" +
				"\t+ void Method0() (3)\n" +
				"\t+ void Method1() (7)",
				list.Controller.Lines.GetText());
		}
		
		private void AssertParse(string expected, string text)
		{
			LineArray lines = new LineArray();
			lines.SetText(text);
			CSTextNodeParser parser = new CSTextNodeParser(null);
			Node node = parser.Parse(lines);
			Assert.AreEqual(expected, StringOfNode(node));
		}
		
		private string StringOfNode(Node node)
		{
			string text = "";
			text += "'" + node["name"] + "'";
			text += " " + (int)node["line"];
			text += " [";
			List<Node> nodes = (List<Node>)node["childs"];
			bool first = true;
			foreach (Node nodeI in nodes)
			{
				if (!first)
				{
					text += ", ";
				}
				first = false;
				text += StringOfNode(nodeI);
			}
			text += "]";
			return text;
		}
		
		[Test]
		public void Simple()
		{
			AssertParse(
				"'class Test' 1 ['+ void Method0()' 3 [], '+ void Method1()' 7 []]",
				@"public class Test
				{
					public void Method0()
					{
					}
					
					public void Method1()
					{
					}
				}");
		}
		
		[Test]
		public void NestedClass()
		{
			AssertParse(
				"'class Test' 1 ['class Nested' 3 ['+ void NestedMethod()' 5 []], '+ void Method()' 10 []]",
				@"public class Test
				{
					public class Nested()
					{
						public void NestedMethod()
						{
						}
					}
					
					public void Method()
					{
					}
				}");
		}
	}
}