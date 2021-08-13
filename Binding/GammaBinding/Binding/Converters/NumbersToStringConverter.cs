using System;
using System.Globalization;

namespace Gamma.Binding.Converters
{
	/// <summary>
	/// Преобразовывает любые числовые типы в строку и наоборт.
	/// Внимание!!! Не все типы реализованы.
	/// </summary>
	public class NumbersToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value?.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(String.IsNullOrWhiteSpace(value as String))
				return null;

			if(targetType == typeof(uint?) && UInt32.TryParse(value.ToString(), out uint nullableUint))
				return nullableUint;

			if(targetType == typeof(int?) && Int32.TryParse(value.ToString(), out int nullableInt))
				return nullableInt;

			return null;
		}
	}
}
