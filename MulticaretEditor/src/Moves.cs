using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public struct Moves
	{
		private LineArray _lines;
		private PlaceIterator _iterator;
		
		public int Position { get { return _iterator.Position; } }
		public Place Place { get { return _iterator.Place; } }
		
		public Moves(LineArray lines, int position)
		{
			_lines = lines;
			_iterator = lines.GetCharIterator(position);
		}
		
		public void NPP_WordRight(bool shiftMove)
		{
			if (shiftMove)
			{
				while (IsSpace(_iterator.RightChar) && _iterator.MoveRightWithRN());
			}
			CharType type = GetCharType(_iterator.RightChar);
			if (type != CharType.Space)
			{
				for (CharType typeI = type;
					typeI == type && _iterator.MoveRightWithRN();
					typeI = GetCharType(_iterator.RightChar));
			}
			if (!shiftMove)
			{
				while (IsSpace(_iterator.RightChar) && _iterator.MoveRightWithRN());
			}
		}
		
		public void NPP_WordLeft()
		{
			while (IsSpace(_iterator.LeftChar) && _iterator.MoveLeftWithRN());
			CharType type = GetCharType(_iterator.LeftChar);
			if (type != CharType.Space)
			{
				for (CharType typeI = type;
					typeI == type && _iterator.MoveLeftWithRN();
					typeI = GetCharType(_iterator.LeftChar));
			}
		}
		
		public void Vi_w(bool change, bool allowNewLine)
		{
			if (GetCharType(_iterator.RightChar) == CharType.Identifier)
			{
				while (GetCharType(_iterator.RightChar) == CharType.Identifier &&
					_iterator.MoveRightWithRN());
				if (!change && IsSpaceOrNewLine(_iterator.RightChar, allowNewLine))
				{
					while (IsSpaceOrNewLine(_iterator.RightChar, allowNewLine) &&
						_iterator.MoveRightWithRN());
				}
			}
			else if (GetCharType(_iterator.RightChar) == CharType.Punctuation)
			{
				while (GetCharType(_iterator.RightChar) == CharType.Punctuation &&
					_iterator.MoveRightWithRN());
				if (!change && IsSpaceOrNewLine(_iterator.RightChar, allowNewLine))
				{
					while (IsSpaceOrNewLine(_iterator.RightChar, allowNewLine) &&
						_iterator.MoveRightWithRN());
				}
			}
			else if (IsSpaceOrNewLine(_iterator.RightChar, allowNewLine))
			{
				while (IsSpaceOrNewLine(_iterator.RightChar, allowNewLine) &&
					_iterator.MoveRightWithRN());
			}
		}
		
		public void Vi_e(bool shift)
		{
			char c = _iterator.RightChar;
			if (IsSpaceOrNewLine(c))
			{
				while (true)
				{
					if (!_iterator.MoveRightWithRN())
						return;
					if (!IsSpaceOrNewLine(_iterator.RightChar))
						break;
				}
				CharType type = GetCharType(_iterator.RightChar);
				while (true)
				{
					if (!_iterator.MoveRightWithRN())
						return;
					if (GetCharType(_iterator.RightChar) != type)
						break;
				}
				if (!shift)
				{
					_iterator.MoveLeftWithRN();
				}
			}
			else
			{
				if (!_iterator.MoveRightWithRN())
					return;
				if (IsSpaceOrNewLine(_iterator.RightChar))
				{
					while (true)
					{
						if (!_iterator.MoveRightWithRN())
							return;
						if (!IsSpaceOrNewLine(_iterator.RightChar))
							break;
					}
					CharType type = GetCharType(_iterator.RightChar);
					while (true)
					{
						if (!_iterator.MoveRightWithRN())
							return;
						if (GetCharType(_iterator.RightChar) != type)
							break;
					}
					if (!shift)
					{
						_iterator.MoveLeftWithRN();
					}
				}
				else
				{
					CharType type = GetCharType(_iterator.RightChar);
					while (true)
					{
						if (!_iterator.MoveRightWithRN())
							return;
						if (GetCharType(_iterator.RightChar) != type)
							break;
					}
					if (!shift)
					{
						_iterator.MoveLeftWithRN();
					}
				}
			}
		}
		
		public void Vi_b()
		{
			_iterator.MoveLeftWithRN();
			if (IsSpaceOrNewLine(_iterator.RightChar))
			{
				while (IsSpaceOrNewLine(_iterator.RightChar) &&
					_iterator.MoveLeftWithRN());
			}
			CharType type = GetCharType(_iterator.RightChar);
			while (GetCharType(_iterator.LeftChar) == type &&
				_iterator.MoveLeftWithRN());
		}
		
		public void Vi_W(bool change)
		{
			CharType type = GetCharType(_iterator.RightChar);
			if (type == CharType.Identifier || type == CharType.Punctuation)
			{
				while (true)
				{
					type = GetCharType(_iterator.RightChar);
					if (type != CharType.Identifier && type != CharType.Punctuation ||
						!_iterator.MoveRightWithRN())
					{
						break;
					}
				}
				if (!change && IsSpaceOrNewLine(_iterator.RightChar))
				{
					while (IsSpaceOrNewLine(_iterator.RightChar) &&
						_iterator.MoveRightWithRN());
				}
			}
			else if (GetCharType(_iterator.RightChar) == CharType.Punctuation)
			{
				while (GetCharType(_iterator.RightChar) == CharType.Punctuation &&
					_iterator.MoveRightWithRN());
				if (!change && IsSpaceOrNewLine(_iterator.RightChar))
				{
					while (IsSpaceOrNewLine(_iterator.RightChar) &&
						_iterator.MoveRightWithRN());
				}
			}
			else if (IsSpaceOrNewLine(_iterator.RightChar))
			{
				while (IsSpaceOrNewLine(_iterator.RightChar) &&
					_iterator.MoveRightWithRN());
			}
		}
		
		public void Vi_E(bool shift)
		{
			char c = _iterator.RightChar;
			if (IsSpaceOrNewLine(c))
			{
				while (true)
				{
					if (!_iterator.MoveRightWithRN())
						return;
					if (!IsSpaceOrNewLine(_iterator.RightChar))
						break;
				}
				CharType type = GetCharType(_iterator.RightChar);
				bool isSpace = type == CharType.Space || type == CharType.Special;
				while (true)
				{
					if (!_iterator.MoveRightWithRN())
						return;
					type = GetCharType(_iterator.RightChar);
					if ((type == CharType.Space || type == CharType.Special) != isSpace)
						break;
				}
				if (!shift)
				{
					_iterator.MoveLeftWithRN();
				}
			}
			else
			{
				if (!_iterator.MoveRightWithRN())
					return;
				if (IsSpaceOrNewLine(_iterator.RightChar))
				{
					while (true)
					{
						if (!_iterator.MoveRightWithRN())
							return;
						if (!IsSpaceOrNewLine(_iterator.RightChar))
							break;
					}
					CharType type = GetCharType(_iterator.RightChar);
					while (true)
					{
						if (!_iterator.MoveRightWithRN())
							return;
						if (GetCharType(_iterator.RightChar) != type)
							break;
					}
					if (!shift)
					{
						_iterator.MoveLeftWithRN();
					}
				}
				else
				{
					CharType type = GetCharType(_iterator.RightChar);
					bool isSpace = type == CharType.Space || type == CharType.Special;
					while (true)
					{
						if (!_iterator.MoveRightWithRN())
							return;
						type = GetCharType(_iterator.RightChar);
						if ((type == CharType.Space || type == CharType.Special) != isSpace)
							break;
					}
					if (!shift)
					{
						_iterator.MoveLeftWithRN();
					}
				}
			}
		}
		
		public void Vi_B()
		{
			_iterator.MoveLeftWithRN();
			if (IsSpaceOrNewLine(_iterator.RightChar))
			{
				while (IsSpaceOrNewLine(_iterator.RightChar) &&
					_iterator.MoveLeftWithRN());
			}
			CharType type = GetCharType(_iterator.RightChar);
			if (type == CharType.Identifier || type == CharType.Punctuation)
			{
				while (true)
				{
					type = GetCharType(_iterator.LeftChar);
					if (type != CharType.Identifier && type != CharType.Punctuation ||
						!_iterator.MoveLeftWithRN())
						break;
				}
			}
			else
			{
				while (true)
				{
					type = GetCharType(_iterator.LeftChar);
					if (type != CharType.Space && type != CharType.Special ||
						!_iterator.MoveLeftWithRN())
						break;
				}
			}
		}
		
		public void Vi_WordStart()
		{
			CharType type = GetCharType(_iterator.RightChar);
			while (GetCharType(_iterator.LeftChar) == type &&
				_iterator.MoveLeftWithRN());
		}
		
		public bool Vi_BracketStart(char bra, char ket, int count)
		{
			int position = _iterator.Position;
			bool failByQuotes;
			bool result = BracketStart(bra, ket, count, out failByQuotes);
			if (failByQuotes)
			{
				_iterator = _lines.GetCharIterator(position);
				while (_iterator.MoveLeft())
				{
					char c = _iterator.RightChar;
					if ((c == '"' || c == '\'') && _iterator.LeftChar != '\'')
					{
						if (_iterator.MoveLeft())
						{
							result = BracketStart(bra, ket, count, out failByQuotes);
						}
						break;
					}
				}
			}
			return result;
		}
		
		private bool BracketStart(char bra, char ket, int count, out bool failByQuotes)
		{
			failByQuotes = false;
			int depth = count - 1;
			if (_iterator.RightChar == ket)
			{
				_iterator.MoveLeft();
			}
			while (true)
			{
				for (int i = 0; i < 2; ++i)
				{
					char quotes = i == 0 ? '"' : '\'';
					while (_iterator.RightChar == quotes && _iterator.LeftChar != '\\')
					{
						while (true)
						{
							if (!_iterator.MoveLeft())
							{
								failByQuotes = true;
								return false;
							}
							char rightC = _iterator.RightChar;
							if (rightC == quotes && _iterator.LeftChar != '\\')
							{
								if (!_iterator.MoveLeft())
								{
									failByQuotes = true;
									return false;
								}
								break;
							}
							if (rightC == '\r' || rightC == '\n')
							{
								failByQuotes = true;
								return false;
							}
						}
					}
				}
				char c = _iterator.RightChar;
				if (c == ket)
				{
					++depth;
				}
				if (c == bra)
				{
					if (depth <= 0)
					{
						_iterator.MoveRight();
						return true;
					}
					--depth;
				}
				if (!_iterator.MoveLeft())
				{
					return false;
				}
			}
		}
		
		public bool Vi_BracketEnd(char bra, char ket)
		{
			int depth = 0;
			while (true)
			{
				for (int i = 0; i < 2; ++i)
				{
					char quotes = i == 0 ? '"' : '\'';
					while (_iterator.RightChar == quotes && _iterator.LeftChar != '\\')
					{
						while (true)
						{
							if (!_iterator.MoveRight())
							{
								return false;
							}
							char rightC = _iterator.RightChar;
							if (rightC == quotes && _iterator.LeftChar != '\\')
							{
								if (!_iterator.MoveRight())
								{
									return false;
								}
								break;
							}
							if (rightC == '\r' || rightC == '\n')
							{
								return false;
							}
						}
					}
				}
				char c = _iterator.RightChar;
				if (c == bra)
				{
					++depth;
				}
				if (c == ket)
				{
					if (depth <= 0)
					{
						return true;
					}
					--depth;
				}
				if (!_iterator.MoveRight())
				{
					return false;
				}
			}
		}
		
		public bool Vi_QuotesStart(char quote)
		{
			while (true)
			{
				char c = _iterator.RightChar;
				if (c == quote)
				{
					if (_iterator.LeftChar != '\\')
					{
						_iterator.MoveRight();
						return true;
					}
				}
				if (!_iterator.MoveLeft())
				{
					return false;
				}
			}
		}
		
		public bool Vi_QuotesEnd(char quote)
		{
			while (true)
			{
				char c = _iterator.RightChar;
				if (c == '\\')
				{
					if (!_iterator.MoveRight())
					{
						return false;
					}
				}
				else if (c == quote)
				{
					return true;
				}
				if (!_iterator.MoveRight())
				{
					return false;
				}
			}
		}
		
		public bool Vi_PairBracket(bool shift)
		{
			char c = _iterator.RightChar;
			char bra;
			char ket;
			GetBrackets(c, out bra, out ket);
			if (bra == '\0')
			{
				while (true)
				{
					if (!_iterator.MoveRight())
					{
						return false;
					}
					c = _iterator.RightChar;
					if (c == '\n' || c == '\r')
					{
						return false;
					}
					GetBrackets(c, out bra, out ket);
					if (bra != '\0')
					{
						break;
					}
				}
			}
			if (bra != '\0')
			{
				if (c == bra)
				{
					_iterator.MoveRight();
					Vi_BracketEnd(bra, ket);
					if (shift)
					{
						_iterator.MoveRight();
					}
					return true;
				}
				if (c == ket)
				{
					_iterator.MoveLeft();
					Vi_BracketStart(bra, ket, 1);
					_iterator.MoveLeft();
					return true;
				}
			}
			return false;
		}
		
		private void GetBrackets(char c, out char bra, out char ket)
		{
			bra = '\0';
			ket = '\0';
			if (c == '{' || c == '}')
			{
				bra = '{';
				ket = '}';
			}
			else if (c == '(' || c == ')')
			{
				bra = '(';
				ket = ')';
			}
			else if (c == '[' || c == ']')
			{
				bra = '[';
				ket = ']';
			}
		}
		
		public void Apply(Selection selection, bool shift)
		{
			selection.caret = Position;
			if (!shift)
			{
				selection.anchor = selection.caret;
			}
			_lines.SetPreferredPos(selection, Place);
		}
		
		public void Apply(Selection selection, bool shift, int offset)
		{
			selection.caret = Position + offset;
			if (!shift)
			{
				selection.anchor = selection.caret;
			}
			_lines.SetPreferredPos(selection, Place);
		}
		
		private static CharType GetCharType(char c)
        {
	        switch (c)
	        {
		        case ' ':
		        case '\t':
					return CharType.Space;
				case '\r':
				case '\n':
				case '\0':
					return CharType.Special;
				case '_':
					return CharType.Identifier;
				default:
					return char.IsLetterOrDigit(c) ?
						CharType.Identifier :
						CharType.Punctuation;
		    }
        }
        
        private static bool IsSpace(char c)
        {
	        switch (c)
	        {
		        case ' ':
		        case '\t':
					return true;
				default:
					return false;
			}
	    }
        
        public static bool IsSpaceOrNewLine(char c)
        {
	        switch (c)
	        {
		        case ' ':
		        case '\t':
		        case '\r':
		        case '\n':
		        	return true;
		        default:
		        	return false;
		    }
        }
        
        public static bool IsSpaceOrNewLine(char c, bool allowNewLine)
        {
	        switch (c)
	        {
		        case '\n':
		        case '\r':
			        return allowNewLine;
		        case ' ':
		        case '\t':
		        	return true;
		        default:
		        	return false;
		    }
        }
	}	
}
