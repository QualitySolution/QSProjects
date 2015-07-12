using System;
using System.Data.Bindings;
using Gtk;
using QSProjectsLib;
using QSTDI;
using System.ComponentModel;
using System.Linq;

namespace QSOrmProject
{
	public abstract class OrmGtkDialogBase<TEntity> : Bin, ITdiDialog, IOrmDialog
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
				OnTabNameChanged();
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

		public virtual bool HasChanges { 
			get { return UoWGeneric.HasChanges; }
		}

		public object EntityObject {
			get { return UoWGeneric.Root; }
		}

		public TEntity Entity {
			get { return UoWGeneric.Root; }
		}
			
		private string tabName = String.Empty;

		public string TabName {
			get {
				if(!String.IsNullOrWhiteSpace(tabName))
					return tabName;
				if (UoW != null && UoW.RootObject != null)
				{
					var att = typeof(TEntity).GetCustomAttributes (typeof(OrmSubjectAttribute), true);
					OrmSubjectAttribute subAtt = (att.FirstOrDefault () as OrmSubjectAttribute);

					if(UoW.IsNew)
					{
						if (subAtt != null && !String.IsNullOrWhiteSpace(subAtt.ObjectName))
						{
							switch(subAtt.AllNames.Gender){
								case GrammaticalGender.Masculine: 
									return "Новый " + subAtt.ObjectName;
								case GrammaticalGender.Feminine :
									return "Новая " + subAtt.ObjectName;
								case GrammaticalGender.Neuter :
									return "Новое " + subAtt.ObjectName;
								default:
									return "Новый(ая) " + subAtt.ObjectName;
							}
						}
					}
					else
					{
						var notifySubject = UoW.RootObject as INotifyPropertyChanged;

						var prop = UoW.RootObject.GetType ().GetProperty ("Title");
						if (prop != null) {
							if(notifySubject != null)
							{
								notifySubject.PropertyChanged -= Subject_TitlePropertyChanged;
								notifySubject.PropertyChanged += Subject_TitlePropertyChanged;
							}
							return prop.GetValue (UoW.RootObject, null).ToString();
						}

						prop = UoW.RootObject.GetType ().GetProperty ("Name");
						if (prop != null) {
							if (notifySubject != null) {
								notifySubject.PropertyChanged -= Subject_NamePropertyChanged;
								notifySubject.PropertyChanged += Subject_NamePropertyChanged;
							}
							return prop.GetValue (UoW.RootObject, null).ToString();
						}

						if(subAtt != null && !String.IsNullOrWhiteSpace(subAtt.ObjectName))
							return StringWorks.StringToTitleCase (subAtt.ObjectName);
					}
					return UoW.RootObject.ToString ();
				}
				return String.Empty;
			}
			set {
				if (tabName == value)
					return;
				tabName = value;
				OnTabNameChanged();
			}

		}

		void Subject_NamePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Name")
				OnTabNameChanged ();
		}

		void Subject_TitlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Title")
				OnTabNameChanged ();
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

		protected void OnTabNameChanged()
		{
			if (TabNameChanged != null)
				TabNameChanged (this, new TdiTabNameChangedEventArgs (TabName));
		}

		public OrmGtkDialogBase()
		{
		}
	}
}

