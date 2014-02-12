using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class FSBArray<T, TBlock> where TBlock : FSBBlock<T>
	{
		protected int blockSize;
		
		public int blocksCount;
		public TBlock[] blocks;
		
		public FSBArray(int blockSize)
		{
			this.blockSize = blockSize;
			blocks = new TBlock[4];
		}
		
		protected int valuesCount = 0;
		
		protected T GetValue(int index)
		{
			if (index >= valuesCount || index < 0)
				throw new IndexOutOfRangeException("index=" + index + " is out of [0, " + valuesCount + ")");
			FSBBlock<T> block = blocks[GetBlockIndex(index)];
			return block.array[index - block.offset];
		}
		
		protected void SetValue(int index, T value)
		{
			if (index >= valuesCount || index < 0)
				throw new IndexOutOfRangeException("index=" + index + " is out of [0, " + valuesCount + ")");
			FSBBlock<T> block = blocks[GetBlockIndex(index)];
			block.array[index - block.offset] = value;
			block.valid = 0;
			block.wwSizeX = 0;
		}
		
		protected void ClearValues()
		{
			Array.Clear(blocks, 0, blocksCount);
			blocksCount = 0;
			valuesCount = 0;
		}
		
		protected void AddValue(T value)
		{
			bool needNewBlock = true;
			if (blocksCount > 0)
			{
				FSBBlock<T> block = blocks[blocksCount - 1];
				if (block.count < blockSize)
				{
					block.array[block.count++] = value;
					block.valid = 0;
					block.wwSizeX = 0;
					needNewBlock = false;
				}
			}
			if (needNewBlock)
			{
				TBlock block = NewBlock();
				block.offset = valuesCount;
				block.count = 1;
				block.array[0] = value;
				AllocateBlocks(blocksCount + 1);
				blocks[blocksCount - 1] = block;
			}
			valuesCount++;
		}
		
		protected void InsertValue(int index, T value)
		{
			if (index > valuesCount || index < 0)
				throw new IndexOutOfRangeException("index=" + index + " is out of [0, " + valuesCount + "]");
			int i = GetBlockIndex(index);
			if (i == -1)
			{
				AddValue(value);
			}
			else
			{
				FSBBlock<T> block = blocks[i];
				int j = index - block.offset;
				if (j == 0 && i > 0 && blocks[i - 1].count < blockSize)
				{
					FSBBlock<T> left = blocks[i - 1];
					left.array[left.count++] = value;
                    left.valid = 0;
                    left.wwSizeX = 0;
				}
				else if (i + 1 > blocksCount)
				{
					TBlock rightBlock = NewBlock();
					rightBlock.offset = valuesCount;
					rightBlock.count = 1;
					rightBlock.array[0] = value;
					AllocateBlocks(i + 1);
					blocks[blocksCount - 1] = rightBlock;
				}
				else if (block.count < blockSize)
				{
					Array.Copy(block.array, j, block.array, j + 1, block.count - j);
					block.array[j] = value;
					block.count++;
                    block.valid = 0;
                    block.wwSizeX = 0;
				}
				else
				{
					T last = block.array[blockSize - 1];
					Array.Copy(block.array, j, block.array, j + 1, block.count - j - 1);
					block.array[j] = value;
                    block.valid = 0;
                    block.wwSizeX = 0;
					if (i < blocksCount - 1 && blocks[i + 1].count < blockSize)
					{
						FSBBlock<T> right = blocks[i + 1];
						Array.Copy(right.array, 0, right.array, 1, right.count);
						right.array[0] = last;
						right.count++;
                        right.valid = 0;
                        right.wwSizeX = 0;
					}
					else
					{
						AllocateBlocks(blocksCount + 1);
						Array.Copy(blocks, i + 1, blocks, i + 2, blocksCount - i - 2);
						TBlock right = NewBlock();
						right.offset = block.offset + block.count;
						right.count = 1;
						right.array[0] = last;
                        right.valid = 0;
                        right.wwSizeX = 0;
						blocks[i + 1] = right;
					}
				}
				UpdateIndices(i);
			}
		}
		
		protected void RemoveValueAt(int index)
		{
			if (index >= valuesCount || index < 0)
				throw new IndexOutOfRangeException("index=" + index + " is out of [0, " + valuesCount + ")");
			int i = GetBlockIndex(index);
			FSBBlock<T> block = blocks[i];
			int j = index - block.offset;
			Array.Copy(block.array, j + 1, block.array, j, block.count - j - 1);
			block.array[block.count - 1] = default(T);
			block.count--;
            block.valid = 0;
            block.wwSizeX = 0;
			if (block.count == 0)
			{
				Array.Copy(blocks, i + 1, blocks, i, blocksCount - i - 1);
				blocks[blocksCount - 1] = null;
				blocksCount--;
			}
			else if (i > 0 && blockSize - blocks[i - 1].count >= block.count)
			{
				FSBBlock<T> left = blocks[i - 1];
				Array.Copy(block.array, 0, left.array, left.count, block.count);
				left.count += block.count;
                left.valid = 0;
                left.wwSizeX = 0;
				Array.Copy(blocks, i + 1, blocks, i, blocksCount - i - 1);
				blocks[blocksCount - 1] = null;
				blocksCount--;
			}
			else if (i < blocksCount - 1)
			{
				FSBBlock<T> right = blocks[i + 1];
				if (blockSize - block.count >= right.count)
				{
					Array.Copy(right.array, 0, block.array, block.count, right.count);
					block.count += right.count;
					Array.Copy(blocks, i + 2, blocks, i + 1, blocksCount - i - 2);
					blocks[blocksCount - 1] = null;
					blocksCount--;
				}
			}
			UpdateIndices(i);
		}
		
		protected void RemoveValuesRange(int index, int count)
		{
			if (index + count > this.valuesCount || index < 0)
				throw new IndexOutOfRangeException("index=" + index + ", count=" + count + " is out of [0, " + this.valuesCount + ")");
			if (count == 0)
				return;
			int startI = GetBlockIndex(index);
			int endI = GetBlockIndex(index + count - 1);
			if (endI == -1)
				endI = startI;
			TBlock start = blocks[startI];
			TBlock end = blocks[endI];
			int startJ = index - start.offset;
			int endJ = index + count - end.offset;
			if (blockSize - startJ >= end.count - endJ)
			{
				Array.Copy(end.array, endJ, start.array, startJ, end.count - endJ);
				int oldLeftCount = start.count;
				start.count = startJ + end.count - endJ;
                start.valid = 0;
                start.wwSizeX = 0;
				Array.Clear(start.array, start.count, oldLeftCount - start.count);// FIXME fails here in difficult case
				
				RemoveBlocks(start.count == 0 ? startI : startI + 1, endI + 1);
			}
			else
			{
				end.count -= endJ;
                end.valid = 0;
                end.wwSizeX = 0;
				Array.Copy(end.array, endJ, end.array, 0, end.count);
				Array.Clear(end.array, end.count, endJ);
				
				int removedCount = start.count - startJ;
				start.count -= removedCount;
                start.valid = 0;
                start.wwSizeX = 0;
				Array.Clear(start.array, start.count, removedCount);
				
				RemoveBlocks(startI + 1, endI);
			}
			start = blocks[startI];
			if (startI - 1 >= 0 && blockSize - blocks[startI - 1].count >= start.count)
			{
				TBlock prev = blocks[startI - 1];
				Array.Copy(start.array, 0, prev.array, prev.count, start.count);
				prev.count += start.count;
                prev.valid = 0;
                prev.wwSizeX = 0;
				RemoveBlocks(startI, startI + 1);
			}
			UpdateIndices(startI);
		}
		
		private PredictableList<TBlock> blocksBuffer = new PredictableList<TBlock>();
		
		protected void InsertValuesRange(int index, T[] values)
		{
			if (index > this.valuesCount || index < 0)
				throw new IndexOutOfRangeException("index=" + index + " is out of [0, " + this.valuesCount + "]");
			int valuesCount = values.Length;
			int i;
			if (this.valuesCount == 0)
			{
				AllocateBlocks(1);
				blocksCount = 1;
				blocks[0] = NewBlock();
				i = 0;
			}
			else if (index == this.valuesCount)
			{
				i = blocksCount - 1;
			}
			else
			{
				i = GetBlockIndex(index);
			}
			TBlock target = blocks[i];
			int j = index - target.offset;
			if (j == 0 && i > 0 && blockSize - blocks[i - 1].count >= valuesCount)
			{
				i--;
				target = blocks[i];
				j = index - target.offset;
			}
			if (valuesCount <= blockSize - target.count)
			{
				Array.Copy(target.array, j, target.array, j + valuesCount, target.count - j);
				Array.Copy(values, 0, target.array, j, valuesCount);
				target.count += valuesCount;
				target.valid = 0;
				target.wwSizeX = 0;
				UpdateIndices(i);
				return;
			}
			blocksBuffer.Clear();
			TBlock first;
			int firstJ;
			if (i > 0)
			{
				TBlock left = blocks[i - 1];
				if (j <= blockSize - left.count)
				{
					int leftCount = left.count;
					first = left;
					firstJ = leftCount + j;
					Array.Copy(target.array, 0, left.array, leftCount, j);
					first.count += j;
				}
				else
				{
					int leftCount = left.count;
					first = NewBlock();
					firstJ = j - (blockSize - leftCount);
					blocksBuffer.Add(first);
					Array.Copy(target.array, 0, left.array, leftCount, blockSize - leftCount);
					Array.Copy(target.array, 0, first.array, blockSize - leftCount, j - (blockSize - leftCount));
					left.count = blockSize;
                    left.valid = 0;
                    left.wwSizeX = 0;
					first.count = j - (blockSize - leftCount);
				}
                first.valid = 0;
                first.wwSizeX = 0;
			}
			else
			{
				first = NewBlock();
				firstJ = j;
				blocksBuffer.Add(first);
				Array.Copy(target.array, 0, first.array, 0, j);
				first.count = j;
			}
			int targetRightCount = target.count - j;
			if (first.count + valuesCount + targetRightCount <= blockSize)//!!!! NOT COVERED !!!!!!
			{
				// first: [--------|-values-|-targetRight-|---]
				Array.Copy(values, 0, first.array, first.count, valuesCount);
				Array.Copy(target.array, j, first.array, first.count + valuesCount, targetRightCount);
				first.count = valuesCount + targetRightCount;
			}
			else if (first.count + valuesCount <= blockSize)
			{
				// first: [--------|-values-|-targetRight-----]
				// last:  [-targetRight-|---------------------]
				int targetRightCount0 = blockSize - (first.count + valuesCount);
				Array.Copy(values, 0, first.array, first.count, valuesCount);
				Array.Copy(target.array, j, first.array, first.count + valuesCount, targetRightCount0);
				TBlock last = NewBlock();
				blocksBuffer.Add(last);
				Array.Copy(target.array, j + targetRightCount0, last.array, 0, targetRightCount - targetRightCount0);
				first.count = blockSize;
				last.count = targetRightCount - targetRightCount0;
			}
			else
			{
				int valuesFirstCount = blockSize - first.count;
				Array.Copy(values, 0, first.array, first.count, valuesFirstCount);
				first.count = blockSize;
				int n = (valuesCount - valuesFirstCount) / blockSize;
				int valuesLastCount = (valuesCount - valuesFirstCount) - n * blockSize;
				for (int ii = 0; ii < n; ii++)
				{
					TBlock block = NewBlock();
					blocksBuffer.Add(block);
					Array.Copy(values, valuesFirstCount + ii * blockSize, block.array, 0, blockSize);
					block.count = blockSize;
				}
				if (valuesLastCount + targetRightCount > 0)
				{
					TBlock last = NewBlock();
					blocksBuffer.Add(last);
					Array.Copy(values, valuesCount - valuesLastCount, last.array, 0, valuesLastCount);
					if (valuesLastCount + targetRightCount <= blockSize)
					{
						Array.Copy(target.array, j, last.array, valuesLastCount, targetRightCount);
						last.count = valuesLastCount + targetRightCount;
					}
					else
					{
						Array.Copy(target.array, j, last.array, valuesLastCount, blockSize - valuesLastCount);
						TBlock last2 = NewBlock();
						blocksBuffer.Add(last2);
						Array.Copy(target.array, j + (blockSize - valuesLastCount), last2.array, 0, targetRightCount - (blockSize - valuesLastCount));
						last.count = blockSize;
						last2.count = targetRightCount - (blockSize - valuesLastCount);
					}
				}
			}
			if (blocksBuffer.count == 0)//!!!! NOT COVERED !!!!!!
			{
				RemoveBlocks(i, i + 1);
			}
			else if (blocksBuffer.count == 1)
			{
				blocks[i] = blocksBuffer.buffer[0];
			}
			else
			{
				int oldBlocksCount = blocksCount;
				AllocateBlocks(blocksCount - 1 + blocksBuffer.count);
				Array.Copy(blocks, i + 1, blocks, i + blocksBuffer.count, oldBlocksCount - i - 1);
				int blocksBufferCount = blocksBuffer.count;
				for (int ii = 0; ii < blocksBufferCount; ii++)
				{
					blocks[ii + i] = blocksBuffer.buffer[ii];
				}
			}
			blocksBuffer.Clear();
			UpdateIndices(i);
		}
		
		private void RemoveBlocks(int blockI0, int blockI1)
		{
			Array.Copy(blocks, blockI1, blocks, blockI0, blocksCount - blockI1);
			Array.Clear(blocks, blocksCount - blockI1 + blockI0, blockI1 - blockI0);
			blocksCount += blockI0 - blockI1;
		}
		
		private void UpdateIndices(int blockI)
		{
			int offset = 0;
			if (blockI > 0)
			{
				TBlock block = blocks[blockI - 1];
				offset = block.offset + block.count;
			}
			for (int ii = blockI; ii < blocksCount; ii++)
			{
				TBlock block = blocks[ii];
				block.offset = offset;
				offset += block.count;
			}
			valuesCount = offset;
		}
		
		protected void AllocateBlocks(int blocksCount)
		{
			this.blocksCount = blocksCount;
			if (blocksCount > blocks.Length)
			{
				int oldLength = blocks.Length;
				int length = oldLength * 2;
				while (blocksCount > length)
				{
					length *= 2;
				}
				Array.Resize(ref blocks, length);
			}
		}
		
		public int GetBlockIndex(int index)
		{
			int bra = 0;
			int ket = blocksCount - 1;
			if (ket >= 0)
			{
				while (true)
				{
					int i = (bra + ket) / 2;
					if (index < blocks[i].offset)
					{
						ket = i;
					}
					else
					{
						bra = i;
					}
					if (bra == ket || bra + 1 == ket)
					{
						FSBBlock<T> block;
						block = blocks[bra];
						if (index >= block.offset && index < block.offset + block.count)
							return bra;
						block = blocks[ket];
						if (index >= block.offset && index < block.offset + block.count)
							return ket;
						break;
					}
				}
			}
			return -1;
		}
		
		virtual protected TBlock NewBlock()
		{
			return (TBlock)new FSBBlock<T>(blockSize);
		}
	}
}
