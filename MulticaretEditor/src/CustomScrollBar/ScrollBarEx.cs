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
		private int thumbWidth = 18;
		private int thumbHeight;
		private int arrowWidth = 18;
		private int arrowHeight = 17;
		private int thumbMouseOffset;
		private int trackPosition;
		private Timer progressTimer = new Timer();

		public ScrollBarEx(bool isVertical, Scheme scheme)
		{
			this.isVertical = isVertical;
			this.scheme = scheme;
			
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);

			Width = 18;
			Height = 200;
			SetUpScrollBar();
			progressTimer.Interval = 20;
			progressTimer.Tick += ProgressTimerTick;
		}

		public event ScrollEventHandler Scroll;
		
		private Scheme scheme;
		
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
				if (value < 1)
				{
					value = 1;
				}
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
				if (value < 1)
				{
					value = 1;
				}
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
					this.ChangeThumbPosition(this.GetThumbPosition());
					this.OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, -1, this.value, this.scrollOrientation));
					this.Refresh();
				}
			}
		}
		
		private int FixValue(int value)
		{
			if (value < 0)
			{
				return 0;
			}
			if (value > maximum)
			{
				return maximum;
			}
			return value;
		}

		protected virtual void OnScroll(ScrollEventArgs e)
		{
			if (this.Scroll != null)
			{
				this.Scroll(this, e);
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
					thumbMouseOffset = isVertical ? mouseLocation.Y - thumbRectangle.Y : mouseLocation.X - thumbRectangle.X;
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
					trackPosition = isVertical ? mouseLocation.Y : mouseLocation.X;
					if (trackPosition < (isVertical ? thumbRectangle.Y : thumbRectangle.X))
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
					int oldScrollValue = value;
					topButtonState = ScrollBarState.Active;
					bottomButtonState = ScrollBarState.Active;
					ChangeThumbPosition((isVertical ? e.Location.Y : e.Location.X) - thumbMouseOffset);
					value = GetValue();
					ChangeThumbPosition(GetThumbPosition());
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
		
		private int GetValue()
		{
			int pixelRange, thumbPos, arrowSize;
			if (isVertical)
			{
				pixelRange = Height - (2 * arrowHeight) - thumbHeight;
				thumbPos = thumbRectangle.Y;
				arrowSize = arrowHeight;
			}
			else
			{
				pixelRange = Width - (2 * arrowWidth) - thumbWidth;
				thumbPos = thumbRectangle.X;
				arrowSize = arrowWidth;
			}
			if (pixelRange != 0)
			{
				return FixValue((thumbPos - arrowSize) * (maximum - largeChange) / pixelRange);
			}
			return 0;
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			if (DesignMode)
			{
				if (isVertical)
				{
					if (height < (2 * arrowHeight) + 10)
					{
						height = (2 * arrowHeight) + 10;
					}
					width = 18;
				}
				else
				{
					if (width < (2 * arrowWidth) + 10)
					{
						width = (2 * arrowWidth) + 10;
					}
					height = 18;
				}
			}
			base.SetBoundsCore(x, y, width, height, specified);
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
			if (isVertical)
			{
				arrowHeight = 17;
				arrowWidth = 18;
				thumbWidth = 18;
				thumbHeight = GetThumbSize();
				thumbRectangle = new Rectangle(
					ClientRectangle.X,
					ClientRectangle.Y + arrowHeight,
					thumbWidth,
					thumbHeight
				);
				topArrowRectangle = new Rectangle(
					ClientRectangle.X,
					ClientRectangle.Y,
					arrowWidth,
					arrowHeight
				);
				bottomArrowRectangle = new Rectangle(
					ClientRectangle.X,
					ClientRectangle.Bottom - arrowHeight,
					arrowWidth,
					arrowHeight
				);
			}
			else
			{
				arrowHeight = 18;
				arrowWidth = 17;
				thumbHeight = 18;
				thumbWidth = GetThumbSize();
				thumbRectangle = new Rectangle(
					ClientRectangle.X + arrowWidth,
					ClientRectangle.Y,
					thumbWidth,
					thumbHeight
				);
				topArrowRectangle = new Rectangle(
					ClientRectangle.X,
					ClientRectangle.Y,
					arrowWidth,
					arrowHeight
				);
				bottomArrowRectangle = new Rectangle(
					ClientRectangle.Right - arrowWidth,
					ClientRectangle.Y,
					arrowWidth,
					arrowHeight
				);
			}
			ChangeThumbPosition(GetThumbPosition());
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

		private int GetThumbPosition()
		{
			int pixelRange, arrowSize;
			if (isVertical)
			{
				pixelRange = Height - (2 * arrowHeight) - thumbHeight;
				arrowSize = arrowHeight;
			}
			else
			{
				pixelRange = Width - (2 * arrowWidth) - thumbWidth;
				arrowSize = arrowWidth;
			}
			int realRange = maximum - largeChange;
			if (realRange <= 0)
			{
				return arrowSize;
			}			
			return value * pixelRange / realRange + arrowSize;
		}

		private int GetThumbSize()
		{
			int trackSize = isVertical ? Height - (2 * arrowHeight) : Width - (2 * arrowWidth);
			if (maximum == 0 || largeChange == 0)
			{
				return trackSize;
			}
			int newThumbSize = largeChange * trackSize / maximum;
			return Math.Min(trackSize, Math.Max(newThumbSize, 10));
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

		private void ChangeThumbPosition(int position)
		{
			if (isVertical)
			{
				thumbRectangle.Y = position;
			}
			else
			{
				thumbRectangle.X = position;
			}
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
				ChangeThumbPosition(GetThumbPosition());
			}
			else if (topArrowClicked)
			{
				type = ScrollEventType.SmallIncrement;
				value = FixValue(value - smallChange);
				ChangeThumbPosition(GetThumbPosition());
			}
			else if (bottomBarClicked && thumbPos + thumbSize < trackPosition)
			{
				type = ScrollEventType.LargeIncrement;
				value = FixValue(value + largeChange);
				ChangeThumbPosition(GetThumbPosition());
			}
			else if (topBarClicked && thumbPos > trackPosition)
			{
				type = ScrollEventType.LargeDecrement;
				value = FixValue(value - largeChange);
				ChangeThumbPosition(GetThumbPosition());
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