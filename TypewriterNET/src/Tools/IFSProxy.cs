public interface IFSProxy
{
	bool File_Exists(string path);
		
	void File_Copy(string source, string target);
		
	void File_Move(string source, string target);
		
	bool Directory_Exists(string path);
		
	void Directory_Move(string source, string target);
		
	void Directory_CreateDirectory(string path);
		
	string[] Directory_GetFiles(string path);
		
	string[] Directory_GetDirectories(string path);
	
	void File_Delete(string path);
	
	void Directory_DeleteRecursive(string path);
		
	string GetFileName(string path);
		
	string GetFileNameWithoutExtension(string path);
		
	string GetExtension(string path);
		
	string Combine(string path1, string path2);
	
	char Separator { get; }
}