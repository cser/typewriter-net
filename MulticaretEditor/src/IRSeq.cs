using System.Collections.Generic;

namespace MulticaretEditor
{
	public interface IRSeq<T> : IEnumerable<T>
	{
		int Count { get; }
		
		T[] ToArray();
		
		bool Contains(T item);
	}
}