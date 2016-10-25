namespace CustomScrollBar
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;

	[DefaultEvent("Scroll")]
	[DefaultProperty("Value")]
	public class ScrollBarEx : Control
	{
		private bool isVertical = true;
		private ScrollOrientation scrollOrientation = ScrollOrientation.VerticalScroll;
		private Rectangle clickedBarRectangle;
		private Rectangle thumbRectangle;
		private Rectangle topArrowRectangle;
		private Rectangle bottomArrowRectangle;
		private Rectangle channelRectangle;
		private bool topArrowClicked;
		private bool bottomArrowClicked;
		private bool topBarClicked;
		private bool bottomBarClicked;
		private bool thumbClicked;
		private ScrollBarState thumbState = ScrollBarState.Normal;
		private ScrollBarArrowButtonState topButtonState = ScrollBarArrowButtonState.UpNormal;
		private ScrollBarArrowButtonState bottomButtonState = ScrollBarArrowButtonState.DownNormal;
		private int minimum;
		private int maximum = 100;
		private int smallChange = 20;
		private int largeChange = 10;
		private int value;
		private int thumbWidth = 15;
		private int thumbHeight;
		private int arrowWidth = 15;
		private int arrowHeight = 17;
		private int thumbBottomLimitBottom;
		private int thumbBottomLimitTop;
		private int thumbTopLimit;
		private int thumbPosition;
		private int trackPosition;
		private Timer progressTimer = new Timer();

		public ScrollBarEx(bool isVertical)
		{
			this.isVertical = isVertical;
			
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);

			this.Width = 19;
			this.Height = 200;
			this.SetUpScrollBar();
			this.progressTimer.Interval = 20;
			this.progressTimer.Tick += this.ProgressTimerTick;
		}

		public event ScrollEventHandler Scroll;

		[DefaultValue(0)]
		public int Minimum
		{
			get
			{
				return this.minimum;
			}
			set
			{
				if (this.minimum == value || value < 0 || value >= this.maximum)
				{
					return;
				}
				this.minimum = value;
				if (this.value < value)
				{
					this.value = value;
				}
				if (this.largeChange > this.maximum - this.minimum)
				{
					this.largeChange = this.maximum - this.minimum;
				}
				this.SetUpScrollBar();
				if (this.value < value)
				{
					this.Value = value;
				}
				else
				{
					this.ChangeThumbPosition(this.GetThumbPosition());
					this.Refresh();
				}
			}
		}

		[DefaultValue(100)]
		public int Maximum
		{
			get
			{
				return this.maximum;
			}
			set
			{
				if (value == this.maximum || value < 1 || value <= this.minimum)
				{
					return;
				}
				this.maximum = value;
				if (this.largeChange > this.maximum - this.minimum)
				{
					this.largeChange = this.maximum - this.minimum;
				}
				this.SetUpScrollBar();
				if (this.value > value)
				{
					this.Value = this.maximum;
				}
				else
				{
					this.ChangeThumbPosition(this.GetThumbPosition());
					this.Refresh();
				}
			}
		}

		[DefaultValue(1)]
		public int SmallChange
		{
			get
			{
				return this.smallChange;
			}
			set
			{
				if (value == this.smallChange || value < 1 || value >= this.largeChange)
				{
					return;
				}
				this.smallChange = value;
				this.SetUpScrollBar();
			}
		}

		[DefaultValue(10)]
		public int LargeChange
		{
			get
			{
				return this.largeChange;
			}
			set
			{
				if (value == this.largeChange || value < this.smallChange || value < 2)
				{
					return;
				}
				if (value > this.maximum - this.minimum)
				{
					this.largeChange = this.maximum - this.minimum;
				}
				else
				{
					this.largeChange = value;
				}
				this.SetUpScrollBar();
			}
		}

		[DefaultValue(0)]
		public int Value
		{
			get
			{
				return this.value;
			}
			set
			{
				if (this.value == value || value < this.minimum || value > this.maximum)
				{
					return;
				}
				this.value = value;
				this.ChangeThumbPosition(this.GetThumbPosition());
				this.OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, -1, this.value, this.scrollOrientation));
				this.Refresh();
			}
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
			Rectangle rect = ClientRectangle;
			if (isVertical)
			{
				rect.X++;
				rect.Y += this.arrowHeight + 1;
				rect.Width -= 2;
				rect.Height -= (this.arrowHeight * 2) + 2;
			}
			else
			{
				rect.X += this.arrowWidth + 1;
				rect.Y++;
				rect.Width -= (this.arrowWidth * 2) + 2;
				rect.Height -= 2;
			}
			ScrollBarExRenderer.DrawBackground(e.Graphics, ClientRectangle, isVertical);
			ScrollBarExRenderer.DrawTrack(e.Graphics, rect, ScrollBarState.Normal, isVertical);
			ScrollBarExRenderer.DrawThumb(e.Graphics, this.thumbRectangle, this.thumbState, isVertical);
			ScrollBarExRenderer.DrawArrowButton(e.Graphics, this.topArrowRectangle, this.topButtonState, true, isVertical);
			ScrollBarExRenderer.DrawArrowButton(e.Graphics, this.bottomArrowRectangle, this.bottomButtonState, false, isVertical);
			if (this.topBarClicked)
			{
				if (isVertical)
				{
					this.clickedBarRectangle.Y = this.thumbTopLimit;
					this.clickedBarRectangle.Height = this.thumbRectangle.Y - this.thumbTopLimit;
				}
				else
				{
					this.clickedBarRectangle.X = this.thumbTopLimit;
					this.clickedBarRectangle.Width = this.thumbRectangle.X - this.thumbTopLimit;
				}
				ScrollBarExRenderer.DrawTrack(e.Graphics, this.clickedBarRectangle, ScrollBarState.Pressed, isVertical);
			}
			else if (this.bottomBarClicked)
			{
				if (isVertical)
				{
					this.clickedBarRectangle.Y = this.thumbRectangle.Bottom + 1;
					this.clickedBarRectangle.Height = this.thumbBottomLimitBottom - this.clickedBarRectangle.Y + 1;
				}
				else
				{
					this.clickedBarRectangle.X = this.thumbRectangle.Right + 1;
					this.clickedBarRectangle.Width = this.thumbBottomLimitBottom - this.clickedBarRectangle.X + 1;
				}
				ScrollBarExRenderer.DrawTrack(e.Graphics, this.clickedBarRectangle, ScrollBarState.Pressed, isVertical);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left)
			{
				Point mouseLocation = e.Location;
				if (this.thumbRectangle.Contains(mouseLocation))
				{
					this.thumbClicked = true;
					this.thumbPosition = isVertical ? mouseLocation.Y - this.thumbRectangle.Y : mouseLocation.X - this.thumbRectangle.X;
					this.thumbState = ScrollBarState.Pressed;
					Invalidate(this.thumbRectangle);
				}
				else if (this.topArrowRectangle.Contains(mouseLocation))
				{
					this.topArrowClicked = true;
					this.topButtonState = ScrollBarArrowButtonState.UpPressed;
					this.Invalidate(this.topArrowRectangle);
					this.ProgressThumb(true);
				}
				else if (this.bottomArrowRectangle.Contains(mouseLocation))
				{
					this.bottomArrowClicked = true;
					this.bottomButtonState = ScrollBarArrowButtonState.DownPressed;
					this.Invalidate(this.bottomArrowRectangle);
					this.ProgressThumb(true);
				}
				else
				{
					this.trackPosition = isVertical ? mouseLocation.Y : mouseLocation.X;
					if (this.trackPosition < (isVertical ? this.thumbRectangle.Y : this.thumbRectangle.X))
					{
						this.topBarClicked = true;
					}
					else
					{
						this.bottomBarClicked = true;
					}
					this.ProgressThumb(true);
				}
			}
			else if (e.Button == MouseButtons.Right)
			{
				this.trackPosition = isVertical ? e.Y : e.X;
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (e.Button == MouseButtons.Left)
			{
				if (this.thumbClicked)
				{
					this.thumbClicked = false;
					this.thumbState = ScrollBarState.Normal;
					this.OnScroll(new ScrollEventArgs(ScrollEventType.EndScroll, -1, this.value, this.scrollOrientation));
				}
				else if (this.topArrowClicked)
				{
					this.topArrowClicked = false;
					this.topButtonState = ScrollBarArrowButtonState.UpNormal;
					this.StopTimer();
				}
				else if (this.bottomArrowClicked)
				{
					this.bottomArrowClicked = false;
					this.bottomButtonState = ScrollBarArrowButtonState.DownNormal;
					this.StopTimer();
				}
				else if (this.topBarClicked)
				{
					this.topBarClicked = false;
					this.StopTimer();
				}
				else if (this.bottomBarClicked)
				{
					this.bottomBarClicked = false;
					this.StopTimer();
				}
				Invalidate();
			}
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			this.bottomButtonState = ScrollBarArrowButtonState.DownActive;
			this.topButtonState = ScrollBarArrowButtonState.UpActive;
			this.thumbState = ScrollBarState.Active;
			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			this.ResetScrollStatus();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (e.Button == MouseButtons.Left)
			{
				if (this.thumbClicked)
				{
					int oldScrollValue = this.value;
					this.topButtonState = ScrollBarArrowButtonState.UpActive;
					this.bottomButtonState = ScrollBarArrowButtonState.DownActive;
					int pos = isVertical ? e.Location.Y : e.Location.X;
					if (pos <= (this.thumbTopLimit + this.thumbPosition))
					{
						this.ChangeThumbPosition(this.thumbTopLimit);
						this.value = this.minimum;
					}
					else if (pos >= (this.thumbBottomLimitTop + this.thumbPosition))
					{
						this.ChangeThumbPosition(this.thumbBottomLimitTop);
						this.value = this.maximum;
					}
					else
					{
						this.ChangeThumbPosition(pos - this.thumbPosition);
						int pixelRange, thumbPos, arrowSize;
						if (isVertical)
						{
							pixelRange = this.Height - (2 * this.arrowHeight) - this.thumbHeight;
							thumbPos = this.thumbRectangle.Y;
							arrowSize = this.arrowHeight;
						}
						else
						{
							pixelRange = this.Width - (2 * this.arrowWidth) - this.thumbWidth;
							thumbPos = this.thumbRectangle.X;
							arrowSize = this.arrowWidth;
						}
						if (pixelRange <= 0)
						{
							this.value = 0;
						}
						else
						{
							this.value = (thumbPos - arrowSize) * (this.maximum - this.minimum - this.largeChange) / pixelRange + this.minimum;
						}
					}

					if (oldScrollValue != this.value)
					{
						this.OnScroll(new ScrollEventArgs(ScrollEventType.ThumbTrack, oldScrollValue, this.value, this.scrollOrientation));
						this.Refresh();
					}
				}
			}
			else if (!this.ClientRectangle.Contains(e.Location))
			{
				this.ResetScrollStatus();
			}
			{
				if (this.topArrowRectangle.Contains(e.Location))
				{
					this.topButtonState = ScrollBarArrowButtonState.UpHot;
					this.Invalidate(this.topArrowRectangle);
				}
				else if (this.bottomArrowRectangle.Contains(e.Location))
				{
					this.bottomButtonState = ScrollBarArrowButtonState.DownHot;
					Invalidate(this.bottomArrowRectangle);
				}
				else if (this.thumbRectangle.Contains(e.Location))
				{
					this.thumbState = ScrollBarState.Hot;
					this.Invalidate(this.thumbRectangle);
				}
				else if (this.ClientRectangle.Contains(e.Location))
				{
					this.topButtonState = ScrollBarArrowButtonState.UpActive;
					this.bottomButtonState = ScrollBarArrowButtonState.DownActive;
					this.thumbState = ScrollBarState.Active;
					Invalidate();
				}
			}
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			if (this.DesignMode)
			{
				if (isVertical)
				{
					if (height < (2 * this.arrowHeight) + 10)
					{
						height = (2 * this.arrowHeight) + 10;
					}
					width = 19;
				}
				else
				{
					if (width < (2 * this.arrowWidth) + 10)
					{
						width = (2 * this.arrowWidth) + 10;
					}
					height = 19;
				}
			}
			base.SetBoundsCore(x, y, width, height, specified);
			if (this.DesignMode)
			{
				this.SetUpScrollBar();
			}
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			this.SetUpScrollBar();
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			Keys keyUp = Keys.Up;
			Keys keyDown = Keys.Down;
			if (!isVertical)
			{
				keyUp = Keys.Left;
				keyDown = Keys.Right;
			}
			if (keyData == keyUp)
			{
				this.Value -= this.smallChange;
				return true;
			}
			if (keyData == keyDown)
			{
				this.Value += this.smallChange;
				return true;
			}
			if (keyData == Keys.PageUp)
			{
				this.Value = this.GetValue(false, true);
				return true;
			}
			if (keyData == Keys.PageDown)
			{
				if (this.value + this.largeChange > this.maximum)
				{
					this.Value = this.maximum;
				}
				else
				{
					this.Value += this.largeChange;
				}
				return true;
			}
			if (keyData == Keys.Home)
			{
				this.Value = this.minimum;
				return true;
			}
			if (keyData == Keys.End)
			{
				this.Value = this.maximum;
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			if (this.Enabled)
			{
				this.thumbState = ScrollBarState.Normal;
				this.topButtonState = ScrollBarArrowButtonState.UpNormal;
				this.bottomButtonState = ScrollBarArrowButtonState.DownNormal;
			}
			else
			{
				this.thumbState = ScrollBarState.Disabled;
				this.topButtonState = ScrollBarArrowButtonState.UpDisabled;
				this.bottomButtonState = ScrollBarArrowButtonState.DownDisabled;
			}
			this.Refresh();
		}

		[DllImport("user32.dll")]
		private static extern int SendMessage(IntPtr wnd, int msg, bool param, int lparam);

		private void SetUpScrollBar()
		{
			if (isVertical)
			{
				this.arrowHeight = 17;
				this.arrowWidth = 15;
				this.thumbWidth = 15;
				this.thumbHeight = this.GetThumbSize();

				this.clickedBarRectangle = this.ClientRectangle;
				this.clickedBarRectangle.Inflate(-1, -1);
				this.clickedBarRectangle.Y += this.arrowHeight;
				this.clickedBarRectangle.Height -= this.arrowHeight * 2;

				this.channelRectangle = this.clickedBarRectangle;
				this.thumbRectangle = new Rectangle(
					ClientRectangle.X + 2,
					ClientRectangle.Y + this.arrowHeight + 1,
					this.thumbWidth - 1,
					this.thumbHeight
				);
				this.topArrowRectangle = new Rectangle(
					ClientRectangle.X + 2,
					ClientRectangle.Y + 1,
					this.arrowWidth,
					this.arrowHeight
				);
				this.bottomArrowRectangle = new Rectangle(
					ClientRectangle.X + 2,
					ClientRectangle.Bottom - this.arrowHeight - 1,
					this.arrowWidth,
					this.arrowHeight
				);
				this.thumbPosition = this.thumbRectangle.Height / 2;
				this.thumbBottomLimitBottom = ClientRectangle.Bottom - this.arrowHeight - 2;
				this.thumbBottomLimitTop = this.thumbBottomLimitBottom - this.thumbRectangle.Height;
				this.thumbTopLimit = ClientRectangle.Y + this.arrowHeight + 1;
			}
			else
			{
				this.arrowHeight = 15;
				this.arrowWidth = 17;
				this.thumbHeight = 15;
				this.thumbWidth = this.GetThumbSize();

				this.clickedBarRectangle = this.ClientRectangle;
				this.clickedBarRectangle.Inflate(-1, -1);
				this.clickedBarRectangle.X += this.arrowWidth;
				this.clickedBarRectangle.Width -= this.arrowWidth * 2;

				this.channelRectangle = this.clickedBarRectangle;
				this.thumbRectangle = new Rectangle(
					ClientRectangle.X + this.arrowWidth + 1,
					ClientRectangle.Y + 2,
					this.thumbWidth,
					this.thumbHeight - 1
				);
				this.topArrowRectangle = new Rectangle(
					ClientRectangle.X + 1,
					ClientRectangle.Y + 2,
					this.arrowWidth,
					this.arrowHeight
				);
				this.bottomArrowRectangle = new Rectangle(
					ClientRectangle.Right - this.arrowWidth - 1,
					ClientRectangle.Y + 2,
					this.arrowWidth,
					this.arrowHeight
				);
				this.thumbPosition = this.thumbRectangle.Width / 2;
				this.thumbBottomLimitBottom = ClientRectangle.Right - this.arrowWidth - 2;
				this.thumbBottomLimitTop = this.thumbBottomLimitBottom - this.thumbRectangle.Width;
				this.thumbTopLimit = ClientRectangle.X + this.arrowWidth + 1;
			}
			this.ChangeThumbPosition(this.GetThumbPosition());
			this.Refresh();
		}

		private void ProgressTimerTick(object sender, EventArgs e)
		{
			this.ProgressThumb(true);
		}

		private void ResetScrollStatus()
		{
			Point pos = this.PointToClient(Cursor.Position);
			if (this.ClientRectangle.Contains(pos))
			{
				this.bottomButtonState = ScrollBarArrowButtonState.DownActive;
				this.topButtonState = ScrollBarArrowButtonState.UpActive;
			}
			else
			{
				this.bottomButtonState = ScrollBarArrowButtonState.DownNormal;
				this.topButtonState = ScrollBarArrowButtonState.UpNormal;
			}
			this.thumbState = this.thumbRectangle.Contains(pos) ? ScrollBarState.Hot : ScrollBarState.Normal;
			this.bottomArrowClicked = this.bottomBarClicked = this.topArrowClicked = this.topBarClicked = false;
			this.StopTimer();
			this.Refresh();
		}

		private int GetValue(bool smallIncrement, bool up)
		{
			int newValue;
			if (up)
			{
				newValue = this.value - (smallIncrement ? this.smallChange : this.largeChange);
				if (newValue < this.minimum)
				{
					newValue = this.minimum;
				}
			}
			else
			{
				newValue = this.value + (smallIncrement ? this.smallChange : this.largeChange);
				if (newValue > this.maximum)
				{
					newValue = this.maximum;
				}
			}
			return newValue;
		}

		private int GetThumbPosition()
		{
			int pixelRange, arrowSize;
			if (isVertical)
			{
				pixelRange = this.Height - (2 * this.arrowHeight) - this.thumbHeight;
				arrowSize = this.arrowHeight;
			}
			else
			{
				pixelRange = this.Width - (2 * this.arrowWidth) - this.thumbWidth;
				arrowSize = this.arrowWidth;
			}
			int realRange = this.maximum - this.minimum - this.largeChange;
			if (realRange <= 0)
			{
				return arrowSize;
			}			
			return (this.value - this.minimum) * pixelRange / realRange + arrowSize;
		}

		private int GetThumbSize()
		{
			int trackSize = isVertical ? this.Height - (2 * this.arrowHeight) : this.Width - (2 * this.arrowWidth);
			if (this.maximum == 0 || this.largeChange == 0)
			{
				return trackSize;
			}
			int newThumbSize = this.largeChange * trackSize / this.maximum;
			return Math.Min(trackSize, Math.Max(newThumbSize, 10));
		}

		private void EnableTimer()
		{
			if (!this.progressTimer.Enabled)
			{
				this.progressTimer.Interval = 600;
				this.progressTimer.Start();
			}
			else
			{
				this.progressTimer.Interval = 10;
			}
		}

		private void StopTimer()
		{
			this.progressTimer.Stop();
		}

		private void ChangeThumbPosition(int position)
		{
			if (isVertical)
			{
				this.thumbRectangle.Y = position;
			}
			else
			{
				this.thumbRectangle.X = position;
			}
		}

		private void ProgressThumb(bool enableTimer)
		{
			int scrollOldValue = this.value;
			ScrollEventType type = ScrollEventType.First;
			int thumbSize, thumbPos;
			if (isVertical)
			{
				thumbPos = this.thumbRectangle.Y;
				thumbSize = this.thumbRectangle.Height;
			}
			else
			{
				thumbPos = this.thumbRectangle.X;
				thumbSize = this.thumbRectangle.Width;
			}
			if (this.bottomArrowClicked || (this.bottomBarClicked && (thumbPos + thumbSize) < this.trackPosition))
			{
				type = this.bottomArrowClicked ? ScrollEventType.SmallIncrement : ScrollEventType.LargeIncrement;
				this.value = this.GetValue(this.bottomArrowClicked, false);
				if (this.value == this.maximum)
				{
					this.ChangeThumbPosition(this.thumbBottomLimitTop);
					type = ScrollEventType.Last;
				}
				else
				{
					this.ChangeThumbPosition(Math.Min(this.thumbBottomLimitTop, this.GetThumbPosition()));
				}
			}
			else if (this.topArrowClicked || (this.topBarClicked && thumbPos > this.trackPosition))
			{
				type = this.topArrowClicked ? ScrollEventType.SmallDecrement : ScrollEventType.LargeDecrement;
				this.value = this.GetValue(this.topArrowClicked, true);
				if (this.value == this.minimum)
				{
					this.ChangeThumbPosition(this.thumbTopLimit);
					type = ScrollEventType.First;
				}
				else
				{
					this.ChangeThumbPosition(Math.Max(this.thumbTopLimit, this.GetThumbPosition()));
				}
			}
			else if (!((this.topArrowClicked && thumbPos == this.thumbTopLimit) || (this.bottomArrowClicked && thumbPos == this.thumbBottomLimitTop)))
			{
				this.ResetScrollStatus();
				return;
			}
			if (scrollOldValue != this.value)
			{
				this.OnScroll(new ScrollEventArgs(type, scrollOldValue, this.value, this.scrollOrientation));
				this.Invalidate(this.channelRectangle);
				if (enableTimer)
				{
					this.EnableTimer();
				}
			}
			else
			{
				if (this.topArrowClicked)
				{
					type = ScrollEventType.SmallDecrement;
				}
				else if (this.bottomArrowClicked)
				{
					type = ScrollEventType.SmallIncrement;
				}
				this.OnScroll(new ScrollEventArgs(type, this.value));
			}
		}

		private void ScrollHereClick(object sender, EventArgs e)
		{
			int thumbSize, thumbPos, arrowSize, size;
			if (isVertical)
			{
				thumbSize = this.thumbHeight;
				arrowSize = this.arrowHeight;
				size = this.Height;
				this.ChangeThumbPosition(Math.Max(this.thumbTopLimit, Math.Min(this.thumbBottomLimitTop, this.trackPosition - (this.thumbRectangle.Height / 2))));
				thumbPos = this.thumbRectangle.Y;
			}
			else
			{
				thumbSize = this.thumbWidth;
				arrowSize = this.arrowWidth;
				size = this.Width;
				this.ChangeThumbPosition(Math.Max(this.thumbTopLimit, Math.Min(this.thumbBottomLimitTop, this.trackPosition - (this.thumbRectangle.Width / 2))));
				thumbPos = this.thumbRectangle.X;
			}
			int pixelRange = size - (2 * arrowSize) - thumbSize;
			float perc = 0f;
			if (pixelRange != 0)
			{
				perc = (float)(thumbPos - arrowSize) / (float)pixelRange;
			}
			int oldValue = this.value;
			this.value = Convert.ToInt32((perc * (this.maximum - this.minimum)) + this.minimum);
			this.OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, oldValue, this.value, this.scrollOrientation));
			this.Refresh();
		}

		private void TopClick(object sender, EventArgs e)
		{
			this.Value = this.minimum;
		}

		private void BottomClick(object sender, EventArgs e)
		{
			this.Value = this.maximum;
		}

		private void LargeUpClick(object sender, EventArgs e)
		{
			this.Value = this.GetValue(false, true);
		}

		private void LargeDownClick(object sender, EventArgs e)
		{
			this.Value = this.GetValue(false, false);
		}

		private void SmallUpClick(object sender, EventArgs e)
		{
			this.Value = this.GetValue(true, true);
		}

		private void SmallDownClick(object sender, EventArgs e)
		{
			this.Value = this.GetValue(true, false);
		}
	}
}