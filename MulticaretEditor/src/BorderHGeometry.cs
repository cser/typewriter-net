using System;

namespace MulticaretEditor
{
	public class BorderHGeometry
	{
		public bool isEnd;
		public int x0;
		public int x1;
		
		public bool top0Exists;
		public int top0X0;
		public int top0X1;
		
		public bool top1Exists;
		public int top1X0;
		public int top1X1;
		
		private bool hasPrev;
		
		public void Begin()
		{
			hasPrev = false;
			top0Exists = false;
			top1Exists = false;
			isEnd = false;
		}
		
		public void AddLine(int x, int width)
		{
			int xx1 = x + width;
			if (hasPrev)
			{
				if (xx1 < x0 || x > x1)
				{
					top0Exists = true;
					top0X0 = x0;
					top0X1 = x1;
					top1Exists = true;
					top1X0 = x;
					top1X1 = xx1;
				}
				else
				{
					if (x < x0)
					{
						top0Exists = true;
						top0X0 = x;
						top0X1 = x0;
					}
					else if (x > x0)
					{
						top0Exists = true;
						top0X0 = x0;
						top0X1 = x;
					}
					else
					{
						top0Exists = false;
					}
					if (xx1 < x1)
					{
						top1Exists = true;
						top1X0 = xx1;
						top1X1 = x1;
					}
					else if (xx1 > x1)
					{
						top1Exists = true;
						top1X0 = x1;
						top1X1 = xx1;
					}
					else
					{
						top1Exists = false;
					}
				}
			}
			else
			{
				hasPrev = true;
				top0Exists = true;
				top0X0 = x;
				top0X1 = xx1;
			}
			x0 = x;
			x1 = xx1;
		}
		
		public void End()
		{
			hasPrev = false;
			top0Exists = true;
			top0X0 = x0;
			top0X1 = x1;
			top1Exists = false;
			isEnd = true;
		}
	}
}