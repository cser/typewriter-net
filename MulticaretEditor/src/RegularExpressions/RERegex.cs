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
		private bool _emptyMatched;
		
		public RERegex(string pattern)
		{
			_pattern = pattern;
			_root = new REParser().Parse(_pattern);
		}
		
		
		public int MatchLength(string text)
		{
			int length = text.Length;
			int matchLength = -1;
			List<RE.RENode> current = new List<RE.RENode>();
			List<RE.RENode> next = new List<RE.RENode>();
			bool addLow = true;
			if (_root.emptyEntry)
			{
				AddEmpty(current, _root, addLow);
			}
			else
			{
				current.Add(_root);
			}
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				for (int j = 0; j < current.Count; j++)
				{
					RE.RENode state = current[j];
					if (state.MatchChar(c))
					{
						if (state.next0 != null)
						{
							if (state.next0.emptyEntry)
							{
								_emptyMatched = false;
								AddEmpty(next, state.next0, addLow);
								if (_emptyMatched)
								{
									matchLength = i + 1;
									addLow = false;
								}
							}
							else if (!next.Contains(state.next0))
							{
								next.Add(state.next0);
							}
						}
						if (state.next0 == null && state.next1 == null)
						{
							matchLength = i + 1;
							addLow = false;
						}
					}
				}
				List<RE.RENode> temp = current;
				current = next;
				next = temp;
				next.Clear();
				if (current.Count == 0)
				{
					return matchLength;
				}
			}
			return matchLength;
		}
		
		private void AddEmpty(List<RE.RENode> nodes, RE.RENode node, bool addLow)
		{
			if (node.next0 != null)
			{
				if (node.next0.emptyEntry)
				{
					AddEmpty(nodes, node.next0, addLow);
				}
				else if (!nodes.Contains(node.next0))
				{
					nodes.Add(node.next0);
				}
			}
			else
			{
				_emptyMatched = true;
			}
			if (node.next1 != null)
			{
				if (node.next1.emptyEntry)
				{
					if (!node.next1Low || addLow)
					{
						AddEmpty(nodes, node.next1, addLow);
					}
				}
				else if (!nodes.Contains(node.next1))
				{
					nodes.Add(node.next1);
				}
			}
			else
			{
				_emptyMatched = true;
			}
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
						if (node.next1Low)
						{
							builder.Append("(-");
						}
						builder.Append(indexOf[node.next1]);
						if (node.next1Low)
						{
							builder.Append(')');
						}
					}
				}
				builder.Append(')');
			}
			return builder.ToString();
		}
	}
}