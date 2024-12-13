using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NLog;
using QS;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Services;
using QS.Tdi;
using QS.Utilities;
using QS.Utilities.Text;
using QSOrmProject.DomainMapping;
using QSOrmProject.UpdateNotification;

namespace QSOrmProject
{
	[WidgetWindow(DefaultWidth = 852, DefaultHeight = 600)]
	public partial class OrmReference : ReferenceBase, ITdiJournal
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private IUnitOfWork uow;
		private ICriteria objectsCriteria;
		private IReferenceFilter filterWidget;
		private DateTime searchStarted;
		private bool inUpdating;
		private IList fullList;
		private IList viewList;
		private bool isOwnerUow;

		int number = -1;

		public ITdiTabParent TabParent { set; get; }
		public HandleSwitchIn HandleSwitchIn { get; private set; }
		public HandleSwitchOut HandleSwitchOut { get; private set; }

		public override bool FailInitialize => base.FailInitialize;

		/// <summary>
		/// Для хранения пользовательской информации как в WinForms
		/// </summary>
		public object Tag;

		public bool CompareHashName(string hashName)
		{
			return GenerateHashName(objectType) == hashName;
		}

		public static string GenerateHashName<TEntity>()
		{
			return GenerateHashName(typeof(TEntity));
		}

		public static string GenerateHashName(Type clazz)
		{
			return String.Format("Journal_{0}", clazz.Name);
		}

		public event EventHandler<OrmReferenceObjectSectedEventArgs> ObjectSelected;

		public override IUnitOfWork UoW {
			get {
				if (uow == null) {
					uow = ServicesConfig.UnitOfWorkFactory.Create();
					isOwnerUow = true;
				}
				return uow;
			}
			set { uow = value; }
		}

		System.Type filterClass;

		public System.Type FilterClass {
			get { return filterClass; }
			set {
				if (filterClass == value)
					return;
				if (filterWidget != null) {
					hboxFilter.Remove(filterWidget as Widget);
					(filterWidget as Widget).Destroy();
					checkShowFilter.Visible = true;
					filterWidget = null;
				}
				if(value != null && value.GetInterface("IReferenceFilter") == null) {
					throw new NotSupportedException("FilterClass должен реализовывать интерфейс IReferenceFilter.");
				}
				filterClass = value;
				checkShowFilter.Visible = filterClass != null;
				if (CheckDefaultLoadFilterAtt() || checkShowFilter.Active)
					CreateFilterWidget();
			}
		}

		private ITableView tableView;

		public ITableView TableView
		{
			get
			{
				return tableView;
			}
			set
			{
				tableView = value;
				ytreeviewRef.ColumnsConfig = tableView != null ? tableView.GetGammaColumnsConfig() : null;
				SearchProvider = tableView != null ? tableView.SearchProvider : null;
			}
		}

		private ISearchProvider searchProvider;

		public ISearchProvider SearchProvider
		{
			get
			{
				return searchProvider;
			}
			set
			{
				searchProvider = value;
				hboxSearch.Visible = searchProvider != null;
			}
		}

		private OrmReferenceMode mode;

		public OrmReferenceMode Mode {
			get { return mode; }
			set {
				mode = value;
				hboxSelect.Visible = (mode == OrmReferenceMode.Select || mode == OrmReferenceMode.MultiSelect);
				ytreeviewRef.Selection.Mode = (mode == OrmReferenceMode.MultiSelect) ? SelectionMode.Multiple : SelectionMode.Single;
			}
		}

		private ReferenceButtonMode buttonMode;

		public ReferenceButtonMode ButtonMode {
			get { return buttonMode; }
			set {
				buttonMode = value;
				buttonAdd.Sensitive = CanCreate;
				OnTreeviewSelectionChanged(this, EventArgs.Empty);
				Image image = new Image();
				image.Pixbuf = Stetic.IconLoader.LoadIcon(
					this,
					buttonMode.HasFlag(ReferenceButtonMode.TreatEditAsOpen) ? "gtk-open" : "gtk-edit",
					IconSize.Menu);
				buttonEdit.Image = image;
				buttonEdit.Label = buttonMode.HasFlag(ReferenceButtonMode.TreatEditAsOpen) ? "Открыть" : "Изменить";
			}
		}

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;

		private string _tabName = "Справочник";

		public string TabName {
			get { return _tabName; }
			set {
				if (_tabName == value)
					return;
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged(this, new TdiTabNameChangedEventArgs(value));
			}
		}

		public bool? UseSlider
		{
			get
			{
				IOrmObjectMapping map = OrmMain.GetObjectDescription(objectType);
				if (map == null)
					return null;

				return map.DefaultUseSlider;
			}
		}

		//Проверка флага происходит первой для того чтобы программно можно было в обязательном порядке включить кнопку игнорируя проверку прав
		private bool CanCreate => ButtonMode.HasFlag(ReferenceButtonMode.CanAdd) || permissionResult.CanCreate;
		private bool CanEdit => ButtonMode.HasFlag(ReferenceButtonMode.CanEdit) || permissionResult.CanUpdate;
		private bool CanDelete => ButtonMode.HasFlag(ReferenceButtonMode.CanDelete) || permissionResult.CanDelete;

		public OrmReference(System.Type objType)
			: this(objType, ServicesConfig.UnitOfWorkFactory.Create())
		{
			isOwnerUow = true;
		}

		public OrmReference(System.Type objType, IUnitOfWork uow)
			: this(objType, uow, uow.Session.CreateCriteria(objType))
		{
		}

		public OrmReference(QueryOver query)
			: this(ServicesConfig.UnitOfWorkFactory.Create(), query) {
			isOwnerUow = true;
		}

		public OrmReference(IUnitOfWork uow, QueryOver query)
			: this(query.DetachedCriteria.GetRootEntityTypeIfAvailable(), uow, query.DetachedCriteria.GetExecutableCriteria(uow.Session))
		{
		}

		public OrmReference(System.Type objType, IUnitOfWork uow, ICriteria listCriteria)
		{
			this.Build();
			objectType = objType;
			objectsCriteria = listCriteria;
			UoW = uow;
			InitializePermissionValidator();
			ConfigureDlg();
		}
		#region Конструкторы для обхода бага GLib.MissingIntPtrCtorException: GLib.Object 
		public OrmReference(IntPtr raw) : base(raw){}
		protected OrmReference(){}
		#endregion

		void ConfigureDlg()
		{
			OrmMain.Count++;
			number = OrmMain.Count;
			logger.Debug("Открытие {0}", number);
			Mode = OrmReferenceMode.Normal;
			ButtonMode = ReferenceButtonMode.CanAll;
			IOrmObjectMapping map = OrmMain.GetObjectDescription(objectType);
			if (map != null) {
				map.ObjectUpdated += OnRefObjectUpdated;
				TableView = map.TableView;
				if(map.RefFilterClass != null)
					FilterClass = map.RefFilterClass;
			} else
				throw new InvalidOperationException(String.Format("Для использования диалога, класс {0} должен быть добавлен в мапинг OrmMain.ClassMappingList.", objectType));
			object[] att = objectType.GetCustomAttributes(typeof(AppellativeAttribute), true);
			if(att.Length > 0) {
				this.TabName = (att[0] as AppellativeAttribute).NominativePlural.StringToTitleCase();
			}
			var defaultMode = objectType.GetAttribute<DefaultReferenceButtonModeAttribute>(true);
			if(defaultMode != null)
				ButtonMode = defaultMode.ReferenceButtonMode;

			UpdateObjectList();
			ytreeviewRef.Selection.Changed += OnTreeviewSelectionChanged;
		}

		void OnRefObjectUpdated(object sender, OrmObjectUpdatedEventArgs e)
		{
			//Обновляем загруженные сущности так как в методе UpdateObjectList обновится только список(добавятся новые), но уже загруженные возьмутся из кеша.
			foreach (var entity in e.UpdatedSubjects.OfType<IDomainObject>()) {
				var curEntity = fullList.OfType<IDomainObject>().FirstOrDefault(o => o.Id == entity.Id);
				if (curEntity != null)
					UoW.Session.Refresh(curEntity);
			}
			UpdateObjectList();
		}

		private void UpdateObjectList()
		{
			if (inUpdating)
				return;
			logger.Debug("Обновление в диалоге {0}", number);
			logger.Info("Получаем таблицу справочника<{0}>...", objectType.Name);
			var startUpdateTime = DateTime.Now;
			inUpdating = true;
			ICriteria baseCriteria = filterWidget == null ? objectsCriteria : filterWidget.FiltredCriteria;

			var map = OrmMain.GetObjectDescription(objectType);
			if (map.SimpleDialog)
				baseCriteria = baseCriteria.AddOrder(Order.Asc("Name"));
			else if (map.TableView != null && map.TableView.OrderBy.Count > 0) {
				foreach (var ord in map.TableView.OrderBy) {
					if (ord.Direction == QSOrmProject.DomainMapping.OrderDirection.Desc)
						baseCriteria = baseCriteria.AddOrder(Order.Desc(ord.PropertyName));
					else
						baseCriteria = baseCriteria.AddOrder(Order.Asc(ord.PropertyName));
				}
			}

			fullList = baseCriteria.List();

			UpdateTreeViewSource();

			UpdateSum();
			inUpdating = false;
			logger.Debug("Справочник загружен за: {0} сек.", (DateTime.Now - startUpdateTime).TotalSeconds);
			logger.Info("Ok.");
		}

		void SearchRefilter()
		{
			searchStarted = DateTime.Now;
			ytreeviewRef.SearchHighlightText = entrySearch.Text;
			viewList = SearchProvider.FilterList(fullList, entrySearch.Text);
			var delayfilter = DateTime.Now.Subtract(searchStarted);
			logger.Debug("В поиске обработано {0} элементов за {1} секунд, в среднем по {2} милисекунды на элемент.",
				fullList.Count, delayfilter.TotalSeconds, delayfilter.TotalMilliseconds / fullList.Count);
			ytreeviewRef.ItemsDataSource = viewList;
			var loadDelay = DateTime.Now.Subtract(searchStarted) - delayfilter;
			logger.Debug("Загрузка таблицы {0} миллисекунд.", loadDelay.TotalMilliseconds);
			UpdateSum();
		}

		private void UpdateTreeViewSource()
		{
			if(SearchProvider != null && !String.IsNullOrWhiteSpace(entrySearch.Text))
			{
				SearchRefilter();
			}
			else
			{
				ytreeviewRef.SearchHighlightText = null;
				viewList = fullList;
				if(TableView.RecursiveTreeConfig != null)
				{
					ytreeviewRef.YTreeModel = TableView.RecursiveTreeConfig.CreateModel(viewList);
				}
				else
				{
					ytreeviewRef.ItemsDataSource = viewList;
				}
			}
		}

		void OnTreeviewSelectionChanged(object sender, EventArgs e)
		{
			bool selected = ytreeviewRef.Selection.CountSelectedRows() > 0;
			buttonSelect.Sensitive = selected;
			buttonEdit.Sensitive = CanEdit && selected;
			buttonDelete.Sensitive = CanDelete && selected;
		}

		void FilterViewChanged(object aList)
		{
			UpdateSum();
		}

		protected void OnButtonSearchClearClicked(object sender, EventArgs e)
		{
			entrySearch.Text = String.Empty;
		}

		protected void OnEntrySearchChanged(object sender, EventArgs e)
		{
			UpdateTreeViewSource();
		}

		protected void OnCloseTab(CloseSource source)
		{
			logger.Debug("Закрытие диалога {0}", number);
			TabParent.ForceCloseTab(this, source);
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			if(!CanCreate) {
				MessageDialogHelper.RunWarningDialog("У вас нет прав для создания этого документа.");
				return;
			}
			ytreeviewRef.Selection.UnselectAll();
			if (OrmMain.GetObjectDescription(objectType).SimpleDialog) {
				SelectObject(EntityEditSimpleDialog.RunSimpleDialog(this.Toplevel as Window, objectType, null));
			} else {
				var tab = TabParent.OpenTab(DialogHelper.GenerateDialogHashName(objectType, 0),
					() => OrmMain.CreateObjectDialog(objectType), this
				);
				if(tab != null)
					(tab as ITdiDialog).EntitySaved += NewItemDlg_EntitySaved;
			}
		}

		private void SelectObject(object item)
		{
			if (item == null)
				return;

			var ownItem = viewList.OfType<IDomainObject>().FirstOrDefault(i => i.Id == DomainHelper.GetId(item));
			if (ownItem == null)
				return;
			ytreeviewRef.SelectObject(ownItem);
		}

		void NewItemDlg_EntitySaved(object sender, EntitySavedEventArgs e)
		{
			if (e.TabClosed)
				SelectObject(e.Entity);
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			if(!CanEdit) {
				MessageDialogHelper.RunWarningDialog("У вас нет прав для редактирования этого документа.");
				return;
			}
			if (OrmMain.GetObjectDescription(objectType).SimpleDialog) {
				EntityEditSimpleDialog.RunSimpleDialog(this.Toplevel as Window, objectType, ytreeviewRef.GetSelectedObjects().First());
			} else {
				var selected = ytreeviewRef.GetSelectedObjects();

				var text = NumberToTextRus.FormatCase(selected.Length,
					"Вы действительно хотите открыть {0} вкладку?",
					"Вы действительно хотите открыть {0} вкладки?",
					"Вы действительно хотите открыть {0} вкладок?"
				);

				if(selected.Length > 5 && !MessageDialogHelper.RunQuestionDialog(text))
					return;

				foreach(var one in selected) {
					TabParent.OpenTab(DialogHelper.GenerateDialogHashName(objectType, DomainHelper.GetId(one)),
						() => OrmMain.CreateObjectDialog(objectType, one), 
						this, 
						canSlided: selected.Length == 1
					);
				}
			}
		}

		protected void UpdateSum()
		{
			labelSum.LabelProp = String.Format("Количество: {0}", viewList.Count);
			logger.Debug("Количество обновлено {0}", viewList.Count);
		}

		protected void OnYtreeviewRefRowActivated(object o, RowActivatedArgs args)
		{
			if (Mode == OrmReferenceMode.Select || Mode == OrmReferenceMode.MultiSelect)
				buttonSelect.Click();
			else if (ButtonMode.HasFlag(ReferenceButtonMode.CanEdit))
				buttonEdit.Click();
		}

		protected void OnButtonSelectClicked(object sender, EventArgs e)
		{
			if (ObjectSelected != null)
			{
				var selected = ytreeviewRef.GetSelectedObjects();
				logger.Debug("Выбрано {0} id:({1})", objectType,
					String.Join(", ", selected.Select(x => DomainHelper.GetId(x).ToString()))
				);
				ObjectSelected(this, new OrmReferenceObjectSectedEventArgs(
						selected,
						Tag
					));
			}
			else
				logger.Warn("Никто не подписался на событие выбора в диалоге.");
			OnCloseTab(CloseSource.Self);
		}

		private void CreateFilterWidget()
		{
			Type[] paramTypes = new Type[] { typeof(IUnitOfWork) };

			System.Reflection.ConstructorInfo ci = filterClass.GetConstructor(paramTypes);
			if (ci == null) {
				InvalidOperationException ex = new InvalidOperationException(
												   String.Format("Конструктор в классе фильтра {0} c параметрами({1}) не найден.", filterClass.ToString(), NHibernate.Util.CollectionPrinter.ToString(paramTypes)));
				logger.Error(ex);
				throw ex;
			}
			logger.Debug("Вызываем конструктор фильтра {0} c параметрами {1}.", filterClass.ToString(), NHibernate.Util.CollectionPrinter.ToString(paramTypes));
			filterWidget = (IReferenceFilter)ci.Invoke(new object[] { UoW });
			filterWidget.BaseCriteria = objectsCriteria;
			filterWidget.Refiltered += OnFilterWidgetRefiltered;
			hboxFilter.Add(filterWidget as Widget);
			(filterWidget as Widget).ShowAll();
		}

		protected void OnCheckShowFilterToggled(object sender, EventArgs e)
		{
			hboxFilter.Visible = checkShowFilter.Active;
			if (checkShowFilter.Active && filterWidget == null)
				CreateFilterWidget();
		}

		void OnFilterWidgetRefiltered(object sender, EventArgs e)
		{
			UpdateObjectList();
		}

		private bool CheckDefaultLoadFilterAtt()
		{
			if (FilterClass == null)
				return false;
			foreach (var att in FilterClass.GetCustomAttributes(typeof(OrmDefaultIsFilteredAttribute), true)) {
				return (att as OrmDefaultIsFilteredAttribute).DefaultIsFiltered;
			}
			return false;
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			if(!CanDelete) {
				MessageDialogHelper.RunWarningDialog("У вас нет прав для удаления этого документа.");
				return;
			}
			var toDelete = ytreeviewRef.GetSelectedObjects();

			var text = NumberToTextRus.FormatCase(toDelete.Length,
				"Вы собираетесь удалить {0} объект, вам нужно будет проверить ссылки для каждого из них. Продолжить?",
				"Вы собираетесь удалить {0} объекта, вам нужно будет проверить ссылки для каждого из них. Продолжить?",
				"Вы собираетесь удалить {0} объектов, вам нужно будет проверить ссылки для каждого из них. Продолжить?");

			if(toDelete.Length > 1 && !MessageDialogHelper.RunQuestionDialog(text)) {
				return;
			}

			foreach(var delete in toDelete) {
				OrmMain.DeleteObject(delete);
			}

			UpdateObjectList();
		}

		public override void Destroy()
		{
			logger.Debug("OrmReference #{0} Destroy() called.", number);
			IOrmObjectMapping map = OrmMain.GetObjectDescription(objectType);
			if (map != null) {
				map.ObjectUpdated -= OnRefObjectUpdated;
			}
			if(isOwnerUow)
				UoW.Dispose();
			base.Destroy();
		}

		[GLib.ConnectBefore]
		protected void OnYtreeviewRefButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			IOrmObjectMapping map = OrmMain.GetObjectDescription(objectType);

			if(args.Event.Button == 3 && map.PopupMenuExist)
			{
				var selected = ytreeviewRef.GetSelectedObjects();
				var menu = map.GetPopupMenu(selected);
				if(menu != null)
				{
					menu.ShowAll();
					menu.Popup();
				}
			}
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			UpdateObjectList();
		}
	}

	public enum OrmReferenceMode
	{
		Normal,
		Select,
		MultiSelect
	}

	public class OrmReferenceObjectSectedEventArgs : EventArgs
	{
		public object Subject { get { return Subjects[0]; } }

		public object[] Subjects { get; private set; }

		public object Tag { get; private set; }

		public IEnumerable<TEntity> GetEntities<TEntity>()
		{
			return Subjects.Cast<TEntity>();
		}

		public OrmReferenceObjectSectedEventArgs(object[] subjects, object tag)
		{
			Subjects = subjects;
			Tag = tag;
		}
	}

	public interface ISpecialRowsRender
	{
		string TextColor { get; }
	}
}
