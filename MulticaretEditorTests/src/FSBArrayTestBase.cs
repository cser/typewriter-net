using System;
using System.Text;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	public class FSBArrayTestBase
	{
		public delegate void Void();
		
		public class TestArray<T> : FSBArray<T, FSBBlock<T>>
		{
			public TestArray(int blockSize) : base(blockSize)
			{
			}
			
			public int Count { get { return valuesCount; } }
			public int BlocksLength { get { return blocks.Length; } }
			public int BlocksCount { get { return blocksCount; } }
			
			public T this[int index]
			{
				get { return GetValue(index); }
				set { SetValue(index, value); }
			}
			
			public void Clear()
			{
				ClearValues();
			}
			
			public void Add(T value)
			{
				AddValue(value);
			}
			
			public void Insert(int index, T value)
			{
				InsertValue(index, value);
			}
			
			public void RemoveAt(int index)
			{
				RemoveValueAt(index);
			}
			
			public void RemoveRange(int index, int count)
			{
				RemoveValuesRange(index, count);
			}
			
			public void InsertRange(int index, T[] values)
			{
				InsertValuesRange(index, values);
			}
			
			public string GetBlocksInfo()
			{
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < blocksCount; i++)
				{
					if (i != 0)
						builder.Append("; ");
					FSBBlock<T> block = blocks[i];
					builder.Append(block.offset + ":[");
					bool first = true;
					for (int j = 0; j < block.count; j++)
					{
						if (!first)
							builder.Append("; ");
						first = false;
						builder.Append(block.array[j]);
					}
					for (int j = block.count; j < blockSize; j++)
					{
						if (!first)
							builder.Append("; ");
						first = false;
						builder.Append("(" + block.array[j] + ")");
					}
					builder.Append("]");
				}
				return builder.ToString();
			}
			
			public T[] ToArray()
			{
				T[] array = new T[Count];
				for (int i = 0; i < Count; i++)
				{
					array[i] = GetValue(i);
				}
				return array;
			}
			
			public void SetBlocks(params T[][] blocks)
			{
				AllocateBlocks(blocks.Length);
				int offset = 0;
				for (int i = 0; i < blocks.Length; i++)
				{
					this.blocks[i] = NewBlock();
					this.blocks[i].count = blocks[i].Length;
					this.blocks[i].offset = offset;
					offset += this.blocks[i].count;
					for (int j = 0; j < blocks[i].Length; j++)
					{
						this.blocks[i].array[j] = blocks[i][j];
					}
				}
				valuesCount = offset;
			}
	    }
		
		public FSBArrayTestBase()
		{
		}
		
		protected TestArray<string> array;
		
		protected string[] Strings(params string[] args)
		{
			return args;
		}
		
		protected void Init(int blockSize)
		{
			array = new TestArray<string>(blockSize);
		}
		
		protected void AssertIndexOutOfRangeException(string expectedMessage, Void action)
		{
			try
			{
				action();
				Assert.Fail("IndexOutOfRangeException expected");
			}
			catch (IndexOutOfRangeException e)
			{
				Assert.AreEqual(expectedMessage, e.Message);
			}
		}
		
	}
}
