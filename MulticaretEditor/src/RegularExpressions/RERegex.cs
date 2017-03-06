using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class RERegex
	{
		private readonly RE.RENode _root;
		private RE.RENode[] _resetNodes;
		
		public RERegex(RE.RENode node)
		{
			_root = node;
		}
		
		public RERegex(string pattern) : this(new REParser().Parse(pattern))
		{
		}
		
		public int MatchLength(string text)
		{
			int matchLength = -1;
			/*RE.RENode nodeI = _root;
			for (int i = 0; i < text.Length; i++)
			{
				nodeI = nodeI.MatchChar(text[i]);
				if (nodeI == RENode.fail)
				{
					break;
				}
				if (nodeI == null)
				{
					matchLength = i + 1;
					break;
				}
			}*/
			return matchLength;
		}
	}
}