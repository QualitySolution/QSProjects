using System;
using System.Globalization;
using Gamma.Binding;

namespace QSOrmProject
{
	[Obsolete("Используйте аналог из Gamma.Binding.Converters")]
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

	public class IntToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value?.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(String.IsNullOrWhiteSpace(value as String))
				return null;

			int number = 0;
			if(targetType == typeof(int?) && Int32.TryParse(value.ToString(), out number))
				return number;
			
			return null;
		}
	}

	[Obsolete("Используйте аналог из Gamma.Binding.Converters")]
	public class UintToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value?.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(String.IsNullOrWhiteSpace(value as String))
				return null;

			if(targetType == typeof(uint?) && UInt32.TryParse(value.ToString(), out uint number))
				return number;

			return null;
		}
	}

	public class GuidToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value == null)
				return null;

			return ((Guid)value).ToString() ;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(targetType == typeof(Guid)) {
				return new Guid((string)value);
			}

			if(targetType == typeof(Guid?)) {
				if(String.IsNullOrWhiteSpace((string)value))
					return null;
				return new Guid((string)value);
			}

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
			if(targetType == typeof(float))
				return (System.Convert.ToSingle(value) * 0.01f);

			return null;
		}
	}

	[Obsolete("Используйте аналог из Gamma.Binding.Converters")]
	public class NumbersTypeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(int))
				return (System.Convert.ToInt32 (value));
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
			return null;
		}
	}

	[Obsolete("Используйте аналог из Gamma.Binding.Converters")]
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

