using System;
using System.Collections.Generic;
using System.Linq;
using Gtk.DataBindings;
using System.Collections;

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

		public IList ItemsList { get; private set; }

		protected void SetItemsSource (IList<TNode> list)
		{
			ItemsList = (IList)list;
		}

		public abstract IMappingConfig TreeViewConfig { get; }

		private IRepresentationFilter representationFilter;

		public IRepresentationFilter RepresentationFilter {
			get { if (representationFilter != null)
					return representationFilter;
				if (CreateRepresentationFilter != null)
					representationFilter = CreateRepresentationFilter();
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
			var description = OrmMain.GetObjectDiscription<TEntity> ();
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
				var map = OrmMain.GetObjectDiscription (type);
				if (map != null)
					map.ObjectUpdated += OnExternalUpdateCommon;
				else
					logger.Warn ("Невозможно подписаться на обновления класа {0}. Не найден класс маппинга.", type);
			}
		}

		void OnExternalUpdate (object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedGenericEventArgs<TEntity> e)
		{
			if (e.UpdatedSubjects.Any (NeedUpdateFunc))
				UpdateNodes ();
		}

		void OnExternalUpdateCommon (object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedEventArgs e)
		{
			if (e.UpdatedSubjects.Any (NeedUpdateFunc))
				UpdateNodes ();
		}

	}
}

