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
	private MulticaretTextBox textBox;

	public Frame(string name)
	{
		Name = name;
		TabStop = false;
		SwitchList<string> list = new SwitchList<string>();
		tabBar = new TabBar<string>(list, TabBar<string>.DefaultStringOf);
		tabBar.Text = name;
		Controls.Add(tabBar);

		textBox = new MulticaretTextBox();
		textBox.Location = new Point(0, 20);
		Controls.Add(textBox);

		tabBar.MouseDown += OnTabBarMouseDown;
		tabBar.MouseUp += OnTabBarMouseUp;
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
		tabBar.Size = new Size(Width, 20);
		textBox.Size = new Size(Width, Height - 20);
	}

	private Point startPoint;
	private Size startSize;

	private void OnTabBarMouseDown(object sender, MouseEventArgs e)
	{
		if (nest == null)
			return;
		startPoint = new Point(e.X, e.Y);
		startSize = nest.FrameSize;
		tabBar.MouseMove += OnTabBarMouseMove;
	}

	private void OnTabBarMouseUp(object sender, MouseEventArgs e)
	{
		tabBar.MouseMove -= OnTabBarMouseMove;
	}

	private void OnTabBarMouseMove(object sender, MouseEventArgs e)
	{
		if (nest == null)
			return;
		int leftK = nest.left ? -1 : 1;
		if (nest.hDivided)
			nest.size = nest.size * (Width + leftK * (startPoint.X - e.X)) / Width;
		else
			nest.size = nest.size * (Height + leftK * (startPoint.Y - e.Y)) / Height;
		if (nest.size < 1)
			nest.size = 1;
		else if (nest.isPercents && nest.size > 99)
			nest.size = 99;
		nest.MainForm.DoResize();
	}
}
