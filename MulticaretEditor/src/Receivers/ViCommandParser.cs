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
			WaitObject,
			Leader
		}
		
		public class LastCommand
		{
			public int rawCount;
			public ViChar move;
			public ViChar moveChar;
			public ViChar action;
			public char register;
			public string shortcut;
			
			public ViReceiverData startData;
		}
		
		public LastCommand GetLastCommand()
		{
			LastCommand info = new LastCommand();
			info.rawCount = rawCount;
			info.move = move;
			info.moveChar = moveChar;
			info.action = action;
			info.register = register;
			info.shortcut = shortcut;
			return info;
		}
		
		public void SetLastCommand(LastCommand info)
		{
			_lastResult = ParseResult.Complete;
			_state = State.Init;
			_stateText = "";
			rawCount = info.rawCount;
			move = info.move;
			moveChar = info.moveChar;
			action = info.action;
			register = info.register;
			shortcut = info.shortcut;
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
		
		public string GetFictiveShortcut()
		{
			string shortcut = this.shortcut;
			if (!string.IsNullOrEmpty(shortcut))
			{
				return shortcut;
			}
			if (action.control || move.control || moveChar.control || moveChar.c != '\0' || register != '\0')
			{
				return null;
			}
			shortcut = "";
			if (rawCount > 0)
			{
				shortcut += rawCount;
			}
			if (action.c != '\0')
			{
				shortcut += action.c;
			}
			if (move.c != '\0')
			{
				shortcut += move.c;
			}
			return shortcut;
		}
		
		public bool IsIdle { get { return _lastResult == ParseResult.Complete ||
			_lastResult == ParseResult.Incorrect; } }
		
		public int FictiveCount { get { return rawCount > 0 ? rawCount : 1; } }
		
		public ViCommandParser(bool visualMode)
		{
			_visualMode = visualMode;
			Reset();
		}
		
		public void Reset()
		{
			_lastResult = ParseResult.Complete;
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
							case 'o':
							case 'i':
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
							case 'W':
							case 'e':
							case 'E':
							case 'b':
							case 'B':
							case '0':
							case '^':
							case '$':
							case 'G':
							case 'n':
							case 'N':
							case '%':
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
								if (action.IsChar('c'))
								{
									move = code;
									return ParseResult.Complete;
								}
								action = code;
								return _visualMode ? ParseResult.Complete : ParseResult.WaitNext;
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
							case '\'':
							case '`':
								_state = State.WaitChar;
								move = code;
								return ParseResult.WaitNext;
							case 'm':
								_state = State.WaitChar;
								action = code;
								return ParseResult.WaitNext;
							case 'r':
							case ' ':
								_state = State.WaitChar;
								action = code;
								return ParseResult.WaitNext;
							case 'I':
							case 'A':
							case 'u':
							case 'U':
							case 'x':
							case '~':
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
							case '\b':
							case '\r':
								action = code;
								return ParseResult.Complete;
							case ',':
							case '\\':
								action = new ViChar(',', false);
								_state = State.Leader;
								_stateText = "";
								return ParseResult.WaitNext;
						}
						if (char.IsNumber(code.c))
						{
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
				case State.Leader:
					if (!code.control)
					{
						switch (code.c)
						{
							case ' ':
								move = code;
								_state = State.WaitChar;
								return ParseResult.WaitNext;
							case 'b':
								shortcut = "\\b";
								return ParseResult.Complete;
							case 'h':
								shortcut = "\\h";
								return ParseResult.Complete;
							case 'H':
								shortcut = "\\H";
								return ParseResult.Complete;
							case 'n':
								shortcut = "\\n";
								return ParseResult.Complete;
							case 'N':
								shortcut = "\\N";
								return ParseResult.Complete;
							case 's':
								shortcut = "\\s";
								return ParseResult.Complete;
							case 'r':
								shortcut = "\\r";
								return ParseResult.Complete;
							case 'c':
								shortcut = "\\c";
								return ParseResult.Complete;
							case 'f':
								shortcut = "\\f";
								return ParseResult.Complete;
							case 'g':
								shortcut = "\\g";
								return ParseResult.Complete;
						}
					}
					return ParseResult.Incorrect;
			}
			return ParseResult.Incorrect;
		}
	}
}