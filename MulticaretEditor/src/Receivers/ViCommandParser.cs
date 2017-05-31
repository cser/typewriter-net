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
			WaitChar,
			WaitRegister,
			WaitObject
		}
		
		private readonly bool _visualMode;
		private ParseResult _lastResult;
		private State _state;
		private string _stateText;
		
		public int rawCount;
		public ViChar move;
		public ViChar moveChar;
		public ViChar action;
		public char register;
		public string shortcut;
		
		public int FictiveCount { get { return rawCount > 0 ? rawCount : 1; } }
		
		public ViCommandParser(bool visualMode)
		{
			_visualMode = visualMode;
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
			register = '\0';
			shortcut = null;
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
					if (code.c == '"')
					{
						_state = State.WaitRegister;
						return ParseResult.WaitNext;
					}
					if (code.c == '/')
					{
						shortcut = "/";
						return ParseResult.Complete;
					}
					if (code.c == ':')
					{
						shortcut = ":";
						return ParseResult.Complete;
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
							case 'r':
							case 'j':
							case 'k':
							case 'd':
							case 'D':
							case 'J':
							case 'K':
								action = code;
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
							case 'e':
							case 'b':
							case '0':
							case '^':
							case '$':
							case 'G':
							case 'n':
							case 'N':
								move = code;
								return ParseResult.Complete;
							case 'v':
							case 'V':
							case '*':
								action = code;
								return ParseResult.Complete;
							case 'd':
								if (action.IsChar('d'))
								{
									move = code;
									return ParseResult.Complete;
								}
								action = code;
								return _visualMode ? ParseResult.Complete : ParseResult.WaitNext;
							case 'c':
							case 'y':
								if (action.IsChar('y'))
								{
									move = code;
									return ParseResult.Complete;
								}
								action = code;
								return _visualMode ? ParseResult.Complete : ParseResult.WaitNext;
							case '>':
								if (action.IsChar('>'))
								{
									move = code;
									return ParseResult.Complete;
								}
								action = code;
								return _visualMode ? ParseResult.Complete : ParseResult.WaitNext;
							case '<':
								if (action.IsChar('<'))
								{
									move = code;
									return ParseResult.Complete;
								}
								action = code;
								return _visualMode ? ParseResult.Complete : ParseResult.WaitNext;
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
								return _visualMode ? ParseResult.Complete : ParseResult.WaitNext;
							case 'I':
							case 'A':
							case 'u':
							case 'x':
							case 'p':
							case 'P':
							case 'J':
							case '.':
							case 'o':
							case 'O':
							case 'C':
							case 'D':
								action = code;
								return ParseResult.Complete;
							case 'i':
							case 'a':
								if (action.c != '\0' || _visualMode)
								{
									move = code;
									_state = State.WaitObject;
									return ParseResult.WaitNext;
								}
								action = code;
								return ParseResult.Complete;
							case 's':
								action = code;
								return ParseResult.Complete;
						}
						if (char.IsNumber(code.c))
						{
							_stateText = "";
							_state = State.Count;
							return Parse(code);
						}
					}
					return ParseResult.Incorrect;
				case State.WaitChar:
					if (!move.control && move.c == 'g')
					{
						if (code.c == 'j' || code.c == 'k')
						{
							moveChar = move;
							move = code;
							return ParseResult.Complete;
						}
					}
					moveChar = code;
					return ParseResult.Complete;
				case State.WaitRegister:
					register = code.c;
					_state = State.Init;
					return ParseResult.WaitNext;
				case State.WaitObject:
					moveChar = code;
					return ParseResult.Complete;
			}
			return ParseResult.Incorrect;
		}
	}
}