using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor.Commands
{
	public class Node
	{
		public Command command;
		public readonly int index;
		
		public Node(Command command, int index)
		{
			this.command = command;
			this.index = index;
		}
		
		public Node prev;
		public readonly List<Node> nexts = new List<Node>(1);
		public readonly List<CommandTag> tags = new List<CommandTag>(1);
		public bool main;
		
		override public string ToString()
		{
			StringBuilder text = new StringBuilder();
			text.Append(GetSimpleText(prev));
			text.Append(" <- (");
			text.Append(GetSimpleText(this));
			text.Append(") -> [");
			bool first = true;
			foreach (Node node in nexts)
			{
				if (!first)
					text.Append(", ");
				first = false;
				text.Append(GetSimpleText(node));
			}
			text.Append("]");
			return text.ToString();
		}
		
		private string GetSimpleText(Node node)
		{
			if (node == null)
				return "null";
			return "command: " + (node.command != null ? node.command + "" : "null");
		}
	}
}
