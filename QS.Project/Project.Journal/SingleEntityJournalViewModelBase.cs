﻿using System;
using System.ComponentModel;
using NHibernate;
using QS.DomainModel.Entity;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.Tdi;

namespace QS.Project.Journal
{
	public abstract class SingleEntityJournalViewModelBase<TEntity, TEntityTab , TNode> : EntityJournalViewModelBase<TNode> , IEntityAutocompleteSelector
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		where TNode : JournalEntityNodeBase
		where TEntityTab : class, ITdiTab
	{
		protected readonly ICommonServices commonServices;
		public Type EntityType { get; }

		protected SingleEntityJournalViewModelBase(ICommonServices commonServices) : base(commonServices)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			EntityType = typeof(TEntity);
			var config = RegisterEntity(ItemsSourceQueryFunction);
			config.AddDocumentConfiguration("Добавить", CreateDialogFunction, OpenDialogFunction, (node) => node.EntityType == typeof(TEntity)).FinishConfiguration();
			FinishJournalConfiguration();

			if(!EntityConfigs[EntityType].PermissionResult.CanRead) {
				AbortOpening($"Нет прав для просмотра документов типа: {EntityType.GetSubjectName()}", "Невозможно открыть журнал");
			}
		}

		/// <summary>
		/// Функция формирования запроса.
		/// ВАЖНО: Необходимо следить чтобы в запросе не было INNER JOIN с ORDER BY и LIMIT.
		/// Иначе запрос с LIMIT будет выполнятся также медленно как и без него.
		/// В таких случаях необходимо заменять на другой JOIN и условие в WHERE
		/// </summary>
		/// <value>The items source query function.</value>
		protected abstract Func<IQueryOver<TEntity>> ItemsSourceQueryFunction { get; }

		protected abstract Func<TEntityTab> CreateDialogFunction { get; }

		protected abstract Func<TNode, TEntityTab> OpenDialogFunction { get; }

		public bool IsActive => UoW.IsAlive;

		public void SearchValues(params string[] values)
		{
			Search.SearchValues = values;
		}
	}
}
