using System;
using Gamma.GtkWidgets;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Validation;

namespace QSBanks
{
	public partial class BankDlg : QS.Dialog.Gtk.EntityDialogBase<Bank>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public BankDlg (int id)
		{
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<Bank> (id);
			ConfigureDlg ();
		}

		public BankDlg (Bank sub) : this(sub.Id) {}

		private void ConfigureDlg ()
		{
			buttonSave.Sensitive = dataentryBik.IsEditable = dataentryCity.IsEditable = 
				dataentryName.IsEditable = false;
			if (UoWGeneric.Root.Deleted) {
				labelDeleted.Markup = "<span foreground=\"red\">Банк удалён.</span>";
			}

			dataentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			dataentryBik.Binding.AddBinding(Entity, e => e.Bik, w => w.Text).InitializeFromSource();
			comboboxDefaultCorAccount.Binding.AddBinding(Entity, e => e.DefaultCorAccount, w => w.SelectedItem).InitializeFromSource();
			comboboxDefaultCorAccount.RenderTextFunc = (x) => x is CorAccount ? (x as CorAccount).CorAccountNumber : "-";
			comboboxDefaultCorAccount.ItemsList = Entity.ObservableCorAccounts;
			comboboxDefaultCorAccount.SelectedItem = Entity.DefaultCorAccount;
			dataentryCity.Binding.AddBinding(Entity, e => e.City, w => w.Text).InitializeFromSource();
			labelRegion.Binding.AddBinding(Entity, e => e.RegionText, w => w.Text).InitializeFromSource();

			treeViewCorAccounts.ColumnsConfig = ColumnsConfigFactory.Create<CorAccount>()
				.AddColumn("Счет").AddTextRenderer(x => x.CorAccountNumber)
				.Finish();
			treeViewCorAccounts.ItemsDataSource = Entity.ObservableCorAccounts;
		}

		public override bool Save ()
		{ //FIXME Если функция не понадобится в других проектах возможно ее нужно удалить. 
			var validator = ServicesConfig.ValidationService;
			if (!validator.Validate(Entity))
				return false;

			logger.Info ("Сохраняем банк...");
			try {
				UoWGeneric.Save();
			} catch (Exception ex) {
				logger.Error(ex, "Не удалось записать банк.");
				return false;
			}
			logger.Info ("Ok");
			return true;
		}
	}
}

