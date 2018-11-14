using System;

namespace Gamma.Utilities
{
	[AttributeUsage (AttributeTargets.Property | AttributeTargets.Field)]
	public class GtkColorAttribute : Attribute
	{
		public string ColorString;

		public GtkColorAttribute (string color)
		{
			ColorString = color;
		}
	}
}

