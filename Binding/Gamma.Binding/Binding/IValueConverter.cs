using System;
using System.Globalization;

namespace Gamma.Binding
{
	/// <summary>
	/// This interface is an analog of same name interface from System.Windows.Data;
	/// Implemented here, becouse it not exist on mono.
	/// </summary>
	public interface IValueConverter
	{
		object Convert (object value, Type targetType, object parameter, CultureInfo culture);
		object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture);
	}
}

