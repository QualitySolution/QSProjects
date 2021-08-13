using System;
using System.Globalization;

namespace Gamma.Binding.Converters
{
	public class GuidToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value == null)
				return null;

			return ((Guid)value).ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(targetType == typeof(Guid)) {
				return new Guid((string)value);
			}

			if(targetType == typeof(Guid?)) {
				if(String.IsNullOrWhiteSpace((string)value))
					return null;
				return new Guid((string)value);
			}

			return null;
		}
	}

}

