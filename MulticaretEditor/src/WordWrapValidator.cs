using System;

namespace MulticaretEditor
{
	public class WordWrapValidator
	{
		private readonly LineArray lines;
		
		public WordWrapValidator(LineArray lines)
		{
			this.lines = lines;
		}
		
		public void Validate(int wwSizeX)
		{
			if (wwSizeX < 1)
				wwSizeX = 1;
			if (lines.wwSizeX != wwSizeX)
			{
				lines.wwSizeX = wwSizeX;
				for (int i = 0; i < lines.blocksCount; i++)
				{
					LineBlock block = lines.blocks[i];
					if (block.wwSizeX != wwSizeX)
					{
						int blockSizeY = 0;
						block.wwSizeX = wwSizeX;
						for (int j = 0; j < block.count; j++)
						{
							Line line = block.array[j];
							if (line.wwSizeX != wwSizeX)
								line.CalcCutOffs(wwSizeX);
							blockSizeY += line.cutOffs.count + 1;
						}
						block.wwSizeY = blockSizeY;
					}
				}
				int wwOffset = 0;
				for (int i = 0; i < lines.blocksCount; i++)
				{
					LineBlock block = lines.blocks[i];
					block.wwOffset = wwOffset;
					wwOffset += block.wwSizeY;
				}
				lines.wwSizeY = wwOffset;
			}
		}
		
		public int GetWWILine(int iLine)
		{
			int blockI = lines.GetBlockIndex(iLine);
			if (blockI == -1)
				return -1;
			LineBlock block = lines.blocks[blockI];
			int iInBlock = iLine - block.offset;
			int wwOffset = block.wwOffset;
			for (int i = 0; i < iInBlock; i++)
			{
				wwOffset += block.array[i].cutOffs.count + 1;
			}
			return wwOffset;
		}
		
		public LineIndex GetLineIndexOfWW(int wwILine)
		{
			if (wwILine <= 0)
				return new LineIndex(0, 0);
			int blockI = GetBlockIndexOfWW(wwILine);
			LineBlock block;
			if (blockI != -1)
			{
				block = lines.blocks[blockI];
				int wwOffset = block.wwOffset;
				for (int i = 0; i < block.count; i++)
				{
					Line line = block.array[i];
					if (wwOffset + line.cutOffs.count >= wwILine)
					{
						return new LineIndex(block.offset + i, wwILine - wwOffset);
					}
					wwOffset += line.cutOffs.count + 1;
				}
			}
			else
			{
				blockI = lines.blocksCount - 1;
				block = lines.blocks[blockI];
			}
			{
				Line line = block.array[block.count - 1];
				return new LineIndex(block.offset + block.count - 1, line.cutOffs.count);
			}
		}
		
		public int GetBlockIndexOfWW(int wwIndex)
		{
			int bra = 0;
			int ket = lines.blocksCount - 1;
			LineBlock[] blocks = lines.blocks;
			if (ket >= 0)
			{
				while (true)
				{
					int i = (bra + ket) / 2;
					if (wwIndex < blocks[i].wwOffset)
					{
						ket = i;
					}
					else
					{
						bra = i;
					}
					if (bra == ket || bra + 1 == ket)
					{
						LineBlock block;
						block = blocks[bra];
						if (wwIndex >= block.wwOffset && wwIndex < block.wwOffset + block.wwSizeY)
							return bra;
						block = blocks[ket];
						if (wwIndex >= block.wwOffset && wwIndex < block.wwOffset + block.wwSizeY)
							return ket;
						break;
					}
				}
			}
			return -1;
		}
		
		public Pos PosOf(Place place)
		{
			int iLine = place.iLine;
			if (iLine < 0)
			{
				iLine = 0;
			}
			else if (iLine >= lines.LinesCount)
			{
				iLine = lines.LinesCount - 1;
			}
			int wwILine = GetWWILine(place.iLine);
			Pos innerPos = lines[place.iLine].WWPosOfIndex(place.iChar);
			return new Pos(innerPos.ix, wwILine + innerPos.iy);
		}
		
		public Place PlaceOf(Pos pos)
		{
			LineIndex lineIndex = GetLineIndexOfWW(pos.iy);
			return new Place(lines[lineIndex.iLine].WWIndexOfPos(pos.ix, lineIndex.iSubline), lineIndex.iLine);
		}
	}
}
