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
		
		public void NPWordRight(bool shiftMove)
		{
			if (shiftMove)
			{
				while (GetCharType(_iterator.RightChar) == CharType.Space &&
					_iterator.MoveRightWithRN());
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
				while (GetCharType(_iterator.RightChar) == CharType.Space &&
					_iterator.MoveRightWithRN());
			}
		}
		
		public void NPWordLeft()
		{
			while (GetCharType(_iterator.LeftChar) == CharType.Space &&
				_iterator.MoveLeftWithRN());
			CharType type = GetCharType(_iterator.LeftChar);
			if (type != CharType.Space)
			{
				for (CharType typeI = type;
					typeI == type && _iterator.MoveLeftWithRN();
					typeI = GetCharType(_iterator.LeftChar));
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
	}	
}
