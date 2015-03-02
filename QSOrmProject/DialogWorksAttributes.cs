using System;

namespace QSOrmProject
{
	[AttributeUsage(AttributeTargets.Class)]
	public class WidgetWindowAttribute : Attribute 
	{
		public int DefaultHeight;
		public int DefaultWidth;
	}
}
