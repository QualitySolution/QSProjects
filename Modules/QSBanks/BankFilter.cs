using System;
using NHibernate;
using NHibernate.Criterion;
using QSOrmProject;


namespace QSBanks
{
	[OrmDefaultIsFiltered (true)]
	public partial class BankFilter : Gtk.Bin, IReferenceFilter
	{
		public IUnitOfWork UoW { set; get;}

		public ICriteria BaseCriteria { set; get; }

		public event EventHandler Refiltered;

		public bool IsFiltred { get; private set; }

		ICriteria filtredCriteria;

		public ICriteria FiltredCriteria {
			private set { filtredCriteria = value; }
			get {
				if (filtredCriteria == null)
					UpdateCreteria ();
				return filtredCriteria;
			}
		}

		public BankFilter (IUnitOfWork uow)
		{
			this.Build ();
			UoW = uow;
			IsFiltred = false;
		}

		void UpdateCreteria ()
		{
			IsFiltred = false;
			if (BaseCriteria == null)
				return;
			FiltredCriteria = (ICriteria)BaseCriteria.Clone ();

			if (!checkShowDeleted.Active) {
				FiltredCriteria.Add (Restrictions.Eq ("Deleted", false));
				IsFiltred = true;
			}

			OnRefiltered ();
		}

		void OnRefiltered ()
		{
			if (Refiltered != null) {
				Refiltered (this, new EventArgs ());
			}
		}

		protected void OnCheckShowDeletedToggled (object sender, EventArgs e)
		{
			UpdateCreteria ();
		}
	}
}

