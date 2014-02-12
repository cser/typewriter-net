using System.Collections.Generic;

namespace MulticaretEditor
{
	public class RWList<T> : List<T>, IRList<T>
	{
		public RWList()
		{
		}
		
		public RWList(IEnumerable<T> items) : base(items)
		{
		}
		
		public RWList(int bufferSize) : base(bufferSize)
		{
		}
	}
}