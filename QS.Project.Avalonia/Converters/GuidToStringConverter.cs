using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace QS.Project.Converters;

/// <summary>
/// Преобразует <see cref="Guid"/> и <see cref="Nullable{Guid}"/> в строку и обратно.
/// Пустая строка превращается в null, некорректное значение показывается как ошибка валидации поля.
/// </summary>
public class GuidToStringConverter : IValueConverter {
	public static readonly GuidToStringConverter Instance = new();

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value is Guid guid ? guid.ToString() : null;

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		var text = (value as string)?.Trim();
		if(string.IsNullOrEmpty(text))
			return targetType == typeof(Guid) ? Guid.Empty : null;

		return Guid.TryParse(text, out var guid)
			? guid
			: new BindingNotification(new DataValidationException("Некорректный Guid."), BindingErrorType.DataValidationError);
	}
}
