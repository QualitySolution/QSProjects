using System.Linq;
using QS.DomainModel;
using QS.DomainModel.Tracking;

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
			Tracker = TrackerMain.Factory?.Create(Root, TrackerCreateOption.IsNewAndShotThis);
			if(Tracker != null)
				Trackers.Add(Tracker);
		}

		public UnitOfWork(TRootEntity root)
		{
			IsNew = true;
			Root = root;
			Tracker = TrackerMain.Factory?.Create(Root, TrackerCreateOption.IsNewAndShotThis);
			if(Tracker != null)
				Trackers.Add(Tracker);
		}

		public UnitOfWork(int id)
		{
			IsNew = false;
			Root = GetById<TRootEntity>(id);
			Tracker = Trackers.OfType<IObjectTracker<TRootEntity>>().FirstOrDefault(t => t.OriginEntity == Root);
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

