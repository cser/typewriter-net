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
		/*[Test]
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
		}*/
		
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
		
		/*[Test]
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
					public class Nested
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
		
		[Test]
		public void NestedClass_StaticPrivateParameters()
		{
			AssertParse(
				"'class Test' 1 ['class Nested' 3 ['|- void NestedMethod(int index)' 5 []], '- void Method(Node a, out bool b)' 10 []]",
				@"private class Test
				{
					protected class Nested
					{
						private static void NestedMethod(int index)
						{
						}
					}
					
					protected void Method(Node a, out bool b)
					{
					}
				}");
		}
		
		[Test]
		public void NestedClass_Fields()
		{
			AssertParse(
				"'class Test' 1 ['class Nested' 3 [" +
					"'+ bool Active' 6 [], " +
					"'+ bool Inactive' 7 [], " +
					"'+ int NestedMethod(int index)' 9 []" +
				"], '+ int Field' 15 []]",
				@"public class Test
				{
					public class Nested
					{
						public bool _active = false;
						public bool Active { get { return _active; } }
						public bool Inactive { get; private set; }
						
						public int NestedMethod(int index)
						{
							return 100;
						}
					}
					
					public int Field
					{
						get { return -1; }
					}
				}");
		}
		
		[Test]
		public void NestedClass_EgyptianBrackets()
		{
			AssertParse(
				"'class Test' 1 ['class Nested' 2 [" +
					"'+ bool Active' 4 [], " +
					"'+ bool Inactive' 5 [], " +
					"'+ int NestedMethod(int index)' 7 []" +
				"], '+ int Field' 12 []]",
				@"public class Test {
					public class Nested {
						public bool _active = false;
						public bool Active { get { return _active; } }
						public bool Inactive { get; private set; }
						
						public int NestedMethod(int index) {
							return 100;
						}
					}
					
					public int Field {
						get { return -1; }
					}
				}");
		}*/
		
		[Test]
		public void EmptyNestedClasses()
		{
			AssertParse(
				"'class Test' 1 ['class Nested1' 3 [], 'class Nested2' 7 []]",
				@"public class Test
				{
					public class Nested1
					{
					}
					
					public class Nested2
					{
					}
				}");
		}
		
		[Test]
		public void SimpleMethods()
		{
			AssertParse(
				"'class Test' 1 ['~ int Method1()' 3 [], '+ void Method2(int index, string[] items)' 7 []]",
				@"public class Test
				{
					int Method1()
					{
					}
					
					public void Method2(int index, string[] items)
					{
					}
				}");
		}
		
		[Test]
		public void SimpleProperties()
		{
			AssertParse(
				"'class Test' 1 ['~ int Property1' 3 [], '+ string[] Property2' 9 []]",
				@"public class Test
				{
					int Property1
					{
						get { return -1; }
						set { ; }
					}
					
					public string[] Property2 { get; private set; }
				}");
		}
		
		[Test]
		public void MastIgnoreFields()
		{
			AssertParse(
				"'class Test' 1 ['~ int Method()' 3 [], '+ string[] Property' 9 []]",
				@"public class Test
				{
					int Method()
					{
					}
					
					private int property;
					
					public string[] Property { get { return property; } }
				}");
		}
	}
}