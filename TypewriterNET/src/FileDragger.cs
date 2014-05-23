using System;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Resources;
using System.Xml;
using MulticaretEditor;
using MulticaretEditor.Highlighting;
using MulticaretEditor.KeyMapping;

public class FileDragger
{
	private MainForm mainForm;

	public FileDragger(MainForm mainForm)
	{
		this.mainForm = mainForm;
		mainForm.AllowDrop = true;
		mainForm.DragEnter += OnDragEnter;
		mainForm.DragDrop += OnDragDrop;
	}

	private void OnDragEnter(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
			e.Effect = DragDropEffects.All;
	}
	
	private void OnDragDrop(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
			if (files != null)
			{
				foreach (string fileI in files)
				{
					FileAttributes attributes = File.GetAttributes(fileI);
					if ((attributes & FileAttributes.Directory) > 0)
					{
						foreach (string fileJ in Directory.GetFiles(fileI, "*.*", SearchOption.AllDirectories))
						{
							mainForm.LoadFile(fileJ);
						}
					}
					else
					{
						mainForm.LoadFile(fileI);
					}
				}
			}
		}
	}
}
