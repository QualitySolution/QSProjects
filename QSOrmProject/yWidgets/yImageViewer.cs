using System.ComponentModel;
using Gamma.Binding.Core;
using QSWidgetLib;

namespace Gamma.Widgets
{
	[ToolboxItem (true)]
	[Category ("Gamma Widgets")]
	public class yImageViewer : ImageViewer
	{
		private bool _destroyed;
		
		public BindingControler<yImageViewer> Binding { get; private set;}

		public yImageViewer ()
		{
			Binding = new BindingControler<yImageViewer> (this);
		}
		
		protected override void OnDestroyed() {
			if(_destroyed) {
				return;
			}

			Binding.CleanSources();
			base.OnDestroyed();
			
			_destroyed = true;
		}
	}
}

