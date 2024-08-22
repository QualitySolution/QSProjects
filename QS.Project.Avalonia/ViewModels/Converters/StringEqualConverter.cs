using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Text;

namespace QS.Project.Avalonia.ViewModels.Converters;

public class StringEqualConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is string val && parameter is string param)
			return val == param;
		return false;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
