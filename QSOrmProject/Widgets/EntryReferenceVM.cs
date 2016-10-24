using System;
using System.ComponentModel;
using System.Linq;
using Gtk;
using NLog;
using QSOrmProject.UpdateNotification;
using QSTDI;
using QSOrmProject.RepresentationModel;

namespace QSOrmProject
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class EntryReferenceVM : WidgetOnDialogBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		private System.Type subjectType;
		private bool sensitive = true;
		public bool CanEditReference = true;
		public Func<object, string> ObjectDisplayFunc;

		public event EventHandler Changed;
		public event EventHandler ChangedByUser;

		//TODO Реализовать удаление
		//TODO Реализовать удобный выбор через подбор

		[Browsable (false)]
		public new bool Sensitive {
			get { return sensitive; }
			set {
				if (sensitive == value)
					return;
				sensitive = value;
				buttonEdit.Sensitive = entryObject.Sensitive = sensitive;
				buttonOpen.Sensitive = sensitive && CanEditReference && subject != null;
			}
		}

		private IRepresentationModel representationModel;

		public IRepresentationModel RepresentationModel {
			get {
				return representationModel;
			}
			set { if (representationModel == value)
				return;
				representationModel = value;
				SubjectType = RepresentationModel.ObjectType;
			}
		}

		private object subject;

		public object Subject {
			get { return subject; }
			set {
				if (subject == value)
					return;
				if(subject is INotifyPropertyChanged)
				{
					(subject as INotifyPropertyChanged).PropertyChanged -= OnSubjectPropertyChanged;
				}
				subject = value;
				if(subject is INotifyPropertyChanged)
				{
					(subject as INotifyPropertyChanged).PropertyChanged += OnSubjectPropertyChanged;
				}
				UpdateWidget ();
				OnChanged ();
			}
		}

		public TEntity GetSubject<TEntity> ()
		{
			return (TEntity)Subject;
		}

		void OnSubjectPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			UpdateWidget ();
		}

		protected Type SubjectType {
			get { return subjectType; }
			set {
				if (subjectType != null) {
					IOrmObjectMapping map = OrmMain.GetObjectDescription (subjectType);
					map.ObjectUpdated -= OnExternalObjectUpdated;
				}
				subjectType = value;
				if (subjectType != null) {
					IOrmObjectMapping map = OrmMain.GetObjectDescription (subjectType);
					map.ObjectUpdated += OnExternalObjectUpdated;
				}
			}
		}

		private void OnExternalObjectUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			object foundUpdatedObject = e.UpdatedSubjects.FirstOrDefault (s => DomainHelper.EqualDomainObjects (s, Subject));
			if (foundUpdatedObject != null) {
				IOrmDialog dlg = OrmMain.FindMyDialog (this);

				if (dlg != null && !dlg.UoW.Session.Contains (foundUpdatedObject))
					dlg.UoW.Session.Refresh (Subject);

				UpdateWidget ();
				OnChanged ();
			}
		}

		private void UpdateWidget ()
		{
			buttonOpen.Sensitive = CanEditReference && subject != null;
			if (subject == null) {
				entryObject.Text = String.Empty;
				return;
			}

			if(ObjectDisplayFunc != null)
			{
				entryObject.Text = ObjectDisplayFunc (Subject);
				return;
			}

			entryObject.Text = DomainHelper.GetObjectTilte (Subject);
		}

		public EntryReferenceVM ()
		{
			this.Build ();
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			var modelWithParent = RepresentationModel as IRepresentationModelWithParent;
			if(modelWithParent != null)
			{
				if(MyOrmDialog != null && MyOrmDialog.UoW.IsNew 
					&& MyOrmDialog.EntityObject == modelWithParent.GetParent)
				{
					if(CommonDialogs.SaveBeforeSelectFromChildReference (modelWithParent.GetParent.GetType (), SubjectType))
					{
						if (!MyTdiDialog.Save ())
							return;
					}
					else
						return;
				}
			}

			ReferenceRepresentation SelectDialog;

			SelectDialog = new ReferenceRepresentation (RepresentationModel);

			SelectDialog.Mode = OrmReferenceMode.Select;
			if (!CanEditReference)
				SelectDialog.ButtonMode &= ~(ReferenceButtonMode.CanAdd | ReferenceButtonMode.CanDelete);
			SelectDialog.ObjectSelected += SelectDialog_ObjectSelected;
			MyTab.TabParent.AddSlaveTab (MyTab, SelectDialog);
		}

		void SelectDialog_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var dlg = OrmMain.FindMyDialog (this);
			var uow = dlg == null ? RepresentationModel.UoW : dlg.UoW;

			Subject = uow.GetById (SubjectType, e.ObjectId);
			OnChangedByUser();
		}

		protected void OnButtonOpenClicked (object sender, EventArgs e)
		{
			if (OrmMain.GetObjectDescription (SubjectType).SimpleDialog) {
				OrmSimpleDialog.RunSimpleDialog (this.Toplevel as Window, SubjectType, Subject);
				return;
			}

			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}

			ITdiTab dlg = OrmMain.CreateObjectDialog (Subject);
			mytab.TabParent.AddTab (dlg, mytab);
		}

		protected virtual void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		protected virtual void OnChangedByUser ()
		{
			if (ChangedByUser != null)
				ChangedByUser (this, EventArgs.Empty);
		}

		[GLib.ConnectBefore]
		protected void OnEntryObjectKeyPressEvent (object o, KeyPressEventArgs args)
		{
			if(args.Event.Key == Gdk.Key.Delete || args.Event.Key == Gdk.Key.BackSpace)
			{
				Subject = null;
				OnChangedByUser();
			}
		}

		protected override void OnDestroyed()
		{
			logger.Debug ("EntryReferenceVM Destroyed() called.");
			//Отписываемся от событий.
			if (subjectType != null) {
				IOrmObjectMapping map = OrmMain.GetObjectDescription (subjectType);
				map.ObjectUpdated -= OnExternalObjectUpdated;
			}
			if (subject is INotifyPropertyChanged) {
				(subject as INotifyPropertyChanged).PropertyChanged -= OnSubjectPropertyChanged;
			}
			if (RepresentationModel != null)
				RepresentationModel.Destroy();
			base.OnDestroyed();
		}
	}
}

