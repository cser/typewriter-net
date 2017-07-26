namespace MulticaretEditor
{
	public class Pattern
	{
		public string text;
		public bool regex;
		public bool ignoreCase;
		
		public Pattern(string text, bool regex, bool ignoreCase)
		{
			this.text = text;
			this.regex = regex;
			this.ignoreCase = ignoreCase;
		}
		
		public override string ToString()
		{
			return (regex ? "/" + text + "/" : "\"" + text + "\"") + (ignoreCase ? "i" : "");
		}
	}
}