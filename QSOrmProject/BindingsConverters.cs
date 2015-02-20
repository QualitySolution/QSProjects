using System;
using System.Data.Bindings;
using System.Globalization;

namespace QSOrmProject
{
	public class IdToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int id = (int)value;
			if (id > 0)
				return id.ToString ();
			else
				return "не определён";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}

