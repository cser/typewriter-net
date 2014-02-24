using System;
using System.Collections.Generic;

namespace MulticaretEditor.KeyMapping
{
	public class KeyMapNode
	{
		public readonly KeyMap main;
		public readonly int priority;

		public KeyMapNode(KeyMap main, int priority)
		{
			this.main = main;
			this.priority = priority;
		}

		public readonly List<KeyMapNode> before = new List<KeyMapNode>();
		public readonly List<KeyMapNode> after = new List<KeyMapNode>();

		public void AddBefore(KeyMap map)
		{
			AddBefore(map, 0);
		}

		public void AddBefore(KeyMap map, int priority)
		{
			int i = before.Count;
			for (; i-- > 0;)
			{
				if (before[i].priority >= priority)
					break;
			}
			before.Insert(i + 1, new KeyMapNode(map, priority));
		}

		public void RemoveBefore(KeyMap map)
		{
			for (int i = before.Count; i-- > 0;)
			{
				if (before[i].main == map)
					before.RemoveAt(i);
			}
		}

		public void AddAfter(KeyMap map)
		{
			AddAfter(map, 0);
		}

		public void AddAfter(KeyMap map, int priority)
		{
			int i = after.Count;
			for (; i-- > 0;)
			{
				if (after[i].priority >= priority)
					break;
			}
			after.Insert(i + 1, new KeyMapNode(map, priority));
		}

		public void RemoveAfter(KeyMap map)
		{
			for (int i = after.Count; i-- > 0;)
			{
				if (after[i].main == map)
					after.RemoveAt(i);
			}
		}

		public bool Enumerate<T>(Getter<KeyMap, T, bool> enumerator, T parameter)
		{
			for (int i = 0, count = before.Count; i < count; i++)
			{
				if (before[i].Enumerate<T>(enumerator, parameter))
					return true;
			}
			if (main != null && enumerator(main, parameter))
				return true;
			for (int i = 0, count = after.Count; i < count; i++)
			{
				if (after[i].Enumerate<T>(enumerator, parameter))
					return true;
			}
			return false;
		}

		public List<KeyMap> ToList()
		{
			List<KeyMap> list = new List<KeyMap>();
			AddToList(list);
			return list;
		}

		private void AddToList(List<KeyMap> list)
		{
			for (int i = 0, count = before.Count; i < count; i++)
			{
				before[i].AddToList(list);
			}
			if (main != null)
				list.Add(main);
			for (int i = 0, count = after.Count; i < count; i++)
			{
				after[i].AddToList(list);
			}
		}
	}
}
