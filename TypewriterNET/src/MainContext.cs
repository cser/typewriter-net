using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;

namespace TypewriterNET
{
	public class MainContext
	{
		public MainContext()
		{
		}
		
		public ConsoleListController consoleListController;
		public MulticaretTextBox textBox;
		
		public KeyMap keyMap;
		public KeyMap doNothingKeyMap;
		
		public XmlDocument LoadXml(string file, StringBuilder errors)
	    {
	    	if (!File.Exists(file))
	    	{
	    		errors.AppendLine("Missing file: " + file);
	    		return null;
	    	}
			return PrivateLoadXml(file, errors);
	    }

		public XmlDocument LoadXmlIgnoreMissing(string file, StringBuilder errors)
		{
			if (!File.Exists(file))
	    		return null;
			return PrivateLoadXml(file, errors);
		}

		private XmlDocument PrivateLoadXml(string file, StringBuilder errors)
		{
	    	try
	    	{
	    		XmlDocument xml = new XmlDocument();
	    		xml.Load(file);
	    		return xml;
	    	}
	    	catch (Exception e)
	    	{
	    		errors.AppendLine("Error: " + e.Message);
	    		return null;
	    	}
		}
		
		public void ShowEditorConsole()
	    {
	    	consoleListController.Show(SetFocusToTextBox);
	    	consoleListController.AddConsole(EditorConsole.Instance);
	    	consoleListController.SelectedConsole = EditorConsole.Instance;
	    }
		
		public void SetFocusToTextBox()
	    {
	    	textBox.Focus();
	    }
	}
}
