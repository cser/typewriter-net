namespace MulticaretEditor
{
	public static class MathHelper
	{
		public static int Clamp(int value, int min, int max)
		{
			if (value > max)
				value = max;
			if (value < min)
				value = min;
			return value;
		}
	}
}
