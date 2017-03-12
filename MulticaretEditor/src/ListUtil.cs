using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor
{	
	public static class ListHelper
	{
		public static string ToString<T>(IEnumerable<T> list)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append('[');
			bool first = true;
			foreach (T item in list)
			{
				if (!first)
				{
					builder.Append(", ");
				}
				first = false;
				builder.Append(item + "");
			}
			builder.Append(']');
			return builder.ToString();
		}
		
		public static string ToString<T>(IEnumerable<T> list, StringOfDelegate<T> stringOf)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append('[');
			bool first = true;
			foreach (T item in list)
			{
				if (!first)
				{
					builder.Append(", ");
				}
				first = false;
				builder.Append(stringOf(item) + "");
			}
			builder.Append(']');
			return builder.ToString();
		}

		public static T FirstBetter<T>(Getter<T, int> criterion, List<T> list)
		{
			return FirstBetter(null, criterion, list);
		}

		public static T FirstBetter<T>(Getter<T, bool> filter, Getter<T, int> criterion, List<T> list)
		{
			T result = default(T);
			int criterionValue = 0;
			bool first = true;
			for (int i = 0, count = list.Count; i < count; i++)
			{
				T item = list[i];
				if (filter == null || filter(item))
				{
					if (criterion == null)
					{
						result = item;
						break;
					}
					int criterionValueI = criterion(item);
					if (first)
					{
						first = false;
						criterionValue = criterionValueI;
						result = item;
					}
					else if (criterionValue < criterionValueI)
					{
						criterionValue = criterionValueI;
						result = item;
					}
				}
			}
			return result;
		}
	}
}
