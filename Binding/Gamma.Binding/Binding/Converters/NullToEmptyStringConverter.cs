using System;
using System.Globalization;

namespace Gamma.Binding.Converters
{
	/// <summary>
	/// Конвертер, который преобразует null в пустую строку и обратно.
	/// </summary>
	public class NullToEmptyStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(string))
			{
				return value ?? String.Empty;
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(string))
			{
				var converted = (string)value;
				return String.IsNullOrWhiteSpace(converted) ? null : converted;
			}
			return null;
		}
	}
}
