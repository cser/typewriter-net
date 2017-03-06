using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class REParser
	{
		public REParser()
		{
		}
		
		private List<REToken> _tokens = new List<REToken>();
		private Stack<char> _operators = new Stack<char>();
		private Stack<RE.RENode> _operands = new Stack<RE.RENode>();
		
		public RE.RENode Parse(string pattern)
		{
			_tokens.Clear();
			_operators.Clear();
			_operands.Clear();
			for (int i = 0; i < pattern.Length; i++)
			{
				char c = pattern[i];
				if (c == '\\')
				{
					i++;
					if (i >= pattern.Length)
					{
						break;
					}
					c = pattern[i];
					_tokens.Add(new REToken('\\', c));
					continue;
				}
				_tokens.Add(new REToken('\0', c));
			}
			int index;
			return ParseSequence(_tokens.Count - 1, null, out index);
		}
		
		private RE.RENode ParseSequence(int index, RE.RENode next, out int nextIndex)
		{
			RE.RENode result = null;
			while (index >= 0)
			{
				REToken token = _tokens[index];
				if (token.type == '\\')
				{
					if (token.c == '|')
					{
						if (_operators.Count > 0)
						{
							char o = _operators.Peek();
							if (o == '|')
							{
								_operators.Pop();
								result = new RE.REAlternate(result, _operands.Pop());
								result.next0 = next;
							}
						}
						_operators.Push('|');
						_operands.Push(result);
						result = null;
						index--;
						continue;
					}
					if (token.c == '(')
					{
						index--;
						break;
					}
					else if (token.c == ')')
					{
						index--;
						result = ParseSequence(index, result, out index);
						continue;
					}
				}
				result = ParsePart(index, result, out index); 
			}
			if (_operators.Count > 0)
			{
				char o = _operators.Peek();
				if (o == '|')
				{
					_operators.Pop();
					result = new RE.REAlternate(result, _operands.Pop());
					result.next0 = next;
				}
			}
			nextIndex = index;
			return result;
		}
		
		private RE.RENode ParsePart(int index, RE.RENode next, out int nextIndex)
		{
			nextIndex = index - 1;
			REToken token = _tokens[index];
			if (token.type == '\0')
			{
				if (token.c == '.')
				{
					RE.RENode node = new RE.REDot();
					node.next0 = next;
					return node;
				}
				if (token.c == '*')
				{
					index--;
					if (index < 0)
					{
						return null;
					}
					token = _tokens[index];
					RE.RENode target = ParsePart(index, null, out nextIndex);
					RE.RENode node = new RE.RERepetition(target);
					node.next0 = next;
					return node;
				}
				{
					RE.REChar node = new RE.REChar(token.c);
					node.next0 = next;
					return node;
				}
			}
			if (token.type == '\\')
			{
				if (token.c == '.' || token.c == '*')
				{
					RE.REChar node = new RE.REChar(token.c);
					node.next0 = next;
					return node;
				}
				if (token.c == ')')
				{
					index--;
					if (index < 0)
					{
						return null;
					}
					return ParseSequence(index, next, out nextIndex);
				}
				if (token.c == 'w')
				{
					RE.RE_W node = new RE.RE_W(false);
					node.next0 = next;
					return node;
				}
				if (token.c == 'W')
				{
					RE.RE_W node = new RE.RE_W(true);
					node.next0 = next;
					return node;
				}
				if (token.c == 's')
				{
					RE.RE_S node = new RE.RE_S(false);
					node.next0 = next;
					return node;
				}
				if (token.c == 'S')
				{
					RE.RE_S node = new RE.RE_S(true);
					node.next0 = next;
					return node;
				}
				if (token.c == 'a')
				{
					RE.RE_A node = new RE.RE_A(false);
					node.next0 = next;
					return node;
				}
				if (token.c == 'A')
				{
					RE.RE_A node = new RE.RE_A(true);
					node.next0 = next;
					return node;
				}
				if (token.c == 'd')
				{
					RE.RE_D node = new RE.RE_D(false);
					node.next0 = next;
					return node;
				}
				if (token.c == 'D')
				{
					RE.RE_D node = new RE.RE_D(true);
					node.next0 = next;
					return node;
				}
				if (token.c == 'h')
				{
					RE.RE_H node = new RE.RE_H(false);
					node.next0 = next;
					return node;
				}
				if (token.c == 'H')
				{
					RE.RE_H node = new RE.RE_H(true);
					node.next0 = next;
					return node;
				}
				if (token.c == 'l')
				{
					RE.RE_L node = new RE.RE_L(false);
					node.next0 = next;
					return node;
				}
				if (token.c == 'L')
				{
					RE.RE_L node = new RE.RE_L(true);
					node.next0 = next;
					return node;
				}
				if (token.c == 'o')
				{
					RE.RE_O node = new RE.RE_O(false);
					node.next0 = next;
					return node;
				}
				if (token.c == 'O')
				{
					RE.RE_O node = new RE.RE_O(true);
					node.next0 = next;
					return node;
				}
				if (token.c == 'p')
				{
					RE.RE_P node = new RE.RE_P(false);
					node.next0 = next;
					return node;
				}
				if (token.c == 'P')
				{
					RE.RE_P node = new RE.RE_P(true);
					node.next0 = next;
					return node;
				}
				if (token.c == 'u')
				{
					RE.RE_U node = new RE.RE_U(false);
					node.next0 = next;
					return node;
				}
				if (token.c == 'U')
				{
					RE.RE_U node = new RE.RE_U(true);
					node.next0 = next;
					return node;
				}
				if (token.c == 'x')
				{
					RE.RE_X node = new RE.RE_X(false);
					node.next0 = next;
					return node;
				}
				if (token.c == 'X')
				{
					RE.RE_X node = new RE.RE_X(true);
					node.next0 = next;
					return node;
				}
			}
			return null;
		}
	}
}