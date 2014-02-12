using System;
using System.Collections.Generic;

namespace MulticaretEditor.Commands
{
	public class CommandTag
	{
		public readonly int id;
		
		public CommandTag(int id)
		{
			this.id = id;
		}
		
		private Node prev;
		public Node Prev
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
		
		public readonly List<Node> redos = new List<Node>();
		
		override public string ToString()
		{
			return "<" + id + ">";
		}
	}
}
