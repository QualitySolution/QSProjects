using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace QS.Project.Converters;

/// <summary>
/// Показывает null как пустую строку, а пустую или состоящую из пробелов строку при вводе превращает обратно в null.
/// </summary>
public class NullToEmptyStringConverter : IValueConverter {
	public static readonly NullToEmptyStringConverter Instance = new();

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value ?? string.Empty;

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		string.IsNullOrWhiteSpace(value as string) ? null : value;
}
