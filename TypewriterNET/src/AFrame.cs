using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MulticaretEditor;

public class AFrame : Control
{
	private Nest nest;
	public Nest Nest { get { return nest; } }

	public AFrame()
	{
		TabStop = false;
	}

	public MainForm MainForm { get { return nest.MainForm; } }
	public KeyMap KeyMap { get { return nest.MainForm.KeyMap; } }
	public KeyMap DoNothingKeyMap { get { return nest.MainForm.DoNothingKeyMap; } }

	public void Create(Nest nest)
	{
		if (created)
			throw new Exception("Already created");
		created = true;

		this.nest = nest;
		nest.AFrame = this;
		DoCreate();
		MainForm.Controls.Add(this);
		MainForm.DoResize();
		Settings settings = MainForm.Settings;
		UpdateSettings(settings, UpdatePhase.Raw);
		if (settings.Parsed)
			UpdateSettings(settings, UpdatePhase.Parsed);
	}

	private bool created;
	public new bool Created { get { return created; } }

	private bool destroyed;
	public bool Destroyed { get { return destroyed; } }

	public void Destroy()
	{
		if (!created)
			throw new Exception("Not created");
		if (destroyed)
			throw new Exception("Already destroyed");
		destroyed = true;

		DoDestroy();
		nest.AFrame = null;
		MainForm.Controls.Remove(this);
		MainForm.DoResize();
	}

	virtual protected void DoDestroy()
	{
	}

	virtual protected void DoCreate()
	{
	}

	private Control top;
	private Control right;

	protected void InitResizing(Control top, Control right)
	{
		this.top = top;
		this.right = right;
		top.MouseDown += OnTabBarMouseDown;
		top.MouseUp += OnTabBarMouseUp;
		if (right != null)
		{
			right.MouseDown += OnSplitLineMouseDown;
			right.MouseUp += OnSplitLineMouseUp;
		}
	}

	virtual public Size MinSize { get { return new Size(100, 100); } }
	virtual public Frame AsFrame { get { return null; } }

	new virtual public bool Focused { get { return false; } }

	new virtual public void Focus()
	{
	}

	public void UpdateSettings(Settings settings, UpdatePhase phase)
	{
		DoUpdateSettings(settings, phase);
		SetNeedResize();
	}

	virtual protected void DoUpdateSettings(Settings settings, UpdatePhase phase)
	{
	}

	protected void SetNeedResize()
	{
		if (nest != null && nest.Owner != null)
			nest.Owner.needResize = true;
	}

	//--------------------------------------------------------------------------
	// Resizing X
	//--------------------------------------------------------------------------

	private int startX;
	private int startSizeX;
	private int startWidth;

	private void OnSplitLineMouseDown(object sender, MouseEventArgs e)
	{
		if (nest == null)
			return;
		Nest target = FindWidthTarget();
		if (target != null)
		{
			startX = Control.MousePosition.X;
			startSizeX = target.size;
			startWidth = target.frameSize.Width;
			if (right != null)
			{
				right.MouseMove += OnSplitLineMouseMove;
			}
		}
	}

	private void OnSplitLineMouseUp(object sender, MouseEventArgs e)
	{
		right.MouseMove -= OnSplitLineMouseMove;
	}

	private void OnSplitLineMouseMove(object sender, MouseEventArgs e)
	{
		if (nest == null)
			return;
		Nest target = FindWidthTarget();
		if (target != null)
		{
			int k = target.left ? -1 : 1;
			target.frameSize.Width = startWidth + k * (startX - Control.MousePosition.X);
            if (target.isPercents)
                target.size = target.FullWidth > 0 ? 100 * target.frameSize.Width / target.FullWidth : 100;
            else
                target.size = target.frameSize.Width;
			if (target.size < 0)
				target.size = 0;
			else if (target.isPercents && target.size > 100)
				target.size = 100;
			target.MainForm.DoResize();
			target.MarkSizeAsChanged();
		}
	}

	private Nest FindWidthTarget()
	{
		if (nest == null)
			return null;
		if (nest.Child != null && nest.hDivided && nest.left)
			return nest;
		for (Nest nestI = nest.Parent; nestI != null; nestI = nestI.Parent)
		{
			if (nestI.hDivided && !nestI.left)
				return nestI;
		}
		return null;
	}

	//--------------------------------------------------------------------------
	// Resizing Y
	//--------------------------------------------------------------------------

	private int startY;
	private int startSizeY;
	private int startHeight;

	private void OnTabBarMouseDown(object sender, MouseEventArgs e)
	{
		if (nest == null)
			return;
		Nest target = FindHeightTarget(nest);
		if (target != null)
		{
			startY = Control.MousePosition.Y;
			startSizeY = target.size;
			startHeight = target.frameSize.Height;
			top.MouseMove += OnTabBarMouseMove;
		}
	}

	private void OnTabBarMouseUp(object sender, MouseEventArgs e)
	{
		top.MouseMove -= OnTabBarMouseMove;
	}

	private void OnTabBarMouseMove(object sender, MouseEventArgs e)
	{
		if (nest == null)
			return;
		Nest target = FindHeightTarget(nest);
		if (target != null)
		{
			int k = target.left ? -1 : 1;
			target.frameSize.Height = startHeight + k * (startY - Control.MousePosition.Y);
			target.size = target.isPercents ? 100 * target.frameSize.Height / target.FullHeight : target.frameSize.Height;
			if (target.size < 0)
				target.size = 0;
			else if (target.isPercents && target.size > 100)
				target.size = 100;
			target.MainForm.DoResize();
			target.MarkSizeAsChanged();
		}
	}

	private Nest FindHeightTarget(Nest nest)
	{
		if (nest == null)
			return null;
		if (nest.Child != null && !nest.hDivided && !nest.left)
			return nest;
		for (Nest nestI = nest.Parent; nestI != null; nestI = nestI.Parent)
		{
			if (!nestI.hDivided && nestI.left)
				return nestI;
		}
		return null;
	}

	//--------------------------------------------------------------------------
	//
	//--------------------------------------------------------------------------
}
