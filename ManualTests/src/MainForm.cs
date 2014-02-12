using System;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing;
using MulticaretEditor;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ManualTests
{
    public class MainForm : Form
    {
		private MulticaretTextBox textBox;

        public MainForm()
        {
            SuspendLayout();

            textBox = new MulticaretTextBox();
            textBox.Dock = System.Windows.Forms.DockStyle.Fill;
            textBox.ImeMode = System.Windows.Forms.ImeMode.Off;
            textBox.Text = @"Slkjflsjdf sldfaj lasdjf lsd fl Ssdfldsf sdfsdf sdfsdf sdfsdfsdf sdfsdfsfsdfsf
slkjflsjdf sldfaj lasdjf lsd fl asdfldsf";

            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(371, 341);
            Controls.Add(textBox);

            ResumeLayout(false);
            PerformLayout();
            
            TestFSBArray();
        }

        public class TestArray<T> : FSBArray<T, FSBBlock<T>>
		{
			public TestArray(int blockSize) : base(blockSize)
			{
			}
			
			public int Count { get { return valuesCount; } }
			
			public T this[int index]
			{
				get { return GetValue(index); }
				set { SetValue(index, value); }
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
        }
        
        private TestArray<int> _fsbArray;
        private List<int> _list;
        
        private void TestFSBArray()
        {
        	int count = 10000;
        	
        	{
        		Stopwatch sw = Stopwatch.StartNew();
	        	TestArray<int> array = new TestArray<int>(200);
	        	for (int i = 0; i < count; i++)
	        	{
	        		array.Add(i);
	        	}
	        	for (int i = 0; i < count; i++)
	        	{
	        		array.Insert(i, i * 10);
	        	}
	        	for (int i = 0; i < count; i++)
	        	{
	        		array.RemoveAt(i);
	        	}
	        	_fsbArray = array;
	        	sw.Stop();
	        	Console.WriteLine("FSBArray: " + sw.ElapsedMilliseconds + "ms");
        	}
        	
        	{
        		Stopwatch sw = Stopwatch.StartNew();
        		List<int> array = new List<int>(count);
	        	for (int i = 0; i < count; i++)
	        	{
	        		array.Add(i);
	        	}
	        	for (int i = 0; i < count; i++)
	        	{
	        		array.Insert(i, i * 10);
	        	}
	        	for (int i = 0; i < count; i++)
	        	{
	        		array.RemoveAt(i);
	        	}
	        	_list = array;
	        	sw.Stop();
	        	Console.WriteLine("List: " + sw.ElapsedMilliseconds + "ms");
        	}
        }
    }
}
