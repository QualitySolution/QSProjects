using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Gamma.Binding.Converters {
	public class ArrayToEnumerableConverter<TElementType> : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if(value is IEnumerable<TElementType>) {
				return ((IEnumerable<TElementType>)value).ToArray();
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			if(value is object[]) {
				return ((object[])value).Cast<TElementType>();
			}
			return value;
		}
	}
}
