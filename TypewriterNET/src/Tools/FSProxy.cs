using System.IO;

public class FSProxy : IFSProxy
{
	public bool File_Exists(string path)
	{
		return File.Exists(path);
	}
	
	public void File_Copy(string source, string target)
	{
		File.Copy(source, target);
	}
	
	public void File_Move(string source, string target)
	{
		File.Move(source, target);
	}
	
	public bool Directory_Exists(string path)
	{
		return Directory.Exists(path);
	}
	
	public void Directory_Move(string source, string target)
	{
		Directory.Move(source, target);
	}
	
	public void Directory_CreateDirectory(string path)
	{
		Directory.CreateDirectory(path);
	}
	
	public string[] Directory_GetFiles(string path)
	{
		return Directory.GetFiles(path);
	}
	
	public string[] Directory_GetDirectories(string path)
	{
		return Directory.GetDirectories(path);
	}
	
	public void File_Delete(string path)
	{
		File.Delete(path);
	}
	
	public void Directory_DeleteRecursive(string path)
	{
		Directory.Delete(path, true);
	}
	
	public string GetFileName(string path)
	{
		return Path.GetFileName(path);
	}
	
	public string GetFileNameWithoutExtension(string path)
	{
		return Path.GetFileNameWithoutExtension(path);
	}
	
	public string GetExtension(string path)
	{
		return Path.GetExtension(path);
	}
	
	public string Combine(string path1, string path2)
	{
		return Path.Combine(path1, path2);
	}
}