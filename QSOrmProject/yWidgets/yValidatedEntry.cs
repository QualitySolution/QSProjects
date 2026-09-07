using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using QSWidgetLib;

namespace Gamma.Widgets
{
	[ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class yValidatedEntry : ValidatedEntry
	{
		private bool _destroyed;
		
		public BindingControler<yValidatedEntry> Binding { get; private set;}

		public yValidatedEntry ()
		{
			Binding = new BindingControler<yValidatedEntry> (this, new Expression<Func<yValidatedEntry, object>>[] {
				(w => w.Text)
			});
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (w => w.Text);
			base.OnChanged ();
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

