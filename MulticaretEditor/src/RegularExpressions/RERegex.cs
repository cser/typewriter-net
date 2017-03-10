using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor
{
	public class RERegex
	{
		private readonly string _pattern;
		private readonly RE.RENode _root;
		private readonly RE.REEmpty _nextChar = new RE.REEmpty();
		
		public RERegex(string pattern)
		{
			_pattern = pattern;
			_root = new REParser().Parse(_pattern);
		}
		
		
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
					if (state.next1 != null)
						deque.Put(state.next1);
				}
				else if (state.emptyEntry)
				{
					deque.Push(state.next0);
					if (state.next0 != null)
						deque.Push(state.next0);
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
		
		public override string ToString()
		{
			return _pattern;
		}
		
		public string ToGraphString()
		{
			Dictionary<RE.RENode, int> indexOf = new Dictionary<RE.RENode, int>();
			Queue<RE.RENode> queue = new Queue<RE.RENode>();
			List<RE.RENode> nodes = new List<RE.RENode>();
			queue.Enqueue(_root);
			int i = 0;
			while (queue.Count > 0)
			{
				RE.RENode node = queue.Dequeue();
				if (indexOf.ContainsKey(node))
				{
					continue;
				}
				indexOf[node] = i;
				nodes.Add(node);
				i++;
				if (node.next0 != null)
				{
					queue.Enqueue(node.next0);
				}
				if (node.next1 != null)
				{
					queue.Enqueue(node.next1);
				}
			}
			StringBuilder builder = new StringBuilder();
			foreach (RE.RENode node in nodes)
			{
				builder.Append('(');
				builder.Append(indexOf[node]);
				builder.Append(node.ToString());
				if (node.next0 != null || node.next1 != null)
				{
					builder.Append(':');
					if (node.next0 != null)
					{
						builder.Append(indexOf[node.next0]);
						if (node.next1 != null)
						{
							builder.Append('|');
						}
					}
					if (node.next1 != null)
					{
						builder.Append(indexOf[node.next1]);
					}
				}
				builder.Append(')');
			}
			return builder.ToString();
		}
	}
}