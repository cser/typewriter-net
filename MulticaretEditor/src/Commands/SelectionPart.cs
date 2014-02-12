using System;

namespace MulticaretEditor.Commands
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
