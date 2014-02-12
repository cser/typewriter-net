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
	}
}
