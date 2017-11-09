public class PasteMode
{
	public static readonly PasteMode Copy = new PasteMode();
	public static readonly PasteMode Cut = new PasteMode();
	public static readonly PasteMode CutOverwrite = new PasteMode();
	public static readonly PasteMode CopyOverwrite = new PasteMode();
	
	public bool IsCut { get { return this == PasteMode.Cut || this == PasteMode.CutOverwrite; } }
	public bool IsOverwrite { get { return this == PasteMode.CopyOverwrite || this == PasteMode.CutOverwrite; } }
}