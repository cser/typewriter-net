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
			public TestStorage(int gap) : base(gap)
			{
			}

			public List<SValue> List { get { return list; } }
			public Dictionary<int, SValue> QualitiesOf { get { return qualitiesOf; } }
		}

		private TestStorage storage;

		private void Init(int maxCount)
		{
			storage = new TestStorage(2);
			storage.MaxCount = maxCount;
		}

		private FileQualitiesStorageTest SetCursor(string path, int position)
		{
			storage.SetCursor(path, position);
			return this;
		}

		private FileQualitiesStorageTest AssertCursor(string path, int expectedPosition)
		{
			int actualCursor = storage.GetCursor(path);
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
			Assert.AreEqual(2, storage.QualitiesOf.Count);
			Reload(3);
			AssertCursor("b", 3).AssertCursor("c", 4);
			Assert.AreEqual(2, storage.List.Count);
			Assert.AreEqual(2, storage.QualitiesOf.Count);
			SetCursor("d", 1);
			AssertCursor("b", 3).AssertCursor("c", 4).AssertCursor("d", 1);
			Assert.AreEqual(3, storage.List.Count);
			Assert.AreEqual(3, storage.QualitiesOf.Count);
			Reload(3);
			AssertCursor("b", 3).AssertCursor("c", 4).AssertCursor("d", 1);
			Assert.AreEqual(3, storage.List.Count);
			Assert.AreEqual(3, storage.QualitiesOf.Count);
			Reload(2);
			AssertCursor("c", 4).AssertCursor("d", 1);
			Assert.AreEqual(2, storage.List.Count);
			Assert.AreEqual(2, storage.QualitiesOf.Count);
		}
	}
}
