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
		private Stack<RENode> _operands = new Stack<RENode>();
		
		public RENode Parse(string pattern)
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
		
		private RENode ParseSequence(int index, RENode next, out int nextIndex)
		{
			RENode result = null;
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
								result = new REAlternate(result, _operands.Pop(), next);
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
					result = new REAlternate(result, _operands.Pop(), next);
				}
			}
			nextIndex = index;
			return result;
		}
		
		private RENode ParsePart(int index, RENode next, out int nextIndex)
		{
			nextIndex = index - 1;
			REToken token = _tokens[index];
			if (token.type == '\0')
			{
				if (token.c == '.')
				{
					return new REDot(next);
				}
				if (token.c == '*')
				{
					index--;
					if (index < 0)
					{
						return null;
					}
					token = _tokens[index];
					RENode target = ParsePart(index, null, out nextIndex);
					return new RERepetition(target, next);
				}
				return new REChar(token.c, next);
			}
			if (token.type == '\\')
			{
				if (token.c == '.' || token.c == '*')
				{
					return new REChar(token.c, next);
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
					return new RE_W(false, next);
				}
				if (token.c == 'W')
				{
					return new RE_W(true, next);
				}
				if (token.c == 's')
				{
					return new RE_S(false, next);
				}
				if (token.c == 'S')
				{
					return new RE_S(true, next);
				}
				if (token.c == 'a')
				{
					return new RE_A(false, next);
				}
				if (token.c == 'A')
				{
					return new RE_A(true, next);
				}
				if (token.c == 'd')
				{
					return new RE_D(false, next);
				}
				if (token.c == 'D')
				{
					return new RE_D(true, next);
				}
				if (token.c == 'h')
				{
					return new RE_H(false, next);
				}
				if (token.c == 'H')
				{
					return new RE_H(true, next);
				}
				if (token.c == 'l')
				{
					return new RE_L(false, next);
				}
				if (token.c == 'L')
				{
					return new RE_L(true, next);
				}
				if (token.c == 'o')
				{
					return new RE_O(false, next);
				}
				if (token.c == 'O')
				{
					return new RE_O(true, next);
				}
				if (token.c == 'p')
				{
					return new RE_P(false, next);
				}
				if (token.c == 'P')
				{
					return new RE_P(true, next);
				}
				if (token.c == 'u')
				{
					return new RE_U(false, next);
				}
				if (token.c == 'U')
				{
					return new RE_U(true, next);
				}
				if (token.c == 'x')
				{
					return new RE_X(false, next);
				}
				if (token.c == 'X')
				{
					return new RE_X(true, next);
				}
			}
			return null;
		}
	}
}