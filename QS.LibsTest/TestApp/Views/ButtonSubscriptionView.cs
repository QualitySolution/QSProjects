using System;
using QS.Test.TestApp.Domain;
using QS.Test.TestApp.ViewModels;
using QS.Views.GtkUI;

namespace QS.Test.TestApp.Views
{
	public partial class ButtonSubscriptionView : EntityTabViewBase<EntityViewModel, FullEntity>
	{
		public ButtonSubscriptionView(EntityViewModel viewModel) : base(viewModel)
		{
			this.Build();
			CommonButtonSubscription();
		}

		internal Gtk.Button SaveButton => buttonSave;
		internal Gtk.Button CancelButton => buttonCancel;
	}
}
