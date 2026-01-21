using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace QS.Project.Converters;
public class DoubleToRenderTransformYConverter : IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if(targetType != typeof(ITransform))
			throw new InvalidCastException("Target type must be ITransform");
		if(value is not double)
			throw new InvalidCastException("Source type must be double");

		System.Diagnostics.Debug.WriteLine($"DoubleToRenderTransformYConverter: {value}");

		return new TranslateTransform(0, -(double)value);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}
