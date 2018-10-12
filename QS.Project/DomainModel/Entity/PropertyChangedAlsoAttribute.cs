using System;

namespace QS.DomainModel.Entity
{
	[AttributeUsage(AttributeTargets.Property)]
	public class PropertyChangedAlsoAttribute : Attribute
	{
		public string[] PropertiesNames;

		public PropertyChangedAlsoAttribute(params string[] propertiesNames)
		{
			PropertiesNames = propertiesNames;
		}
	}
}
