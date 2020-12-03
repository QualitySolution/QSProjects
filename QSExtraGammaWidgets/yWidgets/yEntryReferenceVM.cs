using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using QSOrmProject;

namespace Gamma.Widgets
{
	[ToolboxItem (true)]
	[Category ("Gamma Widgets")]
	[Obsolete("Используйте новый виджет EntityEntry и новый тип журналов QS.Project.Journal")]
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

