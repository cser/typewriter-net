using System;

namespace MulticaretEditor
{
	public class Selection
	{
		public Selection()
		{
		}
		
		public int anchor;
		public int caret;
		public int preferredPos;
		public int wwPreferredPos;
		public bool needRemove;
		
		public int Count { get { return Math.Abs(caret - anchor); } }
		public int Left { get { return Math.Min(anchor, caret); } }
		public int Right { get { return Math.Max(anchor, caret); } }
		public bool Empty { get { return anchor == caret; } }
		
		public SelectionMemento Memento
		{
			get { return new SelectionMemento(anchor, caret, preferredPos); }
			set
			{
				anchor = value.anchor;
				caret = value.caret;
				preferredPos = value.preferredPos;
			}
		}
		
		public bool Contains(int position)
		{
			return anchor < caret ? position >= anchor && position <= caret : position >= caret && position <= anchor;
		}
		
		public void SetEmptyIfNotShift(bool shift)
		{
			if (!shift)
			{
				anchor = caret;
			}
		}
		
		override public string ToString()
		{
			return "(anchor:" + anchor + ", caret:" + caret + ")";
		}
	}
}
