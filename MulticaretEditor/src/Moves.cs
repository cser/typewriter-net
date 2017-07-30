using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public struct Moves
	{
		private PlaceIterator _iterator;
		
		public int Position { get { return _iterator.Position; } }
		public Place Place { get { return _iterator.Place; } }
		
		public Moves(LineArray lines, int position)
		{
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
		
		public void Vi_w(bool change)
		{
			if (GetCharType(_iterator.RightChar) == CharType.Identifier)
			{
				while (GetCharType(_iterator.RightChar) == CharType.Identifier &&
					_iterator.MoveRightWithRN());
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
	}	
}
