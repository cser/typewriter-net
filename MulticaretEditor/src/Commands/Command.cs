using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public abstract class Command
	{
		public readonly CommandType type;
		
		public Command(CommandType type)
		{
			this.type = type;
		}
		
		public LineArray lines;
		public List<Selection> selections;
		
		virtual public bool Init()
		{
			return true;
		}
		
		abstract public void Redo();
		
		abstract public void Undo();
		
		public CommandTag tag;
		public bool marked = false;
		
		protected SelectionMemento[] GetSelectionMementos()
		{
			SelectionMemento[] mementos = new SelectionMemento[selections.Count];
			for (int i = 0; i < mementos.Length; i++)
			{
				mementos[i] = selections[i].Memento;
				mementos[i].index = i;
			}
			Array.Sort(mementos, CompareSelections);
			return mementos;
		}
		
		protected void SetSelectionMementos(SelectionMemento[] mementos)
		{
			for (int i = selections.Count; i < mementos.Length; i++)
			{
				selections.Add(new Selection());
			}
			if (selections.Count > mementos.Length)
				selections.RemoveRange(mementos.Length, selections.Count - mementos.Length);
			for (int i = 0; i < mementos.Length; i++)
			{
				selections[mementos[i].index].Memento = mementos[i];
			}
		}
		
		protected static int CompareSelections(SelectionMemento a, SelectionMemento b)
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
