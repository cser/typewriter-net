using System;
using System.Collections.Generic;

namespace MulticaretEditor.Highlighting
{
	public class HighlighterSet
	{
		public HighlighterSet()
		{
		}
		
		private Dictionary<string, Highlighter> highlighterBy = new Dictionary<string, Highlighter>();
		private Dictionary<string, Raw> rawBy = new Dictionary<string, Raw>();
		
		public void Reset()
		{
			highlighterBy.Clear();
			rawBy.Clear();
		}
		
		public Highlighter GetHighlighter(string type)
		{
			type = type.ToLowerInvariant();
			Highlighter highlighter;
			highlighterBy.TryGetValue(type, out highlighter);
			if (highlighter == null)
			{
				Raw raw = GetRaw(type);
				if (raw != null)
				{
					highlighter = new Highlighter(raw);
					highlighter.type = type;
					highlighterBy[type] = highlighter;
				}
			}
			return highlighter;
		}
		
		public Raw GetRaw(string type)
		{
			type = type.ToLowerInvariant();
			Raw raw;
			rawBy.TryGetValue(type, out raw);
			if (raw == null)
			{
				raw = NewRaw(type);
				if (raw != null)
					rawBy[type] = raw;
			}
			return raw;
		}
		
		virtual protected Raw NewRaw(string type)
		{
			return null;
		}
	}
}
