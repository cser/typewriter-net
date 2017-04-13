using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MulticaretEditor.Highlighting
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
		
		public const string DefaultDeliminators = " .():!+,-<=>%&/;?[]^{|}~\\*\t\n\r";
		
		public class Keyword : Rule
		{
			private string deliminators;
			private Char[] nodes = new Char[32];
			private int nodesCount;
			
			private void NodesAdd(Char item)
			{
				if (nodesCount >= nodes.Length)
				{
					Char[] newNodes = new Char[nodes.Length << 1];
					Array.Copy(nodes, newNodes, nodes.Length);
					nodes = newNodes;
				}
				nodes[nodesCount++] = item;
			}
			
			public Keyword(string[] words, bool casesensitive, string weakDeliminator, string additionalDeliminator)
			{
				string defaultDeliminators = DefaultDeliminators;
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < defaultDeliminators.Length; ++i)
				{
					char c = defaultDeliminators[i];
					if (weakDeliminator.IndexOf(c) == -1 && additionalDeliminator.IndexOf(c) == -1)
						builder.Append(c);
				}
				builder.Append(additionalDeliminator);
				deliminators = builder.ToString();
				string[] sortedWords = new string[words.Length];
				if (casesensitive)
				{
					Array.Copy(words, sortedWords, words.Length);
				}
				else
				{
					for (int i = 0; i < words.Length; ++i)
					{
						sortedWords[i] = words[i].ToLowerInvariant();
					}
				}
				Array.Sort(sortedWords, System.StringComparer.Ordinal);
				NodesAdd(new Char((char)1));
				ParseNodes(sortedWords, 0, 0, sortedWords.Length, casesensitive);
				
				/*Debug.Log(string.Join(", ", sortedWords));
				builder = new StringBuilder();
				for (int i = 0; i < nodesCount; ++i)
				{
					builder.Append("[" + i + "]");
					Char node = nodes[i];
					if (node.c == 0 && node.style == 0)
					{
						builder.Append("NULL");
					}
					else if (node.c == 1 && node.style == 0)
					{
						builder.Append("END");
					}
					else
					{
						if (node.c == '\0')
						{
							builder.Append("\\0");
						}
						else
						{
							builder.Append(node.c);
						}
						builder.Append("->" + node.style);
					}
				}
				Debug.Log(builder.ToString());*/
			}
			
			private void ParseNodes(string[] words, int position, int i0, int i1, bool casesensitive)
			{
				int index = nodesCount;
				{
					char prevC = '\0';
					bool hasEnds = false;
					for (int i = i0; i < i1; ++i)
					{
						string word = words[i];
						if (position >= word.Length)
						{
							if (position == word.Length)
							{
								hasEnds = true;
							}
							continue;
						}
						char c = word[position];
						if (c != prevC)
						{
							prevC = c;
							NodesAdd(new Char(c));
							if (!casesensitive)
							{
								char upperC = char.ToUpperInvariant(c);
								if (upperC != c)
								{
									NodesAdd(new Char(upperC));
								}
							}
						}
					}
					NodesAdd(new Char(hasEnds ? (char)1 : '\0'));
				}
				{
					bool first = true;
					char prevC = '\0';
					int prevI = i0;
					for (int i = i0; i < i1; ++i)
					{
						string word = words[i];
						if (position >= word.Length)
						{
							continue;
						}
						char c = word[position];
						if (first)
						{
							prevC = c;
							first = false;
						}
						else if (c != prevC)
						{
							short next = (short)nodesCount;
							if (i - prevI == 1 && words[prevI].Length == position + 1)
							{
								next = 0;
							}
							nodes[index].style = next;
							++index;
							if (!casesensitive)
							{
								char upperC = char.ToUpperInvariant(prevC);
								if (upperC != prevC)
								{
									nodes[index].style = next;
									++index;
								}
							}
							if (next != 0)
							{
								ParseNodes(words, position + 1, prevI, i, casesensitive);
							}
							prevC = c;
							prevI = i;
						}
					}
					if (!first)
					{
						short next = (short)nodesCount;
						if (i1 - prevI == 1 && words[prevI].Length == position + 1)
						{
							next = 0;
						}
						nodes[index].style = next;
						if (!casesensitive)
						{
							char currentC = nodes[index].c;
							char upperC = char.ToUpperInvariant(currentC);
							if (upperC != currentC)
							{
								++index;
								nodes[index].style = next;
							}
						}
						if (next != 0)
						{
							ParseNodes(words, position + 1, prevI, i1, casesensitive);
						}
					}
				}
			}
			
			override public bool Match(string text, int position, out int nextPosition)
			{
				if (position == 0 || deliminators.IndexOf(text[position - 1]) != -1)
				{
					int count = text.Length;
					int i = position;
					int iNode = 1;
					while (true)
					{
						Char node = nodes[iNode];
						if (node.c <= 1)
						{
							if (node.c == 1 && (i >= count || deliminators.IndexOf(text[i]) != -1))
							{
								nextPosition = i;
								return true;
							}
							nextPosition = position;
							return false;
						}
						if (i < count && text[i] == node.c)
						{
							++i;
							iNode = node.style;
						}
						else
						{
							++iNode;
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
		
		public class WordDetect : Rule
		{
			private const string DefaultDeliminators = " .():!+,-<=>%&/;?[]^{|}~\\*\t\n\r";
			private bool insensitive;
			private string pattern;
			private string upperPattern;
			
			public WordDetect(string pattern, bool insensitive)
			{
				if (insensitive)
				{
					this.pattern = pattern.ToLowerInvariant();
					this.upperPattern = pattern.ToUpperInvariant();
				}
				else
				{
					this.pattern = pattern;
				}
				this.insensitive = insensitive;
			}
			
			override public bool Match(string text, int position, out int nextPosition)
			{
				nextPosition = position;
				int position1 = position + pattern.Length;
				if (position1 <= text.Length)
				{
					if (insensitive)
					{
						for (int i = position; i < position1; ++i)
						{
							char c = text[i];
							if (pattern[i - position] != c && upperPattern[i - position] != c)
							{
								nextPosition = position;
								return false;
							}
						}
					}
					else
					{
						for (int i = position; i < position1; ++i)
						{
							if (pattern[i - position] != text[i])
							{
								nextPosition = position;
								return false;
							}
						}
					}
					if ((position == 0 || DefaultDeliminators.IndexOf(text[position - 1]) != -1) &&
						(position1 == text.Length || DefaultDeliminators.IndexOf(text[position1]) != -1))
					{
						nextPosition = position1;
						return true;
					}
				}
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
				int length = text.Length;
				char prev = position - 1 >= 0 && position - 1 < length ? text[position - 1] : '\0';
				if (char.IsWhiteSpace(prev) || Rules.IsPunctuation(prev) || prev == '\0')
				{
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
				char prev = position - 1 >= 0 && position - 1 < length ? text[position - 1] : '\0';
				bool hasDot = false;
				bool hasNumber = false;
				if (char.IsWhiteSpace(prev) || Rules.IsPunctuation(prev) || prev == '\0')
				{
					while (i < length)
					{
						char c = text[i];
						if (!char.IsDigit(text[i]) && c != '.')
							break;
						if (c == '.')
						{
							if (hasDot)
								break;
							hasDot = true;
						}
						else
						{
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
							if (!char.IsDigit(c) || c == '8' || c == '9')
								break;
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
