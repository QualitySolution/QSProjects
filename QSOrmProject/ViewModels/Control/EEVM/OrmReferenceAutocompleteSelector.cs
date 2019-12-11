using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QSOrmProject;

namespace QS.ViewModels.Control.EEVM
{
	public class OrmReferenceAutocompleteSelector<TEntity> : IEntityAutocompleteSelector<TEntity>
	{
		readonly IUnitOfWork UoW;

		ICriteria ItemsCriteria;
		QueryOver ItemsQuery;

		#region Конструкторы

		public OrmReferenceAutocompleteSelector(IUnitOfWork unitOfWork)
		{
			UoW = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public OrmReferenceAutocompleteSelector(IUnitOfWork unitOfWork, ICriteria itemsCriteria)
		{
			UoW = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			ItemsCriteria = itemsCriteria ?? throw new ArgumentNullException(nameof(itemsCriteria));
		}

		public OrmReferenceAutocompleteSelector(IUnitOfWork unitOfWork, QueryOver itemsQuery)
		{
			UoW = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			ItemsQuery = itemsQuery ?? throw new ArgumentNullException(nameof(itemsQuery));
		}

		#endregion

		public event EventHandler<AutocompleteUpdatedEventArgs> AutocompleteLoaded;

		public TEntity GetEntityByNode(object node)
		{
			return (TEntity)node;
		}

		public string GetTitle(object node)
		{
			return DomainModel.Entity.DomainHelper.GetObjectTilte(node);
		}

		public void LoadAutocompletion(string[] searchText, int takeCount)
		{
			if(loadedItems == null)
				LoadAllItems();

			IList filtred;
			var map = OrmMain.GetObjectDescription(typeof(TEntity));
			var searchProvider = map?.TableView?.SearchProvider;

			if(searchProvider == null)
				filtred = loadedItems.Where(x => searchText.All(st => GetTitle(x).IndexOf(st, StringComparison.CurrentCultureIgnoreCase) > -1)).ToList();
			else
				filtred = loadedItems.Where(x => searchText.All(st => searchProvider.Match(x, st))).ToList();

			AutocompleteLoaded?.Invoke(this, new AutocompleteUpdatedEventArgs(filtred));
		}

		#region Внутреннее

		IList<TEntity> loadedItems;

		private void LoadAllItems()
		{
			if(ItemsQuery != null) {
				ItemsCriteria = ItemsQuery.DetachedCriteria.GetExecutableCriteria(UoW.Session);
			}
			else {
				if(ItemsCriteria == null)
					ItemsCriteria = UoW.Session.CreateCriteria(typeof(TEntity));
			}

			loadedItems = ItemsCriteria.List<TEntity>();
		}

		#endregion
	}
}
