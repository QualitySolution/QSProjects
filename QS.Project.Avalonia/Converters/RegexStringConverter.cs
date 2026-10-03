using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace QS.Project.Converters;

/// <summary>
/// Показывает null как пустую строку, пустой ввод превращает в null. Если введенная строка не соответствует
/// <see cref="Pattern"/>, значение не применяется, а в поле сразу показывается <see cref="ErrorMessage"/>.
/// </summary>
public class RegexStringConverter : IValueConverter {
	public string Pattern { get; set; } = string.Empty;

	public string ErrorMessage { get; set; } = "Некорректное значение.";

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value ?? string.Empty;

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		var text = value as string;
		if(string.IsNullOrWhiteSpace(text))
			return null;

		return Regex.IsMatch(text, Pattern)
			? text
			: new BindingNotification(new DataValidationException(ErrorMessage), BindingErrorType.DataValidationError);
	}
}
