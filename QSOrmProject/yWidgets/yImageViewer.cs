using System.ComponentModel;
using Gamma.Binding.Core;
using QSWidgetLib;

namespace Gamma.Widgets
{
	[ToolboxItem (true)]
	[Category ("Gamma Widgets")]
	public class yImageViewer : ImageViewer
	{
		public BindingControler<yImageViewer> Binding { get; private set;}

		public yImageViewer ()
		{
			Binding = new BindingControler<yImageViewer> (this);
		}
	}
}

