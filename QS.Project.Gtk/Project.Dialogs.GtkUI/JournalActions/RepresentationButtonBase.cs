using System;
using Gtk;
using QS.RepresentationModel.GtkUI;

namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public abstract class RepresentationButtonBase : IJournalActionButton
	{
		protected readonly Button button = new Button("gtk-add");
		protected readonly IRepresentationModel RepresentationModel;
		protected readonly IJournalDialog dialog;

		protected Type EntityClass => RepresentationModel.EntityType;
		public Button Button => button;

		public bool Sensetive => Button.Sensitive;

		public string Title { get; private set; }

		protected RepresentationButtonBase(IJournalDialog dialog, IRepresentationModel representationModel, string title, string iconStockId = null)
		{
			button = iconStockId == null ? new Button() : new Button(iconStockId);
			this.RepresentationModel = representationModel;
			this.dialog = dialog;
			button.Label = Title = title;
			button.Clicked += Button_Clicked;
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			Execute();
		}

		public abstract void CheckSensetive(object[] selected);

		public abstract void Execute();
	}
}
