using System;
using System.Collections.Generic;
using System.Linq;
using Gtk.DataBindings;
using System.Collections;
using System.Reflection;

namespace QSOrmProject.RepresentationModel
{
	public abstract class RepresentationModelBase<TEntity, TNode> : IRepresentationModel
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region IRepresentationModel implementation

		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get { return uow; }
			set { uow = value; }
		}

		public event EventHandler ItemsListUpdated;

		protected void OnItemsListUpdated()
		{
			if (ItemsListUpdated != null)
				ItemsListUpdated (this, EventArgs.Empty);
		}

		List<TNode> filtredItemsList;

		IList<TNode> itemsList;

		public IList ItemsList {
			get {
				return filtredItemsList ?? itemsList as IList;
			}
		}

		protected void SetItemsSource (IList<TNode> list)
		{
			itemsList = list;

			if (String.IsNullOrWhiteSpace (SearchString))
				OnItemsListUpdated ();
			else
				OnSearchRefilter ();
		}

		public abstract IMappingConfig TreeViewConfig { get; }

		private IRepresentationFilter representationFilter;

		public IRepresentationFilter RepresentationFilter {
			get { if (representationFilter != null)
					return representationFilter;
				if (CreateRepresentationFilter != null)
					RepresentationFilter = CreateRepresentationFilter();
				return representationFilter;
			}
			protected set { 
				if (representationFilter != null)
					representationFilter.Refiltered -= RepresentationFilter_Refiltered;
				representationFilter = value;
				if (representationFilter != null)
					representationFilter.Refiltered += RepresentationFilter_Refiltered;
			}
		}

		/// <summary>
		/// Функция создания нового фильтра необходима для отложенной загрузки.
		/// </summary>
		/// <value>The create representation filter.</value>
		public Func<IRepresentationFilter> CreateRepresentationFilter { get; set;}

		public IEnumerable<string> SearchFields {
			get {
				foreach (var prop in typeof(TNode).GetProperties ()) {
					var att = prop.GetCustomAttributes (typeof(UseForSearchAttribute), true).FirstOrDefault ();
					if (att != null)
						yield return prop.Name;
				}
			}
		}

		void RepresentationFilter_Refiltered (object sender, EventArgs e)
		{
			UpdateNodes ();
		}

		public abstract void UpdateNodes ();

		public Type NodeType {
			get { return typeof(TNode); }
		}

		public Type ObjectType {
			get { return typeof(TEntity); }
		}

		string searchString;

		public string SearchString {
			get{
				return searchString;
			}
			set{
				if (searchString == value)
					return;

				searchString = value;
				OnSearchRefilter ();
			}
		}

		#endregion

		/// <summary>
		/// Запрос у модели о необходимости обновления списка если объект изменился.
		/// </summary>
		/// <returns><c>true</c>, если небходимо обновлять список.</returns>
		/// <param name="updatedSubject">Обновившийся объект</param>
		protected abstract bool NeedUpdateFunc (TEntity updatedSubject);

		/// <summary>
		/// Создает новый базовый клас и подписывается на обновления для типа TEntity, при этом конструкторе необходима реализация NeedUpdateFunc (TEntity updatedSubject)
		/// </summary>
		protected RepresentationModelBase ()
		{
			var description = OrmMain.GetObjectDescription<TEntity> ();
			if (description != null)
				description.ObjectUpdatedGeneric += OnExternalUpdate;
			else
				logger.Warn ("Невозможно подписаться на обновления класа {0}. Не найден класс маппинга.", typeof(TEntity));
		}

		/// <summary>
		/// Запрос у модели о необходимости обновления списка если объект изменился.
		/// </summary>
		/// <returns><c>true</c>, если небходимо обновлять список.</returns>
		/// <param name="updatedSubject">Обновившийся объект</param>
		protected abstract bool NeedUpdateFunc (object updatedSubject);

		/// <summary>
		/// Создает новый базовый клас и подписывается на обновления указанных типов, при этом конструкторе необходима реализация NeedUpdateFunc (object updatedSubject);
		/// </summary>
		protected RepresentationModelBase (params Type[] subcribeOnTypes)
		{
			foreach (var type in subcribeOnTypes) {
				var map = OrmMain.GetObjectDescription (type);
				if (map != null)
					map.ObjectUpdated += OnExternalUpdateCommon;
				else
					logger.Warn ("Невозможно подписаться на обновления класа {0}. Не найден класс маппинга.", type);
			}
		}

		void OnExternalUpdate (object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedGenericEventArgs<TEntity> e)
		{
			if (!UoW.IsAlive)
			{
				logger.Warn ("Получена нотификация о внешнем обновлении данные в {0}, в тот момент когда сессия уже закрыта. Возможно RepresentationModel, осталась в памяти при закрытой сессии.",
				this);
				return;
			}
				
			if (e.UpdatedSubjects.Any (NeedUpdateFunc))
				UpdateNodes ();
		}

		void OnExternalUpdateCommon (object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedEventArgs e)
		{
			if (!UoW.IsAlive)
			{
				logger.Warn ("Получена нотификация о внешнем обновлении данные в {0}, в тот момент когда сессия уже закрыта. Возможно RepresentationModel, осталась в памяти при закрытой сессии.",
					this);
				return;
			}
				
			if (e.UpdatedSubjects.Any (NeedUpdateFunc))
				UpdateNodes ();
		}

		private PropertyInfo[] searchPropCache;

		protected PropertyInfo[] SearchPropCache {
			get {
				if (searchPropCache != null)
					return searchPropCache;

				searchPropCache = typeof(TNode).GetProperties ()
					.Where (prop => prop.GetCustomAttributes (typeof(UseForSearchAttribute), true).Length > 0)
					.ToArray ();

				return searchPropCache;
			}
		}

		protected void OnSearchRefilter()
		{
			if (String.IsNullOrWhiteSpace (SearchString))
				filtredItemsList = null;
			else
				filtredItemsList = itemsList.Where (SearchFilterFunc).ToList ();
			
			OnItemsListUpdated ();
		}

		private bool SearchFilterFunc(TNode item)
		{
			foreach (var prop in SearchPropCache) {
				string Str = (prop.GetValue (item, null) ?? String.Empty).ToString ();
				if (Str.IndexOf (searchString, StringComparison.CurrentCultureIgnoreCase) > -1) {
					return true;
				}
			}
			return false;
		}
	}
}

