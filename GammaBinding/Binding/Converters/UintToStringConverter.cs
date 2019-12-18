using System;
using System.Globalization;

namespace Gamma.Binding.Converters
{
	public class UintToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value?.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(String.IsNullOrWhiteSpace(value as String))
				return null;

			if(targetType == typeof(uint?) && UInt32.TryParse(value.ToString(), out uint number))
				return number;

			return null;
		}
	}
}
