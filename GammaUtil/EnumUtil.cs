using System;

namespace Gamma.Utilities
{
	public static class EnumUtil
	{
		public static T GetAttribute<T>(this Enum value) where T : Attribute {
			var type = value.GetType();
			var memberInfo = type.GetMember(value.ToString());
			var attributes = memberInfo[0].GetCustomAttributes (typeof(T), false);
			if (attributes.Length > 0)
				return (T)attributes [0];
			else
				return null;
		}
	}
}

