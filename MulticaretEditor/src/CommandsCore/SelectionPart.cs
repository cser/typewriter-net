using System;

namespace MulticaretEditor
{
	public struct SelectionPart
	{
		public bool isCaret;
		public int index;
		
		public SelectionPart(bool isCaret, int index)
		{
			this.isCaret = isCaret;
			this.index = index;
		}
	}
}
