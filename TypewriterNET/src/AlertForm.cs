using System;
using System.Windows.Forms;
using MulticaretEditor;

public partial class AlertForm : Form
{
	private Setter onCanceled;
	private MainForm mainForm;
	private Button button;
	
	public AlertForm(MainForm mainForm, Setter onCanceled)
	{
		this.mainForm = mainForm;
		this.onCanceled = onCanceled;
		
		SuspendLayout();
		
		Width = 200;
		Height = 100;
		ControlBox = false;
		FormBorderStyle = FormBorderStyle.FixedSingle;
		
		button = new Button();
		button.Text = "Stop search";
		button.Dock = DockStyle.Fill;
		button.Click += OnCancelClick;
		Controls.Add(button);
		Closing += OnFormClosing;
		ResumeLayout();
		
		Load += OnLoad;
	}
	
	private void OnLoad(object sender, EventArgs e)
	{
		Left = mainForm.Left + mainForm.Width - Width;
		Top = mainForm.Top + mainForm.Height - Height;
	}
	
	public bool forcedClosing;
	
	private void OnFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
	{
		if (!forcedClosing)
			e.Cancel = true;
		button.Visible = false;
		if (onCanceled != null)
			onCanceled();
	}

	private void OnCancelClick(object sender, EventArgs e)
	{
		button.Visible = false;
		if (onCanceled != null)
			onCanceled();
	}
}