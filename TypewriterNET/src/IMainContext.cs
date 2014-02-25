using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;

namespace TypewriterNET
{
	public interface IMainContext
	{
		void SetMenuItems(KeyMapNode node);

		MulticaretTextBox TextBox { get; }
		
		XmlDocument LoadXml(string file, StringBuilder errors);

		XmlDocument LoadXmlIgnoreMissing(string file, StringBuilder errors);

		void ShowEditorConsole();
	}
}
