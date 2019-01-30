using System;
using NHibernate.Util;
using QS.DomainModel.UoW;
using QS.RepresentationModel;

namespace QSOrmProject.RepresentationModel
{
	public abstract class RepresentationFilterBase<TFilter> : Gtk.Bin, IRepresentationFilter, IJournalFilter
		where TFilter : class
	{
		bool canNotify = true;

		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				ConfigureWithUow();
			}
		}

		public event EventHandler Refiltered;

		protected void OnRefiltered()
		{
			if(canNotify && Refiltered != null)
				Refiltered(this, new EventArgs());
		}

		public RepresentationFilterBase()
		{
		}

		public RepresentationFilterBase(IUnitOfWork uow)
		{
			UoW = uow;
		}

		/// <summary>
		/// Устанавливать ограничения через этот метод, т.к. не будет вызываться 
		/// обновления журналов при каждом выставлении ограничения.
		/// </summary>
		/// <param name="setters">Лямбды ограничений</param>
		public void SetAndRefilterAtOnce(params Action<TFilter>[] setters)
		{
			canNotify = false;
			TFilter filter = this as TFilter;
			if(filter == null)
				throw new InvalidProgramException($"Класс {typeof(TFilter)} должен быть наследником RepresentationFilterBase<TFilter>");
			setters.ForEach(x => x(filter));
			canNotify = true;
			OnRefiltered();
		}

		protected virtual void ConfigureWithUow() { }
    }
}

