using System;
using System.Collections.Generic;

namespace MulticaretEditor.KeyMapping
{
	public class KeyMapNode
	{
		public readonly KeyMap main;
		public readonly int priority;

		public readonly RWList<KeyMapNode> _before;
		public readonly RWList<KeyMapNode> _after;

		public readonly IRList<KeyMapNode> before;
		public readonly IRList<KeyMapNode> after;

		public KeyMapNode(KeyMap main, int priority)
		{
			this.main = main;
			this.priority = priority;

			_before = new RWList<KeyMapNode>();
			before = _before;

			_after = new RWList<KeyMapNode>();
			after = _after;
		}

		public void AddBefore(KeyMap map)
		{
			AddBefore(map, 0);
		}

		public void AddBefore(KeyMap map, int priority)
		{
			AddBefore(new KeyMapNode(map, priority));
		}

		public void AddBefore(KeyMapNode node)
		{
			int i = _before.Count;
			for (; i-- > 0;)
			{
				if (_before[i].priority >= node.priority)
					break;
			}
			_before.Insert(i + 1, node);
		}

		public void RemoveBefore(KeyMap map)
		{
			for (int i = _before.Count; i-- > 0;)
			{
				if (_before[i].main == map)
					_before.RemoveAt(i);
			}
		}

		public void AddAfter(KeyMap map)
		{
			AddAfter(map, 0);
		}

		public void AddAfter(KeyMap map, int priority)
		{
			AddAfter(new KeyMapNode(map, priority));
		}

		public void AddAfter(KeyMapNode node)
		{
			int i = _after.Count;
			for (; i-- > 0;)
			{
				if (_after[i].priority >= node.priority)
					break;
			}
			_after.Insert(i + 1, node);
		}

		public void RemoveAfter(KeyMap map)
		{
			for (int i = _after.Count; i-- > 0;)
			{
				if (_after[i].main == map)
					_after.RemoveAt(i);
			}
		}

		public bool Enumerate<T>(Getter<KeyMap, T, bool> enumerator, T parameter)
		{
			for (int i = 0, count = _before.Count; i < count; i++)
			{
				if (_before[i].Enumerate<T>(enumerator, parameter))
					return true;
			}
			if (main != null && enumerator(main, parameter))
				return true;
			for (int i = 0, count = _after.Count; i < count; i++)
			{
				if (_after[i].Enumerate<T>(enumerator, parameter))
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
			for (int i = 0, count = _before.Count; i < count; i++)
			{
				_before[i].AddToList(list);
			}
			if (main != null)
				list.Add(main);
			for (int i = 0, count = _after.Count; i < count; i++)
			{
				_after[i].AddToList(list);
			}
		}
	}
}
