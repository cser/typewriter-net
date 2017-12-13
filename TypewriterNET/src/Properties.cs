using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Globalization;
using MulticaretEditor;

public static class Properties
{
	public static void AddHeadTo(TextTable table)
	{
		table.Add("").Add("Name").Add("Type").Add("Default value").Add("Possible/current values");
	}
	
	public static string NameOfName(string name)
	{
		if (name != null)
		{
			int index = name.IndexOf(":");
			if (index != -1)
				return name.Substring(0, index);
		}
		return name;
	}
	
	public static string SubvalueOfName(string name)
	{
		if (name != null)
		{
			int index = name.IndexOf(":");
			if (index != -1)
				return name.Substring(index + 1);
		}
		return null;
	}
	
	[Flags]
	public enum Constraints
	{
		None = 0x00,
		NotForLocal = 0x01,
		Multiple = 0x02
	}

	public abstract class Property
	{
		public readonly string name;
		public readonly Constraints constraints;
		public bool initedByConfig;

		public Property(string name, Constraints constraints)
		{
			this.name = name;
			this.constraints = constraints;
		}
		
		abstract public string Type { get; }
		virtual public string TypeHelp { get { return null; } }
		
		public bool AllowTemp { get { return (constraints & Constraints.Multiple) == 0; } }

		virtual public string Text { get { return ""; } }
		virtual public string ShowedName { get { return name; } }
		virtual public string DefaultValue { get { return ""; } }
		
		virtual public string PossibleValues { get { return ""; } }

		virtual public string SetText(string value, string subvalue)
		{
			return null;
		}

		public void GetHelpText(TextTable table)
		{
			string temp = "";
			if (AllowTemp)
			{
				temp = initedByConfig ? "C" : "T";
			}
			table.Add(temp).Add(ShowedName).Add(Type).Add(DefaultValue).Add(PossibleValues);
		}
		
		public bool GetHelpTypeText(TextTable table)
		{
			if (TypeHelp != null)
			{
				table.Add("").Add("").Add("").Add("").Add(TypeHelp);
				return true;
			}
			return false;
		}

		virtual public void Reset()
		{
		}
		
		virtual public List<Variant> GetAutocompleteVariants()
		{
			return null;
		}
		
		virtual public SValue GetTemp()
		{
			return SValue.None;
		}
		
		virtual public void SetTemp(SValue sValue)
		{
		}
	}

	public class Float : Property
	{
		private float defaultValue;

		public Float(string name, float value) : base(name, Constraints.None)
		{
			defaultValue = value;
			this.value = value;
		}

		private float min = float.MinValue;
		private float max = float.MaxValue;

		public Float SetMinMax(float min, float max)
		{
			this.min = min;
			this.max = max;
			return this;
		}

		private int precision = -1;

		public Float SetPrecision(int precision)
		{
			this.precision = precision;
			return this;
		}

		private float value;
		public float Value
		{
			get { return value; }
			set { this.value = Math.Max(min, Math.Min(max, value)); }
		}

		public override string Text { get { return StringOf(value); } }

		public override string SetText(string value, string subvalue)
		{
			float temp;
			if (float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out temp))
			{
				Value = temp;
				return null;
			}
			return "Can't parse \"" + value + "\"";
		}
		
		public override string Type { get { return "float"; } }
		public override string DefaultValue { get { return StringOf(defaultValue); } }
		public override string PossibleValues { get { return "min: " + StringOf(min) + ", max: " + StringOf(max); } }

		public override void Reset()
		{
			value = defaultValue;
		}

		private string StringOf(float value)
		{
			string text = value.ToString(CultureInfo.InvariantCulture);
			if (precision == -1)
				return text;
			int index = text.IndexOf('.');
			if (index == -1)
				return text;
			if (text.Length - index > precision)
				return value.ToString("F" + precision, CultureInfo.InvariantCulture);
			return text;
		}
		
		override public SValue GetTemp()
		{
			return SValue.NewFloat(value);
		}
		
		override public void SetTemp(SValue sValue)
		{
			if (sValue.IsFloat)
			{
				Value = sValue.Float;
			}
		}
	}

	public class Int : Property
	{
		private int defaultValue;

		public Int(string name, int value) : base(name, Constraints.None)
		{
			defaultValue = value;
			this.value = value;
		}

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

		public override string Text { get { return value + ""; } }

		public override string SetText(string value, string subvalue)
		{
			int temp;
			if (int.TryParse(value, out temp))
			{
				Value = temp;
				return null;
			}
			return "Can't parse \"" + value + "\"";
		}
		
		public override string Type { get { return "int"; } }
		public override string DefaultValue { get { return defaultValue + ""; } }
		public override string PossibleValues { get { return "min: " + min + ", max: " + max; } }

		public override void Reset()
		{
			value = defaultValue;
		}
		
		override public SValue GetTemp()
		{
			return SValue.NewInt(value);
		}
		
		override public void SetTemp(SValue sValue)
		{
			if (sValue.IsInt)
			{
				Value = sValue.Int;
			}
		}
	}

	public class String : Property
	{
		private string defaultValue;
		private bool convertEscape;
		private string help;

		public String(string name, string value, bool convertEscape, string help) : base(name, Constraints.None)
		{
			defaultValue = value;
			this.value = value ?? "";
			this.convertEscape = convertEscape;
			this.help = help;
		}

		private string value;
		public string Value
		{
			get { return value; }
			set { this.value = value; }
		}

		private string[] variants;

		public String SetVariants(params string[] variants)
		{
			this.variants = variants;
			return this;
		}
		
		override public List<Variant> GetAutocompleteVariants()
		{
			string[] raw = loadVariants != null ? loadVariants() : variants;
			if (raw == null || raw.Length == 0)
				return null;
			List<Variant> result = new List<Variant>();
			foreach (string text in raw)
			{
				string variantText = ReplaceLineBreaks(text);
				Variant variant = new Variant();
				variant.CompletionText = variantText;
				variant.DisplayText = variantText;
				result.Add(variant);
			}
			return result;
		}

		private Getter<string[]> loadVariants;
		
		public string[] GetVariants()
		{
			if (loadVariants != null)
				return loadVariants();
			return variants ?? new string[0];
		}

		public String SetLoadVariants(Getter<string[]> loadVariants)
		{
			this.loadVariants = loadVariants;
			return this;
		}

		public override string Text { get { return convertEscape ? value.Replace("\r", "\\r").Replace("\n", "\\n") : value; } }
		public override string Type { get { return "string"; } }
		public override string DefaultValue { get { return ReplaceLineBreaks(defaultValue); } }
		
		public override string PossibleValues
		{
			get
			{
				string[] variants = this.variants;
				if (variants == null && loadVariants != null)
					variants = loadVariants();
				string text = "";
				if (variants != null && variants.Length > 0)
				{
					foreach (string variant in variants)
					{
						if (text != "")
							text += "\n";
						text += ReplaceLineBreaks(variant);
					}
				}
				if (!string.IsNullOrEmpty(help))
				{
					if (text != "")
						text += "\n";
					text += help;
				}
				return text != "" ? text : "=\"" + ReplaceLineBreaks(value).Replace("\"", "\"\"") + "\"";
			}
		}

		public override string SetText(string value, string subvalue)
		{
			string newValue = convertEscape && value != null ? value.Replace("\\r", "\r").Replace("\\n", "\n") : value + "";
			if (variants != null && variants.Length > 0 && Array.IndexOf(variants, newValue) == -1)
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("\"" + newValue + "\" missing in [");
				bool first = true;
				for (int i = 0; i < variants.Length; i++)
				{
					if (!first)
						builder.Append(", ");
					first = false;
					builder.Append("\"" + variants[i].Replace("\r", "\\r").Replace("\n", "\\n") + "\"");
				}
				builder.Append("]");
				return builder.ToString();
			}
			this.value = newValue;
			return null;
		}

		private string ReplaceLineBreaks(string value)
		{
			value = value.Replace("\n", "\\n");
			value = value.Replace("\r", "\\r");
			return value;
		}

		public override void Reset()
		{
			value = defaultValue;
		}
		
		override public SValue GetTemp()
		{
			return !string.IsNullOrEmpty(value) ? SValue.NewString(value) : SValue.None;
		}
		
		override public void SetTemp(SValue sValue)
		{
			if (sValue.IsString)
			{
				Value = sValue.String;
			}
		}
	}
	
	public class CommandInfo
	{
		public string pattern;
		public string command;
		public FileNameFilter filter;
		
		public override string ToString()
		{
			return "(" + command + ":" + pattern + ")";
		}
	}
	
	public class Command : Property
	{
		public Command(string name) : base(name, Constraints.Multiple)
		{
		}
		
		private readonly RWList<CommandInfo> value = new RWList<CommandInfo>();
		public IRList<CommandInfo> Value { get { return value; } }

		public override string Text
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				bool first = true;
				foreach (CommandInfo info in value)
				{
					if (!first)
						builder.Append("; ");
					first = false;
					builder.Append(info.command + ":" + info.pattern);
				}
				return builder.ToString();
			}
		}

		public override string SetText(string value, string subvalue)
		{
			if (value == "")
			{
				this.value.Clear();
				return null;
			}
			for (int i = this.value.Count; i-- > 0;)
			{
				if (this.value[i].pattern == subvalue)
				{
					this.value.RemoveAt(i);
				}
			}
			CommandInfo info = new CommandInfo();
			info.pattern = subvalue;
			info.command = value;
			if (info.pattern != null)
			{
				info.filter = new FileNameFilter(info.pattern);
			}
			this.value.Add(info);
			return null;
		}
		
		private string desc;
		
		public Command SetDesc(string desc)
		{
			this.desc = desc;
			return this;
		}
		
		private string customType;
		
		public Command SetCustomType(string customType)
		{
			this.customType = customType;
			return this;
		}

		public override string Type { get { return customType != null ? customType : "command"; } }		
		public override string ShowedName { get { return name + "[:<filter>]"; } }
		public override string TypeHelp { get { return "filter example: *.txt;*.md\ntab names allowed too: File tree"; } }
		public override string PossibleValues { get { return desc ?? "(several nodes allowed)"; } }

		public override void Reset()
		{
			value.Clear();
		}
	}

	public class RegexList : Property
	{
		public RegexList(string name) : base(name, Constraints.Multiple)
		{
		}

		private readonly RWList<RegexData> value = new RWList<RegexData>();
		public IRList<RegexData> Value { get { return value; } }

		public override string Text
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				bool first = true;
				foreach (RegexData regexData in value)
				{
					if (!first)
						builder.Append("; ");
					first = false;
					builder.Append(regexData.pattern);
				}
				return builder.ToString();
			}
		}

		public override string SetText(string value, string subvalue)
		{
			if (value == "")
			{
				this.value.Clear();
				return null;
			}
			string errors;
			RegexData data = RegexData.Parse(value, out errors);
			if (!string.IsNullOrEmpty(errors))
				return errors;
			for (int i = this.value.Count; i-- > 0;)
			{
				if (this.value[i].pattern == value)
				{
					this.value.RemoveAt(i);
				}
			}
			this.value.Add(data);
			return null;
		}
		
		public override string Type { get { return "regex"; } }
		
		public override string PossibleValues { get { return "(several nodes allowed)"; } }

		public override void Reset()
		{
			value.Clear();
		}
	}
	
	public class CommandList : Property
	{
		public CommandList(string name) : base(name, Constraints.Multiple)
		{
		}

		private readonly RWList<CommandData> value = new RWList<CommandData>();
		public IRList<CommandData> Value { get { return value; } }

		public override string Text
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				bool first = true;
				foreach (CommandData data in value)
				{
					if (!first)
						builder.Append("\n");
					first = false;
					builder.Append(data.name);
					builder.Append("|");
					builder.Append(data.sequence);
				}
				return builder.ToString();
			}
		}

		public override string SetText(string value, string subvalue)
		{
			if (value == "")
			{
				this.value.Clear();
				return null;
			}
			string errors;
			CommandData data = CommandData.Parse(value, out errors);
			if (!string.IsNullOrEmpty(errors))
				return errors;
			for (int i = this.value.Count; i-- > 0;)
			{
				CommandData dataI = this.value[i];
				if (dataI.name == data.name)
				{
					this.value.RemoveAt(i);
				}
			}
			this.value.Add(data);
			return null;
		}
		
		public override string Type { get { return "command"; } }
		
		public override string PossibleValues
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("name|sequence, multinodes allowed\n");
				builder.Append("e.g.: test|[C-:]!echo AAA[cr]\n");
				builder.Append("[C-x] - Ctrl+X\n");
				builder.Append("[C-X] - Ctrl+Shift+X\n");
				builder.Append("[C-S-x] - Ctrl+Shift+X\n");
				builder.Append("[C-;] - Ctrl+;(command dialog)\n");
				builder.Append("[C-:] - Ctrl+Shift+;(no history)\n");
				builder.Append("[C-[] - Ctrl+[\n");
				builder.Append("[C-]] - Ctrl+]\n");
				foreach (KeyValuePair<string, MacrosExecutor.Action> pair in CommandData.GetSpecials())
				{
					builder.Append("[");
					builder.Append(pair.Key);
					builder.Append("]");
					string actionText = pair.Value.ToString();
					if (actionText.Trim().Length != 0)
					{
						builder.Append(" - ");
						builder.Append(actionText);
					}
					builder.Append("\n");
				}
				builder.Append("commands in config:");
				if (value.Count > 0)
				{
					builder.Append("\n");
					foreach (CommandData data in value)
					{
						builder.Append("  ");
						builder.Append(data.name);
					}
				}
				else
				{
					builder.Append(" missing");
				}
				return builder.ToString();
			}
		}

		public override void Reset()
		{
			value.Clear();
		}
	}

	public class EncodingProperty : Property
	{
		private EncodingPair defaultValue;

		public EncodingProperty(string name, EncodingPair defaultValue) : base(name, Constraints.None)
		{
			this.defaultValue = defaultValue;
		}

		private EncodingPair value;
		public EncodingPair Value { get { return value; } }
		
		public override List<Variant> GetAutocompleteVariants()
		{
			List<Variant> result = new List<Variant>();
			foreach (EncodingInfo info in Encoding.GetEncodings())
			{
				string variantText;
				Variant variant;
				variantText = info.Name;
				variant = new Variant();
				variant.CompletionText = variantText;
				variant.DisplayText = variantText;
				result.Add(variant);
				if (info.GetEncoding().GetPreamble().Length > 0)
				{
					variantText = info.Name + " bom";
					variant = new Variant();
					variant.CompletionText = variantText;
					variant.DisplayText = variantText;
					result.Add(variant);
				}
			}
			return result;
		}

		public override string Text { get { return Value.ToString(); } }

		public override string SetText(string value, string subvalue)
		{
			string error;
			EncodingPair newValue = EncodingPair.ParseEncoding(value, out error);
			if (!newValue.IsNull)
				this.value = newValue;
			return error;
		}
		
		public override string Type { get { return "encoding[ bom]"; } }
		public override string DefaultValue { get { return defaultValue + ""; } }

		public override void Reset()
		{
			value = defaultValue;
		}
		
		override public SValue GetTemp()
		{
			return !value.IsNull ? SValue.NewString(value.ToString()) : SValue.None;
		}
		
		override public void SetTemp(SValue sValue)
		{
			if (sValue.IsString)
			{
				string error;
				EncodingPair newValue = EncodingPair.ParseEncoding(sValue.String, out error);
				if (!newValue.IsNull)
				{
					value = newValue;
				}
			}
		}
	}

	public class Bool : Property
	{
		private bool defaultValue;

		public Bool(string name, bool value) : base(name, Constraints.None)
		{
			defaultValue = value;
			this.value = value;
		}
		
		public Bool(string name, bool value, Constraints constraints) : base(name, constraints)
		{
			defaultValue = value;
			this.value = value;
		}

		private bool value;
		public bool Value
		{
			get { return value; }
			set { this.value = value; }
		}
		
		override public List<Variant> GetAutocompleteVariants()
		{
			List<Variant> result = new List<Variant>();
			foreach (string variantText in new string[] { "true", "false" })
			{
				Variant variant = new Variant();
				variant.CompletionText = variantText;
				variant.DisplayText = variantText;
				result.Add(variant);
			}
			return result;
		}

		public override string Text { get { return value ? "true" : "false"; } }

		public override string SetText(string value, string subvalue)
		{
			Value = value != null && (value == "1" || value.ToLowerInvariant() == "true");
			return null;
		}
		
		public override string Type { get { return "bool"; } }
		public override string DefaultValue { get { return defaultValue ? "true" : "false"; } }
		public override string PossibleValues { get { return "=" + (value ? "true" : "false"); } }

		public override void Reset()
		{
			value = defaultValue;
		}
		
		override public SValue GetTemp()
		{
			return SValue.NewBool(value);
		}
		
		override public void SetTemp(SValue sValue)
		{
			if (sValue.IsBool)
			{
				value = sValue.Bool;
			}
		}
	}
	
	public class BoolInfo
	{
		public string pattern;
		public bool value;
		public FileNameFilter filter;
		
		public BoolInfo(bool value, string pattern)
		{
			this.pattern = pattern;
			this.value = value;
			if (this.pattern != null)
			{
				filter = new FileNameFilter(pattern);
			}
		}
	}
	
	public class IntInfo
	{
		public string pattern;
		public int value;
		public FileNameFilter filter;
		
		public IntInfo(int value, string pattern)
		{
			this.pattern = pattern;
			this.value = value;
			if (this.pattern != null)
			{
				filter = new FileNameFilter(pattern);
			}
		}
	}
	
	public class BoolList : Property
	{
		private bool defaultValue;
		
		public BoolList(string name, bool value) : base(name, Constraints.Multiple)
		{
			defaultValue = value;
			this.value.Add(new BoolInfo(defaultValue, null));
		}

		private readonly RWList<BoolInfo> value = new RWList<BoolInfo>();
		public IRList<BoolInfo> Value { get { return value; } }
		public override string DefaultValue { get { return defaultValue ? "true" : "false"; } }
		
		override public List<Variant> GetAutocompleteVariants()
		{
			List<Variant> result = new List<Variant>();
			foreach (string variantText in new string[] { "true", "false" })
			{
				Variant variant = new Variant();
				variant.CompletionText = variantText;
				variant.DisplayText = variantText;
				result.Add(variant);
			}
			return result;
		}
		
		public bool GetValue(Buffer buffer)
		{
			string name = buffer != null ? buffer.Name : null;
			BoolInfo info = null;
			if (name != null)
			{
				for (int i = value.Count; i-- > 0;)
				{
					BoolInfo infoI = value[i];
					if (infoI.filter != null && infoI.filter.Match(name))
					{
						info = infoI;
						break;
					}
				}
			}
			if (info == null)
			{
				for (int i = value.Count; i-- > 0;)
				{
					BoolInfo infoI = value[i];
					if (infoI.filter == null)
					{
						info = infoI;
						break;
					}
				}
			}
			return info.value;
		}
		
		public override string PossibleValues
		{
			get
			{
				string text = "";
				foreach (BoolInfo info in value)
				{
					if (text != "")
						text += "\n";
					text += "=" + (info.value ? "true" : "false") + (info.pattern != null ? ":" + info.pattern : "");
				}
				return text;
			}
		}

		public override string Text
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				bool first = true;
				foreach (BoolInfo info in value)
				{
					if (!first)
						builder.Append("; ");
					first = false;
					builder.Append((info.value ? "true" : "false") + ":" + info.pattern);
				}
				return builder.ToString();
			}
		}

		public override string SetText(string value, string subvalue)
		{
			if (value == "")
			{
				this.value.Clear();
				return null;
			}
			for (int i = this.value.Count; i-- > 0;)
			{
				if (this.value[i].pattern == subvalue)
				{
					this.value.RemoveAt(i);
				}
			}
			this.value.Add(new BoolInfo(value != null && (value == "1" || value.ToLowerInvariant() == "true"), subvalue));
			return null;
		}

		public override string Type { get { return "bool"; } }		
		public override string ShowedName { get { return name + "[:<filter>]"; } }
		public override string TypeHelp { get { return "filter example: *.txt;*.md"; } }

		public override void Reset()
		{
			value.Clear();
			value.Add(new BoolInfo(defaultValue, null));
		}
	}
	
	public class IntList : Property
	{
		private int defaultValue;
		
		public IntList(string name, int value) : base(name, Constraints.Multiple)
		{
			defaultValue = value;
			this.value.Add(new IntInfo(defaultValue, null));
		}

		private readonly RWList<IntInfo> value = new RWList<IntInfo>();
		public IRList<IntInfo> Value { get { return value; } }
		public override string DefaultValue { get { return defaultValue + ""; } }
		
		public int GetValue(Buffer buffer)
		{
			string name = buffer != null ? buffer.Name : null;
			IntInfo info = null;
			if (name != null)
			{
				for (int i = value.Count; i-- > 0;)
				{
					IntInfo infoI = value[i];
					if (infoI.filter != null && infoI.filter.Match(name))
					{
						info = infoI;
						break;
					}
				}
			}
			if (info == null)
			{
				for (int i = value.Count; i-- > 0;)
				{
					IntInfo infoI = value[i];
					if (infoI.filter == null)
					{
						info = infoI;
						break;
					}
				}
			}
			return Math.Max(min, Math.Min(max, info.value));
		}
		
		public override string PossibleValues
		{
			get
			{
				string text = "";
				foreach (IntInfo info in value)
				{
					if (text != "")
						text += "\n";
					text += "=" + info.value + (info.pattern != null ? ":" + info.pattern : "");
				}
				return text;
			}
		}

		public override string Text
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				bool first = true;
				foreach (IntInfo info in value)
				{
					if (!first)
						builder.Append("; ");
					first = false;
					builder.Append(info.value + ":" + info.pattern);
				}
				return builder.ToString();
			}
		}

		public override string SetText(string value, string subvalue)
		{
			if (value == "")
			{
				this.value.Clear();
				return null;
			}
			for (int i = this.value.Count; i-- > 0;)
			{
				if (this.value[i].pattern == subvalue)
				{
					this.value.RemoveAt(i);
				}
			}
			int parsedValue;
			if (int.TryParse(value, out parsedValue))
			{
				this.value.Add(new IntInfo(parsedValue, subvalue));
			}
			return null;
		}

		public override string Type { get { return "bool"; } }		
		public override string ShowedName { get { return name + "[:<filter>]"; } }
		public override string TypeHelp { get { return "filter example: *.txt;*.md"; } }

		public override void Reset()
		{
			value.Clear();
			value.Add(new IntInfo(defaultValue, null));
		}
		
		private int min = int.MinValue;
		private int max = int.MaxValue;

		public IntList SetMinMax(int min, int max)
		{
			this.min = min;
			this.max = max;
			return this;
		}
	}

	public class Font : Property
	{
		private FontFamily defaultValue;

		public Font(string name, FontFamily value) : base(name, Constraints.None)
		{
			defaultValue = value;
			this.value = value;
		}

		private FontFamily value;
		public FontFamily Value
		{
			get { return value; }
			set { this.value = value; }
		}

		public override string Text { get { return StringOf(value); } }

		public override string SetText(string value, string subvalue)
		{
			if (!IsFamilyInstalled(value))
			{
				Value = defaultValue;
				return "Font \"" + value + "\" is not installed";
			}
			Value = new FontFamily(value);
			return null;
		}
		
		public override string Type { get { return "font"; } }
		public override string DefaultValue { get { return StringOf(defaultValue); } }
		public override string PossibleValues { get { return "=\"" + Value.Name + "\""; } }

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
		
		override public List<Variant> GetAutocompleteVariants()
		{
			List<Variant> result = new List<Variant>();
			InstalledFontCollection installed = new InstalledFontCollection();
			foreach (FontFamily familyI in installed.Families)
			{
				Variant variant = new Variant();
				variant.CompletionText = familyI.Name;
				variant.DisplayText = familyI.Name;
				result.Add(variant);
			}
			return result;
		}

		public override void Reset()
		{
			value = defaultValue;
		}
		
		override public SValue GetTemp()
		{
			return value != null ? SValue.NewString(value.Name) : SValue.None;
		}
		
		override public void SetTemp(SValue sValue)
		{
			if (sValue.IsString)
			{
				SetText(sValue.String, "");
			}
		}
	}
	
	public class PathProperty : Property
	{
		private readonly string defaultValue;
		private readonly string help;
		
		public PathProperty(string name, string defaultValue, string help) : base(name, Constraints.None)
		{
			this.defaultValue = defaultValue;
			this.help = help;
		}

		private string value;
		public string Value { get { return value; } }

		public override string Text { get { return defaultValue; } }
		
		private static bool IsPathGlobal(string path)
		{
			return path.Length > 2 && path[1] == ':' && (path[2] == '\\' || path[2] == '/');
		}

		public override string SetText(string value, string subvalue)
		{
			value = value.Trim();
			if (!IsPathGlobal(value))
			{
				value = Path.Combine(Directory.GetCurrentDirectory(), value);
			}
			if (!Directory.Exists(value) && !File.Exists(value))
			{
				return "No file or directory: " + value;
			}
			this.value = value;
			return null;
		}
		
		public override string Type { get { return "path"; } }
		public override string DefaultValue { get { return defaultValue; } }
		public override string PossibleValues { get { return help; } }

		public override void Reset()
		{
			value = defaultValue;
		}
		
		override public SValue GetTemp()
		{
			return !string.IsNullOrEmpty(value) ? SValue.NewString(value) : SValue.None;
		}
		
		override public void SetTemp(SValue sValue)
		{
			if (sValue.IsString && !string.IsNullOrEmpty(sValue.String))
			{
				SetText(sValue.String, "");
			}
		}
	}
}
