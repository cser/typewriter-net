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
			buffer = new char[MinCapacity];
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
				char[] newBuffer = new char[nextLength];
				Array.Copy(buffer, newBuffer, this.count);
				buffer = newBuffer;
			}
			else if (count < this.count)
			{
				Array.Clear(buffer, count, this.count - count);
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
				char[] newBuffer = new char[length];
				Array.Copy(buffer, newBuffer, newBuffer.Length);
				buffer = newBuffer;
			}
		}
    }
}
