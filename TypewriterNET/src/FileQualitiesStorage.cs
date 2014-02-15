using System.Collections.Generic;
using MulticaretEditor;

namespace TypewriterNET
{
	public class FileQualitiesStorage
	{
		private List<SValue> list = new List<SValue>();
		private Dictionary<int, SValue> qualitiesOf = new Dictionary<int, SValue>();

		public FileQualitiesStorage()
		{
		}

		public int maxCount = 200;
		public int gap = 100;

		public void SetCursor(string fullPath, int position)
		{
			if (string.IsNullOrEmpty(fullPath))
				return;
			int path = fullPath.GetHashCode();
			SValue qualities;
			if (qualitiesOf.TryGetValue(path, out qualities) && qualities.IsHash)
			{
				list.Remove(qualities);
			}
			else
			{
				qualities = SValue.NewHash();
				qualities["path"] = SValue.NewInt(path);
				qualitiesOf[path] = qualities;
			}
			list.Add(qualities);
			qualities["cursor"] = SValue.NewInt(position);
			if (list.Count > maxCount + gap)
			{
				Normalize();
			}
		}

		private void Normalize()
		{
			if (list.Count > maxCount)
			{
				for (int i = 0, count = list.Count - maxCount; i < count; i++)
				{
					qualitiesOf.Remove(list[i]["path"].Int);
				}
				list.RemoveRange(0, list.Count - maxCount);
			}
		}

		public int GetCursor(string fullPath)
		{
			if (string.IsNullOrEmpty(fullPath))
				return 0;
			int path = fullPath.GetHashCode();
			SValue qualities;
			qualitiesOf.TryGetValue(path, out qualities);
			return qualities["cursor"].Int;
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
			foreach (SValue qualities in value.List)
			{
				int path = qualities["path"].Int;
				if (!qualitiesOf.ContainsKey(path))
				{
					list.AddRange(value.List);
					qualitiesOf[path] = qualities;
				}
			}
		}
	}
}
