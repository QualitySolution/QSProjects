using System;
using System.ComponentModel;
using NHibernate;
using QS.DomainModel.Config;
using QS.DomainModel.Entity;
using QS.Services;
using QS.Tdi;
using QS.Utilities.Text;

namespace QS.Project.Journal
{
	public abstract class SimpleEntityJournalViewModelBase : EntityJournalViewModelBase<CommonJournalNode>
	{
		public Type EntityType { get; }

		protected SimpleEntityJournalViewModelBase(Type entityType, IEntityConfigurationProvider entityConfigurationProvider, ICommonServices commonServices) : base(entityConfigurationProvider, commonServices)
		{
			EntityType = entityType;
		}

		protected void Register<TEntity, TEntityTab>(Func<IQueryOver<TEntity>> queryFunc, Func<TEntityTab> createDlgFunc, Func<CommonJournalNode, TEntityTab> openDlgFunc)
			where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
			where TEntityTab : class, ITdiTab
		{
			var config = RegisterEntity(queryFunc);
			config.AddDocumentConfiguration("Добавить", createDlgFunc, openDlgFunc, (node) => node.EntityType == typeof(TEntity)).FinishConfiguration();
			FinishJournalConfiguration();

			if(!EntityConfigs[EntityType].PermissionResult.CanRead) {
				AbortOpening($"Нет прав для просмотра документов типа: {EntityType.GetSubjectName()}", "Невозможно открыть журнал");
			}

			var names = EntityType.GetSubjectNames();
			if(names == null) {
				TabName = "Журнал";
			}
			TabName = names.NominativePlural.StringToTitleCase();
		}
	}
}
