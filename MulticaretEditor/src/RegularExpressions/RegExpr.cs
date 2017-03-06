using System;
using System.Collections.Generic;
using MulticaretEditor;

public class RegExpr
{
	private const int MAXSTATES = 100;
	private const int next_char = -1;
	private const int REGEXPR_NOT_FOUND = -1;
	private const int REGEXPR_NOT_COMPILED = -2;
	
	public struct State
	{
		public char the_char;
		public int next1;
		public int next2;
	}
	
	protected State[] _automaton = new State[MAXSTATES];
	protected Deque<int> _deque = new Deque<int>();
	protected int _j;
	protected int _state;
	protected string _p;
	
	public RegExpr()
	{
		_automaton[0].next1 = 0;
		_automaton[0].next2 = 0;
	}
	
	public RegExpr(string pattern)
	{
       	_p = pattern;
		_state = 0;
		_j = 0;	
		for (int i = 0; i < MAXSTATES; i++)
		{
			_automaton[i].the_char = '\0';
			_automaton[i].next1 = 0;
			_automaton[i].next2 = 0;
		}
		int n = ParseSequence();
		if (_automaton[0].next1 == 0)
		{
			_automaton[0].next1 = n;
			_automaton[0].next2 = n;
		}
		_automaton[_state].next1 = 0;
		_automaton[_state].next2 = 0;
	}

	protected int ParseSequence()
	{
		int s1 = _state++;
		int s2 = ParseElement();
		int s3;
		if (_p[_j] == '|')
		{
			_j++;
			s3 = ++_state;
			_automaton[s3].next1 = s2;
			_automaton[s3].next2 = ParseSequence();
			_automaton[s3-1].next1 = _state;
			_automaton[s3-1].next2 = _state;
			if (_automaton[s1].next1 == s2 || _automaton[s1].next1 == 0)
				_automaton[s1].next1 = s3;
			if (_automaton[s1].next2 == s2 || _automaton[s1].next2 == 0)
				_automaton[s1].next2 = s3;
			return s3;
		}
		return s2;
	}
	
	protected int ParseElement()
	{
		int s1 = _state;
		int s2;
		if (_p[_j] == '(')
		{
			_j++;
			s2 = ParseSequence();
			if (_p[_j] == ')')
			{
				_automaton[_state].next1 = _automaton[_state].next2 = _state + 1;
				_state++;
				_j++;
			}
			else
			{
				throw new Exception("Error at pos: " + _j);
			}
		}
		else
		{
			s2 = ParseV();
		}
		if (_p[_j] == '*')
		{
			_automaton[_state].next1 = s2;
			_automaton[_state].next2 = _state + 1;
			s1 = _state;
			if (_automaton[s2-1].next1 == s2 || _automaton[s2-1].next1 == 0)
				_automaton[s2-1].next1 = _state;
			if (_automaton[s2-1].next2 == s2 || _automaton[s2-1].next2 == 0)
				_automaton[s2-1].next2 = _state;
			_state++;
			_j++;
		}
		else
		{
			if (_automaton[s1-1].next1 == 0)
				_automaton[s1-1].next1 = s2;
			if (_automaton[s1-1].next2 == 0)
				_automaton[s1-1].next2 = s2;
		}
		if (_p[_j] != '|' && _p[_j] != ')' && _p[_j] != '*' && _p[_j] != '\0')
		{
			ParseElement();
		}
		return s1;
	}
	
	protected int ParseV()
	{
		int s1 = _state;
		if (_p[_j] == '\\')
			_j++;
		else if (!IsLetter(_p[_j]))
			throw new Exception("Error at pos: " + _j);
		_automaton[_state].the_char = _p[_j++];
		_automaton[_state].next1 = _automaton[_state].next2 = _state + 1;
		_state++;
		return s1;
	}

	protected bool IsLetter(char c)
	{
		return c != '|' && c != '(' && c != ')' && c != '*' && c != '\\' && c != '\0';
	}
	
	public int Search(string str)
	{
		return Search(str, 0);
	}
	
	public int Search(string str, int start)
	{
		int dummy;
		return SearchLen(str, out dummy, start);
	}
	
	public int SearchLen(string str, out int len, int start)
	{
		len = -1;
		if (_automaton[0].next1 == 0 && _automaton[0].next2 == 0)
			return REGEXPR_NOT_COMPILED;
		int slen = str.Length;
		for (int n = start; n < slen; n++)
		{
			int m = Simulate(str, n);
			if (m < n - 1)
			{
				len = m - n + 1;
				return n;
			}
		}
		return REGEXPR_NOT_FOUND;
	}

	protected int Simulate(string str, int j)
	{
		int state = _automaton[0].next1;
		int lastMatch = j - 1;
		int len = str.Length;
		if (_automaton[0].next1 != _automaton[0].next2)
			_deque.Push(_automaton[0].next2);
		_deque.Put(next_char);
		do
		{
			if (state == next_char)
			{
				if (str[_j] != '\0')
					_j++;
				_deque.Put(next_char);
			}
			else if (_automaton[state].the_char == str[j])
			{
				_deque.Put(_automaton[state].next1);
				if (_automaton[state].next1 != _automaton[state].next2)
					_deque.Put(_automaton[state].next2);
			}
			else if (_automaton[state].the_char == '\0')
			{
				_deque.Push(_automaton[state].next1);
				if (_automaton[state].next1 != _automaton[state].next2)
					_deque.Push(_automaton[state].next2);
			}
			state = _deque.Pop();
			if (state == 0)
			{
				lastMatch = j - 1;
				state = _deque.Pop();
			}
		}
		while (j <= len && !_deque.IsEmpty);
		return lastMatch;
	}
}