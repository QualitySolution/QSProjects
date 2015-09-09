using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;

namespace QSOrmProject
{
	public class ParentReferenceGeneric<TParentEntity, TChildEntity> : IParentReference<TChildEntity>
		where TChildEntity : class, IDomainObject, new()
		where TParentEntity : IDomainObject, new()
	{
		Expression<Func<TParentEntity, IList<TChildEntity>>> ListPropertyExpr;

		Action<TParentEntity, TChildEntity> addNewChild;
		public Action<TParentEntity, TChildEntity> AddNewChild {
			get { if (addNewChild == null)
				{
					var config = ParentReferenceConfig.FindDefaultActions<TParentEntity,TChildEntity> ();
					if (config != null)
						return config.AddNewChild;
				}
				return addNewChild;
			}
			set {
				addNewChild = value;
			}
		}

		public IUnitOfWorkGeneric<TParentEntity> ParentUoWGeneric { get; private set;}

		#region IParentReferenceCommon implementation

		public IUnitOfWork ParentUoW {
			get {
				return ParentUoWGeneric;
			}
		}

		public object ParentObject {
			get { return ParentUoWGeneric.RootObject;
			}
		}

		#endregion

		public ParentReferenceGeneric (IUnitOfWorkGeneric<TParentEntity> parentUoW, Expression<Func<TParentEntity, IList<TChildEntity>>> propertyRefExpr)
		{
			ParentUoWGeneric = parentUoW;
			ListPropertyExpr = propertyRefExpr;
		}

		public IUnitOfWorkGeneric<TChildEntity> CreateUoWForNewItem ()
		{
			return CreateChildUoWForNewItem ();
		}

		public IChildUnitOfWorkGeneric<TParentEntity, TChildEntity> CreateChildUoWForNewItem()
		{
			return new ChildUnitOfWork<TParentEntity, TChildEntity> (this);
		}

		public IUnitOfWorkGeneric<TChildEntity> CreateUoWForItem (TChildEntity childEntity)
		{
			return CreateChildUoWForItem (childEntity);
		}

		public IChildUnitOfWorkGeneric<TParentEntity, TChildEntity> CreateChildUoWForItem(TChildEntity childEntity)
		{
			return new ChildUnitOfWork<TParentEntity, TChildEntity> (this, childEntity);
		}

		public GenericObservableList<TChildEntity> GetObservableList ()
		{
			var list = ListPropertyExpr.Compile ().Invoke (ParentUoWGeneric.Root);
			return new GenericObservableList<TChildEntity> (list);
		}
	}

	public interface IParentReference<TChildEntity> : IParentReferenceCommon 
		where TChildEntity : IDomainObject, new()
	{
		IUnitOfWorkGeneric<TChildEntity> CreateUoWForNewItem ();
		IUnitOfWorkGeneric<TChildEntity> CreateUoWForItem (TChildEntity childEntity);
		GenericObservableList<TChildEntity> GetObservableList ();
	}

	public interface IParentReferenceCommon
	{
		IUnitOfWork ParentUoW { get;}

		object ParentObject { get;}
	}
}