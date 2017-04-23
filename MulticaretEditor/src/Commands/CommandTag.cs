using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class CommandTag
	{
		public readonly int id;
		
		public CommandTag(int id)
		{
			this.id = id;
		}
		
		private HistoryNode prev;
		public HistoryNode Prev
		{
			get { return prev; }
			set
			{
				if (prev != null)
					prev.tags.Remove(this);
				prev = value;
				if (prev != null && !prev.tags.Contains(this))
					prev.tags.Add(this);
			}
		}
		
		public readonly List<HistoryNode> redos = new List<HistoryNode>();
		
		override public string ToString()
		{
			return "<" + id + ">";
		}
	}
}
