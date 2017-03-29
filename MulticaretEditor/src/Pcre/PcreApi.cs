using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace Pcre
{
	public static class PcreApi
	{
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libpcre16_x86.dll")]
		public static extern unsafe IntPtr pcre16_compile(
			char* pattern, int options, out IntPtr errptr, ref int erroffset, IntPtr tableptr);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libpcre16_x86.dll")]
		public static extern unsafe int pcre16_exec(
			IntPtr pcre, IntPtr extra, char* subject, int length, int startoffset, int options, int* ovector, int ovecsize);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libpcre16_x86.dll")]
		public static extern unsafe int pcre16_study(IntPtr pcre, int options, out IntPtr errptr);
	}
}