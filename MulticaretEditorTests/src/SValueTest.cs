using System;
using MulticaretEditor;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class SValueTest
	{
		[Test]
		public void Values()
		{
			SValue value;
			
			value = SValue.NewString("text");
			Assert.AreEqual("text", value.String);
			Assert.AreEqual(true, value.IsString);
			Assert.AreEqual(false, value.IsNone);
			Assert.AreEqual(false, value.IsInt);
			Assert.AreEqual(0, value.Int);
			Assert.AreEqual(0, value.Double);
			Assert.AreEqual(false, value.Bool);
			Assert.AreEqual(false, value.IsFloat);
			Assert.AreEqual(false, value.IsLong);
			
			value = SValue.NewInt(10);
			Assert.AreEqual(10, value.Int);
			Assert.AreEqual(true, value.IsInt);
			Assert.AreEqual(false, value.IsString);
			Assert.AreEqual(false, value.IsNone);
			
			value = SValue.NewDouble(10.2);
			Assert.AreEqual(10.2, value.Double);
			Assert.AreEqual(0, value.Int);
			Assert.AreEqual(true, value.IsDouble);
			Assert.AreEqual(false, value.IsInt);
			Assert.AreEqual(false, value.IsString);
			Assert.AreEqual(false, value.IsNone);
			
			value = SValue.NewBool(true);
			Assert.AreEqual(true, value.Bool);
			Assert.AreEqual(0, value.Int);
			Assert.AreEqual(true, value.IsBool);
			Assert.AreEqual(false, value.IsInt);
			Assert.AreEqual(false, value.IsString);
			Assert.AreEqual(false, value.IsNone);
			
			value = SValue.NewBool(false);
			Assert.AreEqual(false, value.Bool);
			Assert.AreEqual(true, value.IsBool);
			
			value = SValue.NewFloat(10.5f);
			Assert.AreEqual(10.5f, value.Float);
			Assert.AreEqual(true, value.IsFloat);
			
			value = SValue.NewLong(10000L);
			Assert.AreEqual(10000L, value.Long);
			Assert.AreEqual(true, value.IsLong);
		}
		
		[Test]
		public void StringCantBeNull()
		{
			SValue value;
			
			value = SValue.NewString("abcd");
			Assert.AreEqual("abcd", value.String);
			
			value = SValue.NewString("");
			Assert.AreEqual("", value.String);
			
			value = SValue.NewString(null);
			Assert.AreEqual("", value.String);
			
			value = SValue.NewBool(true);
			Assert.AreEqual("", value.String);
		}
		
		[Test]
		public void Hash()
		{
			SValue value;
			
			value = SValue.NewHash();
			value["field"] = SValue.NewString("text");
			Assert.AreEqual("text", value["field"].String);
			Assert.AreEqual("", value["missingField"].String);
		}
		
		[Test]
		public void List()
		{
			SValue value;
			
			value = SValue.NewList();
			value[0] = SValue.NewString("text");
			Assert.AreEqual("text", value[0].String);
			Assert.AreEqual("", value[1].String);
			
			value = SValue.NewList();
			value[0] = SValue.NewDouble(1.5);
			value[1] = SValue.None;
			Assert.AreEqual(2, value.ListCount);
			Assert.AreEqual("[1.5d, None]", value.ToString());
			
			value = SValue.NewList();
			value[0] = SValue.NewInt(0);
			value[1] = SValue.NewInt(1);
			value[2] = SValue.NewInt(2);
			value[3] = SValue.NewInt(3);
			Assert.AreEqual("[0, 1, 2, 3]", value.ToString());
			
			value.ListCount = 4;
			Assert.AreEqual("[0, 1, 2, 3]", value.ToString());
			value.ListCount = 3;
			Assert.AreEqual("[0, 1, 2]", value.ToString());
			value.ListCount = 5;
			Assert.AreEqual("[0, 1, 2, None, None]", value.ToString());
			value.ListCount = 3;
			Assert.AreEqual("[0, 1, 2]", value.ToString());
			value.ListCount = 0;
			Assert.AreEqual("[]", value.ToString());
			
			value.Add(SValue.NewInt(1));
			value.Add(SValue.NewInt(3));
			value.Add(SValue.NewInt(5));
			CollectionAssert.AreEqual(new SValue[] { SValue.NewInt(1), SValue.NewInt(3), SValue.NewInt(5) }, value.List);
		}
		
		[Test]
		public void ToStringTest()
		{
			SValue value;
			
			value = SValue.NewBool(false);
			Assert.AreEqual("False", value.ToString());
			
			value = SValue.NewHash();
			value["field"] = SValue.NewString("value");
			Assert.AreEqual("{'field': 'value'}", value.ToString());
			
			value = SValue.NewHash();
			value["field1"] = SValue.NewString("value1");
			value["field2"] = SValue.NewString("value2");
			Assert.AreEqual("{'field1': 'value1', 'field2': 'value2'}", value.ToString());
			
			value = SValue.NewList();
			value[0] = SValue.NewDouble(1.5);
			value[2] = SValue.NewString("value");
			Assert.AreEqual(3, value.ListCount);
			Assert.AreEqual("[1.5d, None, 'value']", value.ToString());
		}
		
		[Test]
		public void SerializeAndDeserialize_Simple()
		{
			Assert.AreEqual("'some text'", SValue.Unserialize(SValue.Serialize(SValue.NewString("some text"))).ToString());
			Assert.AreEqual("''", SValue.Unserialize(SValue.Serialize(SValue.NewString(""))).ToString());
			Assert.AreEqual("''", SValue.Unserialize(SValue.Serialize(SValue.NewString(null))).ToString());
			Assert.AreEqual("10", SValue.Unserialize(SValue.Serialize(SValue.NewInt(10))).ToString());
			Assert.AreEqual("-150", SValue.Unserialize(SValue.Serialize(SValue.NewInt(-150))).ToString());
			Assert.AreEqual("10.1d", SValue.Unserialize(SValue.Serialize(SValue.NewDouble(10.1))).ToString());
			Assert.AreEqual("-0.1d", SValue.Unserialize(SValue.Serialize(SValue.NewDouble(-0.1))).ToString());
			Assert.AreEqual("True", SValue.Unserialize(SValue.Serialize(SValue.NewBool(true))).ToString());
			Assert.AreEqual("False", SValue.Unserialize(SValue.Serialize(SValue.NewBool(false))).ToString());
			Assert.AreEqual("1.5f", SValue.Unserialize(SValue.Serialize(SValue.NewFloat(1.5f))).ToString());
			Assert.AreEqual("2000L", SValue.Unserialize(SValue.Serialize(SValue.NewLong(2000L))).ToString());
			Assert.AreEqual("None", SValue.Unserialize(SValue.Serialize(SValue.None)).ToString());
		}
		
		[Test]
		public void SerializeAndDeserialize_Hash()
		{
			SValue hash = SValue.NewHash();
			hash["field0"] = SValue.NewString("value");
			hash["field1"] = SValue.NewInt(10);
			Assert.AreEqual("{'field0': 'value', 'field1': 10}", SValue.Unserialize(SValue.Serialize(hash)).ToString());
		}
		
		[Test]
		public void SerializeAndDeserialize_List()
		{
			SValue hash = SValue.NewHash();
			hash["field0"] = SValue.NewString("value");
			hash["field1"] = SValue.NewInt(10);
			
			SValue list = SValue.NewList();
			list.Add(SValue.NewInt(1));
			list.Add(SValue.NewString("value2"));
			list.Add(hash);
			
			Assert.AreEqual("[1, 'value2', {'field0': 'value', 'field1': 10}]", SValue.Unserialize(SValue.Serialize(list)).ToString());
		}
		
		[Test]
		public void SerializeAndDeserialize_CircleLinks()
		{
			SValue hash = SValue.NewHash();
			hash["field0"] = SValue.NewString("value");
			hash["field1"] = SValue.NewInt(10);
			hash["field2"] = hash;
			
			SValue list = SValue.NewList();
			list.Add(SValue.NewInt(1));
			list.Add(SValue.NewString("value2"));
			list.Add(hash);
			
			hash["field3"] = list;
			
			SValue unserialized = SValue.Unserialize(SValue.Serialize(list));
			Assert.AreEqual("[1, 'value2', {'field0': 'value', 'field1': 10, 'field2': …, 'field3': …}]", unserialized.ToString());
			Assert.AreEqual(unserialized[2], unserialized[2]["field2"]);
			Assert.AreEqual(unserialized, unserialized[2]["field3"]);
			Assert.AreNotEqual(unserialized, unserialized[2]["field2"]);
		}
		
		[Test]
		public void SerializeAndDeserialize_EqualTextsCovering()
		{
			SValue list = SValue.NewList();
			SValue hash;
			
			hash = SValue.NewHash();
			hash["field0"] = SValue.NewString("value1");
			hash["field1"] = SValue.NewInt(10);
			list.Add(hash);
			
			hash = SValue.NewHash();
			hash["field3"] = SValue.NewString("value2");
			hash["field1"] = SValue.NewInt(100);
			list.Add(hash);
			
			Assert.AreEqual("[{'field0': 'value1', 'field1': 10}, {'field3': 'value2', 'field1': 100}]", SValue.Unserialize(SValue.Serialize(list)).ToString());
		}
		
		[Test]
		public void Deserialize_NotFailOnIncorrectData()
		{
			Assert.AreEqual(SValue.None, SValue.Unserialize(new byte[] { (byte)'D', (byte)'S', (byte)'-' }));
			Assert.AreEqual(SValue.None, SValue.Unserialize(new byte[] { (byte)'D', (byte)'S', (byte)'V' }));
			Assert.AreEqual(SValue.None, SValue.Unserialize(new byte[] {
				(byte)'D', (byte)'S', (byte)'V',
				(byte)0xff, (byte)0xff, (byte)0xff, 0,
				(byte)0xff, (byte)0xff, (byte)0xff, 0,
				(byte)0xff, (byte)0xff, (byte)0xff, 0
			}));
			Assert.AreEqual(SValue.None, SValue.Unserialize(null));
		}
		
		[Test]
		public void AsDictionary()
		{
			byte[] bytes;
			{
				SValue value = SValue.NewHash();
				value["x"] = SValue.NewInt(10);
				Assert.NotNull(value.AsDictionary, "before serialize");
				Assert.AreEqual(1, value.AsDictionary.Count, "before serialize");
				Assert.AreEqual(10, value.AsDictionary["x"].Int, "before serialize");
				bytes = SValue.Serialize(value);
			}
			{
				SValue value = SValue.Unserialize(bytes);
				Assert.NotNull(value.AsDictionary, "after serialize");
				Assert.AreEqual(1, value.AsDictionary.Count, "after serialize");
				Assert.AreEqual(10, value.AsDictionary["x"].Int, "after serialize");
			}
		}
	}
}
