using System;

namespace Pcre
{
	public class PcreRegex
	{
		private readonly int[] _matches;
		private readonly IntPtr _regexPtr;
		
		public PcreRegex(string pattern, PcreOptions options)
		{
			_pattern = pattern;
			_matches = new int[2];
			unsafe
			{
				fixed(char* patternPtr = _pattern)
				{
					int errorffset = 0;
					IntPtr errPtr;
					_regexPtr = PcreApi.pcre16_compile(patternPtr, (int)options, out errPtr, ref errorffset, IntPtr.Zero);
				}
			}
		}
		
		public void Study(PcreStudyOptions options)
		{
			IntPtr errPtr;
			PcreApi.pcre16_study(_regexPtr, (int)options, out errPtr);
		}
		
		public PcreRegex Match(string text, int start)
		{
			unsafe
			{
				fixed(char* subject = text)
				fixed(int* vector = _matches)
				{
					_success = PcreApi.pcre16_exec(
						_regexPtr, IntPtr.Zero, subject, text.Length, start, 0, vector, _matches.Length
					) == 1;
				}
			}
			return this;
		}
		
		public PcreRegex Match(char[] text, int start, int length)
		{
			unsafe
			{
				fixed(char* subject = text)
				fixed(int* vector = _matches)
				{
					_success = PcreApi.pcre16_exec(
						_regexPtr, IntPtr.Zero, subject, start + length, start, 0, vector, _matches.Length
					) == 1;
				}
			}
			return this;
		}
		
		private readonly string _pattern;
		public string Pattern { get { return _pattern; } }
		
		private bool _success;
		public bool Success { get { return _success; } }
		
		public int MatchIndex { get { return _matches[0]; } }
		public int MatchLength { get { return _matches[1] - _matches[0]; } }
	}
}