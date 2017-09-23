using System.Text;

namespace MulticaretEditor
{
	public class ParserIterator
	{
		public readonly int charsCount;
		
		private readonly LineBlock[] blocks;
		private readonly int blocksCount;
		private int blockI;
		private int blockILine;
		private int iChar;
		private char rightChar;
		private int position;

		public ParserIterator(
			LineBlock[] blocks, int blocksCount, int charsCount, int blockI, int blockILine, int iChar, int position)
		{
			this.blocks = blocks;
			this.blocksCount = blocksCount;
			this.charsCount = charsCount;
			this.blockI = blockI;
			this.blockILine = blockILine;
			this.iChar = iChar;
			this.position = position;
			if (iChar == blocks[blockI].array[blockILine].charsCount)
			{
				rightChar = '\0';
			}
			else
			{
				rightChar = blocks[blockI].array[blockILine].chars[iChar].c;
			}
		}
		
		public Place Place { get { return new Place(iChar, blocks[blockI].offset + blockILine); } }
		
		public char RightChar { get { return rightChar; } }

		public int Position { get { return position; } }
		
		public bool IsEnd { get { return position >= charsCount; } }
		
		public void MoveRight()
		{
			rightChar = '\0';
			if (position < charsCount)
			{
				LineBlock block = blocks[blockI];
				Line line = block.array[blockILine];
				if (iChar == line.charsCount - 1)
				{
					if (blockILine == block.count - 1)
					{
						if (blockI >= blocksCount - 1)
						{
							iChar++;
							position++;
							return;
						}
						blockI++;
						block = blocks[blockI];
						blockILine = 0;
					}
					else
					{
						blockILine++;
					}
					line = block.array[blockILine];
					iChar = 0;
					rightChar = line.charsCount > 0 ? line.chars[iChar].c : '\0';
				}
				else
				{
					iChar++;
					rightChar = line.chars[iChar].c;
				}
				position++;
			}
		}
		
		public void MoveRightOnLine(int count)
		{
			LineBlock block = blocks[blockI];
			Line line = block.array[blockILine];
			if (iChar + count >= line.charsCount)
			{
				int delta = line.charsCount - iChar - 1;
				iChar += delta;
				position += delta;
				rightChar = line.chars[iChar].c;
				MoveRight();
			}
			else
			{
				iChar += count;
				position += count;
				rightChar = line.chars[iChar].c;
			}
		}
		
		public bool IsRightOnLine(string text)
		{
			if (position <= charsCount)
			{
				LineBlock block = blocks[blockI];
				Line line = block.array[blockILine];
				for (int i = 0; i < text.Length; i++)
				{
					if (iChar + i >= line.charsCount)
					{
						return false;
					}
					if (text[i] != line.chars[iChar + i].c)
					{
						return false;
					}
				}
			}
			return true;
		}
		
		public bool IsRightWord(string text)
		{
			if (position <= charsCount)
			{
				LineBlock block = blocks[blockI];
				Line line = block.array[blockILine];
				if (iChar != 0)
				{
					char c = line.chars[iChar - 1].c;
					if (!char.IsWhiteSpace(c) && !char.IsPunctuation(c))
					{
						return false;
					}
				}
				for (int i = 0; i < text.Length; i++)
				{
					if (iChar + i >= line.charsCount)
					{
						return false;
					}
					if (text[i] != line.chars[iChar + i].c)
					{
						return false;
					}
				}
				if (iChar + text.Length < line.charsCount)
				{
					char c = line.chars[iChar + text.Length].c;
					if (!char.IsWhiteSpace(c) && !char.IsPunctuation(c))
					{
						return false;
					}
				}
			}
			return true;
		}
		
		public void MoveSpacesAndRN()
		{
			while (true)
			{
				char c = RightChar;
				if (!char.IsWhiteSpace(c) && c != '\r' && c != '\n')
				{
					break;
				}
				MoveRight();
			}
		}
		
		public void MoveIdent(StringBuilder builder)
		{
			char c = RightChar;
			if (char.IsLetter(c) || c == '_')
			{
				builder.Append(c);
				MoveRight();
				while (true)
				{
					c = RightChar;
					if (!char.IsLetterOrDigit(c) && c != '_')
					{
						break;
					}
					builder.Append(c);
					MoveRight();
				}
			}
		}
	}
}