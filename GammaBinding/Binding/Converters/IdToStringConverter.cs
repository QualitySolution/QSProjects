using System;
using System.Globalization;

namespace Gamma.Binding.Converters
{
	public class IdToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int id = (int)value;
			if(id > 0)
				return id.ToString();
			else
				return "не определён";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(int.TryParse(value.ToString(), out int result))
				return result;
			return null;
		}
	}
}
