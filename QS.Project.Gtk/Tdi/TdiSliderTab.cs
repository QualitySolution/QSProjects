using System;
using System.Collections.Generic;
using Gtk;
using NLog;
using QS.Navigation;
using QS.Tdi.Gtk;

namespace QS.Tdi
{
	public class TdiSliderTab : HBox, ITdiTab, ITdiTabParent, ITdiTabWithPath, ITdiSliderTab
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private Widget journalWidget;
		private Widget activeGlgWidget;
		private ITdiJournal journal;
		private ITdiTab activeDialog;
		private VBox boxSeparator;
		private VSeparator separatorUpper, separatorLower;
		private VBox dialogVBox;
		private Label dialogTilteLabel;
		private Button buttonHide;

		public ITdiTabParent TabParent { set; get; }

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler TabClosed;
		public HandleSwitchIn HandleSwitchIn { get; private set; }
		public HandleSwitchOut HandleSwitchOut { get; private set; }

		#region Внешние зависимости
		readonly ITDIWidgetResolver widgetResolver;
		#endregion

		public TdiSliderTab(ITdiJournal jour, ITDIWidgetResolver widgetResolver)
		{
			this.widgetResolver = widgetResolver ?? throw new ArgumentNullException(nameof(widgetResolver));
			Journal = jour;
			HandleSwitchIn = OnSwitchIn;
			HandleSwitchOut = OnSwitchOut;
		}

		public string TabName {
			get {
				if(activeDialog != null)
					return String.Format("[{0}] {1}", journal.TabName, activeDialog.TabName);
				else if(journal != null)
					return journal.TabName;
				else
					return "Пустой слайдер";
			}
			set {
				throw new NotSupportedException("Нельзя напрямую задавать имя вкладки.");
			}

		}

		private void OnSwitchIn(ITdiTab tabFrom)
		{
			if(journal != null && journal.HandleSwitchIn != null)
				journal.HandleSwitchIn(tabFrom);
			if(activeDialog != null && activeDialog.HandleSwitchIn != null)
				activeDialog.HandleSwitchIn(tabFrom);
		}

		private void OnSwitchOut(ITdiTab tabTo)
		{
			if(journal != null && journal.HandleSwitchOut != null)
				journal.HandleSwitchOut(tabTo);
			if(activeDialog != null && activeDialog.HandleSwitchOut != null)
				activeDialog.HandleSwitchOut(tabTo);
		}

		public ITdiTab FindTab(string hashName, string masterHashName = null)
		{
			return TabParent.FindTab(hashName, masterHashName);
		}

		public void SwitchOnTab(ITdiTab tab)
		{
			TabParent.SwitchOnTab(tab);
		}

		public bool CompareHashName(string hashName)
		{
			return (Journal != null ? Journal.CompareHashName(hashName) : false)
			|| (ActiveDialog != null ? ActiveDialog.CompareHashName(hashName) : false);
		}

		public ITdiJournal Journal {
			get {
				return journal;
			}
			set {
				if(journal != value) {
					if(journal != null) {
						journal.TabNameChanged -= OnJournalTabNameChanged;
						if(journalWidget != null) {
							this.Remove(journalWidget);
							journalWidget.Destroy();
						}
					}
					journal = value;
					journal.TabNameChanged += OnJournalTabNameChanged;
					journalWidget = widgetResolver.Resolve(value);

					if(journalWidget == null)
						throw new InvalidCastException($"Ошибка приведения типа {nameof(ITdiTab)} к типу {nameof(Widget)}.");

					this.PackStart(journalWidget);
					journalWidget.Show();
					journal.TabParent = this;
				}
			}
		}

		public ITdiTab ActiveDialog {
			get {
				return activeDialog;
			}
			set {
				if(activeDialog != value) {
					//Remove
					if(activeDialog != null) {
						activeDialog.TabNameChanged -= ActiveDialog_TabNameChanged;
						dialogVBox.Destroy();
						this.Remove(dialogVBox);
						this.Remove(boxSeparator);
					}
					//Add
					if(value != null) {
						separatorUpper = new VSeparator();
						separatorUpper.Show();
						separatorLower = new VSeparator();
						separatorLower.Show();
						dialogTilteLabel = new Label();

						buttonHide = new Button();
						buttonHide.Label = IsHideJournal ? ">" : "<";
						buttonHide.Clicked += OnButtonHideClicked;
						boxSeparator = new VBox();
						boxSeparator.PackStart(separatorUpper, true, true, 0);
						boxSeparator.PackStart(buttonHide, false, false, 0);
						boxSeparator.PackEnd(separatorLower, true, true, 0);

						dialogVBox = new VBox();
						activeGlgWidget = widgetResolver.Resolve(value);

						if(activeGlgWidget == null)
							throw new InvalidCastException($"Ошибка приведения типа {nameof(ITdiTab)} к типу {nameof(Widget)}.");

						dialogVBox.PackStart(activeGlgWidget);
						this.PackEnd(dialogVBox);
						this.PackEnd(boxSeparator, false, true, 6);
						value.TabParent = this;
						value.TabNameChanged += ActiveDialog_TabNameChanged;

						buttonHide.Show();
						boxSeparator.Show();
						dialogVBox.Show();
						activeGlgWidget.Show();

						(TabParent as TdiNotebook).OnSliderTabAdded(this, value);
					}
					//Show journal if dialog is closed
					if(IsHideJournal && value == null) {
						IsHideJournal = false;
					}

					//Switch
					activeDialog = value;
					if(activeDialog != null)
						ActiveDialog_TabNameChanged(this, new TdiTabNameChangedEventArgs(activeDialog.TabName));
					ITdiTab currentTab = ActiveDialog ?? this;
					(TabParent as TdiNotebook).OnSliderTabSwitched(this, currentTab);

					//I-867 Открыть "контрагенты" при создании нового заказа из журнала.
					(value as ITdiTabAddedNotifier)?.OnTabAdded();
				}
			}
		}
		public bool IsHideJournal {
			get {
				return Journal == null || !(journalWidget).Visible;
			}
			set {
				if(Journal == null)
					return;
				journalWidget.Visible = !value;
				buttonHide.Label = IsHideJournal ? ">" : "<";
			}
		}

		void ActiveDialog_TabNameChanged(object sender, TdiTabNameChangedEventArgs e)
		{
			SetNewDialogTitle(e.NewName);
			OnSladerTabChanged();
		}

		void SetNewDialogTitle(string tilte)
		{
			dialogTilteLabel.Markup = String.Format("<b>{0}</b>", tilte);
		}

		void OnJournalTabNameChanged(object sender, TdiTabNameChangedEventArgs arg)
		{
			OnSladerTabChanged();
		}

		private void OnSladerTabChanged()
		{
			if(TabNameChanged != null)
				TabNameChanged(this, new TdiTabNameChangedEventArgs(TabName));
			OnPathUpdate();
		}

		#region Методы закрытия

		public bool AskToCloseTab(ITdiTab tab, CloseSource source = CloseSource.External)
		{
			if(tab == ActiveDialog) 
				return CloseDialog(source, true);

			if(tab == Journal)
				return TabParent.AskToCloseTab(this, source);

			return TabParent.AskToCloseTab(tab, source);
		}

		public void ForceCloseTab(ITdiTab tab, CloseSource source = CloseSource.External)
		{
			if(tab == ActiveDialog) {
				CloseDialog(source, false);
				return;
			}

			if(tab == Journal) {
				TabParent.ForceCloseTab(this, source);
				return;
			}
			TabParent.ForceCloseTab(tab, source);
		}

		public void OnTabClosed()
		{
			if(ActiveDialog != null)
				ActiveDialog.OnTabClosed();

			if(Journal != null)
				Journal.OnTabClosed();

			TabClosed?.Invoke(this, EventArgs.Empty);
		}

		protected bool CloseDialog(CloseSource source, bool AskSave)
		{
			if(TabParent.CheckClosingSlaveTabs(this as ITdiTab))
				return false;

			if (ActiveDialog is ITdiDialog dlg) {
				if (AskSave && dlg.HasChanges) {
					string Message = "Объект изменён. Сохранить изменения перед закрытием?";
					MessageDialog md = new MessageDialog((Window)this.Toplevel, DialogFlags.Modal,
										   MessageType.Question,
										   ButtonsType.YesNo,
										   Message);
					md.AddButton("Отмена", ResponseType.Cancel);
					int result = md.Run();
					md.Destroy();
					if (result == (int)ResponseType.Cancel)
						return false;
					if (result == (int)ResponseType.Yes) {
						if (!dlg.Save()) {
							logger.Warn("Объект не сохранён. Отмена закрытия...");
							return false;
						}
					}
				}
			}
			var oldTab = ActiveDialog;
			ActiveDialog.OnTabClosed();
			ActiveDialog = null;
			activeGlgWidget.Destroy();
			(TabParent as TdiNotebook)?.OnSliderTabClosed(this, oldTab, source);
			OnSladerTabChanged();
			return true;
		}

		#endregion

		public void AddSlaveTab(ITdiTab masterTab, ITdiTab slaveTab)
		{
			if(slaveTab.FailInitialize) {
				logger.Warn("Вкладка <{0}> не добавлена, так как сообщает что построена с ошибкой(Свойство FailInitialize) .",
					slaveTab.TabName
				);
				return;
			}

			if(masterTab == Journal || masterTab == ActiveDialog)
				TabParent.AddSlaveTab(this as ITdiTab, slaveTab);
			else
				TabParent.AddSlaveTab(masterTab, slaveTab);
		}

		/// <summary>
		/// Добавить вкладку, указав, скрыт ли по умолчанию журнал.
		/// </summary>
		/// <param name="tab">Основная вкладка.</param>
		/// <param name="afterTab">Дочерняя вкладка.</param>
		/// <param name="canSlided">Если true, то открываются в одной вкладке.</param>
		public void AddTab(ITdiTab tab, ITdiTab afterTab, bool canSlided = true)
		{
			if(tab.FailInitialize) {
				logger.Warn("Вкладка <{0}> не добавлена, так как сообщает что построена с ошибкой(Свойство FailInitialize) .",
					tab.TabName
				);
				return;
			}

			if(canSlided && afterTab == Journal) {
				ActiveDialog = tab;
				return;
			}

			if(afterTab == null || afterTab == Journal || afterTab == ActiveDialog)
				TabParent.AddTab(tab, this as ITdiTab);
			else
				TabParent.AddTab(tab, afterTab);
		}

		public bool FailInitialize {
			get {
				return Journal != null ? Journal.FailInitialize : false;
			}
		}

		public bool CheckClosingSlaveTabs(ITdiTab tab)
		{
			//FIXME Если появятся подчиненые вкладки у журналов, нужно переделать проверку, что бы при закрыти диалога не требовалось зарывать подчиненные вкладки журнала.
			if(tab == Journal || tab == ActiveDialog)
				return TabParent.CheckClosingSlaveTabs(this as ITdiTab);
			else
				return TabParent.CheckClosingSlaveTabs(tab);
		}

		#region Методы открытия вкладки

		public ITdiTab OpenTab(Func<ITdiTab> newTabFunc, ITdiTab afterTab = null, Type[] argTypes = null, object[] args = null)
		{
			ITdiTab tab = newTabFunc.Invoke();
			Type tabType = tab.GetType();
			string hashName = TabHashHelper.GetTabHash(tabType, argTypes ?? new Type[] { }, args ?? new object[] { });
			return OpenTab(hashName, () => tab, afterTab);

		}

		public ITdiTab OpenTab(string hashName, Func<ITdiTab> newTabFunc, ITdiTab afterTab = null, bool canSlided = true)
		{
			ITdiTab tab = FindTab(hashName);

			if(tab != null) {
				SwitchOnTab(tab);
				return tab;
			}

			if(afterTab == Journal && ActiveDialog != null) {
				 CloseDialog(CloseSource.FromParentPage, true);
				if(ActiveDialog != null)
					return null;
			}

			tab = newTabFunc();
			AddTab(tab, afterTab, canSlided);
			return tab;
		}

		public ITdiTab OpenTab<TTab>(ITdiTab afterTab = null) where TTab : ITdiTab
		{
			return TabHashHelper.OpenTabSelfCreateTab(this, typeof(TTab), new Type[] { }, new object[] { }, afterTab);
		}

		public ITdiTab OpenTab<TTab, TArg1>(TArg1 arg1, ITdiTab afterTab = null) where TTab : ITdiTab
		{
			return TabHashHelper.OpenTabSelfCreateTab(this, typeof(TTab), new Type[] { typeof(TArg1) }, new object[] { arg1 }, afterTab);
		}

		public ITdiTab OpenTab<TTab, TArg1, TArg2>(TArg1 arg1, TArg2 arg2, ITdiTab afterTab = null) where TTab : ITdiTab
		{
			return TabHashHelper.OpenTabSelfCreateTab(this, typeof(TTab), new Type[] { typeof(TArg1), typeof(TArg2) }, new object[] { arg1, arg2 }, afterTab);
		}

		public ITdiTab OpenTab<TTab, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, ITdiTab afterTab = null) where TTab : ITdiTab
		{
			return TabHashHelper.OpenTabSelfCreateTab(this, typeof(TTab), new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) }, new object[] { arg1, arg2, arg3 }, afterTab);
		}

		#endregion

		public override void Destroy()
		{
			if(activeGlgWidget != null)
				activeGlgWidget.Destroy();
			journalWidget.Destroy();
			base.Destroy();
		}

		private void OnButtonHideClicked(object sender, EventArgs e)
		{
			IsHideJournal = !IsHideJournal;
		}


		#region ITdiTabWithPath implementation

		public event EventHandler PathChanged;

		public string[] PathNames {
			get {
				var names = new List<string>();
				if(Journal != null)
					names.Add(Journal.TabName);
				if(ActiveDialog != null)
					names.Add(ActiveDialog.TabName);
				return names.ToArray();
			}
		}

		protected void OnPathUpdate()
		{
			if(PathChanged != null)
				PathChanged(this, EventArgs.Empty);
		}

		#endregion
	}
}

