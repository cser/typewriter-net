using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class RENode
	{
		public static readonly RENode fail = new RENode();
		
		public bool matchHere;
		
		public virtual RENode MatchChar(char c)
		{
			return null;
		}
		
		public virtual bool NeedReset()
		{
			return false;
		}
		
		public virtual void FillResetNodes(List<RENode> nodes)
		{
		}
		
		public virtual void Reset()
		{
		}
		
		public override string ToString()
		{
			return "Node";
		}
	}
}