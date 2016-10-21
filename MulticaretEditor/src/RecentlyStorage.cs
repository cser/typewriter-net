using System;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class RecentlyStorage
	{
		private readonly List<string> files = new List<string>();
		
		public List<string> GetFiles()
		{
			return new List<string>(files);
		}
		
		private int maxCount = 50;
		public int MaxCount
		{
			get { return maxCount; }
			set
			{
				maxCount = value;
				if (maxCount <= 0)
					maxCount = 0;
			}
		}
		
		public void Add(string path)
		{
			if (!string.IsNullOrEmpty(path))
			{
				string lowerPath = path.ToLowerInvariant();
				for (int i = files.Count; i-- > 0;)
				{
					if (files[i].ToLowerInvariant() == lowerPath)
					{
						files.RemoveAt(i);
						break;
					}
				}
				while (files.Count > maxCount)
				{
					files.RemoveAt(0);
				}
				files.Add(path);
			}
		}
		
		public void Unserialize(SValue value)
		{
			string[] raw = value.String.Split('+');
			files.Clear();
			for (int i = 0; i < raw.Length; i++)
			{
				string file = raw[i];
				if (!string.IsNullOrEmpty(file))
				{
					files.Add(file);
				}
			}
			if (files.Count > maxCount)
				files.RemoveRange(files.Count, files.Count - maxCount);
		}
		
		public SValue Serialize()
		{
			if (files.Count > maxCount)
				files.RemoveRange(0, files.Count - maxCount);
			return SValue.NewString(string.Join("+", files.ToArray()));
		}
	}
}