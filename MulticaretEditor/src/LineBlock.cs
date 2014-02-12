using System;

namespace MulticaretEditor
{
	public class LineBlock : FSBBlock<Line>
	{
		public LineBlock(int blockSize) : base(blockSize)
		{
		}
		
		public const int CharsCountValid = 0x0001;
		public const int MaxSizeXValid = 0x0010;
		public const int ColorValid = 0x0100;
		
		public int charsCount;
		public int maxSizeX;
		public int wwSizeY;
		public int wwOffset;
	}
}
