using System;

public class Properties
{
	public class Property
	{
		public readonly string name;

		public Property(string name)
		{
			this.name = name;
		}

		virtual public Float AsFloat { get { return null; } }
		virtual public Int AsInt { get { return null; } }
		virtual public String AsString { get { return null; } }

		virtual public string Text
		{
			get { return null; }
			set { ; }
		}
	}

	public class Float : Property
	{
		public Float(string name, float value) : base(name)
		{
			this.value = value;
		}

		override public Float AsFloat { get { return this; } }

		private float min = float.MinValue;
		private float max = float.MaxValue;

		public Float SetMinMax(float min, float max)
		{
			this.min = min;
			this.max = max;
			return this;
		}

		private float value;
		public float Value
		{
			get { return value; }
			set { this.value = Math.Max(min, Math.Min(max, value)); }
		}

		override public string Text
		{
			get { return value + ""; }
			set { this.value = float.Parse(value); }
		}
	}

	public class Int : Property
	{
		public Int(string name, int value) : base(name)
		{
			this.value = value;
		}

		override public Int AsInt { get { return this; } }

		private int min = int.MinValue;
		private int max = int.MaxValue;

		public Int SetMinMax(int min, int max)
		{
			this.min = min;
			this.max = max;
			return this;
		}

		private int value;
		public int Value
		{
			get { return value; }
			set { this.value = Math.Max(min, Math.Min(max, value)); }
		}

		override public string Text
		{
			get { return value + ""; }
			set { this.value = int.Parse(value); }
		}
	}

	public class String : Property
	{
		public String(string name, string value) : base(name)
		{
			this.value = value;
		}

		override public String AsString { get { return this; } }

		private string value;
		public string Value
		{
			get { return value; }
			set { this.value = value; }
		}

		override public string Text
		{
			get { return value; }
			set { this.value = value; }
		}
	}
}
