using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Gamma.Binding;

namespace Gamma.Widgets.Additions
{
	public class EnumsListConverter<TEnum> : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is IList<TEnum>) {
				return (value as IList<TEnum>).Cast<Enum>().ToList();
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is IList<Enum>) {
				return (value as IList<Enum>).Cast<TEnum>().ToList();
			}
			return null;
		}

	}


}
