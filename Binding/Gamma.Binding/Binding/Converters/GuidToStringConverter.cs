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
			if(Guid.TryParse((string)value, out Guid result)) {
				if(targetType == typeof(Guid) || targetType == typeof(Guid?)) {
					return result;
				}
			}
			return null;
		}
	}

}

