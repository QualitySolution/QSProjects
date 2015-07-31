﻿using System;
using System.ComponentModel;
using System.Linq;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NLog;
using QSOrmProject.UpdateNotification;
using QSTDI;

namespace QSOrmProject
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class EntryReference : Gtk.Bin
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		private bool sensitive = true;
		public bool CanEditReference = true;
		public ICriteria ItemsCriteria;

		public QueryOver ItemsQuery { get; set; }

		/// <summary>
		/// Используется для cложной обработки строки отображения сущьности. На вход передается объект сущьности. на выходе должна получится строка.
		/// Если ObjectDisplayFunc == null, сущьность отображается через функцию DomainHelper.GetObjectTilte, то есть по свойствам Title или Name.
		/// </summary>
		public Func<object, string> ObjectDisplayFunc;

		public OrmParentReference ParentReference { get; set; }

		private System.Type subjectType;

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
				IOrmDialog dlg = OrmMain.FindMyDialog (this);
				//FIXME Возможно не нужно подписываться пока закомментируем
				//if (dlg != null && !dlg.Session.Contains (foundUpdatedObject))
				//	dlg.Session.Refresh (Subject);

				UpdateWidget ();
				OnChanged ();
			}
		}

		private void UpdateWidget ()
		{
			if (displayFields == null)
			{
				UpdateWidgetNew ();
				return;
			}

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

		private void UpdateWidgetNew ()
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

		private string[] displayFields;

		[Obsolete("Используйте новый механизм отображения через ObjectDisplayFunc или отображение по умочанию.(Будет удалено после 27.07.2016)")]
		public string[] DisplayFields {
			get { return displayFields; }
			set { displayFields = value; }
		}

		private string displayFormatString;

		[Obsolete("Используйте новый механизм отображения через ObjectDisplayFunc или отображение по умочанию.(Будет удалено после 27.07.2016)")]
		public string DisplayFormatString {
			get { return (displayFormatString == null || displayFormatString == String.Empty) 
					? "{0}" : displayFormatString; }
			set { displayFormatString = value; }
		}

		public EntryReference ()
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
				
			IOrmDialog dlg = OrmMain.FindMyDialog (this);
			IUnitOfWork localUoW;
			OrmReference SelectDialog;

			if (ParentReference != null) {
				SelectDialog = new OrmReference (subjectType, ParentReference);
			} else {
				if (dlg != null)
					localUoW = dlg.UoW;
				else
					localUoW = UnitOfWorkFactory.CreateWithoutRoot ();

				if(ItemsQuery != null)
				{
					SelectDialog = new OrmReference (localUoW, ItemsQuery);
				}
				else
				{
					if (ItemsCriteria == null)
						ItemsCriteria = localUoW.Session.CreateCriteria (subjectType);

					SelectDialog = new OrmReference (subjectType, localUoW, ItemsCriteria);
				}
			}
			SelectDialog.Mode = OrmReferenceMode.Select;
			if (!CanEditReference)
				SelectDialog.ButtonMode &= ~(ReferenceButtonMode.CanAdd | ReferenceButtonMode.CanDelete);
			SelectDialog.ObjectSelected += OnSelectDialogObjectSelected;
			mytab.TabParent.AddSlaveTab (mytab, SelectDialog);
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

			ITdiTab dlg;
			if (ParentReference == null)
				dlg = OrmMain.CreateObjectDialog (Subject);
			else
				dlg = OrmMain.CreateObjectDialog (ParentReference, Subject);
			mytab.TabParent.AddTab (dlg, mytab);
		}

		protected virtual void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}
	}
}

