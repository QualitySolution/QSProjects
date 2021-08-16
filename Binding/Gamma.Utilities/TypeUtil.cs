using System;

namespace Gamma.Utilities
{
	public static class TypeUtil
	{
		/// <summary>
		/// Resolves if type is any int or uint variation
		/// </summary>
		/// <param name="aType">
		/// Type to be checked <see cref="System.Type"/>
		/// </param>
		/// <returns>
		/// true if type in any int or uint variation <see cref="System.Boolean"/>
		/// </returns>
		public static bool IsInt (System.Type aType)
		{
			return ((aType == typeof (byte)) ||
				(aType == typeof (Int16)) ||
				(aType == typeof (UInt16)) ||
				(aType == typeof (Int32)) ||
				(aType == typeof (UInt32)) ||
				(aType == typeof (Int64)) ||
				(aType == typeof (UInt64)));
		}

		/// <summary>
		/// Resolves if type is float
		/// </summary>
		/// <param name="aType">
		/// Type to be checked <see cref="System.Type"/>
		/// </param>
		/// <returns>
		/// true if type is float or double <see cref="System.Boolean"/>
		/// </returns>
		public static bool IsFloat (System.Type aType)
		{
			return ((aType == typeof (float)) ||
				(aType == typeof (decimal)) ||
				(aType == typeof (double)));
		}

		/// <summary>
		/// Resolves if type is numeric  
		/// </summary>
		/// <param name="aType">
		/// Type to be checked <see cref="System.Type"/>
		/// </param>
		/// <returns>
		/// true if IsInt or IsFloat return true <see cref="System.Boolean"/>
		/// </returns>
		public static bool IsNumeric (System.Type aType)
		{
			return ((IsInt(aType) == true) || (IsFloat(aType) == true));
		}

		public static bool EqualBoxedValues(object obj1, object obj2)
		{
			return ((obj1 != null) && obj1.GetType().IsValueType)
				 ? obj1.Equals(obj2)
				 : (obj1 == obj2);
		}
	}
}

