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
    }
}
