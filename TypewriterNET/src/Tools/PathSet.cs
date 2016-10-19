using System.Collections.Generic;

public class PathSet
{
	public Dictionary<string, bool> _set = new Dictionary<string, bool>();
	
	public string Add(string fullPath)
	{
		if (!string.IsNullOrEmpty(fullPath))
		{
			string normalized = fullPath;
			if (normalized.EndsWith("\\"))
				normalized.Substring(0, normalized.Length - 1);
			_set[normalized.ToLower()] = true;
		}
		return fullPath;
	}
	
	public string Remove(string fullPath)
	{
		if (!string.IsNullOrEmpty(fullPath))
		{
			string normalized = fullPath;
			if (normalized.EndsWith("\\"))
				normalized.Substring(0, normalized.Length - 1);
			_set.Remove(normalized.ToLower());
		}
		return fullPath;
	}
	
	public bool Contains(string fullPath)
	{
		if (fullPath.EndsWith("\\"))
			fullPath.Substring(0, fullPath.Length - 1);
		return _set.ContainsKey(fullPath.ToLower());
	}
}