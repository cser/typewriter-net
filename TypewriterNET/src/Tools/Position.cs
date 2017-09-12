using MulticaretEditor;

public struct Position
{
	public readonly string fullPath;
	public readonly Place place;
	public readonly int length;

	public Position(string fullPath, Place place, int length)
	{
		this.fullPath = fullPath;
		this.place = place;
		this.length = length;
	}
}