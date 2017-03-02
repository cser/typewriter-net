using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class REDot : RENode
	{
		private readonly RENode _next;
		
		public REDot(RENode next)
		{
			_next = next;
		}
		
		public override RENode MatchChar(char c)
		{
			return c != '\n' && c != '\r' ? _next : RENode.fail;
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
			return "(." + _next + ")";
		}
	}
}