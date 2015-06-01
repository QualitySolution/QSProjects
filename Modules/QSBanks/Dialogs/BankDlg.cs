using System;
using NHibernate;
using System.Data.Bindings;
using QSTDI;
using QSOrmProject;
using QSValidation;

namespace QSBanks
{
	public partial class BankDlg : OrmGtkDialogBase<Bank>
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
			datatableInfo.DataSource = subjectAdaptor;
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
				logger.ErrorException("Не удалось записать банк.", ex);
				return false;
			}
			logger.Info ("Ok");
			return true;
		}
	}
}

