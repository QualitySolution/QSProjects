using System;
using NHibernate;
using QS.DomainModel.Entity;
using QS.Project.DB;

namespace QS.DomainModel.UoW
{
	public class UnitOfWorkChild<TRootEntity> : UnitOfWorkBase, IUnitOfWorkGeneric<TRootEntity>
		where TRootEntity : class, IDomainObject, new()
	{
		IUnitOfWork parentUoW;

		internal UnitOfWorkChild(ISessionProvider sessionProvider, TRootEntity childRoot, IUnitOfWork parentUoW, UnitOfWorkTitle actionTitle) : base(sessionProvider)
		{
			IsNew = childRoot.Id == 0;
			ActionTitle = actionTitle;
			this.parentUoW = parentUoW;
			transaction = parentUoW.Session.Transaction;
			Root = childRoot;
		}

		public object RootObject => Root;
		public TRootEntity Root { get; private set; }

		public override ISession Session => parentUoW.Session;

		public bool CanCheckIfDirty { get; set; } = true;

		public bool HasChanges => IsNew || CanCheckIfDirty && Session.IsDirty();

		public override void Save<TEntity>(TEntity entity, bool orUpdate = true)
		{
			base.Save(entity, orUpdate);
		}

		public void Save()
		{
			Save(Root);
		}

		protected override void DisposeUoW()
		{
			//не нужно откатывать транзакцию и завершать сессию, так как владелец сессии родительский UoW
		}
	}
}
