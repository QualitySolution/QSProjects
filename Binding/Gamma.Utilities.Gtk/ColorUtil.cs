using System;
using Gdk;

namespace Gamma.Utilities
{
	public static class ColorUtil
	{
		public static Color Create(string text)
		{
			var color = new Color();
			if (!Color.Parse(text, ref color))
				throw new InvalidOperationException(String.Format("Can't parse color value \"{0}\".", text));
			return color;
		}

		public static string GetEnumColor(this Enum aEnum)
		{
			var att = aEnum.GetAttribute<ColorNameAttribute>();
			return att != null ? att.ColorString : null;
		}
	}
}
