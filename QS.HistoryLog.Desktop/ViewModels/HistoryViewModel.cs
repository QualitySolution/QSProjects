using QS.DomainModel.UoW;
using QS.Navigation;
using QS.HistoryLog.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;

namespace QS.HistoryLog.ViewModels
{
	public class HistoryViewModel : UowDialogViewModelBase
	{
		public HistoryViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator = null,
			string UoWTitle = null) : base(unitOfWorkFactory, navigation, validator, UoWTitle)
		{
			Title = "Просмотр журнала изменений";
			timeBegin = DateTime.Today;
			
		}

		#region  Filter

		private List<IDomainObject> users;
		public List<IDomainObject> Users {
			get => users;
		}
		private IDomainObject selectedUser;
		public IDomainObject SelectedUser {
			get => selectedUser;
			set { SetField(ref selectedUser, value); }
		}
		private List<IDomainObject> changeObjects;
		public List<IDomainObject> ChangeObjects {
			get => changeObjects;
		}
		private EntityChangeOperation changeOperation;
		public EntityChangeOperation ChangeOperation {
			get => changeOperation;
			set { SetField(ref changeOperation, value); }
		}
		private DateTime timeBegin;
		public DateTime TimeBegin {
			get => timeBegin;
			set { SetField(ref timeBegin, value); }
		}
		private DateTime timeEnd;
		public DateTime TimeEnd {
			get => timeEnd;
			set { SetField(ref timeEnd, value); }
		}
		private string searchByName;
		public string SsearchByName {
			get => searchByName;
			set { SetField(ref searchByName, value); }
		}
		private string searchById;
		public string SearchById {
			get => searchById;
			set { SetField(ref searchById, value); }
		}
		private string searchByChanged;
		public string SearchByChanged {
			get => searchByChanged;
			set { SetField(ref searchByChanged, value); }
		}
		#endregion

		#region Date

		#endregion
	}
}
