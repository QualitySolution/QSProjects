using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace QS.Project.Avalonia.ViewModels.Converters;
public class DoubleToRenderTransformYConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (targetType != typeof(ITransform))
			throw new InvalidCastException("Target type must be ITransform");
		if (value is not double)
			throw new InvalidCastException("Source type must be double");

		return new TranslateTransform(0, -(double)value);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
