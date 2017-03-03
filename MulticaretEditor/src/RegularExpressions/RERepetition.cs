using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class RERepetition : RENode
	{
		private readonly RENode _node;
		private readonly RENode _next;
		
		private RENode _current;
		private bool _lastNextIsNull;
		
		public RERepetition(RENode node0, RENode next)
		{
			_node = node0;
			_next = next;
		}
		
		public override RENode MatchChar(char c)
		{
			matchHere = false;
			RENode node = _current.MatchChar(c);
			if (node == RENode.fail && _next == null && !_lastNextIsNull)
			{
				_lastNextIsNull = true;
				matchHere = true;
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
			_lastNextIsNull = false;
		}
		
		public override string ToString()
		{
			return "(" + _node + "*" + (_next != null ? "`" + _next : "") + ")";
		}
	}
}