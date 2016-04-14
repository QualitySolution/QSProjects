using System;

namespace QSDocTemplates
{

	public class PatternField
	{
		public string Name;
		public PatternFieldType Type;
		public object Value;

		public PatternField()
		{

		}

		public PatternField(string name, PatternFieldType type)
		{
			Name = name;
			Type = type;
		}
	}

	public enum PatternFieldType{
		FString,
		FDate,
		FCurrency
	}
}
