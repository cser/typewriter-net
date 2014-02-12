using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor
{	
	public static class ListUtil
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
	}
}
