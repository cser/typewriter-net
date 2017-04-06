using System;

namespace MulticaretEditor
{
	public class PredictableList<T>
	{
		public PredictableList(int minCapacity)
		{
			this.minCapacity = minCapacity >= 1 ? minCapacity : 1;
			buffer = new T[minCapacity];
		}
		
		private readonly int minCapacity;
		
		public int count;
		public T[] buffer;
		
		public PredictableList() : this(32)
		{
		}
		
		public void Add(T item)
		{
			if (count >= buffer.Length)
			{
				T[] newBuffer = new T[buffer.Length << 1];
				Array.Copy(buffer, newBuffer, buffer.Length);
				buffer = newBuffer;
			}
			buffer[count] = item;
			count++;
		}
		
		public T Pop()
		{
			T result;
			if (count > 0)
			{
				count--;
				result = buffer[count];
				buffer[count] = default(T);
			}
			else
			{
				result = default(T);
			}
			return result;
		}
		
		public T Peek()
		{
			return buffer[count - 1];
		}
		
		public void Clear()
		{
			Array.Clear(buffer, 0, count);
			count = 0;
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
				T[] newBuffer = new T[nextLength];
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
			if (count <= (buffer.Length >> 2))
			{
				int length = buffer.Length;
				while ((length >> 1) >= minCapacity && count <= (length >> 2))
				{
					length = length >> 1;
				}
				if (length < buffer.Length)
				{
					T[] newBuffer = new T[length];
					Array.Copy(buffer, newBuffer, newBuffer.Length);
					buffer = newBuffer;
				}
			}
		}
		
		public T[] ToArray()
		{
			T[] result = new T[count];
			Array.Copy(buffer, result, count);
			return result;
		}
	}
}
