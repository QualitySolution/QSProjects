using System;
using NHibernate;
using QS.DomainModel.Entity;
using QS.Project.DB;

namespace QS.DomainModel.UoW
{
	[Obsolete("В классе присутствует уязвимость при удалении. Не следует его использовать")]
	public class UnitOfWorkChild<TRootEntity> : UnitOfWorkBase, IUnitOfWorkGeneric<TRootEntity>
		where TRootEntity : class, IDomainObject, new()
	{
		IUnitOfWork parentUoW;

		internal UnitOfWorkChild(ISessionProvider sessionProvider, TRootEntity childRoot, IUnitOfWork parentUoW, UnitOfWorkTitle actionTitle) : base(sessionProvider)
		{
			IsNew = childRoot.Id == 0;
			ActionTitle = actionTitle;
			this.parentUoW = parentUoW;
			Root = childRoot;
		}

		public object RootObject => Root;
		
		public TRootEntity Root { get; private set; }

		public bool HasChanges => IsNew || Session.IsDirty();

		public override ISession Session => parentUoW.Session;

		protected override ITransaction transaction => parentUoW.Session.Transaction;

		internal override void OpenTransaction()
		{
			(parentUoW as UnitOfWorkBase).OpenTransaction();
		}

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
