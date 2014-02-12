namespace MulticaretEditor
{
	public interface IRList<T> : IRSeq<T>
	{
		T this[int index] { get; }
		
		int IndexOf(T item);
		
		int IndexOf(T item, int index);
		
		int IndexOf(T item, int index, int count);
	}
}