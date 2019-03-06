﻿using System;
using NHibernate;
using QS.DomainModel.Entity;

namespace QS.DomainModel.UoW
{
	public class UnitOfWorkChild<TRootEntity> : UnitOfWorkBase, IUnitOfWorkGeneric<TRootEntity>
		where TRootEntity : class, IDomainObject, new()
	{
		IUnitOfWork parentUoW;

		internal UnitOfWorkChild(TRootEntity childRoot, IUnitOfWork parentUoW, UnitOfWorkTitle actionTitle)
		{
			IsNew = false;
			ActionTitle = actionTitle;
			this.parentUoW = parentUoW;
			transaction = parentUoW.Session.Transaction;
			Root = childRoot;
		}

		public object RootObject => Root;
		public TRootEntity Root { get; private set; }

		internal override ISession InternalSession => parentUoW.Session;

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
