using System.Collections.Generic;
using System.Text;
using MulticaretEditor;

public class CSTokenIterator
{
	public readonly StringBuilder builder = new StringBuilder();
	
	public List<CSToken> tokens;
	private int nextIndex;
	
	public CSTokenIterator(LineArray lines)
	{
		tokens = ParseTokens(lines);
		MoveNext();
	}
	
	public bool isEnd;
	public CSToken current;
	
	public CSToken Next
	{
		get { return nextIndex < tokens.Count ? tokens[nextIndex] : default(CSToken); }
	}
	
	public void MoveNext()
	{
		if (nextIndex < tokens.Count)
		{
			current = tokens[nextIndex++];
		}
		else
		{
			isEnd = true;
			current = default(CSToken);
		}
	}
	
	private const int Normal = 0;
	private const int MultilineString = 1;
	private const int MultilineComment = 2;
	
	private List<CSToken> ParseTokens(LineArray lines)
	{
		List<CSToken> tokens = new List<CSToken>();
		StringBuilder builder = new StringBuilder();
		int state = Normal;
		for (int iBlock = 0; iBlock < lines.blocksCount; ++iBlock)
		{
			LineBlock block = lines.blocks[iBlock];
			for (int iLine = 0; iLine < block.count; ++iLine)
			{
				Line line = block.array[iLine];
				for (int i = 0; i < line.charsCount;)
				{
					char c = line.chars[i].c;
					if (state == Normal)
					{
						if (char.IsWhiteSpace(c))
						{
							++i;
							continue;
						}
						if (char.IsLetterOrDigit(c) || c == '_')
						{
							CSToken token = new CSToken();
							builder.Length = 0;
							builder.Append(c);
							for (++i; i < line.charsCount; ++i)
							{
								c = line.chars[i].c;
								if (!char.IsLetterOrDigit(c) && c != '_')
								{
									break;
								}
								builder.Append(c);
							}
							token.text = builder.ToString();
							token.place = new Place(i, block.offset + iLine);
							tokens.Add(token);
							continue;
						}
						if (c == '/')
						{
							++i;
							if (i < line.charsCount)
							{
								c = line.chars[i].c;
								if (c == '/')
								{
									i = line.charsCount;
								}
								else if (c == '*')
								{
									++i;
									state = MultilineComment;
								}
								else
								{
									CSToken token = new CSToken();
									token.c = '/';
									token.place = new Place(i, block.offset + iLine);
									tokens.Add(token);
								}
							}
						}
						else
						{
							if (char.IsPunctuation(c))
							{
								CSToken token = new CSToken();
								token.c = c;
								token.place = new Place(i, block.offset + iLine);
								tokens.Add(token);
							}
							++i;
						}
					}
					else if (state == MultilineComment)
					{
						if (c == '*' && i + 1 < line.charsCount && line.chars[i + 1].c == '/')
						{
							++i;
							state = Normal;
						}
						++i;
					}
					else if (state == MultilineString)
					{
						// TODO
						++i;
					}
				}
			}
		}
		return tokens;
	}
}