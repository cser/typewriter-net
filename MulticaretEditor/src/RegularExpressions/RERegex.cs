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
		
		private readonly RE.REEmpty _nextChar = new RE.REEmpty();
		
		public int MatchLength(string text)
		{
			int length = text.Length;
			int matchLength = -1;
			Deque<RE.RENode> deque = new Deque<RE.RENode>();
			RE.RENode state = _root.next0;
			if (_root.next1 != null)
			{
				deque.Push(_root.next1);
			}
			deque.Put(_nextChar);
			int j = 0;
			do
			{
				if (state == _nextChar)
				{
					if (j < length - 1)
						j++;
					deque.Put(_nextChar);
				}
				else if (!state.emptyEntry && state.MatchChar(text[j]))
				{
					deque.Put(state.next0);
					if (state.next0 != state.next1)
						deque.Put(state.next1);
				}
				else if (state.emptyEntry)
				{
					deque.Push(state.next0);
					if (state.next0 != state.next1)
						deque.Push(state.next1);
				}
				state = deque.Pop();
				if (state == _root)
				{
					matchLength = j - 1;
					state = deque.Pop();
				}
			}
			while (j < length && !deque.IsEmpty);
			return matchLength;
		}
	}
}