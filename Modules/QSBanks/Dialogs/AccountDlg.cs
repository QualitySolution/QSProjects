using System;
using NLog;
using System.Data.Bindings;
using QSOrmProject;
using Gtk;
using QSValidation;
using NHibernate.Criterion;
using System.ComponentModel;

namespace QSBanks
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AccountDlg : OrmGtkDialogBase<Account>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		private Adaptor adaptorBank = new Adaptor ();

		IParentReference<Account> parentReference;

		public IParentReference<Account> ParentReference {
			set {
				parentReference = value;
			}
			get { return parentReference; }
		}

		public AccountDlg (IParentReference<Account> parentReference)
		{
			this.Build ();
			ParentReference = parentReference;
			UoWGeneric = ParentReference.CreateUoWForNewItem ();
			ConfigureDlg ();
		}

		public AccountDlg (IParentReference<Account> parentReference, Account sub)
		{
			this.Build ();
			ParentReference = parentReference;
			UoWGeneric = ParentReference.CreateUoWForItem (sub);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			dataentryrefBank.SubjectType = typeof(Bank);
			datatableMain.DataSource = subjectAdaptor;
			datatableBank.DataSource = adaptorBank;
			dataentryNumber.ValidationMode = QSWidgetLib.ValidationType.numeric;
			dataentryrefBank.ItemsCriteria = Session.CreateCriteria<Bank> ()
				.Add (Restrictions.Eq ("Deleted", false));
			(Entity as INotifyPropertyChanged).PropertyChanged += OnAccountPropertyChanged;
			datalabelBik.Text = datalabelRegion.Text = datalabelCity.Text = String.Empty;
		}

		void OnAccountPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == PropertyUtil.GetName<Account> (a => a.InBank)) {
				labelInactive.Markup = Entity.Inactive ? "<span foreground=\"red\">Данный счет находится в более не существующем банке.</span>" : "";
				labelInactive.Visible = Entity.Inactive;
			}
		}

		public override bool Save ()
		{
			var valid = new QSValidator<Account> (Entity);
			if (valid.RunDlgIfNotValid ((Window)this.Toplevel))
				return false;
			logger.Info ("Сохраняем счет организации...");
			UoWGeneric.Save ();
			logger.Info ("Ok");
			return true;
		}

		protected void OnDataentryrefBankChanged (object sender, EventArgs e)
		{
			adaptorBank.Target = Entity.InBank;
		}
	}
}

