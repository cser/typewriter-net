using System;
using System.Collections.Generic;
using MulticaretEditor;

namespace TypewriterNET
{
	public class TabInfoList : SwitchList<TabInfo>
	{
		public TabInfoList()
		{
		}
		
		public string SelectedFullPath { get { return Selected != null ? Selected.FullPath : null; } }
		
		public TabInfo GetByFullPath(string fullPath)
		{
			foreach (TabInfo info in this)
			{
				if (info.FullPath == fullPath)
					return info;
			}
			return null;
		}
		
		public bool ContainsAllFullPaths(IEnumerable<string> fullPaths)
		{
			Dictionary<string, bool> notContains = new Dictionary<string, bool>();
			foreach (string fullPath in fullPaths)
			{
				if (fullPath != null)
					notContains[fullPath] = true;
			}
			foreach (TabInfo info in this)
			{
				if (info.FullPath != null)
					notContains.Remove(info.FullPath);
			}
			return notContains.Count == 0;
		}
	}
}
