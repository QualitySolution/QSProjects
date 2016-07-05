using System;

namespace QSOrmProject.RepresentationModel
{
	public abstract class RepresentationFilterBase : Gtk.Bin, IRepresentationFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				OnUoWSet();
			}
		}

		public event EventHandler Refiltered;

		protected void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

		public RepresentationFilterBase()
		{
		}

		public RepresentationFilterBase(IUnitOfWork uow)
		{
			UoW = uow;
		}

		protected virtual void OnUoWSet()
		{
			
		}
	}
}

