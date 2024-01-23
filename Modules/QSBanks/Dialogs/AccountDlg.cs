using System;
using System.ComponentModel;
using Gamma.Utilities;
using NHibernate.Criterion;
using NLog;
using QS.Banks.Domain;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Validation;

namespace QSBanks
{
	public partial class AccountDlg : TdiTabBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

        private readonly IUnitOfWork uow;
        private Account resultAccount = null;
		private Account entity = null;

		public event EventHandler<Account> AccountSaved;

		public AccountDlg (IUnitOfWork uow, Account account)
		{
			this.Build();
            this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
            resultAccount = account;
			entity = account.CreateCopy();
			ConfigureDlg();

            buttonSave.Clicked += ButtonSave_Clicked;
            buttonCancel.Clicked += ButtonCancel_Clicked;
		}

        private void ConfigureDlg ()
		{
			dataentryrefBank.SubjectType = typeof(Bank);
			dataentryNumber.ValidationMode = QSWidgetLib.ValidationType.numeric;
			dataentryNumber.Binding.AddBinding(entity, e => e.Number, w => w.Text).InitializeFromSource();
			dataentryrefBank.ItemsCriteria = uow.Session.CreateCriteria<Bank> ()
				.Add (Restrictions.Eq ("Deleted", false));
			ycomboboxCorAccount.Binding.AddBinding(entity, e => e.BankCorAccount, w => w.SelectedItem).InitializeFromSource();
			ycomboboxCorAccount.RenderTextFunc = (x) => x is CorAccount ? (x as CorAccount).CorAccountNumber : "";
			ycomboboxCorAccount.ItemsList = entity.InBank?.ObservableCorAccounts;
			ycomboboxCorAccount.SelectedItem = entity.BankCorAccount;
			dataentryrefBank.Binding.AddBinding(entity, e => e.InBank, w => w.Subject).InitializeFromSource();
			entity.PropertyChanged += OnAccountPropertyChanged;

			dataentryName.Binding.AddBinding(entity, e => e.Name, w => w.Text).InitializeFromSource();
			entity.PropertyChanged += Entity_PropertyChanged;
			UpdateBankInfo();
		}

		void Entity_PropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == entity.GetPropertyName(x => x.InBank))
			{
				UpdateBankInfo();
			}
		}

		void UpdateBankInfo()
		{
			if(entity.InBank == null)
				datalabelBik.Text = datalabelRegion.Text = datalabelCity.Text = String.Empty;
			else
			{
				datalabelBik.Text = entity.InBank.Bik;
				datalabelRegion.Text = entity.InBank.RegionText;
				datalabelCity.Text = entity.InBank.City;
				ycomboboxCorAccount.ItemsList = entity.InBank.ObservableCorAccounts;
				entity.BankCorAccount = entity.InBank.DefaultCorAccount;
				ycomboboxCorAccount.SelectedItem = entity.BankCorAccount;
			}
		}

		void OnAccountPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == PropertyUtil.GetName<Account> (a => a.InBank)) {
				labelInactive.Markup = entity.Inactive ? "<span foreground=\"red\">Данный счет находится в более не существующем банке.</span>" : "";
				labelInactive.Visible = entity.Inactive;
			}
		}

		private void ButtonSave_Clicked(object sender, EventArgs e)
		{
            if(Save())
            {
				OnCloseTab(false, QS.Navigation.CloseSource.Save);
            }
		}

		public bool Save()
		{
			var validator = ServicesConfig.ValidationService;
			if (!validator.Validate(entity))
				return false;
			logger.Info ("Сохраняем счет организации...");
			SetToResultAccount();
			AccountSaved?.Invoke(this, resultAccount);
			logger.Info ("Ok");
			return true;
		}

		private void SetToResultAccount()
        {
			resultAccount.Name = entity.Name;
			resultAccount.Number = entity.Number;
			resultAccount.InBank = entity.InBank;
			resultAccount.BankCorAccount = entity.BankCorAccount;
			resultAccount.Code1c = entity.Code1c;
			resultAccount.IsDefault = entity.IsDefault;
			resultAccount.Inactive = entity.Inactive;
		}

		private void ButtonCancel_Clicked(object sender, EventArgs e)
		{
			OnCloseTab(false, QS.Navigation.CloseSource.Cancel);
		}
	}
}

