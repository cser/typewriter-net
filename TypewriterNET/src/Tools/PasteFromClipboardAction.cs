using System;
using System.Collections.Generic;

public class PasteFromClipboardAction
{
	public class Mode
	{
		public bool Cut
		{
			get
			{
				return this == PasteFromClipboardAction.Cut || this == PasteFromClipboardAction.CutOverride;
			}
		}
		
		public bool Overwrite
		{
			get
			{
				return this == PasteFromClipboardAction.CopyOverride || this == PasteFromClipboardAction.CutOverride;
			}
		}
	}
	
	public class FileMoveInfo
	{
		public string prevNorm;
		public string next;
		public bool hasPostfixed;
	}
	
	public readonly PathSet NewFullPaths = new PathSet();
	public readonly List<string> Errors = new List<string>();
	public readonly List<string> Overwrites = new List<string>();
	
	public static readonly Mode Copy = new Mode();
	public static readonly Mode Cut = new Mode();
	public static readonly Mode CutOverride = new Mode();
	public static readonly Mode CopyOverride = new Mode();
	
	private readonly string renamePostfixed;
	private readonly bool pastePostfixedAfterCopy;
	private readonly IFSProxy fs;
	
	public PasteFromClipboardAction(IFSProxy fs, string renamePostfixed, bool pastePostfixedAfterCopy)
	{
		this.fs = fs;
		this.renamePostfixed = renamePostfixed;
		this.pastePostfixedAfterCopy = pastePostfixedAfterCopy;
	}
	
	public void Execute(string[] files, string targetDir, Mode mode)
	{
		NewFullPaths.Clear();
		List<FileMoveInfo> moves = new List<FileMoveInfo>();
		bool renameMode = false;
		{
			Dictionary<string, FileMoveInfo> prevNormPaths = new Dictionary<string, FileMoveInfo>();
			for (int i = 0; i < files.Length; i++)
			{
				string file = files[i];
				FileMoveInfo move = new FileMoveInfo();
				move.prevNorm = PathSet.GetNorm(file);
				move.next = targetDir + fs.Separator + fs.GetFileName(file);
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
			if (mode.Cut)
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
		Overwrites.Clear();
		if (!renameMode && !mode.Overwrite)
		{
			foreach (FileMoveInfo move in moves)
			{
				bool isDir = fs.Directory_Exists(move.prevNorm);
				if (!isDir && !fs.File_Exists(move.prevNorm))
				{
					continue;
				}
				string next = move.next;
				string nextPostfixed = move.hasPostfixed ? move.next + renamePostfixed : null;
				if (fs.File_Exists(next) || fs.Directory_Exists(next))
				{
					Overwrites.Add(next);
				}
			}
		}
		if (Overwrites.Count > 0)
		{
			return;
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
				CopyWithRename(move, isDir, ref next, ref nextPostfixed);
			}
			else if (mode.Overwrite)
			{
				CopyOrMoveOverwrite(move, isDir, ref next, ref nextPostfixed, mode.Cut);
			}
			else
			{
				CopyOrMove(move, isDir, ref next, ref nextPostfixed, mode.Cut);
			}
			NewFullPaths.Add(next);
		}
	}
	
	private void CopyWithRename(FileMoveInfo info, bool isDir, ref string next, ref string nextPostfixed)
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
				CopyDirectoryRecursive(info.prevNorm, next, false);
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
	
	private void CopyOrMove(FileMoveInfo info, bool isDir, ref string next, ref string nextPostfixed, bool move)
	{
		try
		{
			if (isDir)
			{
				if (move)
				{
					fs.Directory_Move(info.prevNorm, next);
				}
				else
				{
					CopyDirectoryRecursive(info.prevNorm, next, false);
				}
			}
			else
			{
				if (move)
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
				if (move)
				{
					fs.File_Move(info.prevNorm + renamePostfixed, nextPostfixed);
				}
				else
				{
					if (pastePostfixedAfterCopy)
					{
						fs.File_Copy(info.prevNorm + renamePostfixed, nextPostfixed);
					}
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
	
	private void CopyOrMoveOverwrite(FileMoveInfo info, bool isDir, ref string next, ref string nextPostfixed, bool move)
	{
		try
		{
			if (isDir)
			{
				if (move)
				{
					fs.Directory_Move(info.prevNorm, next);
				}
				else
				{
					CopyDirectoryRecursive(info.prevNorm, next, true);
				}
			}
			else
			{
				if (fs.File_Exists(next))
				{
					fs.File_Delete(next);
				}
				else if (fs.Directory_Exists(next))
				{
					fs.Directory_DeleteRecursive(next);
				}
				if (move)
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
				if (move)
				{
					if (fs.File_Exists(nextPostfixed))
					{
						fs.File_Delete(nextPostfixed);
					}
					else if (fs.Directory_Exists(nextPostfixed))
					{
						fs.Directory_DeleteRecursive(nextPostfixed);
					}
					fs.File_Move(info.prevNorm + renamePostfixed, nextPostfixed);
				}
				else
				{
					if (pastePostfixedAfterCopy)
					{
						if (fs.File_Exists(nextPostfixed))
						{
							fs.File_Delete(nextPostfixed);
						}
						else if (fs.Directory_Exists(nextPostfixed))
						{
							fs.Directory_DeleteRecursive(nextPostfixed);
						}
						fs.File_Copy(info.prevNorm + renamePostfixed, nextPostfixed);
					}
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
	
	private void CopyDirectoryRecursive(string prev, string targetFolder, bool overwrite)
	{
		string[] dirs = fs.Directory_GetDirectories(prev);
		string[] files = fs.Directory_GetFiles(prev);
		if (!fs.Directory_Exists(targetFolder))
		{
			if (overwrite && fs.File_Exists(targetFolder))
			{
				fs.File_Delete(targetFolder);
			}
			fs.Directory_CreateDirectory(targetFolder);
		}
		foreach (string dir in dirs)
		{
			CopyDirectoryRecursive(dir, targetFolder + fs.Separator + fs.GetFileName(dir), overwrite);
		}
		foreach (string file in files)
		{
			if (!pastePostfixedAfterCopy && !string.IsNullOrEmpty(renamePostfixed) &&
				file.ToLowerInvariant().EndsWith(renamePostfixed))
			{
				continue;
			}
			string target = targetFolder + fs.Separator + fs.GetFileName(file);
			if (overwrite && fs.File_Exists(target))
			{
				fs.File_Delete(target);
			}	
			fs.File_Copy(file, target);
		}
	}
}