using System;
using System.Linq;
using Gtk;
using QS.Navigation;
using QS.Utilities;
using QS.ViewModels.Dialog;

namespace QS.Views.Dialog
{
	public class SavedDialogViewBase<TViewModel> : DialogViewBase<TViewModel>
		where TViewModel : UowDialogViewModelBase
	{
		public SavedDialogViewBase(TViewModel viewModel) : base(viewModel)
		{
		}

		private Button saveButton, cancelButton;
		
		/// <summary>
		/// Метод можно вызывать в конструкторе вьюшки для автоматической подписки на кнопки buttonSave и buttonCancel
		/// </summary>
		protected void CommonButtonSubscription()
		{
			saveButton = GtkHelper.EnumerateAllChildren(this).OfType<Button>().FirstOrDefault(x => x.Name == "buttonSave");
			if(saveButton != null) {
				saveButton.Clicked -= OnButtonSaveClicked;
				saveButton.Clicked += OnButtonSaveClicked;
			}
			cancelButton = GtkHelper.EnumerateAllChildren(this).OfType<Button>().FirstOrDefault(x => x.Name == "buttonCancel");
			if(cancelButton != null) {
				cancelButton.Clicked -= OnButtonCancelClicked;
				cancelButton.Clicked += OnButtonCancelClicked;
			}
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			saveButton.Sensitive = false;
			if(!ViewModel.SaveAndClose())
				saveButton.Sensitive = true;
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(false, CloseSource.Cancel);
		}
	}
}
