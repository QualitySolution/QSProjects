using System;
using NHibernate.Util;

namespace QSOrmProject.RepresentationModel
{
	public abstract class RepresentationFilterBase<TFilter> : Gtk.Bin, IRepresentationFilter
		where TFilter : class
	{
		bool canUpdateNodes = true;

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
			if (canUpdateNodes && Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

		public RepresentationFilterBase()
		{
		}

		public RepresentationFilterBase(IUnitOfWork uow)
		{
			UoW = uow;
		}

		public void SetAtOnce(params Action<TFilter>[] setters)
		{
			canUpdateNodes = false;
			TFilter filter = this as TFilter;
			if(filter == null)
				throw new InvalidProgramException($"Класс {typeof(TFilter)} должен быть наследником RepresentationFilterBase<TFilter>");
			setters.ForEach(x => x.Invoke(filter));
			canUpdateNodes = true;
			OnRefiltered();
		}

		protected virtual void OnUoWSet() { }
	}
}

