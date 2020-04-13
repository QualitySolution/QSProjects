using System;
using System.Linq;
using Gtk;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Utilities.GtkUI;
using QS.ViewModels.Dialog;

namespace QS.Views.Dialog
{
	public abstract class EntityDialogViewBase<TViewModel, TEntity> : DialogViewBase<TViewModel>
		where TViewModel : EntityDialogViewModelBase<TEntity>
		where TEntity : class, IDomainObject, new()
	{
		protected TEntity Entity => ViewModel.Entity;

		protected EntityDialogViewBase(TViewModel viewModel): base(viewModel)
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
