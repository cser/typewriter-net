using System;

namespace MulticaretEditor
{
	public delegate void Setter();
	public delegate void Setter<T>(T value);
	public delegate void Setter<T0, T1>(T0 value0, T1 value1);
}
