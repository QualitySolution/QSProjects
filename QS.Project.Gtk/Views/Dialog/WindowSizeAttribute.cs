using System;
namespace QS.Views.Dialog
{
	[AttributeUsage(AttributeTargets.Class)]
	public class WindowSizeAttribute : Attribute
	{
		public int DefaultHeight;
		public int DefaultWidth;

		public WindowSizeAttribute(int defaultWidth, int defaultHeight)
		{
			DefaultHeight = defaultHeight;
			DefaultWidth = defaultWidth;
		}
	}
}
