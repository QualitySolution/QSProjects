using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Gamma.Utilities;

namespace QS.Project.Converters;

/// <summary>
/// Показывает значение перечисления так, как оно подписано в [Display(Name)].
/// </summary>
public sealed class EnumTitleConverter : IValueConverter {
	public static readonly EnumTitleConverter Instance = new EnumTitleConverter();

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value is Enum item ? item.GetEnumTitle() : value;

	// Только на показ: выбранное значение приходит из ItemsSource, обратное преобразование не нужно
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		BindingOperations.DoNothing;
}
