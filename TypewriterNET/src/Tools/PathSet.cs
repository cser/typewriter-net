using System.Collections.Generic;

public class PathSet
{
	public static string GetNorm(string fullPath)
	{
		if (fullPath.EndsWith("\\"))
			fullPath = fullPath.Substring(0, fullPath.Length - 1);
		return fullPath.ToLower();
	}
	
	private Dictionary<string, bool> _set = new Dictionary<string, bool>();
	
	public IEnumerable<string> NormalizedPaths { get { return _set.Keys; } }
	public int Count { get { return _set.Count; } }
	
	public string Add(string fullPath)
	{
		if (!string.IsNullOrEmpty(fullPath))
		{
			string normalized = fullPath;
			if (normalized.EndsWith("\\"))
				normalized = normalized.Substring(0, normalized.Length - 1);
			_set[normalized.ToLower()] = true;
		}
		return fullPath;
	}
	
	public string AddDirectory(string fullPath, string oldFullPath)
	{
		if (!string.IsNullOrEmpty(fullPath))
		{
			string normalized = fullPath;
			if (normalized.EndsWith("\\"))
				normalized = normalized.Substring(0, normalized.Length - 1);
			normalized = normalized.ToLower();
			
			string oldDir = oldFullPath;
			if (!oldDir.EndsWith("\\"))
				oldDir += "\\";
			oldDir = oldDir.ToLower();
			
			string dir = normalized + "\\";
			foreach (KeyValuePair<string, bool> pair in new List<KeyValuePair<string, bool>>(_set))
			{
				if (pair.Key.StartsWith(oldDir))
				{
					_set.Remove(pair.Key);
					string newKey = pair.Key.Replace(oldDir, dir);
					if (!_set.ContainsKey(newKey))
						_set[newKey] = true;
				}
			}
			_set[normalized] = true;
		}
		return fullPath;
	}
	
	public void Remove(string fullPath)
	{
		if (!string.IsNullOrEmpty(fullPath))
		{
			if (fullPath.EndsWith("\\"))
				fullPath = fullPath.Substring(0, fullPath.Length - 1);
			_set.Remove(fullPath.ToLower());
		}
	}
	
	public bool Contains(string fullPath)
	{
		if (fullPath.EndsWith("\\"))
			fullPath = fullPath.Substring(0, fullPath.Length - 1);
		return _set.ContainsKey(fullPath.ToLower());
	}
	
	public void Clear()
	{
		_set.Clear();
	}
}