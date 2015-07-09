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
	public partial class EntryReferenceVM : Gtk.Bin
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		private System.Type subjectType;
		private bool sensitive = true;
		public bool CanEditReference = true;

		public event EventHandler Changed;

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

		void OnSubjectPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if(DisplayFields.Contains (e.PropertyName))
			{
				UpdateWidget ();
			}
		}

		public System.Type SubjectType {
			get { return subjectType; }
			set {
				if (subjectType != null) {
					IOrmObjectMapping map = OrmMain.GetObjectDiscription (subjectType);
					map.ObjectUpdated -= OnExternalObjectUpdated;
				}
				subjectType = value;
				if (subjectType != null) {
					IOrmObjectMapping map = OrmMain.GetObjectDiscription (subjectType);
					map.ObjectUpdated += OnExternalObjectUpdated;
				}
			}
		}

		private void OnExternalObjectUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			object foundUpdatedObject = e.UpdatedSubjects.FirstOrDefault (s => DomainHelper.EqualDomainObjects (s, Subject));
			if (foundUpdatedObject != null) {
				IOrmDialogNew dlg = OrmMain.FindMyDialog (this);
				//FIXME Возможно не нужно подписываться пока закомментируем
				//if (dlg != null && !dlg.Session.Contains (foundUpdatedObject))
				//	dlg.Session.Refresh (Subject);

				UpdateWidget ();
				OnChanged ();
			}
		}

		private void UpdateWidget ()
		{
			buttonOpen.Sensitive = CanEditReference && subject != null;
			if (subject == null || displayFields == null) {
				entryObject.Text = String.Empty;
				return;
			}

			object[] values = new object[displayFields.Length];
			for (int i = 0; i < displayFields.Length; i++) {
				if(String.IsNullOrWhiteSpace(displayFields [i]))
				{
					logger.Warn("Пустая строка в списке полей DisplayFields. Пропускаем...");
					continue;
				}
				var prop = subjectType.GetProperty(displayFields[i]);
				if (prop == null)
					throw new InvalidOperationException(String.Format("Поле {0} у класса {1} не найдено.", displayFields[i], SubjectType));
				values [i] = prop.GetValue (Subject, null);
			}
			entryObject.Text = String.Format (DisplayFormatString, values);
		}

		private string[] displayFields;

		public string[] DisplayFields {
			get { return displayFields; }
			set { displayFields = value; }
		}

		private string displayFormatString;

		public string DisplayFormatString {
			get { return (displayFormatString == null || displayFormatString == String.Empty) 
					? "{0}" : displayFormatString; }
			set { displayFormatString = value; }
		}

		public EntryReferenceVM ()
		{
			this.Build ();
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}
				
			ReferenceRepresentation SelectDialog;

			SelectDialog = new ReferenceRepresentation (RepresentationModel);

			SelectDialog.Mode = OrmReferenceMode.Select;
			if (!CanEditReference)
				SelectDialog.ButtonMode &= ~(ReferenceButtonMode.CanAdd | ReferenceButtonMode.CanDelete);
			SelectDialog.ObjectSelected += SelectDialog_ObjectSelected;
			mytab.TabParent.AddSlaveTab (mytab, SelectDialog);
		}

		void SelectDialog_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			
		}

		void OnSelectDialogObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Subject = e.Subject;
		}

		protected void OnButtonOpenClicked (object sender, EventArgs e)
		{
			if (OrmMain.GetObjectDiscription (SubjectType).SimpleDialog) {
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
	}
}

