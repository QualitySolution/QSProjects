using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace QS.Project.Avalonia.ViewModels.Converters;
public class NumericIsNotZeroConverter : IValueConverter
{

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (targetType != typeof(bool))
			throw new InvalidCastException("Target type must be boolean");

		return value switch
		{
			null => false,
			int i => i != 0,
			long l => l != 0,
			double d => d != 0,
			float f => f != 0,
			decimal dec => dec != 0,
			_ => (object)false,
		};
	}


	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
