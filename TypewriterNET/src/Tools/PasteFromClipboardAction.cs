using System;
using System.Collections.Generic;

public class PasteFromClipboardAction
{
	public readonly PathSet NewFullPaths = new PathSet();
	public readonly List<string> Errors = new List<string>();
	
	public class FileMoveInfo
	{
		public string prevNorm;
		public string next;
		public bool hasPostfixed;
	}
	
	private readonly string renamePostfixed;
	private readonly bool pastePostfixedAfterCopy;
	private readonly IFSProxy fs;
	
	public PasteFromClipboardAction(IFSProxy fs, string renamePostfixed, bool pastePostfixedAfterCopy)
	{
		this.fs = fs;
		this.renamePostfixed = renamePostfixed;
		this.pastePostfixedAfterCopy = pastePostfixedAfterCopy;
	}
	
	public void Execute(string[] files, string targetDir, bool cutMode)
	{
		List<FileMoveInfo> moves = new List<FileMoveInfo>();
		bool renameMode = false;
		{
			Dictionary<string, FileMoveInfo> prevNormPaths = new Dictionary<string, FileMoveInfo>();
			for (int i = 0; i < files.Length; i++)
			{
				string file = files[i];
				FileMoveInfo move = new FileMoveInfo();
				move.prevNorm = PathSet.GetNorm(file);
				move.next = fs.Combine(targetDir, fs.GetFileName(file));
				moves.Add(move);
				prevNormPaths[move.prevNorm] = move;
			}
			if (!string.IsNullOrEmpty(renamePostfixed))
			{
				string lowerPostfix = renamePostfixed.ToLowerInvariant();
				for (int i = moves.Count; i-- > 0;)
				{
					FileMoveInfo move = moves[i];
					if (move.prevNorm.EndsWith(lowerPostfix))
					{
						string prev = move.prevNorm.Substring(0, move.prevNorm.Length - lowerPostfix.Length);
						if (!prev.EndsWith(lowerPostfix) && prevNormPaths.ContainsKey(prev))
						{
							prevNormPaths[PathSet.GetNorm(prev)].hasPostfixed = true;
							moves.RemoveAt(i);
						}
					}
				}
			}
			if (cutMode)
			{
				for (int i = moves.Count; i-- > 0;)
				{
					FileMoveInfo move = moves[i];
					if (move.prevNorm == PathSet.GetNorm(move.next))
					{
						moves.RemoveAt(i);
					}
				}
			}
			else
			{
				foreach (FileMoveInfo move in moves)
				{
					if (move.prevNorm == PathSet.GetNorm(move.next))
					{
						renameMode = true;
					}
				}
			}
		}
		foreach (FileMoveInfo move in moves)
		{
			bool isDir = fs.Directory_Exists(move.prevNorm);
			if (!isDir && !fs.File_Exists(move.prevNorm))
			{
				continue;
			}
			string next = move.next;
			string nextPostfixed = move.hasPostfixed ? move.next + renamePostfixed : null;
			if (renameMode)
			{
				ProcessMove_RenameMode(move, isDir, ref next, ref nextPostfixed);
			}
			else
			{
				ProcessMove(move, isDir, ref next, ref nextPostfixed, cutMode);
			}
			NewFullPaths.Add(next);
		}
	}
	
	private void ProcessMove_RenameMode(FileMoveInfo info, bool isDir, ref string next, ref string nextPostfixed)
	{
		{
			string nextExtension;
			string nextBase;
			if (fs.GetFileNameWithoutExtension(info.next) == "")
			{
				nextExtension = "";
				nextBase = info.next;
			}
			else
			{
				nextExtension = fs.GetExtension(info.next);
				nextBase = info.next.Substring(0, info.next.Length - nextExtension.Length);
			}
			int index = 1;
			while (fs.Directory_Exists(next) || fs.File_Exists(next) ||
				nextPostfixed != null && (fs.Directory_Exists(nextPostfixed) || fs.File_Exists(nextPostfixed)))
			{
				string suffix = index == 1 ? "-copy" : "-copy" + index;
				next = nextBase + suffix + nextExtension;
				if (nextPostfixed != null)
				{
					nextPostfixed = nextBase + suffix + nextExtension + renamePostfixed;
				}
				++index;
			}
		}
		try
		{
			if (isDir)
			{
				CopyDirectoryRecursive(info.prevNorm, next);
			}
			else
			{
				fs.File_Copy(info.prevNorm, next);
			}
			if (nextPostfixed != null && pastePostfixedAfterCopy)
			{
				fs.File_Copy(info.prevNorm + renamePostfixed, nextPostfixed);
			}
		}
		catch (Exception e)
		{
			Errors.Add(e.Message + "\n" +
				"  " + info.prevNorm + " ->\n" +
				"  " + next + (nextPostfixed != null ? "(" + renamePostfixed + ")" : ""));
		}
	}
	
	private void ProcessMove(FileMoveInfo info, bool isDir, ref string next, ref string nextPostfixed, bool cutMode)
	{
		try
		{
			if (isDir)
			{
				if (cutMode)
				{
					fs.Directory_Move(info.prevNorm, next);
				}
				else
				{
					CopyDirectoryRecursive(info.prevNorm, next);
				}
			}
			else
			{
				if (cutMode)
				{
					fs.File_Move(info.prevNorm, next);
				}
				else
				{
					fs.File_Copy(info.prevNorm, next);
				}
			}
			if (nextPostfixed != null)
			{
				if (cutMode)
				{
					fs.File_Move(info.prevNorm + renamePostfixed, nextPostfixed);
				}
				else if (pastePostfixedAfterCopy)
				{
					fs.File_Copy(info.prevNorm + renamePostfixed, nextPostfixed);
				}
			}
		}
		catch (Exception e)
		{
			Errors.Add(e.Message + "\n" +
				"  " + info.prevNorm + " ->\n" +
				"  " + next + (nextPostfixed != null ? "(" + renamePostfixed + ")" : ""));
		}
	}
	
	private void CopyDirectoryRecursive(string prev, string targetFolder)
	{
		if (!fs.Directory_Exists(targetFolder))
		{
			fs.Directory_CreateDirectory(targetFolder);
		}
		foreach (string dir in fs.Directory_GetDirectories(prev))
		{
			CopyDirectoryRecursive(dir, fs.Combine(targetFolder, fs.GetFileName(dir)));
		}
		foreach (string file in fs.Directory_GetFiles(prev))
		{
			if (!pastePostfixedAfterCopy && !string.IsNullOrEmpty(renamePostfixed) &&
				file.ToLowerInvariant().EndsWith(renamePostfixed))
			{
				continue;
			}
			fs.File_Copy(file, fs.Combine(targetFolder, fs.GetFileName(file)));
		}
	}
}