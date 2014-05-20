using System;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using MulticaretEditor;
using MulticaretEditor.Highlighting;

namespace TypewriterNET
{
	public class ConcreteHighlighterSet : HighlighterSet
	{
		private XmlLoader xmlLoader;
		private Log log;
		private SyntaxFilesScanner scanner;
		
		public ConcreteHighlighterSet(XmlLoader xmlLoader, Log log)
		{
			this.xmlLoader = xmlLoader;
			this.log = log;
		}
		
		public void UpdateParameters(SyntaxFilesScanner scanner)
		{
			Reset();
			this.scanner = scanner;
		}
		
		override protected Raw NewRaw(string type)
		{
			string file = scanner.GetSyntaxFileByName(type);
			XmlDocument xml = xmlLoader.Load(file, false);
			
			Raw raw = Raw.Parse(xml);
			Raw.PrefixContexts(raw, type);
			Raw.InlineIncludeRules(raw, this);
			return raw;
		}
	}
}
