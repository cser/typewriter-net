using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor
{
	public class HistoryNode
	{
		public Command command;
		public readonly int index;
		
		public HistoryNode(Command command, int index)
		{
			this.command = command;
			this.index = index;
		}
		
		public HistoryNode prev;
		public readonly List<HistoryNode> nexts = new List<HistoryNode>(1);
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
			foreach (HistoryNode node in nexts)
			{
				if (!first)
					text.Append(", ");
				first = false;
				text.Append(GetSimpleText(node));
			}
			text.Append("]");
			return text.ToString();
		}
		
		private string GetSimpleText(HistoryNode node)
		{
			if (node == null)
				return "null";
			return "command: " + (node.command != null ? node.command + "" : "null");
		}
	}
}
