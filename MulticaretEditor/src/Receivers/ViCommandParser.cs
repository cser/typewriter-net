using System;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class ViCommandParser
	{
		public enum ParseResult
		{
			Complete,
			WaitNext,
			Incorrect
		}
		
		public enum State
		{
			Init,
			Count,
			Action,
			WaitChar
		}
		
		private ParseResult _lastResult;
		private State _state;
		private string _stateText;
		
		public int rawCount;
		public ViChar move;
		public ViChar moveChar;
		public ViChar action;
		
		public int FictiveCount { get { return rawCount > 0 ? rawCount : 1; } }
		
		public ViCommandParser()
		{
			Reset();
		}
		
		public void Reset()
		{
			_lastResult = ParseResult.WaitNext;
			_state = State.Init;
			_stateText = "";
			
			rawCount = -1;
			move = new ViChar('\0', false);
			moveChar = new ViChar('\0', false);
			action = new ViChar('\0', false);
		}
		
		public bool AddKey(ViChar code)
		{
			if (_lastResult != ParseResult.WaitNext)
			{
				Reset();
			}
			_lastResult = Parse(code);
			return _lastResult == ParseResult.Complete;
		}
		
		private ParseResult Parse(ViChar code)
		{
			switch (_state)
			{
				case State.Init:
					if (char.IsNumber(code.c) && code.c != '0')
					{
						_state = State.Count;
						return Parse(code);
					}
					_state = State.Action;
					return Parse(code);
				case State.Count:
					if (char.IsNumber(code.c))
					{
						_stateText += code;
						return ParseResult.WaitNext;
					}
					if (_stateText != "")
					{
						rawCount = int.Parse(_stateText);
						_stateText = "";
					}
					_state = State.Action;
					return Parse(code);
				case State.Action:
					if (code.control)
					{
						switch (code.c)
						{
							case 'f':
							case 'b':
								move = code;
								return ParseResult.Complete;
						}
					}
					else
					{
						switch (code.c)
						{
							case 'j':
							case 'k':
							case 'h':
							case 'l':
							case 'w':
							case 'b':
							case '0':
							case '^':
							case '$':
							case 'G':
								move = code;
								return ParseResult.Complete;
							case 'd':
							case 'c':
							case 'y':
								action = code;
								return ParseResult.WaitNext;
							case 'f':
							case 'F':
							case 't':
							case 'T':
							case 'g':
								_state = State.WaitChar;
								move = code;
								return ParseResult.WaitNext;
							case 'r':
								_state = State.WaitChar;
								action = code;
								return ParseResult.WaitNext;
						}
					}
					switch (code.c)
					{
						case 'i':
						case 'a':
						case 'I':
						case 'A':
						case 'u':
						case 'x':
						case 'p':
						case 'P':
							action = code;
							return ParseResult.Complete;
						case 'r':
							if (code.control)
							{
								action = code;
								return ParseResult.Complete;
							}
							break;
					}
					if (char.IsNumber(code.c))
					{
						_stateText = "";
						_state = State.Count;
						return Parse(code);
					}
					return ParseResult.Incorrect;
				case State.WaitChar:
					moveChar = code;
					return ParseResult.Complete;
			}
			return ParseResult.Incorrect;
		}
	}
}