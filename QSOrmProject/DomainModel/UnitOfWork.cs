using System;

namespace QSOrmProject.DomainModel
{
	public class UnitOfWork<TRootEntity> : UnitOfWorkBase, IUnitOfWorkGeneric<TRootEntity> 
		where TRootEntity : IDomainObject, new()
	{
		public object RootObject {
			get { return Root;}
		}

		TRootEntity root;
		public TRootEntity Root {
			get {
				return root;
			}
			private set {
				root = value;
				if (Root is IBusinessObject)
					(Root as IBusinessObject).UoW = this;
			}
		}

		public bool HasChanges
		{
			get
			{
				return IsNew || Session.IsDirty();
			}
		}

		public UnitOfWork()
		{
			IsNew = true;
			Root = new TRootEntity();
		}

		public UnitOfWork(TRootEntity root)
		{
			IsNew = true;
			Root = root;
		}

		public UnitOfWork(int id)
		{
			IsNew = false;
			Root = GetById<TRootEntity>(id);
		}

		public override void Save<TEntity>(TEntity entity, bool orUpdate = true)
		{
			base.Save (entity, orUpdate);

			if (RootObject.Equals(entity))
			{
				Commit();
			}
		}

		public void Save()
		{
			Save(Root);
		}
	}
}

