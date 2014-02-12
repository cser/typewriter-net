using System;
using MulticaretEditor;
using MulticaretEditor.Highlighting;

namespace TypewriterNET
{
	public class EditorConsole : ConsoleInfo
	{
		private static EditorConsole instance;
		public static EditorConsole Instance
		{
			get
			{
				if (instance == null)
					instance = new EditorConsole();
				return instance;
			}
		}
		
		public EditorConsole() : base("~")
		{
			controller = new Controller(new LineArray());
			controller.isReadonly = true;
		}
		
		public void Write(string text)
		{
			Write(text, null);
		}
		
		public void Write(string text, Ds ds)
		{
			int index = controller.Lines.charsCount;
			controller.ClearMinorSelections();
	    	controller.PutCursor(controller.Lines.PlaceOf(controller.Lines.charsCount), false);
			controller.Lines.InsertText(index, text);
			if (ds != null)
				controller.Lines.SetRangeStyle(index, text.Length, ds.index);
		}
		
		public void WriteLine(string text)
		{
			WriteLine(text, null);
		}
		
		public void WriteLine(string text, Ds ds)
		{
			Write(text + "\n", ds);
		}
	}
}
