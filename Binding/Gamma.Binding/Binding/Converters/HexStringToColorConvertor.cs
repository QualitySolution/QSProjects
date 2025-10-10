using System;
using System.Globalization;
using Gamma.Utilities;
using Gdk;

namespace Gamma.Binding.Converters {

	public class HexStringToColorConvertor : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(targetType == typeof(Color) && !String.IsNullOrWhiteSpace((string)value)) {
				return ColorUtil.Create((string)value);
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(targetType == typeof(string)) 
				return ColorUtil.GetHex((Color)value);
			
			return null;
		}
	}
}
