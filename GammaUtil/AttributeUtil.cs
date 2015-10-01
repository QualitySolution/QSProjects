using System;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace Gamma.Utilities
{
	public static class AttributeUtil
	{

		public static string GetEnumTitle (this Enum aEnum)
		{
			string desc = aEnum.ToString();
			FieldInfo fi = aEnum.GetType().GetField (desc);
			return (fi.GetEnumTitle());
		}

		public static string GetEnumTitle (this FieldInfo aFieldInfo)
		{
			var attrs = (DisplayAttribute[]) aFieldInfo.GetCustomAttributes (typeof(DisplayAttribute), false);
			if ((attrs != null) && (attrs.Length > 0))
				return (attrs[0].GetName ());
			return (aFieldInfo.Name);
		}

		/// <summary>
		/// Return Description prpprety from DisplayAttribute.
		/// </summary>
		/// <returns>The field description.</returns>
		/// <param name="aFieldInfo">A field info.</param>
		public static string GetFieldDescription (this FieldInfo aFieldInfo)
		{
			var attrs = (DisplayAttribute[]) aFieldInfo.GetCustomAttributes (typeof(DisplayAttribute), false);
			if ((attrs != null) && (attrs.Length > 0))
				return (attrs[0].GetDescription ());
			return null;
		}
	}
}

