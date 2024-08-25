using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Globalization;

namespace QS.Launcher.ViewModels.Converters;
public class ByteArrayToBitmapConverter : IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (value is null or not byte[])
			return null;

		Bitmap bitmap;
		using (var stream = new System.IO.MemoryStream((byte[])value))
			bitmap = new Bitmap(stream);

		return bitmap;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException("Do you really need this?");
	}
}
