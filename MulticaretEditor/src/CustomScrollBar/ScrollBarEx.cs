namespace CustomScrollBar
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;
	using MulticaretEditor.Highlighting;

	public class ScrollBarEx : Control
	{
		public event ScrollEventHandler Scroll;
		
		private bool isVertical = true;
		private ScrollOrientation scrollOrientation = ScrollOrientation.VerticalScroll;
		private Rectangle thumbRectangle;
		private Rectangle topArrowRectangle;
		private Rectangle bottomArrowRectangle;
		private bool topArrowClicked;
		private bool bottomArrowClicked;
		private bool topBarClicked;
		private bool bottomBarClicked;
		private bool thumbClicked;
		private ScrollBarState thumbState = ScrollBarState.Normal;
		private ScrollBarState topButtonState = ScrollBarState.Normal;
		private ScrollBarState bottomButtonState = ScrollBarState.Normal;
		private int maximum = 100;
		private int smallChange = 20;
		private int largeChange = 20;
		private int value;
		private int size;
		private int thumbSize;
		private int arrowSize;
		private int thumbMouseOffset;
		private int trackMousePosition;
		private Timer progressTimer = new Timer();
		private Scheme scheme;

		public ScrollBarEx(bool isVertical, Scheme scheme)
		{
			this.isVertical = isVertical;
			this.scheme = scheme;
			
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);

			if (isVertical)
			{
				Width = 18;
				Height = 200;
			}
			else
			{
				Width = 200;
				Height = 18;
			}
			SetUpScrollBar();
			progressTimer.Interval = 20;
			progressTimer.Tick += ProgressTimerTick;
		}
		
		public void SetScheme(Scheme scheme)
		{
			this.scheme = scheme;
			Invalidate();
		}

		public int Maximum
		{
			get { return maximum; }
			set
			{
				if (maximum != value)
				{
					maximum = value;
					SetUpScrollBar();
				}
			}
		}

		public int SmallChange
		{
			get { return smallChange; }
			set
			{
				value = Math.Max(1, value);
				if (smallChange != value)
				{
					smallChange = value;
					SetUpScrollBar();
				}
			}
		}

		public int LargeChange
		{
			get { return largeChange; }
			set
			{
				value = Math.Max(1, value);
				if (largeChange != value)
				{
					largeChange = value;
					SetUpScrollBar();
				}
			}
		}

		public int Value
		{
			get { return this.value; }
			set
			{
				int newValue = FixValue(value);
				if (this.value != newValue)
				{
					this.value = newValue;
					ThumbPosition = GetThumbByValue();
					Refresh();
				}
			}
		}
		
		private int FixValue(int value)
		{
			return Math.Max(0, Math.Min(maximum - largeChange, value));
		}

		protected virtual void OnScroll(ScrollEventArgs e)
		{
			if (Scroll != null)
			{
				Scroll(this, e);
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
			ScrollBarExRenderer.DrawBackground(e.Graphics, scheme, ClientRectangle, isVertical);
			ScrollBarExRenderer.DrawThumb(e.Graphics, scheme, thumbRectangle, thumbState, isVertical);
			ScrollBarExRenderer.DrawArrowButton(e.Graphics, scheme, topArrowRectangle, topButtonState, true, isVertical);
			ScrollBarExRenderer.DrawArrowButton(e.Graphics, scheme, bottomArrowRectangle, bottomButtonState, false, isVertical);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left)
			{
				Point mouseLocation = e.Location;
				if (thumbRectangle.Contains(mouseLocation))
				{
					thumbClicked = true;
					thumbMouseOffset = (isVertical ? mouseLocation.Y : mouseLocation.X) - ThumbPosition;
					thumbState = ScrollBarState.Pressed;
					Invalidate(thumbRectangle);
				}
				else if (topArrowRectangle.Contains(mouseLocation))
				{
					topArrowClicked = true;
					topButtonState = ScrollBarState.Pressed;
					Invalidate(topArrowRectangle);
					ProgressThumb(true);
				}
				else if (bottomArrowRectangle.Contains(mouseLocation))
				{
					bottomArrowClicked = true;
					bottomButtonState = ScrollBarState.Pressed;
					Invalidate(bottomArrowRectangle);
					ProgressThumb(true);
				}
				else
				{
					trackMousePosition = isVertical ? mouseLocation.Y : mouseLocation.X;
					if (trackMousePosition < (isVertical ? thumbRectangle.Y : thumbRectangle.X))
					{
						topBarClicked = true;
					}
					else
					{
						bottomBarClicked = true;
					}
					ProgressThumb(true);
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (e.Button == MouseButtons.Left)
			{
				if (thumbClicked)
				{
					thumbClicked = false;
					thumbState = ScrollBarState.Normal;
					OnScroll(new ScrollEventArgs(ScrollEventType.EndScroll, -1, value, scrollOrientation));
				}
				else if (topArrowClicked)
				{
					topArrowClicked = false;
					topButtonState = ScrollBarState.Normal;
					StopTimer();
				}
				else if (bottomArrowClicked)
				{
					bottomArrowClicked = false;
					bottomButtonState = ScrollBarState.Normal;
					StopTimer();
				}
				else if (topBarClicked)
				{
					topBarClicked = false;
					StopTimer();
				}
				else if (bottomBarClicked)
				{
					bottomBarClicked = false;
					StopTimer();
				}
				Invalidate();
			}
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			bottomButtonState = ScrollBarState.Active;
			topButtonState = ScrollBarState.Active;
			thumbState = ScrollBarState.Active;
			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			ResetScrollStatus();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (e.Button == MouseButtons.Left)
			{
				if (thumbClicked)
				{
					topButtonState = ScrollBarState.Active;
					bottomButtonState = ScrollBarState.Active;
					int oldScrollValue = value;
					value = GetValueByThumb((isVertical ? e.Location.Y : e.Location.X) - thumbMouseOffset);
					ThumbPosition = GetThumbByValue();
					if (oldScrollValue != value)
					{
						OnScroll(new ScrollEventArgs(ScrollEventType.ThumbTrack, oldScrollValue, value, scrollOrientation));
						Refresh();
					}
				}
			}
			else if (!ClientRectangle.Contains(e.Location))
			{
				ResetScrollStatus();
			}
			if (topArrowRectangle.Contains(e.Location))
			{
				topButtonState = ScrollBarState.Hot;
				Invalidate(topArrowRectangle);
			}
			else if (bottomArrowRectangle.Contains(e.Location))
			{
				bottomButtonState = ScrollBarState.Hot;
				Invalidate(bottomArrowRectangle);
			}
			else if (thumbRectangle.Contains(e.Location))
			{
				thumbState = ScrollBarState.Hot;
				Invalidate(thumbRectangle);
			}
			else if (ClientRectangle.Contains(e.Location))
			{
				topButtonState = ScrollBarState.Active;
				bottomButtonState = ScrollBarState.Active;
				thumbState = ScrollBarState.Active;
				Invalidate();
			}
		}
		
		private int GetThumbByValue()
		{
			int pixelRange = size - 2 * arrowSize - thumbSize;
			int realRange = maximum - largeChange;
			return (realRange > 0 ? FixValue(value) * pixelRange / realRange : 0) + arrowSize;
		}
		
		private int GetValueByThumb(int thumbPosition)
		{
			int pixelRange = size - 2 * arrowSize - thumbSize;
			return FixValue(pixelRange > 0 ? (thumbPosition - arrowSize) * (maximum - largeChange) / pixelRange : 0);
		}
		
		private int ThumbPosition
		{
			get { return isVertical ? thumbRectangle.Y : thumbRectangle.X; }
			set
			{
				if (isVertical)
				{
					thumbRectangle.Y = value;
				}
				else
				{
					thumbRectangle.X = value;
				}
			}
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			SetUpScrollBar();
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			if (Enabled)
			{
				thumbState = ScrollBarState.Normal;
				topButtonState = ScrollBarState.Normal;
				bottomButtonState = ScrollBarState.Normal;
			}
			else
			{
				thumbState = ScrollBarState.Disabled;
				topButtonState = ScrollBarState.Disabled;
				bottomButtonState = ScrollBarState.Disabled;
			}
			Refresh();
		}

		private void SetUpScrollBar()
		{
			size = isVertical ? Height : Width;
			arrowSize = 17;
			thumbSize = maximum > 0 ?
				Math.Min(size - arrowSize * 2, Math.Max(largeChange * (size - arrowSize * 2) / maximum, 10)) :
				size - arrowSize;
			if (isVertical)
			{
				thumbRectangle = new Rectangle(ClientRectangle.X, ClientRectangle.Y + arrowSize, 18, thumbSize);
				topArrowRectangle = new Rectangle(ClientRectangle.X, ClientRectangle.Y, 18, arrowSize);
				bottomArrowRectangle = new Rectangle(ClientRectangle.X, ClientRectangle.Bottom - arrowSize, 18, arrowSize);
			}
			else
			{
				thumbRectangle = new Rectangle(ClientRectangle.X + arrowSize, ClientRectangle.Y, thumbSize, 18);
				topArrowRectangle = new Rectangle(ClientRectangle.X, ClientRectangle.Y, arrowSize, 18);
				bottomArrowRectangle = new Rectangle(ClientRectangle.Right - arrowSize, ClientRectangle.Y, arrowSize, 18);
			}
			ThumbPosition = GetThumbByValue();
			Refresh();
		}

		private void ProgressTimerTick(object sender, EventArgs e)
		{
			ProgressThumb(true);
		}

		private void ResetScrollStatus()
		{
			Point pos = PointToClient(Cursor.Position);
			if (ClientRectangle.Contains(pos))
			{
				bottomButtonState = ScrollBarState.Active;
				topButtonState = ScrollBarState.Active;
			}
			else
			{
				bottomButtonState = ScrollBarState.Normal;
				topButtonState = ScrollBarState.Normal;
			}
			thumbState = thumbRectangle.Contains(pos) ? ScrollBarState.Hot : ScrollBarState.Normal;
			bottomArrowClicked = bottomBarClicked = topArrowClicked = topBarClicked = false;
			StopTimer();
			Refresh();
		}

		private void EnableTimer()
		{
			if (!progressTimer.Enabled)
			{
				progressTimer.Interval = 600;
				progressTimer.Start();
			}
			else
			{
				progressTimer.Interval = 10;
			}
		}

		private void StopTimer()
		{
			progressTimer.Stop();
		}

		private void ProgressThumb(bool enableTimer)
		{
			int scrollOldValue = value;
			ScrollEventType type = ScrollEventType.First;
			int thumbSize, thumbPos;
			if (isVertical)
			{
				thumbPos = thumbRectangle.Y;
				thumbSize = thumbRectangle.Height;
			}
			else
			{
				thumbPos = thumbRectangle.X;
				thumbSize = thumbRectangle.Width;
			}
			if (bottomArrowClicked)
			{
				type = ScrollEventType.SmallIncrement;
				value = FixValue(value + smallChange);
				ThumbPosition = GetThumbByValue();
			}
			else if (topArrowClicked)
			{
				type = ScrollEventType.SmallIncrement;
				value = FixValue(value - smallChange);
				ThumbPosition = GetThumbByValue();
			}
			else if (bottomBarClicked && thumbPos + thumbSize < trackMousePosition)
			{
				type = ScrollEventType.LargeIncrement;
				value = FixValue(value + largeChange);
				ThumbPosition = GetThumbByValue();
			}
			else if (topBarClicked && thumbPos > trackMousePosition)
			{
				type = ScrollEventType.LargeDecrement;
				value = FixValue(value - largeChange);
				ThumbPosition = GetThumbByValue();
			}
			if (scrollOldValue != value)
			{
				OnScroll(new ScrollEventArgs(type, scrollOldValue, value, scrollOrientation));
				Invalidate();
				if (enableTimer)
				{
					EnableTimer();
				}
			}
			else
			{
				if (topArrowClicked)
				{
					type = ScrollEventType.SmallDecrement;
				}
				else if (bottomArrowClicked)
				{
					type = ScrollEventType.SmallIncrement;
				}
				OnScroll(new ScrollEventArgs(type, value));
			}
		}
	}
}