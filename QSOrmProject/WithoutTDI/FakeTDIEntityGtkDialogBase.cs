using System;
using System.ComponentModel;
using System.Linq;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Tdi;
using QS.Utilities.Text;

namespace QSOrmProject
{
	public abstract class FakeTDIEntityGtkDialogBase<TEntity> : FakeTDITabGtkDialogBase, ITdiDialog, IEntityDialog
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
				Title = TabName;
				OnTabNameChanged();
			}
		}


		//FIXME Временно для совместимости
		[Obsolete("Используйте UnitOfWork, это свойство будет удалено.")]
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

		public override string TabName {
			get {
				if(!String.IsNullOrWhiteSpace(base.TabName))
					return TabName;
				if (UoW != null && UoW.RootObject != null)
				{
					var att = typeof(TEntity).GetCustomAttributes (typeof(AppellativeAttribute), true);
					AppellativeAttribute subAtt = (att.FirstOrDefault () as AppellativeAttribute);

					if(UoW.IsNew)
					{
						if (subAtt != null && !String.IsNullOrWhiteSpace(subAtt.Nominative))
						{
							switch(subAtt.Gender){
							case GrammaticalGender.Masculine: 
								return "Новый " + subAtt.Nominative;
							case GrammaticalGender.Feminine :
								return "Новая " + subAtt.Nominative;
							case GrammaticalGender.Neuter :
								return "Новое " + subAtt.Nominative;
							default:
								return "Новый(ая) " + subAtt.Nominative;
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

						if(subAtt != null && !String.IsNullOrWhiteSpace(subAtt.Nominative))
							return subAtt.Nominative.StringToTitleCase();
					}
					return UoW.RootObject.ToString ();
				}
				return String.Empty;
			}
			set {
				if (base.TabName == value)
					return;
				base.TabName = value;
			}

		}

		void Subject_NamePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Name")
			{
				Title = TabName;
				OnTabNameChanged ();
			}
		}

		void Subject_TitlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Title")
			{
				Title = TabName;
				OnTabNameChanged ();
			}
		}

		public abstract bool Save ();

		public void SaveAndClose()
		{
			OnButtonSaveClicked(this, EventArgs.Empty);
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (!this.HasChanges || Save ())
			{
				OnEntitySaved (true);
				OnCloseTab (false);
			}
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			OnCloseTab (false);
		}

		protected void OnEntitySaved (bool tabClosed = false)
		{
			OnEntitySaved (Entity, tabClosed);
		}

		public override void Destroy ()
		{
			if(UoWGeneric != null)
				UoWGeneric.Dispose();
			base.Destroy ();
		}

		#region Для работы методов OpenTab

		public static string GenerateHashName(int id)
		{
			return DialogHelper.GenerateDialogHashName(typeof(TEntity), id);
		}

		public static string GenerateHashName(TEntity entity)
		{
			return DialogHelper.GenerateDialogHashName(typeof(TEntity), entity.Id);
		}

		public static string GenerateHashName()
		{
			return DialogHelper.GenerateDialogHashName(typeof(TEntity), 0);
		}

		#endregion

		public FakeTDIEntityGtkDialogBase ()
		{
		}
	}
}

