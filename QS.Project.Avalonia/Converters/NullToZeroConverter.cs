using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace QS.Project.Converters;

/// <summary>
/// Показывает null как 0, а 0 при вводе превращает обратно в null. Для числовых полей, где 0 означает «не задано».
/// </summary>
public class NullToZeroConverter : IValueConverter {
	public static readonly NullToZeroConverter Instance = new();

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value == null ? 0m : System.Convert.ToDecimal(value, culture);

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if(value == null)
			return null;

		var number = System.Convert.ToDecimal(value, culture);
		if(number == 0)
			return null;

		var type = Nullable.GetUnderlyingType(targetType) ?? targetType;
		return System.Convert.ChangeType(number, type, culture);
	}
}
