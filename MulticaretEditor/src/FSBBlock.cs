using System;

namespace MulticaretEditor
{
	public class FSBBlock<T>
	{
		public readonly T[] array;
		public int offset;
		public int count;
		public int valid = 0;
		public int wwSizeX;
		
		public FSBBlock(int blockSize)
		{
			array = new T[blockSize];
		}
	}
}
