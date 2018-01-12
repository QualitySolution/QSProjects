using QSOrmProject.DomainModel.Tracking;

namespace QSOrmProject.DomainModel
{
	public class UnitOfWork<TRootEntity> : UnitOfWorkBase, IUnitOfWorkGeneric<TRootEntity> 
		where TRootEntity : class, IDomainObject, new()
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

		public IObjectTracker<TRootEntity> Tracker { get; private set; }

		public bool HasChanges
		{
			get
			{
				return IsNew || (Tracker != null ? Tracker.Compare(Root) : Session.IsDirty()) ;
			}
		}

		public UnitOfWork()
		{
			IsNew = true;
			Root = new TRootEntity();
			Tracker = TrackerMain.Factory?.Create(Root, IsNew);
		}

		public UnitOfWork(TRootEntity root)
		{
			IsNew = true;
			Root = root;
			Tracker = TrackerMain.Factory?.Create(Root, IsNew);
		}

		public UnitOfWork(int id)
		{
			IsNew = false;
			Root = GetById<TRootEntity>(id);
			Tracker = TrackerMain.Factory?.Create(Root, IsNew);
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

