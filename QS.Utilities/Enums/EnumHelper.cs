using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Utilities.Enums
{
	public static class EnumHelper
	{
		public static IList<TEnum> GetValuesList<TEnum>() where TEnum : Enum
		{
			return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
		}
	}
}
