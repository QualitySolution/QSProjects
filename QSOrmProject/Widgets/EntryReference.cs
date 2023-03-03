﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Gamma.Binding.Core.Helpers;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Gtk.Widgets;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QS.Tdi;
using QSOrmProject.DomainMapping;
using QSOrmProject.UpdateNotification;

namespace QSOrmProject
{
	[System.ComponentModel.ToolboxItem (true)]
	[Obsolete("Используйте новый виджет EntityEntry и новый тип журналов QS.Project.Journal")]
	public partial class EntryReference : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		private bool sensitive = true;

		private bool entryChangedByUser = true;

		private bool canEditReference = true;

		private ListStore completionListStore;

		private ISearchProvider searchSearchProvider;

		private ICriteria itemsCriteria;

		public ICriteria ItemsCriteria {
			get {
				return itemsCriteria;
			}
			set {
				itemsCriteria = value;

				SubjectType = itemsCriteria.GetRootEntityTypeIfAvailable ();
			}
		}

		QueryOver itemsQuery;

		public QueryOver ItemsQuery {
			get {
				return itemsQuery;
			}
			set {
				itemsQuery = value;

				SubjectType = ItemsQuery.DetachedCriteria.GetRootEntityTypeIfAvailable ();
			}
		}

		/// <summary>
		/// Используется для сложной обработки строки отображения сущности. На вход передается объект сущности на выходе должна получится строка.
		/// Если ObjectDisplayFunc == null, сущность отображается через функцию DomainHelper.GetObjectTitle, то есть по свойствам Title или Name.
		/// Для установки используете метод SetObjectDisplayFunc
		/// </summary>
		public Func<object, string> ObjectDisplayFunc { get; private set; }

		private System.Type subjectType;

		public event EventHandler Changed;

		public event EventHandler ChangedByUser;

		public event EventHandler<EntryReferenceBeforeChangeEventArgs> BeforeChangeByUser;

		[Browsable (false)] //FIXME Пока не работает корректно установка значения по умолчанию нужно разбираться.
		[DefaultValue (true)]
		public bool CanEditReference {
			get {
				return canEditReference;
			}
			set {
				canEditReference = value;
			}
		}

		[Browsable(false)]
		public new bool Sensitive {
			get { return sensitive; }
			set {
				if(sensitive == value)
					return;
				sensitive = value;
				UpdateSensitive();
			}
		}

		bool isEditable = true;
		[Browsable(false)]
		public bool IsEditable {
			get { return isEditable; }
			set {
				isEditable = value;
				UpdateSensitive();
			}
		}

		void UpdateSensitive()
		{
			buttonSelectEntity.Sensitive = entryObject.Sensitive = sensitive && IsEditable;
			buttonViewEntity.Sensitive = sensitive && CanEditReference && subject != null;
		}

		private object subject;

		public object Subject {
			get { return subject; }
			set {
				if (subject == value)
					return;
				if (subject is INotifyPropertyChanged) {
					(subject as INotifyPropertyChanged).PropertyChanged -= OnSubjectPropertyChanged;
				}
				subject = value;
				if (subject is INotifyPropertyChanged) {
					(subject as INotifyPropertyChanged).PropertyChanged += OnSubjectPropertyChanged;
				}
				UpdateWidget ();
				OnChanged ();
			}
		}

		public int SubjectId{
			get{
				return DomainHelper.GetId(Subject);
			}
		}

        public TEntity GetSubject<TEntity>()
        {
            return (TEntity)Subject;
        }

		void OnSubjectPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (displayFields == null || displayFields.Contains (e.PropertyName)) {
				UpdateWidget ();
			}
		}

		public System.Type SubjectType {
			get { return subjectType; }
			set {
				if (subjectType != null) {
					IOrmObjectMapping map = OrmMain.GetObjectDescription (subjectType);
					map.ObjectUpdated -= OnExternalObjectUpdated;
				}
				subjectType = value;
				if (subjectType != null) {
					IOrmObjectMapping map = OrmMain.GetObjectDescription (subjectType);
					if(map == null)
						throw new Exception(String.Format("Не создан маппинг в CreateProjectParam для SubjectType = {0}", SubjectType));
					map.ObjectUpdated += OnExternalObjectUpdated;
					if(map.TableView != null && map.TableView.SearchProvider != null)
						searchSearchProvider = map.TableView.SearchProvider;
				}
			}
		}

		public void SetObjectDisplayFunc<TObject> (Expression<Func<TObject, string>> expDisplayFunc) where TObject : class
		{
			if (typeof(TObject) != SubjectType)
				throw new InvalidCastException (String.Format ("SubjectType = {0}, а DisplayFunc задается для {1}", SubjectType, typeof(TObject)));
			displayFields = FetchPropertyFromExpression.Fetch(expDisplayFunc);
			ObjectDisplayFunc = o => expDisplayFunc.Compile() (o as TObject);
		}

		private void OnExternalObjectUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			object foundUpdatedObject = e.UpdatedSubjects.FirstOrDefault (s => DomainHelper.EqualDomainObjects (s, Subject));
			if (foundUpdatedObject != null) {
				var dlg = DialogHelper.FindParentUowDialog(this);
				if (MyEntityDialogExist)
					MyEntityDialog.UoW.Session.Refresh (Subject);

				UpdateWidget ();
				OnChanged ();
			}
		}

		private void UpdateWidget ()
		{
			buttonViewEntity.Sensitive = CanEditReference && subject != null;

			entryChangedByUser = false;
			entryObject.Text = GetObjectTitle (Subject);
			entryChangedByUser = true;
		}

		private string GetObjectTitle (object item)
		{
			if (item == null) {
				return String.Empty;
			}

			if (ObjectDisplayFunc != null) {
				return ObjectDisplayFunc (item);
			}

			return DomainHelper.GetTitle (item);
		}

		private string[] displayFields;

		public EntryReference ()
		{
			this.Build ();

			entryObject.Completion = new EntryCompletion ();
			entryObject.Completion.MatchSelected += Completion_MatchSelected;
			entryObject.Completion.MatchFunc = Completion_MatchFunc;
			var cell = new CellRendererText ();
			entryObject.Completion.PackStart (cell, true);
			entryObject.Completion.SetCellDataFunc (cell, OnCellLayoutDataFunc);
		}

		void OnCellLayoutDataFunc (CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var title = (string)tree_model.GetValue (iter, (int)СompletionColumn.Tilte);
			string pattern = String.Format ("{0}", Regex.Escape (entryObject.Text));
			(cell as CellRendererText).Markup = 
				Regex.Replace (title, pattern, (match) => String.Format ("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
		}

		bool Completion_MatchFunc (EntryCompletion completion, string key, TreeIter iter)
		{
			if (searchSearchProvider == null) {
				var val = completion.Model.GetValue (iter, (int)СompletionColumn.Tilte).ToString ();
				return val.IndexOf (key, StringComparison.CurrentCultureIgnoreCase) > -1;
			} else {
				var val = completion.Model.GetValue (iter, (int)СompletionColumn.Item);
				return searchSearchProvider.Match(val, key);
			}
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected (object o, MatchSelectedArgs args)
		{
			Subject = args.Model.GetValue (args.Iter, (int)СompletionColumn.Item);
			OnChangedByUser();
			args.RetVal = true;
		}

		protected void OnButtonSelectEntityClicked (object sender, EventArgs e)
		{
			OpenSelectDialog();
		}

		/// <summary>
		/// Открывает диалог выбора объекта
		/// </summary>
		public void OpenSelectDialog()
		{
			if(!OnBeforeChangeByUser()) {
				return;
			}

			var dlg = DialogHelper.FindParentUowDialog(this);
			IUnitOfWork localUoW;
			OrmReference SelectDialog;

			if(dlg != null)
				localUoW = dlg.UoW;
			else
				localUoW = UnitOfWorkFactory.CreateWithoutRoot();

			if(ItemsQuery != null) {
				SelectDialog = new OrmReference(localUoW, ItemsQuery);
			} else {
				if(ItemsCriteria == null)
					ItemsCriteria = localUoW.Session.CreateCriteria(subjectType);

				SelectDialog = new OrmReference(subjectType, localUoW, ItemsCriteria);
			}

			SelectDialog.Mode = OrmReferenceMode.Select;
			if(!CanEditReference)
				SelectDialog.ButtonMode &= ~(ReferenceButtonMode.CanAdd | ReferenceButtonMode.CanDelete);
			SelectDialog.ObjectSelected += OnSelectDialogObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, SelectDialog);
		}

		private bool OnBeforeChangeByUser()
		{
			if(BeforeChangeByUser != null)
			{
				var args = new EntryReferenceBeforeChangeEventArgs();
				args.CanChange = true;
				BeforeChangeByUser(this, args);
				if (!args.CanChange)
					return false;
			}
			return true;
		}

		void OnSelectDialogObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Subject = e.Subject;
			OnChangedByUser();
		}

		protected void OnButtonViewEntityClicked (object sender, EventArgs e)
		{
			if(SubjectType == null)
				throw new NullReferenceException("SubjectType не задан.");

			if(OrmMain.GetObjectDescription(SubjectType).SimpleDialog) {
				EntityEditSimpleDialog.RunSimpleDialog(this.Toplevel as Window, SubjectType, Subject);
				return;
			}

			ITdiTab dlg = OrmMain.CreateObjectDialog(Subject);
			MyTab.TabParent.AddTab(dlg, MyTab);
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

		private void fillAutocomplete ()
		{
			logger.Info ("Запрос данных для автодополнения...");
			completionListStore = new ListStore (typeof(string), typeof(object));

			IUnitOfWork localUoW;

			var dlg = DialogHelper.FindParentUowDialog(this);

			if (dlg != null)
				localUoW = dlg.UoW;
			else
				localUoW = UnitOfWorkFactory.CreateWithoutRoot ();

			if (ItemsQuery != null) {
				ItemsCriteria = ItemsQuery.DetachedCriteria.GetExecutableCriteria (localUoW.Session);
			} else {
				if(SubjectType == null)
				{
					logger.Warn ("SubjectType = null, не возможно выполнить заполнение автокомплита.");
					return;
				}
				if (ItemsCriteria == null)
					ItemsCriteria = localUoW.Session.CreateCriteria (SubjectType);
			}

			foreach (var item in ItemsCriteria.List ()) {
				completionListStore.AppendValues (
					GetObjectTitle (item),
					item
				);
			}
			entryObject.Completion.Model = completionListStore;
			logger.Debug ("Получено {0} строк автодополения...", completionListStore.IterNChildren ());
			//if (this.HasFocus)
			//	this.Completion.Complete ();
		}

		protected void OnEntryObjectChanged (object sender, EventArgs e)
		{
			if (entryChangedByUser && completionListStore == null)
				fillAutocomplete ();
		}

		protected void OnEntryObjectFocusOutEvent (object o, FocusOutEventArgs args)
		{
			if (string.IsNullOrWhiteSpace (entryObject.Text))
			{
				Subject = null;
				OnChangedByUser();
			}
			else
				UpdateWidget ();
		}

		protected override void OnDestroyed()
		{
			logger.Debug ($"EntryReference {subjectType} Destroyed() called.");
			//Отписываемся от событий.
			if (subjectType != null) {
				IOrmObjectMapping map = OrmMain.GetObjectDescription (subjectType);
				map.ObjectUpdated -= OnExternalObjectUpdated;
			}
			if (subject is INotifyPropertyChanged) {
				(subject as INotifyPropertyChanged).PropertyChanged -= OnSubjectPropertyChanged;
			}
			base.OnDestroyed();
		}
	}
}

