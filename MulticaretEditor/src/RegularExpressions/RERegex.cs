using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class RERegex
	{
		private readonly RENode _root;
		private RENode[] _resetNodes;
		
		public RERegex(RENode node)
		{
			_root = node;
			List<RENode> resetNodes = new List<RENode>();
			_root.FillResetNodes(resetNodes);
			_resetNodes = resetNodes.ToArray();
		}
		
		public RERegex(string pattern) : this(new REParser().Parse(pattern))
		{
		}
		
		public int MatchLength(string text)
		{
			for (int i = 0; i < _resetNodes.Length; i++)
			{
				_resetNodes[i].Reset();
			}
			int matchLength = -1;
			RENode nodeI = _root;
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
				if (nodeI.matchHere)
				{
					matchLength = i;
				}
			}
			return matchLength;
		}
	}
}