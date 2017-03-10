using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class RE
	{
		public abstract class RENode
		{
			public RENode next0;
			public RENode next1;
			
			public readonly bool emptyEntry;
			
			public RENode(bool emptyEntry)
			{
				this.emptyEntry = emptyEntry;
			}
			
			public RENode() : this(false)
			{
			}
			
			public virtual bool MatchChar(char c)
			{
				return false;
			}
		}
		
		public class RERepetition : RENode
		{
			public readonly RENode body;
			
			public RERepetition(RENode body)
			{
				this.body = body;
			}
			
			public override bool MatchChar(char c)
			{
				return false;
			}
			
			public override string ToString()
			{
				return "*";
			}
		}
		
		public class REEmpty : RENode
		{
			public REEmpty() : base(true)
			{
			}
			
			public override bool MatchChar(char c)
			{
				return true;
			}
			
			public override string ToString()
			{
				return "o";
			}
		}
		
		public class REDot : RENode
		{
			public override bool MatchChar(char c)
			{
				return c != '\n' && c != '\r';
			}
			
			public override string ToString()
			{
				return ".";
			}
		}
		
		public class REChar : RENode
		{
			private readonly char _c;
			
			public REChar(char c)
			{
				_c = c;
			}
			
			public override bool MatchChar(char c)
			{
				return _c == c;
			}
			
			public override string ToString()
			{
				return "'" + _c + "'";
			}
		}
		
		public class RE_A : RENode
		{
			private readonly bool _upper;
			
			public RE_A(bool upper)
			{
				_upper = upper;
			}
			
			public override bool MatchChar(char c)
			{
				bool result = char.IsLetter(c);
				if (_upper)
				{
					result = !result && c != '\n' && c != '\r';
				}
				return result;
			}
			
			public override string ToString()
			{
				return _upper ? "A" : "a";
			}
		}
		
		public class RE_D : RENode
		{
			private readonly bool _upper;
			
			public RE_D(bool upper)
			{
				_upper = upper;
			}
			
			public override bool MatchChar(char c)
			{
				bool result = char.IsDigit(c);
				if (_upper)
				{
					result = !result && c != '\n' && c != '\r';
				}
				return result;
			}
			
			public override string ToString()
			{
				return _upper ? "D" : "d";
			}
		}
		
		public class RE_H : RENode
		{
			private readonly bool _upper;
			
			public RE_H(bool upper)
			{
				_upper = upper;
			}
			
			public override bool MatchChar(char c)
			{
				bool result = char.IsLetter(c) || c == '_';
				if (_upper)
				{
					result = !result && c != '\n' && c != '\r';
				}
				return result;
			}
			
			public override string ToString()
			{
				return _upper ? "H" : "h";
			}
		}
		
		public class RE_L : RENode
		{
			private readonly bool _upper;
			
			public RE_L(bool upper)
			{
				_upper = upper;
			}
			
			public override bool MatchChar(char c)
			{
				bool result = char.IsLower(c);
				if (_upper)
				{
					result = !result && c != '\n' && c != '\r';
				}
				return result;
			}
			
			public override string ToString()
			{
				return _upper ? "L" : "l";
			}
		}
		
		public class RE_O : RENode
		{
			private readonly bool _upper;
			
			public RE_O(bool upper)
			{
				_upper = upper;
			}
			
			public override bool MatchChar(char c)
			{
				bool result = "01234567".IndexOf(c) != -1;
				if (_upper)
				{
					result = !result && c != '\n' && c != '\r';
				}
				return result;
			}
			
			public override string ToString()
			{
				return _upper ? "O" : "o";
			}
		}
		
		public class RE_P : RENode
		{
			private readonly bool _upper;
			
			public RE_P(bool upper)
			{
				_upper = upper;
			}
			
			public override bool MatchChar(char c)
			{
				return c != '\t' && c != '\n' && c != '\r' && (!_upper || !char.IsDigit(c));
			}
			
			public override string ToString()
			{
				return _upper ? "P" : "p";
			}
		}
		
		public class RE_S : RENode
		{
			private readonly bool _upper;
			
			public RE_S(bool upper)
			{
				_upper = upper;
			}
			
			public override bool MatchChar(char c)
			{
				return char.IsWhiteSpace(c) ^ _upper && c != '\n' && c != '\r';
			}
			
			public override string ToString()
			{
				return _upper ? "S" : "s";
			}
		}
		
		public class RE_U : RENode
		{
			private readonly bool _upper;
			
			public RE_U(bool upper)
			{
				_upper = upper;
			}
			
			public override bool MatchChar(char c)
			{
				bool result = char.IsUpper(c);
				if (_upper)
				{
					result = !result && c != '\n' && c != '\r';
				}
				return result;
			}
			
			public override string ToString()
			{
				return _upper ? "U" : "u";
			}
		}
		
		public class RE_W : RENode
		{
			private readonly bool _upper;
			
			public RE_W(bool upper)
			{
				_upper = upper;
			}
			
			public override bool MatchChar(char c)
			{
				bool result = char.IsLetterOrDigit(c) || c == '_';
				if (_upper)
				{
					result = !result && c != '\n' && c != '\r';
				}
				return result;
			}
			
			public override string ToString()
			{
				return _upper ? "W" : "w";
			}
		}
		
		public class RE_X : RENode
		{
			private readonly bool _upper;
			
			public RE_X(bool upper)
			{
				_upper = upper;
			}
			
			public override bool MatchChar(char c)
			{
				bool result = c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F';
				if (_upper)
				{
					result = !result && c != '\n' && c != '\r';
				}
				return result;
			}
			
			public override string ToString()
			{
				return _upper ? "X" : "x";
			}
		}
	}
}