using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class RE_P : RENode
	{
		private readonly RENode _next;
		private readonly bool _upper;
		
		public RE_P(bool upper, RENode next)
		{
			_upper = upper;
			_next = next;
		}
		
		public override RENode MatchChar(char c)
		{
			return c != '\t' && c != '\n' && c != '\r' && (!_upper || !char.IsDigit(c)) ? _next : RENode.fail;
		}
		
		public override void FillResetNodes(List<RENode> nodes)
		{
			if (_next != null)
			{
				_next.FillResetNodes(nodes);
			}
		}
		
		public override string ToString()
		{
			return "(" + (_upper ? "P" : "p") + _next + ")";
		}
	}
}