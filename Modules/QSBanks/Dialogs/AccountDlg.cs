using System;
using System.ComponentModel;
using Gamma.Utilities;
using Gtk;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSValidation;

namespace QSBanks
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AccountDlg : OrmGtkDialogBase<Account>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

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
			dataentryNumber.ValidationMode = QSWidgetLib.ValidationType.numeric;
			dataentryNumber.Binding.AddBinding(Entity, e => e.Number, w => w.Text).InitializeFromSource();
			dataentryrefBank.ItemsCriteria = UoW.Session.CreateCriteria<Bank> ()
				.Add (Restrictions.Eq ("Deleted", false));
			dataentryrefBank.Binding.AddBinding(Entity, e => e.InBank, w => w.Subject).InitializeFromSource();
			Entity.PropertyChanged += OnAccountPropertyChanged;

			dataentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			Entity.PropertyChanged += Entity_PropertyChanged;
			UpdateBankInfo();
		}

		void Entity_PropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName ==  Entity.GetPropertyName(x => x.InBank))
			{
				UpdateBankInfo();
			}
		}

		void UpdateBankInfo()
		{
			if(Entity.InBank == null)
				datalabelBik.Text = datalabelRegion.Text = datalabelCity.Text = String.Empty;
			else
			{
				datalabelBik.Text = Entity.InBank.Bik;
				datalabelRegion.Text = Entity.InBank.RegionText;
				datalabelCity.Text = Entity.InBank.City;
			}
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
	}
}

