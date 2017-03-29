using System;

namespace Pcre
{
	[Flags]
	public enum PcreStudyOptions : int
	{
		JIT_COMPILE = 0x0001,
		JIT_PARTIAL_SOFT_COMPILE = 0x0002,
		JIT_PARTIAL_HARD_COMPILE = 0x0004,
		EXTRA_NEEDED = 0x0008
	}
}