using System;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSValidation;

namespace QSBanks
{
	public partial class BankDlg : EntityDialogBase<Bank>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public BankDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Bank> (id);
			ConfigureDlg ();
		}

		public BankDlg (Bank sub) : this(sub.Id) {}

		private void ConfigureDlg ()
		{
			buttonSave.Sensitive = dataentryBik.IsEditable = dataentryCity.IsEditable = 
				dataentryCorAccount.IsEditable = dataentryName.IsEditable = false;
			if (UoWGeneric.Root.Deleted) {
				labelDeleted.Markup = "<span foreground=\"red\">Банк удалён.</span>";
			}

			dataentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			dataentryBik.Binding.AddBinding(Entity, e => e.Bik, w => w.Text).InitializeFromSource();
			dataentryCorAccount.Binding.AddBinding(Entity, e => e.CorAccount, w => w.Text).InitializeFromSource();
			dataentryCity.Binding.AddBinding(Entity, e => e.City, w => w.Text).InitializeFromSource();
			labelRegion.Binding.AddBinding(Entity, e => e.RegionText, w => w.Text).InitializeFromSource();
		}

		public override bool Save ()
		{ //FIXME Если функция не понадобится в других проектах возможно ее нужно удалить. 
			var valid = new QSValidator<Bank> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
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

