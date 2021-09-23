using System;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Tdi;

namespace QS.ViewModels
{
	public abstract class DialogTabViewModelBase : UoWTabViewModelBase, ITdiDialog, IDisposable
	{
		protected DialogTabViewModelBase(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigation) : base(unitOfWorkFactory, interactiveService, navigation)
		{
		}

		#region ITdiDialog implementation

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
				Close(false, CloseSource.Save);
				return true;
			}

			SaveUoW();
			Close(false, CloseSource.Save);
			return true;
		}

		private void SaveUoW()
		{
			UoW.Save();
			if(UoW.RootObject != null) {
				EntitySaved?.Invoke(this, new EntitySavedEventArgs(UoW.RootObject));
			}
		}
		
		#endregion
	}
}
