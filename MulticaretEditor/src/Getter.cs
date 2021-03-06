﻿using System;

namespace MulticaretEditor
{
	public delegate TResult Getter<TResult>();
	public delegate TResult Getter<T, TResult>(T value);
	public delegate TResult Getter<T0, T1, TResult>(T0 value0, T1 value1);
	public delegate TResult Getter<T0, T1, T2, TResult>(T0 value0, T1 value1, T2 value2);
}
