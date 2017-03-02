using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class RE_S : RENode
	{
		private readonly RENode _next;
		private readonly bool _upper;
		
		public RE_S(bool upper, RENode next)
		{
			_upper = upper;
			_next = next;
		}
		
		public override RENode MatchChar(char c)
		{
			return char.IsWhiteSpace(c) ^ _upper && c != '\n' && c != '\r' ? _next : RENode.fail;
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
			return "(" + (_upper ? "S" : "s") + _next + ")";
		}
	}
}