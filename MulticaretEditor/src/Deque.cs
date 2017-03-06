using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class Deque<T>
	{
		private Stack<T> _leftStack = new Stack<T>();
		private Stack<T> _rightStack = new Stack<T>();
		private Stack<T> _buffer = new Stack<T>();
		
		public void Push(T n)
		{
			_rightStack.Push(n);
		}
		
		public void Put(T n)
		{
			_leftStack.Push(n);
		}
		
		public T Pop()
		{
			if (_rightStack.Count != 0)
			{
				return _rightStack.Pop();
			}
			else
			{
				int size = _leftStack.Count;
				_buffer.Clear();
				for (int i = 0; i < size / 2; i++)
					_buffer.Push(_leftStack.Pop());
				while (_leftStack.Count != 0)
					_rightStack.Push(_leftStack.Pop());
				while (_buffer.Count != 0)
					_leftStack.Push(_buffer.Pop());
				return _rightStack.Pop();
			}
		}

		public bool IsEmpty { get { return _leftStack.Count == 0 && _rightStack.Count == 0; } }
	}
}