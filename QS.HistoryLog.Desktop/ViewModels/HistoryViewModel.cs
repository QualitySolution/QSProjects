using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog.Domain;
using QS.Navigation;
using QS.Project.Domain;
using QS.Utilities;
using QS.Validation;
using QS.ViewModels.Dialog;

namespace QS.HistoryLog.ViewModels
{
	public class HistoryViewModel : UowDialogViewModelBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		private const int entityBatchSize = 300;
		private int EntitiesTaken = 0;

		public HistoryViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator = null,
			string UoWTitle = null) : base(unitOfWorkFactory, navigation, validator, UoWTitle)
		{
			Title = "Просмотр журнала изменений";
			changedEntities = new List<ChangedEntity>();
		}
		#region  Filter
		private IList<UserBase> users;
		public IList<UserBase> Users {
			get => users ?? (users = UoW.Session.QueryOver<UserBase>().List());
			set => SetField(ref users, value);
		}
		private UserBase selectedUser;
		public UserBase SelectedUser {
			get => selectedUser;
			set => SetField(ref selectedUser, value);
		}
		private List<HistoryObjectDesc> traceClasses;
		public List<HistoryObjectDesc> TraceClasses {
			get => traceClasses ?? (traceClasses = HistoryMain.TraceClasses.OrderBy(x => x.DisplayName)?.ToList());
			set => SetField(ref traceClasses, value);
		}
		private HistoryObjectDesc selectedTraceClass;
		[PropertyChangedAlso(nameof(TracedProperties))]
		public HistoryObjectDesc SelectedTraceClass {
			get => selectedTraceClass;
			set => SetField(ref selectedTraceClass, value);
		}
		private EntityChangeOperation? operation;
		public EntityChangeOperation? Operation {
			get => operation;
			set => SetField(ref operation, value);
		}
		public IEnumerable<HistoryFieldDesc> TracedProperties => SelectedTraceClass?.TracedProperties ?? new List<HistoryFieldDesc>();

		private HistoryFieldDesc selectedTracedProperties;
		public HistoryFieldDesc SelectedTracedProperties
		{
			get => selectedTracedProperties;
			set => SetField(ref selectedTracedProperties, value);
		}
		private DateTime? periodStartDate;
		public DateTime? PeriodStartDate {
			get {
				if(periodStartDate == null)
					periodStartDate = DateTime.Today;
				return periodStartDate;
			}
			set => SetField(ref periodStartDate, value);
		}
		private DateTime? periodEndDate;
		public DateTime? PeriodEndDate {
			get {
				if(periodEndDate == null)
					periodEndDate = DateTime.Today.AddDays(1).AddTicks(-1);
				return periodEndDate;
			}
			set => SetField(ref periodEndDate, value);
		}

		private string searchByName;
		public string SearchByName {
			get => searchByName;
			set => SetField(ref searchByName, value);
		}
		private string searchById;
		public string SearchById {
			get => searchById;
			set => SetField(ref searchById, value);
		}
		private string searchByChanged;
		public string SearchByChanged {
			get => searchByChanged;
			set => SetField(ref searchByChanged, value);
		}
		#endregion
		#region Query
		public bool DontRefresh = false;
		public bool HasUnloaded = true;
		public void UpdateChangedEntities(bool nextPage = false)
		{
			if(DontRefresh)
				return;
			DateTime startTime = DateTime.Now;
			if(!nextPage) {
				ChangedEntities.Clear();
				EntitiesTaken = 0;
				HasUnloaded = true;
			}
			logger.Info("Получаем журнал изменений{0}...", EntitiesTaken > 0 ? $"({EntitiesTaken}+)" : "");
			ChangeSet changeSetAlias = null;

			var query = UoW.Session.QueryOver<ChangedEntity>()
				.JoinAlias(ce => ce.ChangeSet, () => changeSetAlias)
				.Fetch(SelectMode.Fetch, x => x.ChangeSet)
				.Fetch(SelectMode.Fetch, x => x.ChangeSet.User);

			query.Where(ce => ce.ChangeTime >= PeriodStartDate && ce.ChangeTime < PeriodEndDate);
			
			if(SelectedUser != null)
				query.Where(() => changeSetAlias.User == selectedUser);
			if (SelectedTraceClass != null)
				query.Where(ce => ce.EntityClassName == SelectedTraceClass.ObjectName);
			if(Operation != null)
				query.Where(ce => ce.Operation == Operation);
			
			if(SelectedTracedProperties != null || !string.IsNullOrWhiteSpace(searchByChanged)) {
				FieldChange fieldChangeAlias = null;
				query.JoinAlias(ce => ce.Changes, () => fieldChangeAlias);
				
				if(SelectedTracedProperties != null)
					query.Where(() => fieldChangeAlias.Path == SelectedTracedProperties.FieldName);
				
				if (!string.IsNullOrWhiteSpace(searchByChanged)) {
					var pattern = $"%{searchByChanged}%";
					query.Where(
						() => fieldChangeAlias.OldValue.IsLike(pattern) || fieldChangeAlias.NewValue.IsLike(pattern)
					);
				}
			}
			if(!string.IsNullOrWhiteSpace(searchById)) {
				if(int.TryParse(searchById, out int id))
					query.Where(ce => ce.EntityId == id);
			}
			if (!string.IsNullOrWhiteSpace(SearchByName)) {
				var pattern = $"%{searchByName}%";
				query.Where(ce => ce.EntityTitle.IsLike(pattern));
			}

			var taked = query
				.OrderBy(x => x.ChangeTime).Desc
				.Skip(EntitiesTaken)
				.Take(entityBatchSize)
				.List();
			
			if(taked.Count < entityBatchSize)
				HasUnloaded = false;
			
			if (EntitiesTaken > 0)
				ChangedEntities.AddRange(taked);
			else
				ChangedEntities = taked.ToList();

			EntitiesTaken = ChangedEntities.Count;

			logger.Debug("Время запроса {0}", DateTime.Now - startTime);
			logger.Info(NumberToTextRus.FormatCase(changedEntities.Count, "Загружено изменение {0} объект.", "Загружено изменение {0} объекта.", "Загружено изменение {0} объектов."));
		}
		#endregion
		#region  Properties
		private List<ChangedEntity> changedEntities;
		public List<ChangedEntity> ChangedEntities {
			get => changedEntities;
			set => SetField(ref changedEntities, value);
		}
		private ChangedEntity selectedEntity;
		public ChangedEntity SelectedEntity {
			get => selectedEntity;
			set {
				SetField(ref selectedEntity, value);
				OnPropertyChanged(nameof(ChangesSelectedEntity));
			}
		}
		public IList<FieldChange> ChangesSelectedEntity => SelectedEntity == null ? new List<FieldChange>() : SelectedEntity.Changes;
		#endregion
	}
}
