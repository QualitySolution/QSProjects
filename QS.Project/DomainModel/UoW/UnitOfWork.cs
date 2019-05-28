using QS.DomainModel.Entity;

namespace QS.DomainModel.UoW
{
	public class UnitOfWork<TRootEntity> : UnitOfWorkBase, IUnitOfWorkGeneric<TRootEntity> 
		where TRootEntity : class, IDomainObject, new()
	{
		public object RootObject => Root;
		public TRootEntity Root { get; private set; }

		public bool CanCheckIfDirty { get; set; } = true;

		public bool HasChanges {
			get {
				if(IsNew) {
					return true;
				}
				OpenTransaction();
				return CanCheckIfDirty && Session.IsDirty();
			}
		}

		internal UnitOfWork(UnitOfWorkTitle title)
		{
			IsNew = true;
            ActionTitle = title;
			Root = new TRootEntity();
			if(Root is IBusinessObject)
				((IBusinessObject)Root).UoW = this;
		}

		internal UnitOfWork(TRootEntity root, UnitOfWorkTitle title)
		{
			IsNew = true;
			Root = root;
            ActionTitle = title;
			if(Root is IBusinessObject)
				((IBusinessObject)Root).UoW = this;
		}

		internal UnitOfWork(int id, UnitOfWorkTitle title)
		{
			IsNew = false;
            ActionTitle = title;
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

