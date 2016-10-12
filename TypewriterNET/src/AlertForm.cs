using System;
using System.Windows.Forms;
using MulticaretEditor;

public partial class AlertForm : Form
{
	public event Setter Canceled;
	
	public AlertForm()
	{
		Width = 200;
		Height = 100;
		ControlBox = false;
		FormBorderStyle = FormBorderStyle.FixedSingle;
		Button button = new Button();
		button.Text = "Stop search";
		button.Dock = DockStyle.Fill;
		button.Click += OnCancelClick;
		Controls.Add(button);
	}

	private void OnCancelClick(object sender, EventArgs e)
	{
		if (Canceled != null)
			Canceled();
	}
}