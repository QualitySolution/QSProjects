using System;
using QS.Tdi;
using QS.DomainModel.UoW;
using QS.Services;

namespace QS.ViewModels
{
	public abstract class UoWTabViewModelBase : TabViewModelBase, ITdiDialog, IDisposable
	{
		protected UoWTabViewModelBase(IInteractiveService interactiveService) : base(interactiveService)
		{
		}

		#region ITdiDialog implementation

		public virtual IUnitOfWork UoW { get; set; }

		private bool manualChange = false;

		public virtual bool HasChanges {
			get { return manualChange || UoW.HasChanges; }
			set { manualChange = value; }
		}

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public void SaveAndClose()
		{
			Save(true);
		}

		public bool Save()
		{
			return Save(false);
		}

		public virtual bool Save(bool needClose)
		{
			if(!needClose) {
				SaveUoW();
				return true;
			}

			if(!HasChanges) {
				Close(false);
				return true;
			}

			SaveUoW();
			Close(false);
			return true;
		}

		private void SaveUoW()
		{
			UoW.Save();
			if(UoW.RootObject != null) {
				EntitySaved?.Invoke(this, new EntitySavedEventArgs(UoW.RootObject));
			}
			return;
		}

		public virtual void Dispose()
		{
			if(UoW != null) {
				UoW.Dispose();
			}
		}

		public override void Close(bool askSave)
		{
			base.Close(askSave);
			Dispose();
		}

		#endregion
	}
}
