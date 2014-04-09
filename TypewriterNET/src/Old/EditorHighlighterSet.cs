using System;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using MulticaretEditor;
using MulticaretEditor.Highlighting;

namespace TypewriterNET
{
	public class EditorHighlighterSet : HighlighterSet
	{
		private IMainContext context;
		private SyntaxFilesScanner config;
		
		public EditorHighlighterSet(IMainContext context)
		{
			this.context = context;
		}
		
		public void UpdateParameters(SyntaxFilesScanner config)
		{
			Reset();
			this.config = config;
		}
		
		override protected Raw NewRaw(string type)
		{
			StringBuilder errors = new StringBuilder();
			string file = config.GetSyntaxFileByName(type);
			XmlDocument xml = context.LoadXml(file, errors);
			if (errors.Length > 0)
			{
				EditorConsole.Instance.WriteLine("-- Syntaxes errors:", Ds.Comment);
				EditorConsole.Instance.Write(errors.ToString());
				context.ShowEditorConsole();
			}
			
			Raw raw = Raw.Parse(xml);
			Raw.PrefixContexts(raw, type);
			Raw.InlineIncludeRules(raw, this);
			return raw;
		}
	}
}
