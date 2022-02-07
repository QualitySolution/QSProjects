using QS.DomainModel.UoW;
using QS.Navigation;
using QS.HistoryLog.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using QS.Project.Domain;

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
			PeriodStartDate = DateTime.Today;
			PeriodEndDate = DateTime.Today.AddDays(1).AddTicks(-1);

		}
		#region  Filter
		private IList<UserBase> users;
		public IList<UserBase> Users {
			get => users ?? (users = UoW.Session.QueryOver<UserBase>().List());

			set { SetField(ref users, value); }
		}
		private UserBase selectedUser;
		public UserBase SelectedUser {
			get => selectedUser;
			set { SetField(ref selectedUser, value); }
		}
		private List<HistoryObjectDesc> changeObjects;
		public List<HistoryObjectDesc> ChangeObjects {
			get => changeObjects ?? (changeObjects = HistoryMain.TraceClasses.OrderBy(x => x.DisplayName)?.ToList());
			set { SetField(ref changeObjects, value); }
		}
		private HistoryObjectDesc selectedChangeObject;
		public HistoryObjectDesc SelectedChangeObject {
			get => selectedChangeObject;
			set { SetField(ref selectedChangeObject, value);
					OnPropertyChanged(nameof(HistoryField));
			}
		}
		private EntityChangeOperation? changeOperation;
		public EntityChangeOperation? ChangeOperation {
			get => changeOperation;
			set {SetField(ref changeOperation, value);}
		}
		public IEnumerable<HistoryFieldDesc> HistoryField {
			get {
				return SelectedChangeObject?.TracedProperties ?? new List<HistoryFieldDesc>();
			}
		}
		private DateTime periodStartDate;
		public DateTime PeriodStartDate {
			get => periodStartDate;
			set { SetField(ref periodStartDate, value); }
		}
		private DateTime periodEndDate;
		public DateTime PeriodEndDate {
			get => periodEndDate;
			set { SetField(ref periodEndDate, value); }
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
	}
}
