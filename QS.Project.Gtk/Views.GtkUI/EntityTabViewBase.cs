using System;
using System.ComponentModel;
using System.Linq;
using Gtk;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Utilities.GtkUI;
using QS.ViewModels;

namespace QS.Views.GtkUI
{
	public class EntityTabViewBase<TViewModel, TEntity> : TabViewBase<TViewModel>
		where TViewModel : EntityTabViewModelBase<TEntity>
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
	{
		protected TEntity Entity => ViewModel.Entity;

		public EntityTabViewBase(TViewModel viewModel): base(viewModel)
		{

		}

		/// <summary>
		/// Метод можно вызывать в конструкторе вьюшки для автоматической подписки на кнопки buttonSave и buttonCancel
		/// </summary>
		protected void CommonButtonSubscription()
		{
			var saveButton = GtkHelper.EnumerateAllChildren(this).OfType<Button> ().FirstOrDefault (x => x.Name == "buttonSave");
			if(saveButton != null)
			{
				saveButton.Clicked -= OnButtonSaveClicked;
				saveButton.Clicked += OnButtonSaveClicked;
			}
			var cancelButton = GtkHelper.EnumerateAllChildren (this).OfType<Button> ().FirstOrDefault (x => x.Name == "buttonCancel");
			if (cancelButton != null) {
				cancelButton.Clicked -= OnButtonCancelClicked;
				cancelButton.Clicked += OnButtonCancelClicked;
			}
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			ViewModel.SaveAndClose();
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			ViewModel.Close(false, CloseSource.Cancel);
		}
	}
}
