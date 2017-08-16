using TinyJSON;
using MulticaretEditor;

public abstract class TextNodeParser
{
	public readonly string name;
	
	public TextNodeParser(string name)
	{
		this.name = name;
	}
	
	public abstract Node Parse(LineArray lines);
}