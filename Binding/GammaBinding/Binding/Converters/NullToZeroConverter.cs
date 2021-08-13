using System;
using System.Globalization;

namespace Gamma.Binding.Converters
{
	public class NullToZeroConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(targetType == typeof(int)) {
				return value == null ? (int)0 : System.Convert.ToInt32(value);
			}
			if(targetType == typeof(uint)) {
				return value == null ? (uint)0 : System.Convert.ToUInt32(value);
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(targetType == typeof(int?)) {
				var converted = System.Convert.ToInt32(value);
				return converted == 0 ? (int?)null : converted;
			}
			if(targetType == typeof(uint?)) {
				var converted = System.Convert.ToUInt32(value);
				return converted == 0 ? (uint?)null : converted;
			}
			return null;
		}
	}
}
