namespace MulticaretEditor
{
	public class ParserIterator
	{
		private readonly LineBlock[] blocks;
		private readonly int blocksCount;
		private readonly int charsCount;
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

		public bool MoveRight()
		{
			bool result = position < charsCount;
			if (result)
			{
				rightChar = '\0';
				LineBlock block = blocks[blockI];
				Line line = block.array[blockILine];
				if (iChar == line.charsCount - 1)
				{
					if (blockILine == block.count - 1)
					{
						if (blockI >= blocksCount - 1)
						{
							rightChar = '\0';
							iChar++;
							position++;
							return true;
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
			else
			{
				rightChar = '\0';
			}
			return result;
		}
	}
}