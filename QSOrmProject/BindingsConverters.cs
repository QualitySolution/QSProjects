using System;
using System.Globalization;
using Gamma.Binding;

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

	public class MultiplierToPercentConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(double))
				return (System.Convert.ToDouble (value) * 100);
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(targetType == typeof(decimal))
				return (System.Convert.ToDecimal(value) * 0.01m);
			return null;
		}
	}

	public class NullToZeroConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(int))
			{
				return value == null ? (int)0 : System.Convert.ToInt32(value);
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(int?))
			{
				var converted = System.Convert.ToInt32(value);
				return converted == 0 ? (int?)null : converted;
			}
			return null;
		}
	}

}

