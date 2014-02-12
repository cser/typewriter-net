using System;
using System.Collections;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class SwitchList<T> : IRList<T>
	{
		public event Setter SelectedChange;
		
		private readonly List<T> list;
		private readonly List<T> history;
		
		public SwitchList()
		{
			list = new List<T>();
			history = new List<T>();
		}
		
		public void Clear()
		{
			list.Clear();
			history.Clear();
			SetSelected(default(T));
		}
		
		public int Count { get { return list.Count; } }
		
		public T[] ToArray()
		{
			return list.ToArray();
		}
		
		public bool Contains(T item)
		{
			return list.Contains(item);
		}
		
		public T this[int index] { get { return list[index]; } }
		
		public int IndexOf(T item)
		{
			return list.IndexOf(item);
		}
		
		public int IndexOf(T item, int index)
		{
			return list.IndexOf(item, index);
		}
		
		public int IndexOf(T item, int index, int count)
		{
			return list.IndexOf(item, index, count);
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			return list.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}
		
		public SwitchList<T> Add(T value)
		{
			int j = history.IndexOf(value);
			if (j == -1)
			{
				list.Add(value);
				history.Insert(0, value);
			}
			else
			{
				history.RemoveAt(j);
				history.Insert(0, value);
			}
			SetSelected(value);
			return this;
		}
		
		public SwitchList<T> Remove(T value)
		{
			int i = list.IndexOf(value);
			if (i != -1)
			{
				list.RemoveAt(i);
				history.Remove(value);
				if (object.Equals(selected, value))
				{
					if (i >= list.Count)
						i = list.Count - 1;
					if (i != -1)
					{
						T newSelected = list[i];
						history.Remove(newSelected);
						history.Insert(0, newSelected);
						SetSelected(newSelected);
					}
					else
					{
						SetSelected(default(T));
					}
				}
			}
			return this;
		}
		
		private T selected;
		public T Selected
		{
			get { return selected; }
			set
			{				
				int j = history.IndexOf(value);
				if (j != -1)
				{
					history.RemoveAt(j);
					history.Insert(0, value);
					SetSelected(value);
				}
			}
		}
		
		public T Oldest
		{
			get { return history.Count > 0 ? history[history.Count - 1] : default(T); }
		}
		
		private void SetSelected(T value)
		{
			if (!object.Equals(selected, value))
			{
				selected = value;
				if (SelectedChange != null)
					SelectedChange();
			}
		}
		
		private int index = 0;		
		private bool modePressed = false;
		private bool changed = false;
		
		public SwitchList<T> ModeOn()
		{
			modePressed = true;
			changed = false;
			return this;
		}
		
		public SwitchList<T> ModeOff()
		{
			if (modePressed)
			{
				modePressed = false;
				if (changed && index < history.Count)
				{
					T item = history[index];
					history.RemoveAt(index);
					history.Insert(0, item);
					index = 0;
					SetSelected(selected);
				}
			}
			return this;
		}
		
		public SwitchList<T> Down()
		{
			if (history.Count > 0 && modePressed)
			{
				changed = true;
				index = (index + 1) % history.Count;
				SetSelected(history[index]);
			}
			return this;
		}
	}
}
