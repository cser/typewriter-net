using System;
using System.Windows.Forms;
using CustomScrollBar;

namespace MulticaretEditor
{
	public class ScrollBarData
	{
		public int value;
		public int areaSize;
		public int contentSize;
		public bool visible;
		
		public void ApplyParamsTo(ScrollBarEx scrollBar)
		{
			scrollBar.Minimum = 0;
			scrollBar.Maximum = contentSize;
			scrollBar.LargeChange = Math.Max(0, areaSize);
			scrollBar.Visible = visible;
		}
		
		public int ClampValue(int value)
		{
			return CommonHelper.Clamp(value, 0, contentSize - areaSize);
		}
	}
}
