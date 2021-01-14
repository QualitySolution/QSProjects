using System;
using System.ComponentModel;
using Gtk;
using QS.Updater.DB.ViewModels;
using QS.Utilities;
using QS.Views.Dialog;

namespace QS.Updater.DB.Views
{
	public partial class UpdateProcessView : DialogViewBase<UpdateProcessViewModel>
	{
		static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public UpdateProcessView(UpdateProcessViewModel viewModel) : base(viewModel)
		{
			this.Build();

			entryFileName.Binding.AddBinding(viewModel, v => v.FileName, w => w.Text).InitializeFromSource();
			viewModel.OperationProgress = progressbarOperation;
			viewModel.TotalProgress = progressbarTotal;
			checkCreateBackup.Binding.AddBinding(viewModel, v => v.NeedCreateBackup, w => w.Active).InitializeFromSource();
			textviewLog.Binding.AddBinding(viewModel, v => v.CommandsLog, w => w.Buffer.Text).InitializeFromSource();
			viewModel.PropertyChanged += ViewModel_PropertyChanged;
			buttonExecute.Binding.AddBinding(viewModel, v => v.ButtonExcuteSensitive, w => w.Sensitive).InitializeFromSource();
		}

		void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			GtkHelper.WaitRedraw();
		}

		protected void OnCheckCreateBackupToggled (object sender, EventArgs e)
		{
			entryFileName.Sensitive = buttonFileChooser.Sensitive = checkCreateBackup.Active;
		}

		protected void OnButtonFileChooserClicked (object sender, EventArgs e)
		{
			FileChooserDialog fc =
				new FileChooserDialog ("Укажите файл резервной копии",
					(Window)this.Toplevel,
					FileChooserAction.Save,
					"Отмена", ResponseType.Cancel,
					"Сохранить", ResponseType.Accept);
			fc.SetFilename (ViewModel.FileName);
			fc.Show (); 
			if (fc.Run () == (int)ResponseType.Accept) {
				ViewModel.FileName = fc.Filename;
			}
			fc.Destroy ();
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Cancel();
		}

		protected void OnButtonExecuteClicked(object sender, EventArgs e)
		{
			buttonExecute.Sensitive = false;
			ViewModel.Execute();
			buttonExecute.Sensitive = true;
		}
	}
}