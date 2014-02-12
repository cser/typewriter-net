using System;
using System.Windows.Forms;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;

namespace TypewriterNET
{
	public class SearchPanel : TableLayoutPanel
	{
		private MainContext context;
		private MulticaretTextBox mainTextBox;
		private MulticaretTextBox textBox;
		
		public SearchPanel(MainContext context)
		{
			this.context = context;
			this.mainTextBox = context.textBox;
			int charHeight = mainTextBox.CharHeight;
			
			SuspendLayout();
			Margin = new Padding(0);
			Dock = DockStyle.Bottom;
    		Height = charHeight + 4;
    		TabStop = false;
    		RowStyles.Add(new RowStyle(SizeType.AutoSize));
    		BackColor = mainTextBox.Scheme.lineNumberBgColor;
	    		
    		textBox = new MulticaretTextBox();
    		textBox.WordWrap = true;
    		textBox.Height = textBox.CharHeight;
    		textBox.Dock = DockStyle.Fill;
    		textBox.Controller = new Controller(new LineArray());
    		textBox.ShowLineNumbers = false;
    		textBox.HighlightCurrentLine = false;
    		textBox.Scheme = mainTextBox.Scheme;
    		textBox.Text = mainTextBox.Controller.Lines.LastSelection.Empty ?
    			context.searchOldText :
    			mainTextBox.Controller.Lines.GetText(mainTextBox.Controller.Lines.LastSelection.Left, mainTextBox.Controller.Lines.LastSelection.Count);
    		textBox.Controller.ClearMinorSelections();
    		textBox.Controller.Lines.LastSelection.anchor = 0;
    		textBox.Controller.Lines.LastSelection.caret = textBox.Controller.Lines.charsCount;
    		Controls.Add(textBox);
    		
    		textBox.KeyMap.AddItem(new KeyItem(Keys.Enter, null, new KeyAction("&View\\Search next", DoSearchNext, null, false)), true);
    		textBox.parentKeyMaps.Add(context.keyMap);
    		textBox.parentKeyMaps.Add(context.doNothingKeyMap);
    		
    		ResumeLayout();
		}
	    
	    public new void Focus()
	    {
	    	textBox.Focus();
	    }
	    
	    public void DoOnClose()
	    {
	    	context.searchOldText = textBox.Text;
	    }
	    
	    private bool DoSearchNext(Controller controller)
	    {
	    	string text = controller.Lines.GetText();
	    	int index = mainTextBox.Controller.Lines.IndexOf(text, mainTextBox.Controller.Lines.LastSelection.Right);
	    	if (index == -1)
	    		index = mainTextBox.Controller.Lines.IndexOf(text, 0);
	    	if (index != -1)
	    	{
	    		mainTextBox.Controller.PutCursor(mainTextBox.Controller.Lines.PlaceOf(index), false);
	    		mainTextBox.Controller.PutCursor(mainTextBox.Controller.Lines.PlaceOf(index + text.Length), true);
	    		mainTextBox.MoveToCaret();
	    	}
	    	return true;
	    }
	}
}
