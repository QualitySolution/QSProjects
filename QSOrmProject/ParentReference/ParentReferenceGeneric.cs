using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using QSOrmProject.DomainModel;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System.Runtime.CompilerServices;

namespace QSOrmProject
{
	public class ParentReferenceGeneric<TParentEntity, TChildEntity> : IParentReference<TChildEntity>
		where TChildEntity : class, IDomainObject, new()
		where TParentEntity : IDomainObject, new()
	{
		Expression<Func<TParentEntity, IList<TChildEntity>>> ListPropertyExpr;

		Action<TParentEntity, TChildEntity> addNewChild;
		public Action<TParentEntity, TChildEntity> AddNewChild {
			get {
				if(addNewChild == null) {
					var config = ParentReferenceConfig.FindDefaultActions<TParentEntity, TChildEntity>();
					if(config != null)
						return config.AddNewChild;
				}
				return addNewChild;
			}
			set => addNewChild = value;
		}

		public IUnitOfWorkGeneric<TParentEntity> ParentUoWGeneric { get; private set; }

		#region IParentReferenceCommon implementation

		public IUnitOfWork ParentUoW => ParentUoWGeneric;

		public object ParentObject => ParentUoWGeneric.RootObject;

		#endregion

		public ParentReferenceGeneric(IUnitOfWorkGeneric<TParentEntity> parentUoW, Expression<Func<TParentEntity, IList<TChildEntity>>> propertyRefExpr)
		{
			ParentUoWGeneric = parentUoW;
			ListPropertyExpr = propertyRefExpr;
		}

		public IUnitOfWorkGeneric<TChildEntity> CreateUoWForNewItem(string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0)
		{
			return CreateChildUoWForNewItem(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}

		public IChildUnitOfWorkGeneric<TParentEntity, TChildEntity> CreateChildUoWForNewItem(string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0)
		{
			var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
			return new ChildUnitOfWork<TParentEntity, TChildEntity>(this, title);
		}

		public IUnitOfWorkGeneric<TChildEntity> CreateUoWForItem(TChildEntity childEntity, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0)
		{
			return CreateChildUoWForItem(childEntity, userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}

		public IChildUnitOfWorkGeneric<TParentEntity, TChildEntity> CreateChildUoWForItem(TChildEntity childEntity, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0)
		{
			var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
			return new ChildUnitOfWork<TParentEntity, TChildEntity>(this, childEntity, title);
		}

		public GenericObservableList<TChildEntity> GetObservableList()
		{
			var list = ListPropertyExpr.Compile().Invoke(ParentUoWGeneric.Root);
			return new GenericObservableList<TChildEntity>(list);
		}
	}

	public interface IParentReference<TChildEntity> : IParentReferenceCommon
		where TChildEntity : IDomainObject, new()
	{
		IUnitOfWorkGeneric<TChildEntity> CreateUoWForNewItem(string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0);
		IUnitOfWorkGeneric<TChildEntity> CreateUoWForItem(TChildEntity childEntity, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0);
		GenericObservableList<TChildEntity> GetObservableList();
	}

	public interface IParentReferenceCommon
	{
		IUnitOfWork ParentUoW { get; }

		object ParentObject { get; }
	}
}