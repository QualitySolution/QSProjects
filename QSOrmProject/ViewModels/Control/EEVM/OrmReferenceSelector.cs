using System;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Tdi;
using QSOrmProject;

namespace QS.ViewModels.Control.EEVM
{
	public class OrmReferenceSelector : IEntitySelector
	{
		readonly ITdiTab MyTab;
		readonly IUnitOfWork UoW;

		ICriteria ItemsCriteria;
		QueryOver ItemsQuery;
		readonly Type SubjectType;

		#region Конструкторы

		public OrmReferenceSelector(ITdiTab parrentTab, IUnitOfWork unitOfWork, ICriteria itemsCriteria)
		{
			MyTab = parrentTab ?? throw new ArgumentNullException(nameof(parrentTab));
			UoW = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			ItemsCriteria = itemsCriteria ?? throw new ArgumentNullException(nameof(itemsCriteria));
			SubjectType = itemsCriteria.GetRootEntityTypeIfAvailable();
		}

		public OrmReferenceSelector(ITdiTab parrentTab, IUnitOfWork unitOfWork, QueryOver itemsQuery)
		{
			MyTab = parrentTab ?? throw new ArgumentNullException(nameof(parrentTab));
			UoW = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			ItemsQuery = itemsQuery ?? throw new ArgumentNullException(nameof(itemsQuery));
			SubjectType = ItemsQuery.DetachedCriteria.GetRootEntityTypeIfAvailable();
		}

		public OrmReferenceSelector(ITdiTab parrentTab, IUnitOfWork unitOfWork, Type subjectType)
		{
			MyTab = parrentTab ?? throw new ArgumentNullException(nameof(parrentTab));
			UoW = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			SubjectType = subjectType;
		}

		#endregion

		public event EventHandler<EntitySelectedEventArgs> EntitySelected;

		public void OpenSelector(string dialogTitle = null)
		{
			OrmReference SelectDialog;

			if(ItemsQuery != null) {
				SelectDialog = new OrmReference(UoW, ItemsQuery);
			}
			else {
				if(ItemsCriteria == null)
					ItemsCriteria = UoW.Session.CreateCriteria(SubjectType);

				SelectDialog = new OrmReference(SubjectType, UoW, ItemsCriteria);
			}

			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ObjectSelected += OnSelectDialogObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, SelectDialog);
		}

		void OnSelectDialogObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			EntitySelected?.Invoke(this, new EntitySelectedEventArgs(e.Subject));
		}

		public object RefreshEntity(object entity)
		{
			if(UoW.Session.IsOpen && UoW.Session.Contains(entity)) {
				UoW.Session.Refresh(entity);
			}
			return entity;
		}
	}
}
