using System;

namespace MulticaretEditor
{
	public struct SelectionMemento
	{
		public SelectionMemento(int anchor, int caret, int preferredPos)
		{
			this.anchor = anchor;
			this.caret = caret;
			this.preferredPos = preferredPos;
			index = 0;
		}
		
		public int anchor;
		public int caret;
		public int preferredPos;
		public int index;
		
		public int Left { get { return anchor < caret ? anchor : caret; } }
		public int Right { get { return anchor < caret ? caret : anchor; } }
		public int Count { get { return anchor > caret ? anchor - caret : caret - anchor; } }
		public bool Empty { get { return anchor == caret; } }
		
		public static int CompareSelections(SelectionMemento a, SelectionMemento b)
		{
			int aLeft = a.anchor < a.caret ? a.anchor : a.caret;
			int bLeft = b.anchor < b.caret ? b.anchor : b.caret;
			int result;
			if (aLeft == bLeft)
			{
				result = (a.anchor < a.caret ? a.caret : a.anchor) - (b.anchor < b.caret ? b.caret : b.anchor);
			}
			else
			{
				result = aLeft - bLeft;
			}
			return result;
		}
	}
}
