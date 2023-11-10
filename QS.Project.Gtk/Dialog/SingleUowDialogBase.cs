using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Tdi;

namespace QS.Dialog.Gtk
{
	public abstract class SingleUowDialogBase : TdiTabBase, ITdiDialog, ISingleUoWDialog
	{
		public SingleUowDialogBase()
		{
		}

		public virtual IUnitOfWork UoW { get; protected set; }

		private bool manualChange = false;

		public virtual bool HasChanges {
			get { return manualChange || UoW.HasChanges; }
			set { manualChange = value; }
		}

		public virtual bool HasCustomCancellationConfirmationDialog { get; private set; }

		public virtual Func<int> CustomCancellationConfirmationDialogFunc { get; private set; }

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public abstract bool Save();

		public void SaveAndClose()
		{
			Save();
			OnCloseTab(false, CloseSource.Save);
		}

		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}
	}
}
