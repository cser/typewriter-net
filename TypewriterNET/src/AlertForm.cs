using System;
using System.Windows.Forms;
using MulticaretEditor;

public class AlertForm : Form
{
	private Getter<bool> onCanceled;
	private MainForm mainForm;
	private Label label;
	private Button button;
	
	public AlertForm(MainForm mainForm, Getter<bool> onCanceled)
	{
		this.mainForm = mainForm;
		this.onCanceled = onCanceled;
		
		SuspendLayout();
		
		Width = 200;
		Height = 100;
		ControlBox = false;
		FormBorderStyle = FormBorderStyle.FixedSingle;
		
		label = new Label();
		label.Text = "Wait...";
		label.Dock = DockStyle.Fill;
		label.Visible = false;
		Controls.Add(label);
		
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
		label.Visible = true;
		if (onCanceled != null)
		{
			if (onCanceled())
				e.Cancel = false;
		}
	}

	private void OnCancelClick(object sender, EventArgs e)
	{
		Close();
	}
}