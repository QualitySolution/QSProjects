using System;
using System.ComponentModel;
using Gamma.Binding.Core;
using System.Linq.Expressions;

namespace QSOrmProject
{
	[ToolboxItem (true)]
	[Category ("Gamma Widgets")]
	public class yEntryReferenceVM : EntryReferenceVM
	{
		public BindingControler<yEntryReferenceVM> Binding { get; private set; }

		public yEntryReferenceVM ()
		{
			Binding = new BindingControler<yEntryReferenceVM> (this, new Expression<Func<yEntryReferenceVM, object>>[] {
				(w => w.Subject)
			});
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (new Expression<Func<yEntryReferenceVM, object>>[] {
				(w => w.Subject)
			});
			base.OnChanged ();
		}
	}
}

