using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NLog;
using QSOrmProject.DomainMapping;
using QSOrmProject.UpdateNotification;
using QSTDI;

namespace QSOrmProject
{
	enum completionCol
	{
		Tilte,
		Item
	}

	[System.ComponentModel.ToolboxItem (true)]
	public partial class EntryReference : WidgetOnDialogBase
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
		/// Используется для cложной обработки строки отображения сущьности. На вход передается объект сущьности. на выходе должна получится строка.
		/// Если ObjectDisplayFunc == null, сущьность отображается через функцию DomainHelper.GetObjectTilte, то есть по свойствам Title или Name.
		/// </summary>
		public Func<object, string> ObjectDisplayFunc;

		private System.Type subjectType;

		public event EventHandler Changed;

		public event EventHandler ChangedByUser;

		public event EventHandler<EntryReferenceBeforeChangeEventArgs> BeforeChangeByUser;

		[Browsable (false)] //FIXME Пока не работает корректно установка заначения по умолчанию нужно разбираться.
		[DefaultValue (true)]
		public bool CanEditReference {
			get {
				return canEditReference;
			}
			set {
				canEditReference = value;
			}
		}

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

		void OnSubjectPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (DisplayFields == null || DisplayFields.Contains (e.PropertyName)) {
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
					map.ObjectUpdated += OnExternalObjectUpdated;
					if(map.TableView != null && map.TableView.SearchProvider != null)
						searchSearchProvider = map.TableView.SearchProvider;
				}
			}
		}

		public void SetObjectDisplayFunc<TObject> (Func<TObject, string> displayFunc) where TObject : class
		{
			if (typeof(TObject) != SubjectType)
				throw new InvalidCastException (String.Format ("SubjectType = {0}, а DisplayFunc задается для {1}", SubjectType, typeof(TObject)));
			ObjectDisplayFunc = o => displayFunc (o as TObject);
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
			if (displayFields == null || ObjectDisplayFunc != null) {
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
				if (String.IsNullOrWhiteSpace (displayFields [i])) {
					logger.Warn ("Пустая строка в списке полей DisplayFields. Пропускаем...");
					continue;
				}
				var prop = subjectType.GetProperty (displayFields [i]);
				if (prop == null)
					throw new InvalidOperationException (String.Format ("Поле {0} у класса {1} не найдено.", displayFields [i], SubjectType));
				values [i] = prop.GetValue (Subject, null);
			}
			entryChangedByUser = false;
			entryObject.Text = String.Format (DisplayFormatString, values);
			entryChangedByUser = true;
		}

		private void UpdateWidgetNew ()
		{
			buttonOpen.Sensitive = CanEditReference && subject != null;

			entryChangedByUser = false;
			entryObject.Text = GetObjectTitle (Subject);
			entryChangedByUser = true;
		}

		//Используется только новое отображение объекта
		private string GetObjectTitle (object item)
		{
			if (item == null) {
				return String.Empty;
			}

			if (ObjectDisplayFunc != null) {
				return ObjectDisplayFunc (item);
			}

			return DomainHelper.GetObjectTilte (item);
		}

		private string[] displayFields;

		[Obsolete ("Используйте новый механизм отображения через ObjectDisplayFunc или отображение по умочанию.(Будет удалено после 27.07.2016)")]
		public string[] DisplayFields {
			get { return displayFields; }
			set { displayFields = value; }
		}

		private string displayFormatString;

		[Obsolete ("Используйте новый механизм отображения через ObjectDisplayFunc или отображение по умочанию.(Будет удалено после 27.07.2016)")]
		public string DisplayFormatString {
			get {
				return (displayFormatString == null || displayFormatString == String.Empty) 
					? "{0}" : displayFormatString;
			}
			set { displayFormatString = value; }
		}

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
			var title = (string)tree_model.GetValue (iter, (int)completionCol.Tilte);
			string pattern = String.Format ("{0}", Regex.Escape (entryObject.Text));
			(cell as CellRendererText).Markup = 
				Regex.Replace (title, pattern, (match) => String.Format ("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
		}

		bool Completion_MatchFunc (EntryCompletion completion, string key, TreeIter iter)
		{
			if (searchSearchProvider == null) {
				var val = completion.Model.GetValue (iter, (int)completionCol.Tilte).ToString ();
				return val.IndexOf (key, StringComparison.CurrentCultureIgnoreCase) > -1;
			} else {
				var val = completion.Model.GetValue (iter, (int)completionCol.Item);
				return searchSearchProvider.Match(val, key);
			}
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected (object o, MatchSelectedArgs args)
		{
			Subject = args.Model.GetValue (args.Iter, (int)completionCol.Item);
			OnChangedByUser();
			args.RetVal = true;
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			if(!OnBeforeChangeByUser())
			{
				return;
			}

			IOrmDialog dlg = OrmMain.FindMyDialog (this);
			IUnitOfWork localUoW;
			OrmReference SelectDialog;

			if (dlg != null)
				localUoW = dlg.UoW;
			else
				localUoW = UnitOfWorkFactory.CreateWithoutRoot ();

			if (ItemsQuery != null) {
				SelectDialog = new OrmReference (localUoW, ItemsQuery);
			} else {
				if (ItemsCriteria == null)
					ItemsCriteria = localUoW.Session.CreateCriteria (subjectType);

				SelectDialog = new OrmReference (subjectType, localUoW, ItemsCriteria);
			}

			SelectDialog.Mode = OrmReferenceMode.Select;
			if (!CanEditReference)
				SelectDialog.ButtonMode &= ~(ReferenceButtonMode.CanAdd | ReferenceButtonMode.CanDelete);
			SelectDialog.ObjectSelected += OnSelectDialogObjectSelected;
			MyTab.TabParent.AddSlaveTab (MyTab, SelectDialog);
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

		protected void OnButtonOpenClicked (object sender, EventArgs e)
		{
			if (SubjectType == null)
				throw new NullReferenceException ("SubjectType не задан.");

			if (OrmMain.GetObjectDescription (SubjectType).SimpleDialog) {
				OrmSimpleDialog.RunSimpleDialog (this.Toplevel as Window, SubjectType, Subject);
				return;
			}

			ITdiTab dlg = OrmMain.CreateObjectDialog (Subject);
			MyTab.TabParent.AddTab (dlg, MyTab);
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

			var dlg = OrmMain.FindMyDialog (this);

			if (dlg != null)
				localUoW = dlg.UoW;
			else
				localUoW = UnitOfWorkFactory.CreateWithoutRoot ();

			if (ItemsQuery != null) {
				ItemsCriteria = ItemsQuery.DetachedCriteria.GetExecutableCriteria (localUoW.Session);
			} else {
				if (ItemsCriteria == null)
					ItemsCriteria = localUoW.Session.CreateCriteria (subjectType);
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
				UpdateWidgetNew ();
		}

		public override void Destroy ()
		{
			logger.Debug ("EntryReference Destroy() called.");
			SubjectType = null;
			base.Destroy ();
		}
	}
}

