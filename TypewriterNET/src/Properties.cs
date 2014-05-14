using System;
using System.Drawing;
using System.Drawing.Text;

public class Properties
{
	public static void AddHeadTo(TextTable table)
	{
		table.Add("Name").Add("Type").Add("Default value");
	}

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
		virtual public Bool AsBool { get { return null; } }
		virtual public Font AsFont { get { return null; } }

		virtual public string Text
		{
			get { return null; }
			set { ; }
		}

		virtual public void GetHelpText(TextTable table)
		{
			table.Add(name);
		}
	}

	public class Float : Property
	{
		private float defaultValue;

		public Float(string name, float value) : base(name)
		{
			defaultValue = value;
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

		override public void GetHelpText(TextTable table)
		{
			table.Add(name).Add("float").Add(defaultValue + "").Add("min = " + min + ", max = " + max);
		}
	}

	public class Int : Property
	{
		private int defaultValue;

		public Int(string name, int value) : base(name)
		{
			defaultValue = value;
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

		override public void GetHelpText(TextTable table)
		{
			table.Add(name).Add("int").Add(defaultValue + "").Add("min = " + min + ", max = " + max);
		}
	}

	public class String : Property
	{
		private string defaultValue;
		private bool convertEscape;

		public String(string name, string value, bool convertEscape) : base(name)
		{
			defaultValue = value;
			this.value = value ?? "";
			this.convertEscape = convertEscape;
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
			get { return convertEscape ? value.Replace("\r", "\\r").Replace("\n", "\\n") : value; }
			set { this.value = convertEscape && value != null ? value.Replace("\\r", "\r").Replace("\\n", "\n") : value + ""; }
		}

		override public void GetHelpText(TextTable table)
		{
			table.Add(name).Add("string").Add(ReplaceLineBreaks(defaultValue));
		}

		private string ReplaceLineBreaks(string value)
		{
			value = value.Replace("\n", "\\n");
			value = value.Replace("\r", "\\r");
			return value;
		}
	}

	public class Bool : Property
	{
		private bool defaultValue;

		public Bool(string name, bool value) : base(name)
		{
			defaultValue = value;
			this.value = value;
		}

		override public Bool AsBool { get { return this; } }

		private bool value;
		public bool Value
		{
			get { return value; }
			set { this.value = value; }
		}

		override public string Text
		{
			get { return value ? "true" : "false"; }
			set { this.value = value != null && (value == "1" || value.ToLowerInvariant() == "true"); }
		}

		override public void GetHelpText(TextTable table)
		{
			table.Add(name).Add("bool").Add(defaultValue + "");
		}
	}

	public class Font : Property
	{
		private FontFamily defaultValue;

		public Font(string name, FontFamily value) : base(name)
		{
			defaultValue = value;
			this.value = value;
		}

		override public Font AsFont { get { return this; } }

		private FontFamily value;
		public FontFamily Value
		{
			get { return value; }
			set { this.value = value; }
		}

		override public string Text
		{
			get { return StringOf(value); }
			set
			{
				if (IsFamilyInstalled(value))
					this.value = new FontFamily(value);
				else
					this.value = defaultValue;
			}
		}

		override public void GetHelpText(TextTable table)
		{
			table.Add(name).Add("font").Add(StringOf(defaultValue)).Add(StringOf(value));
		}

		private string StringOf(FontFamily fontFamily)
		{
			return "\"" + (fontFamily != null ? fontFamily.Name : "") + "\"";
		}

		private bool IsFamilyInstalled(string fontFamily)
		{
			InstalledFontCollection installed = new InstalledFontCollection();
			foreach (FontFamily familyI in installed.Families)
			{
				if (familyI.Name == fontFamily)
					return true;
			}
			return false;
		}
	}
}
