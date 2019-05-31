using QS.DomainModel.Entity;
using QS.Project.DB;

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

		internal UnitOfWork(ISessionProvider sessionProvider, UnitOfWorkTitle title) : base(sessionProvider)
		{
			IsNew = true;
            ActionTitle = title;
			Root = new TRootEntity();
			if(Root is IBusinessObject)
				((IBusinessObject)Root).UoW = this;
		}

		internal UnitOfWork(ISessionProvider sessionProvider, TRootEntity root, UnitOfWorkTitle title) : base(sessionProvider)
		{
			IsNew = true;
			Root = root;
            ActionTitle = title;
			if(Root is IBusinessObject)
				((IBusinessObject)Root).UoW = this;
		}

		internal UnitOfWork(ISessionProvider sessionProvider, int id, UnitOfWorkTitle title) : base(sessionProvider)
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

