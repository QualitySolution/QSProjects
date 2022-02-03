using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.HistoryLog.Domain;
using QS.Utilities;
using QS.Validation;
using QS.ViewModels.Dialog;

namespace QS.HistoryLog.ViewModels
{
	public class HistoryViewModel : UowDialogViewModelBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		private int pageSize = 250;
		private int takenRows = 0;
		private bool takenAll = false;
		bool canUpdate = false;
		
		public HistoryViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator = null,
			string UoWTitle = null) : base(unitOfWorkFactory, navigation, validator, UoWTitle)
		{
			Title = "Просмотр журнала изменений";
		}

		#region  Filter

		private DateTime? StartSelectPeriod { get; set; }
		private DateTime? EndSelectPeriod { get; set; }
		public object HistoryObjectDesc { get; set; }
		public object Works { get; set; }
		public  object Action { get; set; }
		public object SearchEntity {get; set; }
		public  object Property { get; set; }
		
		#endregion

		private List<ChangedEntity> changedEntities;
		public  List<ChangedEntity> ChangedEntities {
			get
			{
				if (changedEntities == null)
					changedEntities = FillChangedEntities();
				return changedEntities;
			}
			set => value = changedEntities;
		}
		List<ChangedEntity> FillChangedEntities()
		{
			ChangeSet changeSetAlias = null;
			
			var query = UoW.Session.QueryOver<ChangedEntity>()
				.JoinAlias(ce => ce.ChangeSet, () => changeSetAlias)
				.Fetch(SelectMode.Fetch, x => x.ChangeSet)
				.Fetch(SelectMode.Fetch, x => x.ChangeSet.User);

			if(StartSelectPeriod != null)
				query.Where(ce => ce.ChangeTime >= StartSelectPeriod && ce.ChangeTime < EndSelectPeriod);

			//if(false)
				//query.Where(ce => ce.EntityClassName == selectedClassType.ObjectName);

			//if(false)
				//query.Where(() => changeSetAlias.User.Id == 1);

			//if(false)
				//query.Where(ce => ce.Operation == (EntityChangeOperation)Action);

			//if(!string.IsNullOrWhiteSpace((string)SearchEntity)) {
				//var pattern = $"%{SearchEntity}%";
				//query.Where(ce => ce.EntityTitle.IsLike(pattern));
			//}

			//if(!string.IsNullOrWhiteSpace((string)SearchEntity)) {
				//if(int.TryParse((string)SearchEntity, out int id))
					//query.Where(ce => ce.EntityId == id);
			//}

			//if(!string.IsNullOrWhiteSpace((string)SearchEntity) || Property is HistoryFieldDesc) {
				//FieldChange fieldChangeAlias = null;
				//query.JoinAlias(ce => ce.Changes, () => fieldChangeAlias);

				//if(Property is HistoryFieldDesc selectedProperty)
					//query.Where(() => fieldChangeAlias.Path == selectedProperty.FieldName);

				//if(!string.IsNullOrWhiteSpace((string)SearchEntity)) {
					//var pattern = $"%{SearchEntity}%";
					//query.Where(
						//() => fieldChangeAlias.OldValue.IsLike(pattern) || fieldChangeAlias.NewValue.IsLike(pattern)
					//);
				//}
			//}

			return (List<ChangedEntity>) query.OrderBy(x => x.ChangeTime).Desc
				.Skip(takenRows)
				.Take(pageSize)
				.List();

		}
		
		public void UpdateJournal(bool nextPage = false)
		{
			DateTime startTime = DateTime.Now;
			if(!nextPage) {
				takenRows = 0;
				takenAll = false;
			}

			if(!canUpdate)
				return;

			logger.Info("Получаем журнал изменений{0}...", takenRows > 0 ? $"({takenRows}+)" : "");
			ChangeSet changeSetAlias = null;

			if(takenRows > 0) {
				changedEntities.AddRange(changedEntities);
			} else {
				changedEntities = changedEntities;
			}

			if(changedEntities.Count < pageSize)
				takenAll = true;

			takenRows = changedEntities.Count;

			logger.Debug("Время запроса {0}", DateTime.Now - startTime);
			logger.Info(NumberToTextRus.FormatCase(changedEntities.Count, "Загружено изменение {0}{1} объекта.", "Загружено изменение {0}{1} объектов.", "Загружено изменение {0}{1} объектов.", takenAll ? "" : "+"));
		}
	}
}
