using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class REChar : RENode
	{
		private readonly char _c;
		private readonly RENode _next;
		
		public REChar(char c, RENode next)
		{
			_c = c;
			_next = next;
		}
		
		public override RENode MatchChar(char c)
		{
			return _c == c ? _next : RENode.fail;
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
			return "('" + _c + "'" + _next + ")";
		}
	}
}