using System;
using System.Globalization;

namespace Gamma.Binding.Converters
{
	public class NumbersTypeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(int))
				return (System.Convert.ToInt32 (value));
			if(targetType == typeof(double))
				return System.Convert.ToDouble(value);
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(targetType == typeof(short))
				return (System.Convert.ToInt16(value));
			if (targetType == typeof(ushort))
				return (System.Convert.ToUInt16(value));
			if (targetType == typeof(byte))
				return (System.Convert.ToByte(value));
			if(targetType == typeof(double))
				return System.Convert.ToDouble(value);
			return null;
		}
	}
}
