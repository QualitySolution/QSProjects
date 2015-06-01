using System;
using Gtk;
using QSTDI;
using System.Data.Bindings;

namespace QSOrmProject
{
	public abstract class OrmGtkDialogBase<TEntity> : Bin, ITdiDialog, IOrmDialogNew, IOrmDialog 
		where TEntity : IDomainObject, new()
	{
		public IUnitOfWork UoW {
			get
			{
				return UoWGeneric;
			}
		}

		private IUnitOfWorkGeneric<TEntity> uowGeneric;

		public IUnitOfWorkGeneric<TEntity> UoWGeneric {
			get
			{
				return uowGeneric;
			}
			protected set
			{
				uowGeneric = value;
				subjectAdaptor.Target = UoWGeneric.Root;
			}
		}


		//FIXME Временно для совместимости
		public NHibernate.ISession Session
		{
			get
			{
				return UoW.Session;
			}
		}

		public bool HasChanges { 
			get { return UoWGeneric.HasChanges; }
		}

		public object Subject {
			get { return UoWGeneric.Root; }
		}
			
		private string tabName = String.Empty;

		public string TabName {
			get {
				if(!String.IsNullOrWhiteSpace(tabName))
					return tabName;
				if (UoW != null)
				{
					if(UoW.IsNew)
					{
						var att = typeof(TEntity).GetCustomAttributes (typeof(OrmSubjectAttribute), true);
						if (att.Length > 0 && !String.IsNullOrWhiteSpace((att [0] as OrmSubjectAttribute).ObjectName))
							return "Новый(ая) " + (att [0] as OrmSubjectAttribute).ObjectName; //FIXME Желательно добавить склонение
					}
					else
					{
						return DomainHelper.GetObjectTilte(UoW.RootObject);
					}
				}

				return String.Empty;
			}
			set {
				if (tabName == value)
					return;
				tabName = value;
				if (TabNameChanged != null)
					TabNameChanged (this, new TdiTabNameChangedEventArgs (value));
			}

		}

		public abstract bool Save ();

		public ITdiTabParent TabParent { set; get; }

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		protected void OnCloseTab (bool askSave)
		{
			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (askSave));
		}

		protected Adaptor subjectAdaptor = new Adaptor ();

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (!this.HasChanges || Save ())
				OnCloseTab (false);
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			OnCloseTab (false);
		}

		public override void Destroy ()
		{
			UoWGeneric.Dispose();
			subjectAdaptor.Disconnect ();
			base.Destroy ();
		}

		public OrmGtkDialogBase()
		{
		}
	}
}

