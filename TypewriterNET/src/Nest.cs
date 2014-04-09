using System.Collections.Generic;
using System.Drawing;

public class Nest
{
	public readonly Frame frame;
	public readonly Nest child;

	public bool hDivided;
	public bool left;

	public bool isPercents = true;
	public int size = 50;
	public Size minSize;

	public Size frameSize;
	public Size FrameSize { get { return frameSize; } }

	public Nest(Frame frame, Nest child)
	{
		this.frame = frame;
		this.child = child;
		frame.SetNest(this);
	}

	public int GetSize(int nestSize)
	{
		int result = isPercents ? nestSize * size / 100 : nestSize;
		if (result > nestSize)
		{
			result = nestSize;
		}
		else if (hDivided)
		{
			if (result < minSize.Width)
				result = minSize.Width;
		}
		else
		{
			if (result < minSize.Height)
				result = minSize.Height;
		}
		return result;
	}

	public void SetFrameSize(Size size)
	{
		this.frameSize = size;
		frame.Size = size;
	}

	private MainForm mainForm;
	public MainForm MainForm { get { return mainForm; } }

	public void Init(MainForm mainForm)
	{
		this.mainForm = mainForm;
	}
}
