using System;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class FileQualitiesStorage
	{
		private readonly int gap;

		public FileQualitiesStorage() : this(50)
		{
		}

		public FileQualitiesStorage(int gap)
		{
			this.gap = gap;
		}

		protected List<SValue> list = new List<SValue>();
		protected Dictionary<int, SValue> qualitiesOf = new Dictionary<int, SValue>();

		private int maxCount = 200;
		public int MaxCount
		{
			get { return maxCount; }
			set { maxCount = Math.Max(0, Math.Min(int.MaxValue - maxCount, value)); }
		}

		public SValue Set(string path)
		{
			if (string.IsNullOrEmpty(path))
				return SValue.None;
			int hash = path.GetHashCode();
			SValue qualities;
			bool exists = qualitiesOf.TryGetValue(hash, out qualities);
			if (exists)
				list.Remove(qualities);
			if (!exists || !qualities.IsHash)
			{
				qualities = SValue.NewHash();
				qualities["#"] = SValue.NewInt(hash);
				qualitiesOf[hash] = qualities;
			}
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
			SValue qualities;
			qualitiesOf.TryGetValue(hash, out qualities);
			return qualities;
		}

		private void Normalize()
		{
			if (list.Count > maxCount)
			{
				for (int i = 0, count = list.Count - maxCount; i < count; i++)
				{
					qualitiesOf.Remove(list[i]["#"].Int);
				}
				list.RemoveRange(0, list.Count - maxCount);
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
			qualitiesOf.Clear();
			IRList<SValue> valueList = value.List;
			int count = 0;
			for (int i = valueList.Count; i-- > 0;)
			{
				SValue qualities = valueList[i];
				int hash = qualities["#"].Int;
				if (!qualitiesOf.ContainsKey(hash))
				{
					list.Add(qualities);
					qualitiesOf[hash] = qualities;
					count++;
					if (count >= maxCount)
						break;
				}
			}
			list.Reverse();
		}
	}
}
