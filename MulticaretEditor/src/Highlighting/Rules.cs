using System;
using System.Text;
using System.Text.RegularExpressions;

namespace MulticaretEditor
{
	public static class Rules
	{
		public class Context : CommonAttributes
		{
			public string name;
			public Rule[] childs;
			public SwitchInfo lineEndContext;
			
			public bool fallthrough;
			public SwitchInfo fallthroughContext;
			public bool dynamic;
			
			override public string ToString()
			{
				return name;
			}
		}
		
		public class CommonAttributes
		{
			public StyleData attribute;
		}
		
		public struct SwitchInfo
		{
			public int pops;
			public Context next;
		}
		
		public abstract class Rule : CommonAttributes
		{
			public SwitchInfo context;
			public bool lookAhead;
			public int column;
			public Rule[] childs;
			
			abstract public bool Match(string text, int position, out int nextPosition);
		}
		
		public class KeywordNode
		{
			public char c;
			public bool end;
			public KeywordNode next;
			public KeywordNode alternative;
			
			public string ToString(StringBuilder builder, string tab)
			{
				KeywordNode node = this;
				while (node != null)
				{
					builder.Append(tab + node.c + (node.end ? "(end)" : "") + ":\n");
					if (node.next != null)
					{
						node.next.ToString(builder, tab + "\t");
					}
					else
					{
						builder.Append(tab + "\tnull\n");
					}
					node = node.alternative;
				}
				return builder.ToString();
			}
		}
		
		public class KeywordCasesensitive : Rule
		{
			private string deliminators;
			private KeywordNode rootNode;
			
			public KeywordCasesensitive(string[] words, string weakDeliminator, string additionalDeliminator)
			{
				string defaultDeliminators = " .():!+,-<=>%&/;?[]^{|}~\\*\t\n\r";
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < defaultDeliminators.Length; i++)
				{
					char c = defaultDeliminators[i];
					if (weakDeliminator.IndexOf(c) == -1 && additionalDeliminator.IndexOf(c) == -1)
						builder.Append(c);
				}
				builder.Append(additionalDeliminator);
				deliminators = builder.ToString();
				Array.Sort(words);
				rootNode = ParseNodes(words, 0, 0, words.Length);
				//Console.WriteLine("-------------------------------");
				//Console.WriteLine(string.Join("\n", words));
				//builder = new StringBuilder();
				//rootNode.ToString(builder, "");
				//Console.WriteLine(builder.ToString());
			}
			
			private KeywordNode ParseNodes(string[] words, int position, int i0, int i1)
			{
				KeywordNode startNode = null;
				KeywordNode nodeI = null;
				for (int i = i0; i < i1; i++)
				{
					if (position >= words[i].Length)
					{
						continue;
					}
					char c = words[i][position];
					if (nodeI == null)
					{
						nodeI = new KeywordNode();
						nodeI.c = c;
						startNode = nodeI;
					}
					else if (nodeI.c != c)
					{
						nodeI.next = ParseNodes(words, position + 1, i0, i);
						i0 = i;
						nodeI.alternative = new KeywordNode();
						nodeI.alternative.c = c;
						nodeI = nodeI.alternative;
					}
					if (position == words[i].Length - 1)
					{
						nodeI.end = true;
					}
				}
				if (nodeI != null)
				{
					nodeI.next = ParseNodes(words, position + 1, i0, i1);
				}
				return startNode;
			}
			
			override public bool Match(string text, int position, out int nextPosition)
			{
				int count = text.Length;
				int wordEnd = position;
				if (position == 0 || deliminators.IndexOf(text[position - 1]) != -1)
				{
					while (wordEnd < count && deliminators.IndexOf(text[wordEnd]) == -1)
					{
						wordEnd++;
					}
				}
				if (wordEnd > position)
				{
					KeywordNode node = rootNode;
					int i = position;
					while (true)
					{
						if (i >= wordEnd)
						{
							nextPosition = position;
							return false;
						}
						char c = text[i];
						while (true)
						{
							if (node == null)
							{
								nextPosition = position;
								return false;
							}
							else if (node.end)
							{
								if (i + 1 == wordEnd)
								{
									nextPosition = wordEnd;
									return true;
								}
							}
							if (c == node.c)
							{
								i++;
								node = node.next;
								goto outer;
							}
							node = node.alternative;
						}
						outer:
						continue;
					}
				}
				nextPosition = position;
				return false;
			}
		}
		
		public class Keyword : Rule
		{
			private const int HashSize = 16;
			
			private string[][] hash;
			private string deliminators;
			
			public Keyword(string[] words, string weakDeliminator, string additionalDeliminator)
			{
				string defaultDeliminators = " .():!+,-<=>%&/;?[]^{|}~\\*\t\n\r";
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < defaultDeliminators.Length; i++)
				{
					char c = defaultDeliminators[i];
					if (weakDeliminator.IndexOf(c) == -1 && additionalDeliminator.IndexOf(c) == -1)
						builder.Append(c);
				}
				builder.Append(additionalDeliminator);
				deliminators = builder.ToString();
				hash = new string[HashSize][];
				int[] counts = new int[HashSize];
				foreach (string word in words)
				{
					counts[word.Length % HashSize]++;
				}
				foreach (string word in words)
				{
					int i = word.Length % HashSize;
					string[] array = hash[i];
					if (array == null)
					{
						array = new string[counts[i]];
						hash[i] = array;
					}
					counts[i]--;
					array[counts[i]] = word.ToLowerInvariant();
				}
			}
			
			override public bool Match(string text, int position, out int nextPosition)
			{
				int count = text.Length;
				int wordEnd = position;
				if (position == 0 || deliminators.IndexOf(text[position - 1]) != -1)
				{
					while (wordEnd < count && deliminators.IndexOf(text[wordEnd]) == -1)
					{
						wordEnd++;
					}
				}
				if (wordEnd > position)
				{
					int length = wordEnd - position;
					int i = length % HashSize;
					if (hash[i] != null)
					{
						string word = text.Substring(position, length).ToLowerInvariant();;
						if (Array.IndexOf<string>(hash[i], word) != -1)
						{
							nextPosition = position + length;
							return true;
						}
					}
				}
				nextPosition = position;
				return false;
			}
		}
		
		public class DetectChar : Rule
		{	
			public char char0;
			
			override public bool Match(string text, int position, out int nextPosition)
			{
				if (text[position] == char0)
				{
					nextPosition = position + 1;
					return true;
				}
				nextPosition = position;
				return false;
			}
		}

		public class Detect2Chars : Rule
		{
			public char char0;
			public char char1;
			
			override public bool Match(string text, int position, out int nextPosition)
			{
				if (position + 1 < text.Length && text[position] == char0 && text[position + 1] == char1)
				{
					nextPosition = position + 2;
					return true;
				}
				nextPosition = position;
				return false;
			}
		}
		
		public class AnyChar : Rule
		{
			public string chars;
			
			override public bool Match(string text, int position, out int nextPosition)
			{
				if (chars.IndexOf(text[position]) != -1)
				{
					nextPosition = position + 1;
					return true;
				}
				nextPosition = position;
				return false;
			}
		}
		
		public class StringDetect : Rule
		{
			public bool insensitive;
			public string text;
			
			override public bool Match(string text, int position, out int nextPosition)
			{
				int position1 = position + this.text.Length;
				StringComparison comparision = insensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
				if (text.IndexOf(this.text, position, Math.Min(position1, text.Length) - position, comparision) == position)
				{
					nextPosition = position1;
					return true;
				}
				nextPosition = position;
				return false;
			}
		}
		
		public class RegExpr : Rule
		{
			public Regex regex;
			public int[] awakePositions;
			public int awakeIndex;
			
			override public bool Match(string text, int position, out int nextPosition)
			{
				if (position >= awakePositions[awakeIndex])
				{
					Match match = regex.Match(text, position);
					if (match.Success)
					{
						if (match.Index == position)
						{
							int position1 = position + match.Length;
							if (position1 > position)
							{
								nextPosition = position1;
								return true;
							}
						}
						else
						{
							awakePositions[awakeIndex] = match.Index;
						}
					}
					else
					{
						awakePositions[awakeIndex] = text.Length;
					}
				}
				nextPosition = position;
				return false;
			}
		}
		
		public class Int : Rule
		{
			override public bool Match(string text, int position, out int nextPosition)
			{
				nextPosition = position;
				if (position == 0 || char.IsWhiteSpace(text[position - 1]) || Rules.IsPunctuation(text[position - 1]))
				{
					int length = text.Length;
					while (nextPosition < length && char.IsDigit(text[nextPosition]))
					{
						nextPosition++;
					}
				}
				return nextPosition > position;
			}
		}
		
		public class Float : Rule
		{
			override public bool Match(string text, int position, out int nextPosition)
			{
				int i = position;
				int length = text.Length;
				char prev = position - 1 >= 0 && position > 0 ? text[position - 1] : '\0';
				bool hasDot = false;
				bool hasNumber = false;
				if (char.IsWhiteSpace(prev) || Rules.IsPunctuation(prev) || prev == '\0')
				{
					while (i < length)
					{
						char c = text[i];
						if (c == '.')
						{
							if (hasDot)
							{
								break;
							}
							hasDot = true;
						}
						else
						{
							if (c < '0' | c > '9')
							{
								break;
							}
							hasNumber = true;
						}
						i++;
					}
				}
				if (hasDot && hasNumber)
				{
					nextPosition = i;
					return true;
				}
				nextPosition = position;
				return false;
			}
		}
		
		public class HlCOct : Rule
		{
			override public bool Match(string text, int position, out int nextPosition)
			{
				int i = position;
				if (text[i] == '0')
				{
					int length = text.Length;
					char prev = i - 1 >= 0 && i - 1 < length ? text[i - 1] : '\0';
					if (char.IsWhiteSpace(prev) || Rules.IsPunctuation(prev) || prev == '\0')
					{
						i++;
						while (i < length)
						{
							char c = text[i];
							if (c < '0' | c > '7')
							{
								break;
							}
							i++;
						}
					}
					if (i > position + 1)
					{
						nextPosition = i;
						return true;
					}
				}
				nextPosition = position;
				return false;
			}
		}
		
		public class HlCHex : Rule
		{
			override public bool Match(string text, int position, out int nextPosition)
			{
				int length = text.Length;
				int i = position;
				if (i + 1 < length && text[i] == '0')
				{
					char c1 = text[i + 1];
					if (c1 == 'x' || c1 == 'X')
					{
						char prev = i - 1 >= 0 && i - 1 < length ? text[i - 1] : '\0';
						if (char.IsWhiteSpace(prev) || Rules.IsPunctuation(prev) || prev == '\0')
						{
							i += 2;
							while (i < length)
							{
								char c = text[i];
								if (!(c >= '0' & c <= '9' | c >= 'a' & c <= 'f' | c >= 'A' & c <= 'F'))
									break;
								i++;
							}
						}
						if (i > position + 2)
						{
							nextPosition = i;
							return true;
						}
					}
				}
				nextPosition = position;
				return false;
			}
		}
		
		public class RangeDetect : Rule
		{
			public char char0;
			public char char1;
			
			override public bool Match(string text, int position, out int nextPosition)
			{
				int i = position;
				if (text[i] == char0)
				{
					int length = text.Length;
					i++;
					while (i < length)
					{
						if (text[i] == char1)
						{
							nextPosition = i + 1;
							return true;
						}
						i++;
					}
				}
				nextPosition = position;
				return false;
			}
		}
		
		public class DetectSpaces : Rule
		{
			override public bool Match(string text, int position, out int nextPosition)
			{
				if (char.IsWhiteSpace(text[position]))
				{
					int length = text.Length;
					nextPosition = position + 1;
					while (nextPosition < length && char.IsWhiteSpace(text[nextPosition]))
					{
						nextPosition++;
					}
					return true;
				}
				nextPosition = position;
				return false;
			}
		}
		
		public class DetectIdentifier : Rule
		{
			override public bool Match(string text, int position, out int nextPosition)
			{
				char c = text[position];
				if (char.IsLetter(c) || c == '_')
				{
					int length = text.Length;
					char prev = position > 0 ? text[position - 1] : '\0';
					if (char.IsWhiteSpace(prev) || Rules.IsPunctuation(prev) || prev == '\0')
					{
						nextPosition = position + 1;
						while (nextPosition < length)
						{
							c = text[nextPosition];
							if (!char.IsLetterOrDigit(c) && c != '_')
								break;
							nextPosition++;
						}
						return true;
					}
				}
				nextPosition = position;
				return false;
			}
		}
		
		public class HlCStringChar : Rule
		{
			override public bool Match(string text, int position, out int nextPosition)
			{
				if (text[position] == '\\')
				{
					int index = Rules.ParseRightEscape(text, position);
					if (index != -1)
					{
						nextPosition = index;
						return true;
					}
				}
				nextPosition = position;
				return false;
			}
		}
		
		public class HlCChar : Rule
		{
			override public bool Match(string text, int position, out int nextPosition)
			{
				if (text[position] == '\'')
				{
					int length = text.Length;
					if (position + 2 < length)
					{
						char c = text[position + 1];
						if (c == '\\')
						{
							int index = Rules.ParseRightEscape(text, position + 1);
							if (index != -1 && index < length && text[index] == '\'')
							{
								nextPosition = index + 1;
								return true;
							}
						}
						else if (c != '\'' && text[position + 2] == '\'')
						{
							nextPosition = position + 3;
							return true;
						}
					}
				}
				nextPosition = position;
				return false;
			}
		}
		
		public class LineContinue : Rule
		{
			override public bool Match(string text, int position, out int nextPosition)
			{
				int length = text.Length;
				bool result = text[position] == '\\' &&
					(position >= length - 1 || (position >= length - 2 && (text[position + 1] == '\n' || text[position + 1] == '\r')));
				nextPosition = result ? position + length - position : position;
				return result;
			}
		}

		private static bool IsPunctuation(char c)
		{
			bool result = false;
			switch (c)
			{
				case '!':
				case '%':
				case '&':
				case '(':
				case ')':
				case '*':
				case '+':
				case ',':
				case '-':
				case '.':
				case '/':
				case ':':
				case ';':
				case '<':
				case '=':
				case '>':
				case '?':
				case '[':
				case '\\':
				case ']':
				case '^':
				case '{':
				case '|':
				case '}':
				case '~':
					result = true;
					break;
			}
			return result;
		}
		
		private static int ParseRightEscape(string text, int slashPosition)
		{
			int length = text.Length;
			if (slashPosition + 1 < length)
			{
				switch (text[slashPosition + 1])
				{
					case 'a':
					case 'b':
					case 'f':
					case 'n':
					case 'r':
					case 't':
					case 'v':
					case '\'':
					case '"':
					case '\\':
					case '?':
					case 'e':
						return slashPosition + 2;
					case 'x':
					{
						bool matched = false;
						int i = slashPosition + 2;
						for (; i < slashPosition + 4 && i < length; i++)
						{
							char c = text[i];
							if (!(c >= '0' & c <= '9' | c >= 'a' & c <= 'f' | c >= 'A' & c <= 'F'))
								break;
							matched = true;
						}
						if (matched)
							return i;
						break;
					}
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					{
						int i = slashPosition + 2;
						for (; i < slashPosition + 4 && i < length; i++)
						{
							char c = text[i];
							if (!(c >= '0' && c <= '7'))
								break;
						}
						return i;
					}
					default:
						break;
				}
			}
			return -1;
		}
	}
}
