using System;

namespace MulticaretEditor
{
	public struct LineIterator
	{
		private LineArray lines;
		private int index;
		private int endIndex;
		private int blockI;
		private int linesCount;
		private bool direct;
		
		public LineIterator(LineArray lines, int index, int count, int blockI)
		{
			this.lines = lines;
			this.index = index;
			current = null;
			
			linesCount = lines.LinesCount;
			endIndex = index + count;
			direct = index <= endIndex;
			if (endIndex > linesCount || index < 0 || endIndex < -1 || index > linesCount - 1)
				throw new IndexOutOfRangeException("index=" + index + ", count=" + count + " is out of [0, " + linesCount + ")");
			this.blockI = blockI != -1 ? blockI : lines.GetBlockIndex(index);
		}
		
		public Line current;
		
		public bool MoveNext()
		{
			bool result;
			if (direct)
			{
				result = index < endIndex;
				if (result)
				{
					LineBlock block = lines.blocks[blockI];
					if (index - block.offset >= block.count)
					{
						blockI++;
						block = lines.blocks[blockI];
					}
					current = block.array[index - block.offset];
					index++;
				}
			}
			else
			{
				result = index > endIndex;
				if (result)
				{
					LineBlock block = lines.blocks[blockI];
					if (index - block.offset < 0)
					{
						blockI--;
						block = lines.blocks[blockI];
					}
					current = block.array[index - block.offset];
					index--;
				}
			}
			return result;
		}
		
		public int Index { get { return direct ? index - 1 : index + 1; } }
		
		public void InvalidateCurrentText(int deltaCharsCount)
		{
			current.cachedText = null;
			current.cachedSize = -1;
			current.endState = null;
			current.wwSizeX = 0;
			lines.blocks[blockI].valid = 0;
			lines.blocks[blockI].wwSizeX = 0;
			lines.charsCount += deltaCharsCount;
			lines.wwSizeX = 0;
		}
		
		public LineIterator GetNextRange(int count)
		{
			return new LineIterator(lines, index, count, blockI);
		}
		
		public void SwapCurrent(bool withPrev)
		{
			LineBlock block = lines.blocks[blockI];
			int index = (direct ? this.index - 1 : this.index + 1) - block.offset;
			Line line;
			if (withPrev)
			{
				if (index - 1 < 0)
				{
					LineBlock prevBlock = lines.blocks[blockI - 1];
					line = prevBlock.array[prevBlock.count - 1];
					prevBlock.array[prevBlock.count - 1] = current;
					block.array[index] = line;
					
					prevBlock.valid = 0;
					prevBlock.wwSizeX = 0;
				}
				else
				{
					line = block.array[index - 1];
					block.array[index - 1] = current;
					block.array[index] = line;
				}
			}
			else
			{
				if (index + 1 > block.count - 1)
				{
					LineBlock nextBlock = lines.blocks[blockI + 1];
					line = nextBlock.array[0];
					nextBlock.array[0] = current;
					block.array[index] = line;
					
					nextBlock.valid = 0;
					nextBlock.wwSizeX = 0;
				}
				else
				{
					line = block.array[index + 1];
					block.array[index + 1] = current;
					block.array[index] = line;
				}
			}
			if (blockI == lines.blocksCount - 1 && (withPrev ? index : index + 1) == block.count - 1)
			{
				Line line0;
				Line line1;
				if (withPrev)
				{
					line0 = line;
					line1 = current;
				}
				else
				{
					line0 = current;
					line1 = line;
				}
				string rn = line0.RemoveRN();
				for (int i = 0; i < rn.Length; i++)
				{
					line1.Chars_Add(new Char(rn[i]));
				}
				line0.cachedText = null;
				line0.cachedSize = -1;
				line0.wwSizeX = 0;
				line1.cachedText = null;
				line1.cachedSize = -1;
				line1.wwSizeX = 0;
			}
			current.endState = null;
			line.endState = null;
			
			block.valid = 0;
			block.wwSizeX = 0;
			lines.cachedText = null;
		}
	}
}
