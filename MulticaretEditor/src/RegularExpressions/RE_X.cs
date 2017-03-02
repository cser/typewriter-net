using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class RE_X : RENode
	{
		private readonly RENode _next;
		private readonly bool _upper;
		
		public RE_X(bool upper, RENode next)
		{
			_upper = upper;
			_next = next;
		}
		
		public override RENode MatchChar(char c)
		{
			bool result = c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F';
			if (_upper)
			{
				result = !result && c != '\n' && c != '\r';
			}
			return result ? _next : RENode.fail;
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
			return "(" + (_upper ? "X" : "x") + _next + ")";
		}
	}
}