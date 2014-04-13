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

public class Frame : Control
{
	private TabBar<string> tabBar;
	private SplitLine splitLine;
	private MulticaretTextBox textBox;

	public Frame(string name)
	{
		Name = name;
		TabStop = false;

		SwitchList<string> list = new SwitchList<string>();
		list.Add("File 1");
		list.Add("File 2");
		tabBar = new TabBar<string>(list, TabBar<string>.DefaultStringOf);
		tabBar.Text = name;
		Controls.Add(tabBar);

		splitLine = new SplitLine();
		Controls.Add(splitLine);

		textBox = new MulticaretTextBox();
		textBox.FocusedChange += OnTextBoxFocusedChange;
		Controls.Add(textBox);

		tabBar.MouseDown += OnTabBarMouseDown;
		tabBar.MouseUp += OnTabBarMouseUp;
		splitLine.MouseDown += OnSplitLineMouseDown;
		splitLine.MouseUp += OnSplitLineMouseUp;
	}

	private void OnTextBoxFocusedChange()
	{
		tabBar.Selected = textBox.Focused;
	}

	public string Title
	{
		get { return tabBar.Text; }
		set { tabBar.Text = value; }
	}

	private Nest nest;
	public Nest Nest { get { return nest; } }

	public void SetNest(Nest nest)
	{
		this.nest = nest;
	}

	override protected void OnResize(EventArgs e)
	{
		base.OnResize(e);
		int tabBarHeight = tabBar.Height;
		tabBar.Size = new Size(Width, tabBarHeight);
		splitLine.Location = new Point(Width - 10, tabBarHeight);
		splitLine.Size = new Size(10, Height - tabBarHeight);
		textBox.Location = new Point(0, tabBarHeight);
		textBox.Size = new Size(Width - 10, Height - tabBarHeight);
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
			splitLine.MouseMove += OnSplitLineMouseMove;
		}
	}

	private void OnSplitLineMouseUp(object sender, MouseEventArgs e)
	{
		splitLine.MouseMove -= OnSplitLineMouseMove;
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
		if (nest.hDivided && nest.left)
			return nest;
		for (Nest nestI = nest.parent; nestI != null; nestI = nestI.parent)
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
		textBox.Focus();
		if (nest == null)
			return;
		Nest target = FindHeightTarget();
		if (target != null)
		{
			startY = Control.MousePosition.Y;
			startSizeY = target.size;
			startHeight = target.frameSize.Height;
			tabBar.MouseMove += OnTabBarMouseMove;
		}
	}

	private void OnTabBarMouseUp(object sender, MouseEventArgs e)
	{
		tabBar.MouseMove -= OnTabBarMouseMove;
	}

	private void OnTabBarMouseMove(object sender, MouseEventArgs e)
	{
		if (nest == null)
			return;
		Nest target = FindHeightTarget();
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

	private Nest FindHeightTarget()
	{
		if (nest == null)
			return null;
		if (!nest.hDivided && !nest.left)
			return nest;
		for (Nest nestI = nest.parent; nestI != null; nestI = nestI.parent)
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
