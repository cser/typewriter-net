using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.IO;

namespace MulticaretEditor
{
	public struct SValue
	{
		public class SList : List<SValue>, IRList<SValue>
		{
			public SList()
			{
			}

			public SList(IEnumerable<SValue> collection) : base(collection)
			{
			}
		}

		public class SHash : Dictionary<string, SValue>
		{
		}
		
		public const byte TypeNone = 0;
		public const byte TypeList = 1;
		public const byte TypeHash = 2;
		public const byte TypeString = 3;
		public const byte TypeInt = 4;
		public const byte TypeDouble = 5;
		public const byte TypeBool = 6;
		public const byte TypeFloat = 7;
		public const byte TypeLong = 8;
		public const byte TypeBytes = 9;
		
		public static SValue None { get { return new SValue(TypeNone, null); } }
		
		public static SValue NewList()
		{
			return new SValue(TypeList, new SList());
		}

		public static SValue NewList(IEnumerable<SValue> collection)
		{
			return new SValue(TypeList, new SList(collection));
		}
		
		public static SValue NewHash()
		{
			return new SValue(TypeHash, new SHash());
		}
		
		public static SValue NewString(string value)
		{
			return new SValue(TypeString, value);
		}
		
		public static SValue NewInt(int value)
		{
			return new SValue(TypeInt, value);
		}
		
		public static SValue NewDouble(double value)
		{
			return new SValue(TypeDouble, value);
		}
		
		public static SValue NewBool(bool value)
		{
			return new SValue(TypeBool, value);
		}
		
		public static SValue NewFloat(float value)
		{
			return new SValue(TypeFloat, value);
		}
		
		public static SValue NewLong(long value)
		{
			return new SValue(TypeLong, value);
		}
		
		public static SValue NewBytes(byte[] value)
		{
			return new SValue(TypeBytes, value);
		}
		
		private readonly byte type;
		private readonly object value;
		
		private SValue(byte type, object value)
		{
			this.type = type;
			this.value = value;
		}
		
		public bool IsString { get { return type == TypeString; } }
		public bool IsInt { get { return type == TypeInt; } }
		public bool IsDouble { get { return type == TypeDouble; } }
		public bool IsBool { get { return type == TypeBool; } }
		public bool IsNone { get { return type == TypeNone; } }
		public bool IsFloat { get { return type == TypeFloat; } }
		public bool IsLong { get { return type == TypeLong; } }
		public bool IsBytes { get { return type == TypeBytes; } }

		public bool IsList { get { return type == TypeList; } }
		public bool IsHash { get { return type == TypeHash; } }
		
		public string String { get { return (value as string) ?? ""; } }
		public int Int { get { return value is int ? (int)value : 0; } }
		public double Double { get { return value is double ? (double)value : 0; } }
		public bool Bool { get { return value is bool ? (bool)value : false; } }
		public float Float { get { return value is float ? (float)value : 0f; } }
		public long Long { get { return value is long ? (long)value : 0L; } }
		public byte[] Bytes { get { return value is byte[] ? (byte[])value : null; } }
		
		public string GetString(string alt)
		{
			return type == TypeString ? (string)value : alt;
		}
		
		public int GetInt(int alt)
		{
			return type == TypeInt ? (int)value : alt;
		}
		
		public double GetDouble(double alt)
		{
			return type == TypeDouble ? (double)value : alt;
		}
		
		public bool GetBool(bool alt)
		{
			return type == TypeBool ? (bool)value : alt;
		}
		
		public float GetFloat(float alt)
		{
			return type == TypeFloat ? (float)value : alt;
		}
		
		public long GetLong(long alt)
		{
			return type == TypeLong ? (long)value : alt;
		}
		
		public byte[] GetBytes(byte[] alt)
		{
			return type == TypeBytes ? (byte[])value : alt;
		}
		
		private static SList emptyList = new SList();
		public IRList<SValue> List { get { return (value as SList) ?? emptyList; } }
		
		public Dictionary<string, SValue> AsDictionary { get { return this.value as Dictionary<string, SValue>; } }
		
		public SValue this[string key]
		{
			get
			{
				Dictionary<string, SValue> hash = this.value as Dictionary<string, SValue>;
				SValue value = new SValue();
				if (hash != null)
					hash.TryGetValue(key, out value);
				return value;
			}
			set
			{
				Dictionary<string, SValue> hash = this.value as Dictionary<string, SValue>;
				if (hash != null)
				{
					if (value.type != TypeNone)
						hash[key] = value;
					else
						hash.Remove(key);
				}
			}
		}
		
		public int ListCount
		{
			get
			{
				List<SValue> list = this.value as List<SValue>;
				return list != null ? list.Count : 0;
			}
			set
			{
				List<SValue> list = this.value as List<SValue>;
				if (list != null)
				{
					if (list.Count - value > 0)
					{
						list.RemoveRange(value, list.Count - value);
					}
					else
					{
						for (int i = list.Count; i < value; i++)
						{
							list.Add(new SValue());
						}
					}
				}
			}
		}
		
		public void Add(SValue value)
		{
			List<SValue> list = this.value as List<SValue>;
			if (list != null)
				list.Add(value);
		}
		
		public SValue this[int index]
		{
			get
			{
				List<SValue> list = this.value as List<SValue>;
				SValue value = new SValue();
				if (list != null & index >= 0 & index < list.Count)
					value = list[index];
				return value;
			}
			set
			{
				List<SValue> list = this.value as List<SValue>;
				if (list != null)
				{
					for (int i = list.Count; i <= index; i++)
					{
						list.Add(new SValue());
					}
					list[index] = value;
				}
			}
		}
		
		public SValue SetNewHash(string key)
		{
			SValue hash = NewHash();
			this[key] = hash;
			return hash;
		}
		
		public SValue SetNewList(string key)
		{
			SValue list = NewList();
			this[key] = list;
			return list;
		}
		
		public SValue SetNewHash(int index)
		{
			SValue hash = NewHash();
			this[index] = hash;
			return hash;
		}
		
		public SValue SetNewList(int index)
		{
			SValue list = NewList();
			this[index] = list;
			return list;
		}
		
		public SValue AddNewHash()
		{
			SValue hash = NewHash();
			Add(hash);
			return hash;
		}
		
		public SValue AddNewList()
		{
			SValue list = NewList();
			Add(list);
			return list;
		}
		
		public SValue With(string key, SValue value)
		{
			this[key] = value;
			return this;
		}
		
		public SValue With(int index, SValue value)
		{
			this[index] = value;
			return this;
		}
		
		override public string ToString()
		{
			return ToString(new Dictionary<SValue, bool>());
		}
		
		private string ToString(Dictionary<SValue, bool> processed)
		{
			string text = "None";
			switch (type)
			{
				case TypeHash:
				{
					if (processed.ContainsKey(this))
					{
						text = "…";
						break;
					}
					processed[this] = true;
					
					StringBuilder builder = new StringBuilder();
					builder.Append("{");
					bool first = true;
					foreach (KeyValuePair<string, SValue> pair in (Dictionary<string, SValue>)value)
					{
						if (!first)
							builder.Append(", ");
						first = false;
						builder.Append("'" + pair.Key + "': " + pair.Value.ToString(processed));
					}
					builder.Append("}");
					text = builder.ToString();
					break;
				}
				case TypeList:
				{
					if (processed.ContainsKey(this))
					{
						text = "…";
						break;
					}
					processed[this] = true;
					
					StringBuilder builder = new StringBuilder();
					builder.Append("[");
					bool first = true;
					foreach (SValue valueI in (List<SValue>)value)
					{
						if (!first)
							builder.Append(", ");
						first = false;
						builder.Append(valueI.ToString(processed));
					}
					builder.Append("]");
					text = builder.ToString();
					break;
				}
				case TypeBool:
				case TypeInt:
					text = value + "";
					break;
				case TypeDouble:
					text = ((double)value).ToString(CultureInfo.InvariantCulture) + "d";
					break;
				case TypeString:
					text = "'" + value + "'";
					break;
				case TypeFloat:
					text = ((float)value).ToString(CultureInfo.InvariantCulture) + "f";
					break;
				case TypeLong:
					text = ((long)value).ToString(CultureInfo.InvariantCulture) + "L";
					break;
				case TypeBytes:
					text = "[bytes:" + (value != null ? "length=" + ((byte[])value).Length : "null") + "]";
					break;
			}
			return text;
		}
		
		//-----------------------------------------------------------
		// Serialization
		//-----------------------------------------------------------
		
		private static bool IsStoredText(string text)
		{
			return text.Length >= 2 && text.Length <= 1024;
		}
		
		public static byte[] Serialize(SValue value)
		{
			using (MemoryStream stream = new MemoryStream())
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				Dictionary<SList, int> listSet = new Dictionary<SList, int>();
				Dictionary<SHash, int> hashSet = new Dictionary<SHash, int>();
				Dictionary<string, int> textSet = new Dictionary<string, int>();
				int listIndex = 0;
				int hashIndex = 0;
				int textIndex = 0;
				FillComplexValues(listSet, hashSet, textSet, value, ref listIndex, ref hashIndex, ref textIndex);
				SList[] lists = new SList[listIndex];
				SHash[] hashes = new SHash[hashIndex];
				string[] texts = new string[textIndex];
				foreach (KeyValuePair<SList, int> pair in listSet)
				{
					lists[pair.Value] = pair.Key;
				}
				foreach (KeyValuePair<SHash, int> pair in hashSet)
				{
					hashes[pair.Value] = pair.Key;
				}
				foreach (KeyValuePair<string, int> pair in textSet)
				{
					if (pair.Value != -1)
						texts[pair.Value] = pair.Key;
				}
				writer.Write((byte)'D');
				writer.Write((byte)'S');
				writer.Write((byte)'V');
				writer.Write(texts.Length);
				writer.Write(lists.Length);
				writer.Write(hashes.Length);
				for (int i = 0; i < texts.Length; i++)
				{
					writer.Write(texts[i]);
				}
				for (int i = 0; i < lists.Length; i++)
				{
					SList list = lists[i];
					writer.Write(list.Count);
					for (int j = 0, count = list.Count; j < count; j++)
					{
						SerializePrimitive(listSet, hashSet, textSet, writer, list[j]);
					}
				}
				for (int i = 0; i < hashes.Length; i++)
				{
					SHash hash = hashes[i];
					writer.Write(hash.Count);
					foreach (KeyValuePair<string, SValue> pair in hash)
					{
						int textIndexI;
						if (IsStoredText(pair.Key) && textSet.TryGetValue(pair.Key, out textIndexI) && textIndexI != -1)
						{
							writer.Write(true);
							writer.Write(textIndexI);
						}
						else
						{
							writer.Write(false);
							writer.Write(pair.Key);
						}
						SerializePrimitive(listSet, hashSet, textSet, writer, pair.Value);
					}
				}
				SerializePrimitive(listSet, hashSet, textSet, writer, value);
				return stream.ToArray();
			}
		}
		
		public static SValue Unserialize(byte[] bytes)
		{
			if (bytes == null)
				return new SValue();
			using (MemoryStream stream = new MemoryStream(bytes))
			using (BinaryReader reader = new BinaryReader(stream))
			{
				try
				{
					if (reader.ReadByte() != 'D' || reader.ReadByte() != 'S' || reader.ReadByte() != 'V')
						return new SValue();
					int bytesLength = bytes.Length;
					int length;
					
					length = reader.ReadInt32();
					if (length > bytesLength)
						return new SValue();
					string[] texts = new string[length];
					
					length = reader.ReadInt32();
					if (length > bytesLength)
						return new SValue();
					SList[] lists = new SList[length];
					
					length = reader.ReadInt32();
					if (length > bytesLength)
						return new SValue();
					SHash[] hashes = new SHash[length];
					
					for (int i = 0; i < lists.Length; i++)
					{
						lists[i] = new SList();
					}
					for (int i = 0; i < hashes.Length; i++)
					{
						hashes[i] = new SHash();
					}
					for (int i = 0; i < texts.Length; i++)
					{
						texts[i] = reader.ReadString();
					}
					for (int i = 0; i < lists.Length; i++)
					{
						SList list = lists[i];
						int count = reader.ReadInt32();
						for (int j = 0; j < count; j++)
						{
							list.Add(ReadPrimitive(lists, hashes, texts, reader));
						}
					}
					for (int i = 0; i < hashes.Length; i++)
					{
						SHash hash = hashes[i];
						int count = reader.ReadInt32();
						for (int j = 0; j < count; j++)
						{
							string key;
							if (reader.ReadBoolean())
								key = texts[reader.ReadInt32()];
							else
								key = reader.ReadString();
							hash[key] = ReadPrimitive(lists, hashes, texts, reader);
						}
					}
					return ReadPrimitive(lists, hashes, texts, reader);
				}
				catch
				{
					return new SValue();
				}
			}
		}
		
		private static void FillComplexValues(
			Dictionary<SList, int> listSet, Dictionary<SHash, int> hashSet, Dictionary<string, int> textSet,
			SValue value,
			ref int listIndex, ref int hashIndex, ref int textIndex)
		{
			switch (value.type)
			{
				case TypeList:
					SList list = value.value as SList;
					if (listSet.ContainsKey(list))
						return;
					listSet[list] = listIndex++;
					
					for (int i = 0, count = list.Count; i < count; i++)
					{
						FillComplexValues(listSet, hashSet, textSet, list[i], ref listIndex, ref hashIndex, ref textIndex);
					}
					break;
				case TypeHash:
					SHash hash = value.value as SHash;
					if (hashSet.ContainsKey(hash))
						return;
					hashSet[hash] = hashIndex++;
					
					foreach (KeyValuePair<string, SValue> pair in hash)
					{
						string key = pair.Key;
						if (IsStoredText(key))
						{
							int textsCount;
							if (!textSet.TryGetValue(key, out textsCount))
								textSet[key] = -1;
							else if (textsCount == -1)
								textSet[key] = textIndex++;
						}
							
						FillComplexValues(listSet, hashSet, textSet, pair.Value, ref listIndex, ref hashIndex, ref textIndex);
					}
					break;
				case TypeString:
				{
					string key = (value.value as string) ?? "";
					int textsCount;
					if (!textSet.TryGetValue(key, out textsCount))
						textSet[key] = -1;
					else if (textsCount == -1)
						textSet[key] = textIndex++;
					break;
				}
			}
		}
		
		private static void SerializePrimitive(
			Dictionary<SList, int> listSet, Dictionary<SHash, int> hashSet, Dictionary<string, int> textSet,
			BinaryWriter writer, SValue value)
		{
			writer.Write((byte)value.type);
			switch (value.type)
			{
				case TypeList:
					writer.Write(listSet[(SList)value.value]);
					break;
				case TypeHash:
					writer.Write(hashSet[(SHash)value.value]);
					break;
				case TypeBool:
					writer.Write((bool)value.value);
					break;
				case TypeDouble:
					writer.Write((double)value.value);
					break;
				case TypeInt:
					writer.Write((int)value.value);
					break;
				case TypeString:
					string text = (string)value.value ?? "";
					int textIndexI;
					if (IsStoredText(text) && textSet.TryGetValue(text, out textIndexI) && textIndexI != -1)
					{
						writer.Write(true);
						writer.Write(textIndexI);
					}
					else
					{
						writer.Write(false);
						writer.Write(text);
					}
					break;
				case TypeFloat:
					writer.Write((float)value.value);
					break;
				case TypeLong:
					writer.Write((long)value.value);
					break;
				case TypeBytes:
					byte[] bytes = (byte[])value.value;
					if (bytes != null)
					{
						writer.Write(bytes.Length);
						writer.Write(bytes);
					}
					else
					{
						writer.Write(-1);
					}
					break;
			}
		}
		
		private static SValue ReadPrimitive(SList[] lists, SHash[] hashes, string[] texts, BinaryReader reader)
		{
			byte type = reader.ReadByte();
			switch (type)
			{
				case TypeList:
					return new SValue(type, lists[reader.ReadInt32()]);
				case TypeHash:
					return new SValue(type, hashes[reader.ReadInt32()]);
				case TypeBool:
					return new SValue(type, reader.ReadBoolean());
				case TypeDouble:
					return new SValue(type, reader.ReadDouble());
				case TypeInt:
					return new SValue(type, reader.ReadInt32());
				case TypeString:
					string text;
					if (reader.ReadBoolean())
						text = texts[reader.ReadInt32()];
					else
						text = reader.ReadString();
					return new SValue(type, text);
				case TypeFloat:
					return new SValue(type, reader.ReadSingle());
				case TypeLong:
					return new SValue(type, reader.ReadInt64());
				case TypeBytes:
					int length = reader.ReadInt32();
					return new SValue(type, length != -1 ? reader.ReadBytes(length) : null);
			}
			return new SValue();
		}
	}
}
