using System;
using System.ComponentModel;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.Navigation;
using QS.Utilities;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;

namespace QS.Views.Dialog
{
	public class SavedDialogViewBase<TViewModel> : DialogViewBase<TViewModel>
		where TViewModel : UowDialogViewModelBase
	{
		public SavedDialogViewBase(TViewModel viewModel) : base(viewModel)
		{
		}

		private Button saveButton, cancelButton;
		private bool busyStateApplied;
		private bool saveButtonSensitiveBeforeBusy;
		private bool cancelButtonSensitiveBeforeBusy;
		
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
			SetupISaveCancelManagement();
			ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
			ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
			UpdateBusyState();
		}

		private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.IsBusy)
				|| e.PropertyName == nameof(ViewModel.CanCancelBusyOperation))
				UpdateBusyState();
		}

		private void UpdateBusyState()
		{
			if(ViewModel.IsBusy) {
				if(!busyStateApplied) {
					saveButtonSensitiveBeforeBusy = saveButton?.Sensitive ?? false;
					cancelButtonSensitiveBeforeBusy = cancelButton?.Sensitive ?? false;
					busyStateApplied = true;
				}

				if(saveButton != null)
					saveButton.Sensitive = false;
				if(cancelButton != null)
					cancelButton.Sensitive = ViewModel.CanCancelBusyOperation;
				return;
			}

			if(!busyStateApplied)
				return;

			if(saveButton != null)
				saveButton.Sensitive = saveButtonSensitiveBeforeBusy;
			if(cancelButton != null)
				cancelButton.Sensitive = cancelButtonSensitiveBeforeBusy;
			busyStateApplied = false;
		}

		/// <summary>
		/// Настраивает кнопки сохранения и отмены если ViewModel реализует интерфейс ISaveCancelManagement
		/// Интерфейс позволяет ViewModel управлять состоянием кнопок
		/// </summary>
		private void SetupISaveCancelManagement() {
			if(ViewModel is ISaveCancelManagement saveCancelManagement) {
				if(saveButton is yButton yButtonSave)
					yButtonSave.Binding.AddBinding(saveCancelManagement, v => v.SaveButtonVisible, w => w.Visible).InitializeFromSource();
				else if(saveButton != null)
					saveButton.Visible = saveCancelManagement.SaveButtonVisible;
				if(cancelButton is yButton yButtonCancel)
					yButtonCancel.Binding.AddBinding(saveCancelManagement, v => v.CancelButtonLabel, w => w.Label).InitializeFromSource();
				else if(cancelButton != null)
					cancelButton.Label = saveCancelManagement.CancelButtonLabel;
			}
		}
		
		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			ViewModel.SaveAndClose();
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			if(ViewModel.IsBusy) {
				ViewModel.RequestCancelBusyOperation();
				return;
			}

			ViewModel.Close(false, CloseSource.Cancel);
		}
	}
}
