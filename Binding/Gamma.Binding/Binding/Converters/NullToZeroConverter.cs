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
			if(targetType == typeof(decimal)) {
				return value == null ? (decimal)0 : System.Convert.ToDecimal(value);
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
			if(targetType == typeof(decimal?)) {
				var conerted = System.Convert.ToDecimal(value);
				return conerted == 0 ? (decimal?)null : conerted;
			}
			return null;
		}
	}
}
