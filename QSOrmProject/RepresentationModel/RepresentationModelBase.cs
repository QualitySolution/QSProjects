using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gtk;

namespace QSOrmProject.RepresentationModel
{
	public abstract class RepresentationModelBase<TEntity> : IRepresentationModel
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region IRepresentationModel implementation

		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
			}
		}

		private IRepresentationFilter representationFilter;

		public IRepresentationFilter RepresentationFilter {
			get { return representationFilter;
			}
			protected set { 
				if (representationFilter != null)
					representationFilter.Refiltered -= RepresentationFilter_Refiltered;
				representationFilter = value;
				if(representationFilter != null)
					representationFilter.Refiltered += RepresentationFilter_Refiltered;
			}
		}

		void RepresentationFilter_Refiltered (object sender, EventArgs e)
		{
			UpdateNodes ();
		}

		public abstract void UpdateNodes ();

		public abstract Type NodeType { get; }

		public Type ObjectType {
			get {
				return typeof(TEntity);
			}
		}

		public NodeStore NodeStore { get; set;}

		private List<IColumnInfo> columns = new List<IColumnInfo> ();

		public List<IColumnInfo> Columns {
			get {
				return columns;
			}
		}

		#endregion

		public void SetRowAttribute<TVMNode> (string attibuteName, Expression<Func<TVMNode, object>> propertyRefExpr)
		{
			Columns.ConvertAll (c => (ColumnInfo) c)
				.ForEach ((ColumnInfo column) => column.SetAttributeProperty(attibuteName, propertyRefExpr));
		}

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
			OrmMain.GetObjectDiscription<TEntity>().ObjectUpdatedGeneric += OnExternalUpdate;
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
		protected RepresentationModelBase (params Type[] subcribeOnTypes )
		{
			foreach(var type in subcribeOnTypes)
			{
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

