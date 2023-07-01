using System;

namespace Gamma.Utilities
{
	[AttributeUsage (AttributeTargets.Property | AttributeTargets.Field)]
	public class ColorNameAttribute : Attribute
	{
		public string ColorString;

		public ColorNameAttribute (string color)
		{
			ColorString = color;
		}
	}
}

