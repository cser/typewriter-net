using System;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;

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

		virtual public string Text { get { return ""; } }
		virtual public string SetText(string value)
		{
			return null;
		}

		virtual public void GetHelpText(TextTable table)
		{
			table.Add(name);
		}

		virtual public void Reset()
		{
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

		override public string Text { get { return value + ""; } }

		override public string SetText(string value)
		{
			float temp;
			if (float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out temp))
			{
				Value = temp;
				return null;
			}
			return "Can't parse \"" + value + "\"";
		}

		override public void GetHelpText(TextTable table)
		{
			table.Add(name).Add("float").Add(defaultValue + "").Add("min = " + min + ", max = " + max);
		}

		override public void Reset()
		{
			value = defaultValue;
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

		override public string Text { get { return value + ""; } }

		override public string SetText(string value)
		{
			int temp;
			if (int.TryParse(value, out temp))
			{
				Value = temp;
				return null;
			}
			return "Can't parse \"" + value + "\"";
		}

		override public void GetHelpText(TextTable table)
		{
			table.Add(name).Add("int").Add(defaultValue + "").Add("min = " + min + ", max = " + max);
		}

		override public void Reset()
		{
			value = defaultValue;
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

		override public string Text { get { return convertEscape ? value.Replace("\r", "\\r").Replace("\n", "\\n") : value; } }

		override public string SetText(string value)
		{
			Value = convertEscape && value != null ? value.Replace("\\r", "\r").Replace("\\n", "\n") : value + "";
			return null;
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

		override public void Reset()
		{
			value = defaultValue;
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

		override public string Text { get { return value ? "true" : "false"; } }

		override public string SetText(string value)
		{
			Value = value != null && (value == "1" || value.ToLowerInvariant() == "true");
			return null;
		}

		override public void GetHelpText(TextTable table)
		{
			table.Add(name).Add("bool").Add(defaultValue + "");
		}

		override public void Reset()
		{
			value = defaultValue;
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

		override public string Text { get { return StringOf(value); } }

		override public string SetText(string value)
		{
			if (!IsFamilyInstalled(value))
			{
				Value = defaultValue;
				return "Font \"" + value + "\" is not installed";
			}
			Value = new FontFamily(value);
			return null;
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

		override public void Reset()
		{
			value = defaultValue;
		}
	}
}
