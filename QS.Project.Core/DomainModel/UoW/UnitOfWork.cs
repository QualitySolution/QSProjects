using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Project.DB;
using System.Threading.Tasks;

namespace QS.DomainModel.UoW
{
	public class UnitOfWork<TRootEntity> : UnitOfWorkBase, IUnitOfWorkGeneric<TRootEntity> 
		where TRootEntity : class, IDomainObject, new()
	{
		public object RootObject => Root;
		public TRootEntity Root { get; private set; }

		public bool HasChanges {
			get {
				if(IsNew)
					return true;
				OpenTransaction();
				return Session.IsDirtyNoFlush();
			}
		}

		internal UnitOfWork(ISessionProvider sessionProvider, IMainThreadDispatcher mainThreadDispatcher, UnitOfWorkTitle title) 
			: base(sessionProvider, mainThreadDispatcher)
		{
			IsNew = true;
            ActionTitle = title;
			Root = new TRootEntity();
			if(Root is IBusinessObject)
				((IBusinessObject)Root).UoW = this;
		}

		internal UnitOfWork(ISessionProvider sessionProvider, IMainThreadDispatcher mainThreadDispatcher, TRootEntity root, UnitOfWorkTitle title) 
			: base(sessionProvider, mainThreadDispatcher)
		{
			IsNew = true;
			Root = root;
            ActionTitle = title;
			if(Root is IBusinessObject)
				((IBusinessObject)Root).UoW = this;
		}

		internal UnitOfWork(ISessionProvider sessionProvider, IMainThreadDispatcher mainThreadDispatcher, int id, UnitOfWorkTitle title) 
			: base(sessionProvider, mainThreadDispatcher)
		{
			IsNew = false;
            ActionTitle = title;
			Root = GetById<TRootEntity>(id);
		}

		public override void Save(object entity, bool orUpdate = true)
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
		
		public override async Task SaveAsync(object entity, bool orUpdate = true) {
			await base.SaveAsync(entity, orUpdate);

			if(RootObject.Equals(entity)) {
				await CommitAsync();
			}
		}

		public async Task SaveAsync() {
			await SaveAsync(Root);
		}
	}
}

