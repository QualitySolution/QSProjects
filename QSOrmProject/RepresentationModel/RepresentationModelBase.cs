using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gtk;

namespace QSOrmProject.RepresentationModel
{
	public abstract class RepresentationModelBase<TEntity> : IRepresentationModel
	{
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

		protected abstract bool NeedUpdateFunc (TEntity updatedSubject);

		protected RepresentationModelBase ()
		{
			OrmMain.GetObjectDiscription<TEntity>().ObjectUpdatedGeneric += OnExternalUpdate;
		}

		void OnExternalUpdate (object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedGenericEventArgs<TEntity> e)
		{
			if (e.UpdatedSubjects.Any (NeedUpdateFunc))
				UpdateNodes ();
		}
	}
}

