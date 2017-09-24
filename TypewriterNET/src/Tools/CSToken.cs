using MulticaretEditor;

public struct CSToken
{
	public string text;
	public char c;
	public Place place;
	
	public override string ToString()
	{
		return text != null ? "<<" + text + ">>" : "'" + c + "'";
	}
	
	public bool IsIdent { get { return text != null; } }
}