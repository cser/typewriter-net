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
		
		[Test]
		public void SeveralClassesIntegration()
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
			}
			
			public class Test2
			{
				public void Method2()
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
				"\t+ void Method1() (7)\n" +
				"class Test2 (12)\n" +
				"\t+ void Method2() (14)",
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
			text += "'" + (node["name"].IsString() ? (string)node["name"] : "")  + "'";
			text += " " + (node["line"].IsInt() ? (int)node["line"] : -1);
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
				"'class Test' 1 ['class Nested' 3 ['+ void NestedMethod()' 5 []], '+ void NotNestedMethod()' 10 []]",
				@"public class Test
				{
					public class Nested
					{
						public void NestedMethod()
						{
						}
					}
					
					public void NotNestedMethod()
					{
					}
				}");
		}
		
		[Test]
		public void NestedClass_StaticPrivateParameters()
		{
			AssertParse(
				"'class Test' 1 ['class Nested' 3 ['|- void NestedMethod(int index)' 5 []], '# void Method(Node a, out bool b)' 10 []]",
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
		}
		
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
		
		[Test]
		public void IndexedProperty()
		{
			AssertParse(
				"'class Test' 1 ['+ string this[int index]' 3 [], '~ string[] this[int i, int j]' 11 []]",
				@"public class Test
				{
					public string this[int index]
					{
						get { return property; }
						set { ; }
					}
					
					private int property;
					
					string[] this[int i, int j]
					{
						get { return null; }
						set { ; }
					}
				}");
		}
		
		[Test]
		public void IndexedProperty2()
		{
			AssertParse(
				"'class Test' 1 ['~ string[] this[int[] i, int j]' 3 []]",
				@"public class Test
				{
					string[] this[int[] i, int j]
					{
						get { return null; }
						set { ; }
					}
				}");
		}
		
		[Test]
		public void ComplexType()
		{
			AssertParse(
				"'class Test' 1 ['~ Dictionary<string[], List<int[]>>[] Method(int index)' 3 []]",
				@"public class Test
				{
					Dictionary<string[], List<int[]>>[] Method(int index)
					{
					}
				}");
		}
		
		[Test]
		public void Comments1()
		{
			AssertParse(
				"'class Test' 1 ['~ int Method()' 3 [], '+ string[] Property' 15 []]",
				@"public class Test
				{
					int Method()
					{
					}
					/*
					void Method2()
					{
					}
					*/
					
					private int property;
					
					//public string[] Property2 { get { return property; } }
					public string[] Property { get { return property; } }
				}");
		}
		
		[Test]
		public void Comments2()
		{
			AssertParse(
				"'class Test' 1 ['~ int Method()' 3 [], '+ string[] Property' 9 []]",
				@"public class Test// comment
				{
					int Method()
					{/*}*/
					}
					
					private int property;
					
					public string[] Property { get { return property; } }
				}");
		}
	
		[Test]
		public void Constructor()
		{
			AssertParse("'class A' 1 ['+ A(C c)' 3 [], '+ void C()' 4 []]",
				@"public class A {
					private B b;
					public A(C c) { }
					public void C() { }
				}");
		}
		
		[Test]
		public void Text_Simple()
		{
			AssertParse("'class A' 1 ['+ A(C c)' 3 [], '+ void C()' 4 []]",
				@"public class A {
					private B b;
					public A(C c) { string x = ""}""; }
					public void C() {
						string y = @""{{
						{{"";
					}
				}");
		}
		
		[Test]
		public void DefaultParameters()
		{
			AssertParse("'class A' 1 ['+ A(string c = \"default\")' 3 [], '+ void C(int[] index = null)' 4 []]",
				@"public class A {
					private B b;
					public A(string c = ""default"") {}
					public void C(int[] index = null) {
					}
				}");
		}
		
		[Test]
		public void ClassGenerics()
		{
			AssertParse("'class A<K, List<T>>' 1 ['+ void B()' 3 []]",
				@"public class A<K, List<T>>
				{
					public void B()
					{
					}
				}");
		}
		
		[Test]
		public void ClassGenericsWhere()
		{
			AssertParse("'class A<K, List<T>>' 1 ['+ void B()' 3 []]",
				@"public class A<K, List<T>> where T : int
				{
					public void B()
					{
					}
				}");
		}
		
		[Test]
		public void MethodGenerics()
		{
			AssertParse("'class A' 1 ['+ void B()' 3 [], '+ void Method<T, List<T>>()' 7 [], '+ void C()' 11 []]",
				@"public class A
				{
					public void B()
					{
					}
					
					public void Method<T, List<T>>()
					{
					}
					
					public void C()
					{
					}
				}");
		}
		
		[Test]
		public void MethodGenericsWhere()
		{
			AssertParse("'class A' 1 ['+ void B()' 3 [], '+ void Method<T, List<T>>()' 7 [], '+ void C()' 11 []]",
				@"public class A
				{
					public void B()
					{
					}
					
					public void Method<T, List<T>>() where T : List<Dictionary<string[], int>>
					{
					}
					
					public void C()
					{
					}
				}");
		}
		
		[Test]
		public void Attributes()
		{
			AssertParse("'class Test' 1 ['+ void A()' 5 [], '+ int Property' 10 [], '+ void B()' 14 []]",
				@"public class Test
				{
					[A4] private int _field;
					[A1(string[] {""cdef""})]
					public void A()
					{
					}
					
					[A4]
					public int Property { get; set; }
					
					[A2]
					[A3]
					public void B()
					{
					}
				}");
		}
		
		[Test]
		public void NestedTypes()
		{
			AssertParse(
				"'class Test' 1 ['+ Type.Subtype A()' 3 [], '+ Type.Subtype Property' 7 [], '+ void B()' 9 []]",
				@"public class Test
				{
					public Type.Subtype A()
					{
					}
					
					public Type.Subtype Property { get; set; }
					
					public void B()
					{
					}
				}");
		}
		
		[Test]
		public void Extern()
		{
			AssertParse(
				"'class Test' 1 ['@|+ bool ShowWindow(IntPtr hWnd, Int32 nCmdShow)' 4 [], '+ void B()' 6 []]",
				@"public class Test
				{
					[DllImport(""user32.dll"")]
					public static extern bool ShowWindow(IntPtr hWnd, Int32 nCmdShow);
					
					public void B()
					{
					}
				}");
		}
		
		[Test]
		public void Struct()
		{
			AssertParse(
				"'struct P' 1 ['+ int X' 4 [], '+ int Y' 7 [], '+ P(int x, int y)' 9 [], '+ void SetX(int x)' 15 []]",
				@"public struct P
				{
					private int x;
					public int X { get { return x; } }
					
					private int y;
					public int Y { get { return y; } }
					
					public P(int x, int y)
					{
						this.x = x;
						this.y = y;
					}
					
					public void SetX(int x)
					{
						this.x = x;
					}
				}");
		}
		
		[Test]
		public void NestedGenericStruct()
		{
			AssertParse(
				"'class Test' 1 ['struct P<T>' 3 ['+ int X' 5 [], '+ int Y' 6 []], '+ void B()' 9 []]",
				@"public class Test
				{
					public struct P<T>
					{
						public int X { get; set; }
						public int Y { get; set; }
					}
					
					public void B()
					{
					}
				}");
		}
		
		[Test]
		public void Enum()
		{
			AssertParse(
				"'enum Mode' 1 []",
				@"public enum Mode
				{
					None,
					Some
				}");
		}
		
		[Test]
		public void NestedEnum()
		{
			AssertParse(
				"'class Test' 1 ['+ Subtype A()' 3 [], 'enum Mode' 7 [], '+ void B()' 13 []]",
				@"public class Test
				{
					public Subtype A()
					{
					}
					
					public enum Mode
					{
						None,
						Some
					}
					
					public void B()
					{
					}
				}");
		}
		
		[Test]
		public void EnumExtended()
		{
			AssertParse(
				"'class Test' 1 ['enum A : int' 1 [], '+ void B()' 1 []]",
				@"class Test { enum A : int { Value0, Value1 } public void B() { } }");
		}
		
		[Test]
		public void Delegate_InClass()
		{
			AssertParse(
				"'namespace Name' 1 ['class Test' 2 ['+* void EventData()' 3 [], '+ void B()' 4 []]]",
				@"namespace Name {
					class Test {
						public delegate void EventData();
						public void B() { }
					}
				}");
		}
		
		[Test]
		public void Delegate_InNamespace()
		{
			AssertParse(
				"'namespace Name' 1 ['+* void EventData()' 2 [], 'class Test' 3 ['+ void B()' 4 []]]",
				@"namespace Name {
					public delegate void EventData();
					class Test {
						public void B() { }
					}
				}");
		}
		
		[Test]
		public void Delegate_InNamespaceGeneric()
		{
			AssertParse(
				"'namespace Name' 1 [" +
				"'+* void EventData<T0, T1>(T0 value0, T1 value1)' 2 [], " +
				"'class Test' 3 ['+ void B()' 4 []]" + 
				"]",
				@"namespace Name {
					public delegate void EventData<T0, T1>(T0 value0, T1 value1);
					class Test {
						public void B() { }
					}
				}");
		}
		
		[Test]
		public void Interface()
		{
			AssertParse(
				"'interface ITest' 1 ['+ void A()' 3 [], '+ void B(int x, int y)' 4 []]",
				@"public interface ITest
				{
					void A();
					void B(int x, int y);
				}");
		}
		
		[Test]
		public void SeveralClasses()
		{
			AssertParse(
				"'' -1 [" +
				"'class A' 1 ['+ A()' 3 [], '+ void Method()' 7 []], " +
				"'class B' 12 ['+ B()' 14 []]" +
				"]",
				@"public class A
				{
					public A()
					{
					}
					
					public void Method()
					{
					}
				}
				
				public class B
				{
					public B()
					{
					}
				}");
		}
		
		[Test]
		public void Namespace()
		{
			AssertParse(
				"'namespace Name' 1 [" +
				"'class A' 3 ['+ void Method()' 5 []]" +
				"]",
				@"namespace Name
				{
					public class A
					{
						public void Method()
						{
						}
					}
				}");
		}
		
		[Test]
		public void ComplexNamespace()
		{
			AssertParse(
				"'namespace Name.Subname' 1 [" +
				"'class A' 3 ['+ void Method()' 5 []]" +
				"]",
				@"namespace Name.Subname
				{
					public class A
					{
						public void Method()
						{
						}
					}
				}");
		}
		
		[Test]
		public void NamespaceSeveralClasses()
		{
			AssertParse(
				"'namespace Name' 1 [" +
				"'class A' 3 ['+ void Method()' 5 []], " +
				"'class B' 10 []" +
				"]",
				@"namespace Name
				{
					public class A
					{
						public void Method()
						{
						}
					}
					
					public class B
					{
					}
				}");
		}
		
		[Test]
		public void IgnoreUsings()
		{
			AssertParse(
				"'class A' 3 ['+ void Method()' 5 []]",
				@"using System;
				using System.Action;
				public class A
				{
					public void Method()
					{
					}
				}");
		}
		
		[Test]
		public void IgnoreUsingsInsideNamespaces()
		{
			AssertParse(
				"'namespace Namespace' 1 ['class A' 5 ['+ void Method()' 7 []]]",
				@"namespace Namespace
				{
					using System;
					using System.Action;
					public class A
					{
						public void Method()
						{
						}
					}
				}");
		}
		
		[Test]
		public void NestedNamespaces()
		{
			AssertParse(
				"'namespace Namespace' 1 ['namespace Subnamespace' 5 [" +
				"'class A' 9 ['+ void Method()' 11 []]" +
				"]]",
				@"namespace Namespace
				{
					using System;
					using System.Action;
					namespace Subnamespace
					{
						using System;
						using System.Action;
						public class A
						{
							public void Method()
							{
							}
						}
					}
				}");
		}
		
		[Test]
		public void NestedNamespacesEmptyBrackets()
		{
			AssertParse(
				"'namespace Namespace' 1 ['class A' 6 ['+ void Method()' 8 []]]",
				@"namespace Namespace
				{
					{
					}
					
					public class A
					{
						public void Method()
						{
						}
					}
				}");
		}
		
		[Test]
		public void Extends()
		{
			AssertParse(
				"'class A : B' 1 ['+ void Method()' 3 []]",
				@"public class A : B
				{
					public void Method()
					{
					}
				}");
		}
		
		[Test]
		public void ExtendsGeneric()
		{
			AssertParse(
				"'class A : B<Dictionary<string[], int>>' 1 ['+ void Method()' 3 []]",
				@"public class A : B<Dictionary<string[], int>>
				{
					public void Method()
					{
					}
				}");
		}
		
		[Test]
		public void Virtual()
		{
			AssertParse(
				"'class Test' 1 ['+ void Method()' 3 [], '+ int Value' 7 []]",
				@"public class Test
				{
					virtual public void Method()
					{
					}
					
					virtual public int Value { get; set; }
				}");
		}
		
		[Test]
		public void Abstract()
		{
			AssertParse(
				"'class Test' 1 ['+ void Method0()' 3 [], '+ void Method1()' 7 []]",
				@"abstract class Test
				{
					abstract public void Method0()
					{
					}
					
					public void Method1()
					{
					}
				}");
		}
		
		[Test]
		public void Base()
		{
			AssertParse(
				"'class Test' 1 ['+ Test(int index) : base(index, 10)' 3 [], '+ void Method()' 7 []]",
				@"abstract class Test
				{
					public Test(int index) : base(index, 10)
					{
					}
					
					public void Method()
					{
					}
				}");
		}
		
		[Test]
		public void TokenIteratorTest()
		{
			LineArray lines = new LineArray();
			lines.SetText(@"public class A {
				private B b;
				public void C() { }
			}");
			CSTokenIterator iterator = new CSTokenIterator(lines);
			Assert.AreEqual("[" +
				"<<public>>, <<class>>, <<A>>, '{', <<private>>, <<B>>, <<b>>, ';', " +
				"<<public>>, <<void>>, <<C>>, '(', ')', '{', '}', '}'" +
			"]", ListUtil.ToString(iterator.tokens));
		}
		
		[Test]
		public void TokenIteratorTest2()
		{
			LineArray lines = new LineArray();
			lines.SetText(@"public class Test
			{
				public void Method0()
				{
				}
				
				public void Method1()
				{
				}
			}");
			CSTokenIterator iterator = new CSTokenIterator(lines);
			Assert.AreEqual("[" +
				"<<public>>, <<class>>, <<Test>>, '{', " +
				"<<public>>, <<void>>, <<Method0>>, '(', ')', '{', '}', " +
				"<<public>>, <<void>>, <<Method1>>, '(', ')', '{', '}', '}'" +
			"]", ListUtil.ToString(iterator.tokens));
		}
		
		[Test]
		public void TokenIteratorTest3()
		{
			LineArray lines = new LineArray();
			lines.SetText(@"public class Test
			{
				/*public */void Method0()
				{
				}
				
				public void Method1()//}
				{
				}
			}");
			CSTokenIterator iterator = new CSTokenIterator(lines);
			Assert.AreEqual("[" +
				"<<public>>, <<class>>, <<Test>>, '{', " +
				"<<void>>, <<Method0>>, '(', ')', '{', '}', " +
				"<<public>>, <<void>>, <<Method1>>, '(', ')', '{', '}', '}'" +
			"]", ListUtil.ToString(iterator.tokens));
		}
		
		[Test]
		public void TokenIteratorTest4()
		{
			LineArray lines = new LineArray();
			lines.SetText(@"public class Test
			{
				private string text = ""ab\""c\n"";
				private string text2 = @""ab""""c"";
				private string text3 = @""ab" + "\n" + @"c"";
			}");
			CSTokenIterator iterator = new CSTokenIterator(lines);
			Assert.AreEqual("[" +
				"<<public>>, <<class>>, <<Test>>, '{', " +
				"<<private>>, <<string>>, <<text>>, '=', <<\"ab\\\"c\\n\">>, ';', " +
				"<<private>>, <<string>>, <<text2>>, '=', <<@\"ab\"\"c\">>, ';', " +
				"<<private>>, <<string>>, <<text3>>, '=', <<@\"ab\nc\">>, ';', '}'" +
			"]", ListUtil.ToString(iterator.tokens));
		}
		
		[Test]
		public void TokenIteratorTest5()
		{
			LineArray lines = new LineArray();
			lines.SetText(@"public class A {
				private char a = '}';
				private char b = '""';
				private char c = '\'';
				private char d = '\n';
				
				public void Method0(char c = 'c')
				{
				}
				
				public void Method1()
				{
				}
			}");
			CSTokenIterator iterator = new CSTokenIterator(lines);
			Assert.AreEqual("[<<public>>, <<class>>, <<A>>, '{', " +
				"<<private>>, <<char>>, <<a>>, '=', <<'}'>>, ';', " +
				"<<private>>, <<char>>, <<b>>, '=', <<'\"'>>, ';', " +
				"<<private>>, <<char>>, <<c>>, '=', <<'\\''>>, ';', " +
				"<<private>>, <<char>>, <<d>>, '=', <<'\\n'>>, ';', " +
				"<<public>>, <<void>>, <<Method0>>, '(', <<char>>, <<c>>, '=', <<'c'>>, ')', '{', '}', " +
				"<<public>>, <<void>>, <<Method1>>, '(', ')', '{', '}', " +
			"'}']", ListUtil.ToString(iterator.tokens));
		}
		
		[Test]
		public void RegionMarksMastBeIgnored()
		{
			AssertParse(
				"'class Test' 1 ['+ void Method0()' 3 [], '+ void Method1()' 7 [], '+ void Method2()' 12 []]",
				@"partial class Test
				{
					public void Method0()
					{
					}
					#region Region
					public void Method1()
					{
					}
					#endregion
					
					public void Method2()
					{
					}
				}");
		}
	}
}