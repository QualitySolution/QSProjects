using System;
using System.Data.Bindings;
using Gtk;
using QSProjectsLib;
using QSTDI;
using System.ComponentModel;
using System.Linq;
using QSProjectsLib.Helpers;

namespace QSOrmProject
{
	public abstract class OrmGtkDialogBase<TEntity> : Bin, ITdiDialog, IOrmDialog
		where TEntity : IDomainObject, new()
	{
		public IUnitOfWork UoW {
			get {
				return UoWGeneric;
			}
		}

		public HandleSwitchIn HandleSwitchIn { get; private set; }
		public HandleSwitchOut HandleSwitchOut { get; private set; }

		private IUnitOfWorkGeneric<TEntity> uowGeneric;

		public IUnitOfWorkGeneric<TEntity> UoWGeneric {
			get {
				return uowGeneric;
			}
			protected set {
				uowGeneric = value;
				OnTabNameChanged ();
				CheckButtonSubscription ();
			}
		}

		public bool CompareHashName (string hashName)
		{
			if (Entity == null || UoWGeneric == null || UoWGeneric.IsNew)
				return false;
			return GenerateHashName (Entity.Id) == hashName;
		}

		public static string GenerateHashName (int id)
		{
			return OrmMain.GenerateDialogHashName (typeof (TEntity), id);
		}

		//FIXME Временно для совместимости
		[Obsolete ("Используйте UnitOfWork, это свойство будет удалено.")]
		public NHibernate.ISession Session {
			get {
				return UoW.Session;
			}
		}

		private bool manualChange = false;

		public virtual bool HasChanges {
			get { return manualChange || UoWGeneric.HasChanges; }
			set { manualChange = value; }
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
				if (!String.IsNullOrWhiteSpace (tabName))
					return tabName;
				if (UoW != null && UoW.RootObject != null) {
					var att = typeof (TEntity).GetCustomAttributes (typeof (OrmSubjectAttribute), true);
					OrmSubjectAttribute subAtt = (att.FirstOrDefault () as OrmSubjectAttribute);

					if (UoW.IsNew) {
						if (subAtt != null && !String.IsNullOrWhiteSpace (subAtt.ObjectName)) {
							switch (subAtt.AllNames.Gender) {
							case GrammaticalGender.Masculine:
								return "Новый " + subAtt.ObjectName;
							case GrammaticalGender.Feminine:
								return "Новая " + subAtt.ObjectName;
							case GrammaticalGender.Neuter:
								return "Новое " + subAtt.ObjectName;
							default:
								return "Новый(ая) " + subAtt.ObjectName;
							}
						}
					} else {
						var notifySubject = UoW.RootObject as INotifyPropertyChanged;

						var prop = UoW.RootObject.GetType ().GetProperty ("Title");
						if (prop != null) {
							if (notifySubject != null) {
								notifySubject.PropertyChanged -= Subject_TitlePropertyChanged;
								notifySubject.PropertyChanged += Subject_TitlePropertyChanged;
							}
							return prop.GetValue (UoW.RootObject, null).ToString ();
						}

						prop = UoW.RootObject.GetType ().GetProperty ("Name");
						if (prop != null) {
							if (notifySubject != null) {
								notifySubject.PropertyChanged -= Subject_NamePropertyChanged;
								notifySubject.PropertyChanged += Subject_NamePropertyChanged;
							}
							return prop.GetValue (UoW.RootObject, null).ToString ();
						}

						if (subAtt != null && !String.IsNullOrWhiteSpace (subAtt.ObjectName))
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
				OnTabNameChanged ();
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
		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public bool FailInitialize { get; protected set; }

		protected void OnCloseTab (bool askSave)
		{
			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (askSave));
		}

		protected void OnEntitySaved (bool tabClosed = false)
		{
			if (EntitySaved != null)
				EntitySaved (this, new EntitySavedEventArgs (Entity, tabClosed));
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (!this.HasChanges || Save ()) {
				OnEntitySaved (true);
				OnCloseTab (false);
			}
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			OnCloseTab (false);
		}

		public override void Destroy ()
		{
			UoWGeneric.Dispose ();
			base.Destroy ();
		}

		protected void OnTabNameChanged ()
		{
			if (TabNameChanged != null)
				TabNameChanged (this, new TdiTabNameChangedEventArgs (TabName));
		}

		protected void OpenTab (string hashName, Func<ITdiTab> newTabFunc)
		{
			ITdiTab tab = TabParent.FindTab (hashName);

			if (tab == null)
				TabParent.AddTab (newTabFunc (), this);
			else
				TabParent.SwitchOnTab (tab);
		}

		public void SaveAndClose ()
		{
			OnButtonSaveClicked (this, EventArgs.Empty);
		}

		//FIXME Возможно временный метод, в нем не было необходиости пока в MonoDevelop не появился баг, с удалением подписок с кнопок Save и Cancel
		private void CheckButtonSubscription()
		{
			var saveButton = GtkHelper.EnumerateAllChildren(this).OfType<Button> ().FirstOrDefault (x => x.Name == "buttonSave");
			if(saveButton != null)
			{
				saveButton.Clicked -= OnButtonSaveClicked;
				saveButton.Clicked += OnButtonSaveClicked;
			}
			var cancelButton = GtkHelper.EnumerateAllChildren (this).OfType<Button> ().FirstOrDefault (x => x.Name == "buttonCancel");
			if (cancelButton != null) {
				cancelButton.Clicked -= OnButtonCancelClicked;
				cancelButton.Clicked += OnButtonCancelClicked;
			}
		}
	}
}

