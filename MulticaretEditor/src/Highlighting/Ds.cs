using System;
using System.Collections.Generic;

namespace MulticaretEditor.Highlighting
{
	public class Ds
	{
		public readonly string name;
		public readonly short index;
		
		public Ds(string name, short index)
		{
			this.name = name;
			this.index = index;
		}
		
		private static Ds AddStyle(string name)
		{
			Ds type = new Ds(name, lastIndex++);
			_all.Add(type);
			byName.Add(name, type);
			return type;
		}
		
		private static short lastIndex = 0;
		private static RWList<Ds> _all = new RWList<Ds>();
		public static readonly IRList<Ds> all = _all;
		
		private static Dictionary<string, Ds> byName = new Dictionary<string, Ds>();
		
		public static readonly Ds Normal = AddStyle("dsNormal");
		public static readonly Ds Keyword = AddStyle("dsKeyword");
		public static readonly Ds DataType = AddStyle("dsDataType");
		public static readonly Ds DecVal = AddStyle("dsDecVal");
		public static readonly Ds BaseN = AddStyle("dsBaseN");
		public static readonly Ds Float = AddStyle("dsFloat");
		public static readonly Ds Char = AddStyle("dsChar");
		public static readonly Ds String = AddStyle("dsString");
		public static readonly Ds Comment = AddStyle("dsComment");
		public static readonly Ds Others = AddStyle("dsOthers");
		public static readonly Ds Alert = AddStyle("dsAlert");
		public static readonly Ds Function = AddStyle("dsFunction");
		public static readonly Ds RegionMarker = AddStyle("dsRegionMarker");
		public static readonly Ds Error = AddStyle("dsError");
		
		// Need more default styles
		public static readonly Ds Operator = AddStyle("dsOperator");
		public static readonly Ds Constructor = AddStyle("dsConstructor");
		public static readonly Ds Normal2 = AddStyle("dsNormal2");
		public static readonly Ds Keyword2 = AddStyle("dsKeyword2");
		public static readonly Ds String2 = AddStyle("dsString2");
		public static readonly Ds Others2 = AddStyle("dsOthers2");
		
		// And more comments styles
		public static readonly Ds DocStart = AddStyle("dsDocStart");
		public static readonly Ds DocHtml1 = AddStyle("dsDocHtml1");
		public static readonly Ds DocHtml2 = AddStyle("dsDocHtml2");
		public static readonly Ds DocHtml3 = AddStyle("dsDocHtml3");
		public static readonly Ds DocWord = AddStyle("dsDocWord");
		public static readonly Ds DocTag = AddStyle("dsDocTag");
		public static readonly Ds DocAlertTag3 = AddStyle("dsDocAlertTag3");
		public static readonly Ds DocAlertTag2 = AddStyle("dsDocAlertTag2");
		public static readonly Ds DocAlertTag1 = AddStyle("dsDocAlertTag1");
		public static readonly Ds DocCustomTag = AddStyle("dsDocCustomTag");
		public static readonly Ds DocEntities = AddStyle("dsDocEntities");
		public static readonly Ds DocDescription = AddStyle("dsDocDescription");
		public static readonly Ds DocInner = AddStyle("dsDocInner");
		public static readonly Ds DocAlert1 = AddStyle("dsDocAlert1");
    	public static readonly Ds DocAlert2 = AddStyle("dsDocAlert2");
    	public static readonly Ds DocAlert3 = AddStyle("dsDocAlert3");
		
		public static Ds GetByName(string name)
		{
			Ds type;
			byName.TryGetValue(name, out type);
			return type != null ? type : Normal;
		}
	}
}
