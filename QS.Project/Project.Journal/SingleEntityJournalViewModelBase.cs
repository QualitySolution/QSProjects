﻿using System;
using System.ComponentModel;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.Tdi;

namespace QS.Project.Journal
{
	public abstract class SingleEntityJournalViewModelBase<TEntity, TEntityTab , TNode> : EntitiesJournalViewModelBase<TNode> , IEntityAutocompleteSelector
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		where TNode : JournalEntityNodeBase
		where TEntityTab : class, ITdiTab
	{
		protected readonly ICommonServices commonServices;

		public event EventHandler ListUpdated;

		public Type EntityType { get; }

		protected SingleEntityJournalViewModelBase(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices,
			bool hideJournalForOpenDialog = false, bool hideJournalForCreateDialog = false, INavigationManager navigation = null) : base(unitOfWorkFactory, commonServices, navigation)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			EntityType = typeof(TEntity);
			var config = RegisterEntity(ItemsSourceQueryFunction, ItemsCountFunction);
			config.AddDocumentConfiguration("Добавить", CreateDialogFunction, OpenDialogFunction, (node) => node.EntityType == typeof(TEntity),
				new JournalParametersForDocument { HideJournalForCreateDialog = hideJournalForCreateDialog, HideJournalForOpenDialog = hideJournalForOpenDialog})
				.FinishConfiguration();
			FinishJournalConfiguration();

			if(!EntityConfigs[EntityType].PermissionResult.CanRead) {
				AbortOpening($"Нет прав для просмотра документов типа: {EntityType.GetSubjectName()}", "Невозможно открыть журнал");
			}
			DataLoader.ItemsListUpdated += (sender, e) => ListUpdated?.Invoke(sender, e);
		}

		/// <summary>
		/// Функция формирования запроса.
		/// ВАЖНО: Необходимо следить чтобы в запросе не было INNER JOIN с ORDER BY и LIMIT.
		/// Иначе запрос с LIMIT будет выполнятся также медленно как и без него.
		/// В таких случаях необходимо заменять на другой JOIN и условие в WHERE
		/// </summary>
		/// <value>The items source query function.</value>
		protected abstract Func<IUnitOfWork, IQueryOver<TEntity>> ItemsSourceQueryFunction { get; }
		
		/// <summary>
		/// Кастомная функция подсчёта кол-ва элементов
		/// </summary>
		protected virtual Func<IUnitOfWork, int> ItemsCountFunction { get; }

		protected abstract Func<TEntityTab> CreateDialogFunction { get; }

		protected abstract Func<TNode, TEntityTab> OpenDialogFunction { get; }

		public void SearchValues(params string[] values)
		{
			Search.SearchValues = values;
		}
	}
}
