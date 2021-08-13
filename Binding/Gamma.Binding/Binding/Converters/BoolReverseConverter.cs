using System;
using System.Globalization;

namespace Gamma.Binding.Converters
{
	public class BoolReverseConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(targetType == typeof(bool))
				return !System.Convert.ToBoolean(value);
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(targetType == typeof(bool))
				return !System.Convert.ToBoolean(value);
			return null;
		}
	}
}
