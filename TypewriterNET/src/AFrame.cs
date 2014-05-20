using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;
using MulticaretEditor;

public class AFrame : Control
{
	public AFrame()
	{
		TabStop = false;
	}

	private Control top;
	private Control right;

	protected void InitResizing(Control top, Control right)
	{
		this.top = top;
		this.right = right;
		top.MouseDown += OnTabBarMouseDown;
		top.MouseUp += OnTabBarMouseUp;
		right.MouseDown += OnSplitLineMouseDown;
		right.MouseUp += OnSplitLineMouseUp;
	}

	private Nest nest;
	public Nest Nest { get { return nest; } }

	public void SetNest(Nest nest)
	{
		this.nest = nest;
	}

	public MainForm MainForm { get { return nest != null ? nest.MainForm : null; } }

	virtual public Size MinSize { get { return new Size(100, 100); } }
	virtual public Frame AsFrame { get { return null; } }

	new virtual public bool Focused { get { return false; } }

	new virtual public void Focus()
	{
	}

	public void UpdateSettings(Settings settings, FrameUpdateType type)
	{
		DoUpdateSettings(settings, type);
		SetNeedResize();
	}

	virtual protected void DoUpdateSettings(Settings settings, FrameUpdateType type)
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
			right.MouseMove += OnSplitLineMouseMove;
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
			target.size = target.isPercents ? 100 * target.frameSize.Width / target.FullWidth : target.frameSize.Width;
			if (target.size < 0)
				target.size = 0;
			else if (target.isPercents && target.size > 100)
				target.size = 100;
			target.MainForm.DoResize();
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
