using System;
using System.Collections.Generic;
using MulticaretEditor;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class FileQualitiesStorageTest
	{
		public class TestStorage : FileQualitiesStorage
		{
			public List<SValue> List { get { return list; } }
			public Dictionary<int, int> IndexOf { get { return indexOf; } }
		}

		private TestStorage storage;

		private void Init(int maxCount)
		{
			storage = new TestStorage();
			storage.MaxCount = maxCount;
		}

		private FileQualitiesStorageTest SetCursor(string path, int position)
		{
			SValue value = storage.Set(path);
			value["cursor"] = SValue.NewInt(position);
			return this;
		}

		private FileQualitiesStorageTest AssertCursor(string path, int expectedPosition)
		{
			int actualCursor = storage.Get(path)["cursor"].Int;
			Assert.AreEqual(expectedPosition, actualCursor, "Expected position[" + path + "]: " + expectedPosition + ", got: " + actualCursor);
			return this;
		}

		private void Reload(int maxCount)
		{
			SValue value = storage.Serialize();
			Init(maxCount);
			storage.Unserialize(value);
		}

		[Test]
		public void Test1()
		{
			Init(2);
			SetCursor("a", 0);
			Reload(2);
			AssertCursor("a", 0);

			Init(2);
			SetCursor("a", 1).SetCursor("b", 3).SetCursor("c", 4);
			Reload(2);
			AssertCursor("b", 3).AssertCursor("c", 4);
		}

		[Test]
		public void Test2()
		{
			Init(2);
			SetCursor("a", 1).SetCursor("b", 3).SetCursor("c", 4);
			Reload(2);
			AssertCursor("b", 3).AssertCursor("c", 4);
			Assert.AreEqual(2, storage.List.Count);
			Assert.AreEqual(2, storage.IndexOf.Count);
			Reload(3);
			AssertCursor("b", 3).AssertCursor("c", 4);
			Assert.AreEqual(2, storage.List.Count);
			Assert.AreEqual(2, storage.IndexOf.Count);
			SetCursor("d", 1);
			AssertCursor("b", 3).AssertCursor("c", 4).AssertCursor("d", 1);
			Assert.AreEqual(3, storage.List.Count);
			Assert.AreEqual(3, storage.IndexOf.Count);
			Reload(3);
			AssertCursor("b", 3).AssertCursor("c", 4).AssertCursor("d", 1);
			Assert.AreEqual(3, storage.List.Count);
			Assert.AreEqual(3, storage.IndexOf.Count);
			Reload(2);
			AssertCursor("c", 4).AssertCursor("d", 1);
			Assert.AreEqual(2, storage.List.Count);
			Assert.AreEqual(2, storage.IndexOf.Count);
		}

		[Test]
		public void Test3()
		{
			Init(2);
			SetCursor("a", 1).SetCursor("b", 3).SetCursor("c", 4).SetCursor("b", 2).SetCursor("b", 8);
			Reload(2);
			AssertCursor("c", 4).AssertCursor("b", 8);
			Assert.AreEqual(2, storage.List.Count);
			Assert.AreEqual(2, storage.IndexOf.Count);

			SetCursor("e", 7);
			Reload(2);
			AssertCursor("b", 8).AssertCursor("e", 7);
			Assert.AreEqual(2, storage.List.Count);
			Assert.AreEqual(2, storage.IndexOf.Count);
		}

		[Test]
		public void Test4()
		{
			Init(2);
			SetCursor("a", 1).SetCursor("b", 3).SetCursor("c", 4).SetCursor("b", 2).SetCursor("b", 8);
			Reload(2);
			AssertCursor("missing", 0);
			AssertCursor("c", 4).AssertCursor("b", 8);
			Assert.AreEqual(2, storage.List.Count);
			Assert.AreEqual(2, storage.IndexOf.Count);

			SetCursor("e", 7);
			Reload(2);
			AssertCursor("b", 8).AssertCursor("e", 7);
			Assert.AreEqual(2, storage.List.Count);
			Assert.AreEqual(2, storage.IndexOf.Count);
		}

		[Test]
		public void Test5()
		{
			Init(2);
			SetCursor("a", 1).SetCursor("b", 3).SetCursor("c", 4).SetCursor("b", 2).SetCursor("b", 8);
			AssertCursor("c", 4).AssertCursor("b", 8);
		}

		[Test]
		public void RealizationTraitsTest()
		{
			Init(2);
			SetCursor("a", 1).SetCursor("b", 3).SetCursor("c", 4).SetCursor("b", 2);
			AssertCursor("a", 1).AssertCursor("c", 4).AssertCursor("b", 2);
			Assert.AreEqual(4, storage.List.Count);
			Assert.AreEqual(3, storage.IndexOf.Count);
			SetCursor("b", 8);
			Assert.AreEqual(2, storage.List.Count);
			Assert.AreEqual(2, storage.IndexOf.Count);
			AssertCursor("c", 4).AssertCursor("b", 8);
			SetCursor("e", 1).SetCursor("f", 3);
			Assert.AreEqual(4, storage.List.Count);
			Assert.AreEqual(4, storage.IndexOf.Count);
			AssertCursor("c", 4).AssertCursor("b", 8).AssertCursor("e", 1).AssertCursor("f", 3);
			SetCursor("d", 2);
			Assert.AreEqual(2, storage.List.Count);
			Assert.AreEqual(2, storage.IndexOf.Count);
			AssertCursor("f", 3).AssertCursor("d", 2);
		}
	}
}
