using System;
using System.Windows.Forms;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;
using TypewriterNET.Frames;

namespace TypewriterNET.Frames
{
	public class SearchPanel : TableLayoutPanel
	{
		private SearchFrame frame;
		private MulticaretTextBox textBox;
		
		public SearchPanel(SearchFrame frame)
		{
			this.frame = frame;
			int charHeight = frame.TextBox.CharHeight;
			
			SuspendLayout();
			Margin = new Padding(0);
			Dock = DockStyle.Bottom;
    		Height = charHeight + 4;
    		TabStop = false;
    		RowStyles.Add(new RowStyle(SizeType.AutoSize));
    		BackColor = frame.TextBox.Scheme.lineNumberBgColor;
    		textBox = new MulticaretTextBox();
    		textBox.WordWrap = true;
    		textBox.Height = textBox.CharHeight;
    		textBox.Dock = DockStyle.Fill;
    		textBox.Controller = new Controller(new LineArray());
    		textBox.ShowLineNumbers = false;
    		textBox.HighlightCurrentLine = false;
    		textBox.Scheme = frame.TextBox.Scheme;
    		textBox.Text = frame.TextBox.Controller.Lines.LastSelection.Empty ?
    			frame.searchOldText :
    			frame.TextBox.Controller.Lines.GetText(frame.TextBox.Controller.Lines.LastSelection.Left, frame.TextBox.Controller.Lines.LastSelection.Count);
    		textBox.Controller.ClearMinorSelections();
    		textBox.Controller.Lines.LastSelection.anchor = 0;
    		textBox.Controller.Lines.LastSelection.caret = textBox.Controller.Lines.charsCount;
    		Controls.Add(textBox);
    		
    		textBox.KeyMap.main.AddItem(new KeyItem(Keys.Enter, null, new KeyAction("&View\\Search next", DoSearchNext, null, false)), true);
    		textBox.KeyMap.AddAfter(frame.TextBox.KeyMap);

			textBox.GotFocus += OnGotFocus;
    		
    		ResumeLayout();
		}
	    
	    public new void Focus()
	    {
	    	textBox.Focus();
	    }
	    
	    public void DoOnClose()
	    {
	    	frame.searchOldText = textBox.Text;
	    }
	    
	    private bool DoSearchNext(Controller controller)
	    {
	    	string text = controller.Lines.GetText();
	    	int index = frame.TextBox.Controller.Lines.IndexOf(text, frame.TextBox.Controller.Lines.LastSelection.Right);
	    	if (index == -1)
	    		index = frame.TextBox.Controller.Lines.IndexOf(text, 0);
	    	if (index != -1)
	    	{
	    		frame.TextBox.Controller.PutCursor(frame.TextBox.Controller.Lines.PlaceOf(index), false);
	    		frame.TextBox.Controller.PutCursor(frame.TextBox.Controller.Lines.PlaceOf(index + text.Length), true);
	    		frame.TextBox.MoveToCaret();
	    	}
	    	return true;
	    }

		private void OnGotFocus(object sender, EventArgs e)
		{
			frame.Context.SetMenuItems(textBox.KeyMap);
		}
	}
}
