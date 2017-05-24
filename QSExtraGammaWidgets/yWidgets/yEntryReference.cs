using System;
using System.ComponentModel;
using Gamma.Binding.Core;
using System.Linq.Expressions;
using QSOrmProject;

namespace Gamma.Widgets
{
	[ToolboxItem (true)]
	[Category ("Gamma Widgets")]
	public class yEntryReference : EntryReference
	{
		public BindingControler<yEntryReference> Binding { get; private set;}

		public yEntryReference ()
		{
			Binding = new BindingControler<yEntryReference> (this, new Expression<Func<yEntryReference, object>>[] {
				(w => w.Subject)
			});
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (new Expression<Func<yEntryReference, object>>[] {
				(w => w.Subject)
			});
			base.OnChanged ();
		}
	}
}

