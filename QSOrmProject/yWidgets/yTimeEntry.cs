using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using QSWidgetLib;

namespace Gamma.Widgets
{
	[ToolboxItem (true)]
	[Category ("Gamma Widgets")]
	public class yTimeEntry : TimeEntry
	{
		public BindingControler<yTimeEntry> Binding { get; private set;}

		public yTimeEntry ()
		{
			Binding = new BindingControler<yTimeEntry> (this, new Expression<Func<yTimeEntry, object>>[] {
				(w => w.DateTime),
				(w => w.Time),
			});
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (new Expression<Func<yTimeEntry, object>>[] {
				(w => w.DateTime),
				(w => w.Time),
			});

			base.OnChanged ();
		}
	}
}

