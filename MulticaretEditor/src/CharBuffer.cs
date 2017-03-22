using System;

namespace MulticaretEditor
{
    public class CharBuffer
    {
    	public char[] buffer;
        public int count;
        
        private const int MinCapacity = 32;
        
        public CharBuffer()
		{
		}
        
        public void Resize(int count)
		{
			int length = buffer != null ? buffer.Length : MinCapacity;
			if (count > length)
			{
				int nextLength = length << 1;
				while (nextLength < count)
				{
					nextLength = nextLength << 1;
				}
				buffer = new char[nextLength];
				length = nextLength;
			}
			else if (buffer == null)
			{
				buffer = new char[MinCapacity];
			}
			else
			{
				this.count = count;
				while (true)
				{
					int halfLength = length >> 1;
					if (halfLength < MinCapacity || count > (halfLength >> 1))
						break;
					length = halfLength;
				}
				if (length < buffer.Length)
				{
					buffer = new char[length];
				}
			}
		}
    }
}
