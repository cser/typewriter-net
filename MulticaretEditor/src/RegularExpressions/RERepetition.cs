using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class RERepetition : RENode
	{
		private readonly RENode _node;
		private readonly RENode _next;
		
		private RENode _current;
		
		public RERepetition(RENode node0, RENode next)
		{
			_node = node0;
			_next = next;
		}
		
		public override RENode MatchChar(char c)
		{
			RENode node = _current.MatchChar(c);
			if (node == RENode.fail)
			{
				return _next;
			}
			return this;
		}
		
		public override void FillResetNodes(List<RENode> nodes)
		{
			nodes.Add(this);
			if (_node != null)
			{
				_node.FillResetNodes(nodes);
			}
			if (_next != null)
			{
				_next.FillResetNodes(nodes);
			}
		}
		
		public override bool NeedReset()
		{
			return true;
		}
		
		public override void Reset()
		{
			_current = _node;
		}
		
		public override string ToString()
		{
			return "(" + _node + "*" + (_next != null ? "`" + _next : "") + ")";
		}
	}
}