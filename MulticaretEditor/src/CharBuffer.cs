using System;

namespace MulticaretEditor
{
    public class CharBuffer
    {
    	public string buffer;
        public int count;
        
        private const int MinCapacity = 32;
        
        public CharBuffer()
		{
			buffer = new string('\0', MinCapacity);
		}
        
        public void Resize(int count)
		{
			if (count > buffer.Length)
			{
				int nextLength = buffer.Length << 1;
				while (nextLength < count)
				{
					nextLength = nextLength << 1;
				}
				buffer = new string('\0', nextLength);
			}
			this.count = count;
		}
		
		public void Realocate()
		{
			int length = buffer.Length;
			while (true)
			{
				int halfLength = length >> 1;
				if (halfLength < MinCapacity || count > (halfLength >> 1))
					break;
				length = halfLength;
			}
			if (length < buffer.Length)
			{
				buffer = new string('\0', length);
			}
		}
    }
}
