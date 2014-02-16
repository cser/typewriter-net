using System;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class FileQualitiesStorage
	{
		private const string HashField = "#";

		public FileQualitiesStorage()
		{
			MaxCount = 200;
		}

		protected List<SValue> list = new List<SValue>();
		protected Dictionary<int, int> indexOf = new Dictionary<int, int>();

		private int gap;

		private int maxCount = 200;
		public int MaxCount
		{
			get { return maxCount; }
			set
			{
				maxCount = Math.Max(0, Math.Min(int.MaxValue / 3, value));
				gap = maxCount;
			}
		}

		public SValue Set(string path)
		{
			if (string.IsNullOrEmpty(path))
				return SValue.None;
			int hash = path.GetHashCode();
			int index;
			SValue qualities = SValue.None;
			bool exists = indexOf.TryGetValue(hash, out index);
			if (exists)
			{
				qualities = list[index];
				list[index] = SValue.None;
			}
			if (!qualities.IsHash)
			{
				qualities = SValue.NewHash();
				qualities[HashField] = SValue.NewInt(hash);
			}
			indexOf[hash] = list.Count;
			list.Add(qualities);
			if (list.Count > maxCount + gap)
				Normalize();
			return qualities;
		}

		public SValue Get(string path)
		{
			if (string.IsNullOrEmpty(path))
				return SValue.None;
			int hash = path.GetHashCode();
			int index;
			return indexOf.TryGetValue(hash, out index) ? list[index] : SValue.None;
		}

		private void Normalize()
		{
			indexOf.Clear();
			int compactedI = 0;
			for (int i = 0, count = list.Count; i < count; i++)
			{
				SValue valueI = list[i];
				if (!valueI.IsNone)
					list[compactedI++] = valueI;
			}
			list.RemoveRange(compactedI, list.Count - compactedI);
			int delta = list.Count - maxCount;
			if (delta > 0)
			{
				list.RemoveRange(0, delta);
				for (int i = 0, count = list.Count; i < count; i++)
				{
					indexOf[list[i][HashField].Int] = i;
				}
			}
		}

		public SValue Serialize()
		{
			Normalize();
			return SValue.NewList(list);
		}

		public void Unserialize(SValue value)
		{
			list.Clear();
			indexOf.Clear();
			IRList<SValue> valueList = value.List;
			for (int i = valueList.Count, count = 0; i-- > 0;)
			{
				SValue valueI = valueList[i];
				int hash = valueI[HashField].Int;
				if (!indexOf.ContainsKey(hash))
				{
					indexOf[hash] = -1;
					list.Add(valueI);
					count++;
					if (count >= maxCount)
						break;
				}
			}
			list.Reverse();
			for (int i = 0, count = list.Count; i < count; i++)
			{
				indexOf[list[i][HashField].Int] = i;
			}
		}
	}
}
