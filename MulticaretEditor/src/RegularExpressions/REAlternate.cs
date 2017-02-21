using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class REAlternate : RENode
	{
		private readonly RENode _node0;
		private readonly RENode _node1;
		private readonly RENode _next;
		
		private RENode _current0;
		private RENode _current1;
		
		public REAlternate(RENode node0, RENode node1, RENode next)
		{
			_node0 = node0;
			_node1 = node1;
			_next = next;
		}
		
		public override RENode MatchChar(char c)
		{
			_current0 = _current0.MatchChar(c);
			if (_current0 == null)
			{
				return _next;
			}
			_current1 = _current1.MatchChar(c);
			if (_current1 == null)
			{
				return _next;
			}
			return this;
		}
		
		public override void FillResetNodes(List<RENode> nodes)
		{
			nodes.Add(this);
			if (_node0 != null)
			{
				_node0.FillResetNodes(nodes);
			}
			if (_node1 != null)
			{
				_node1.FillResetNodes(nodes);
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
			_current0 = _node0;
			_current1 = _node1;
		}
		
		public override string ToString()
		{
			return "(" + _node0 + "|" + _node1 + (_next != null ? "`" + _next : "") + ")";
		}
	}
}