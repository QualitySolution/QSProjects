using System;
using Gtk;
using NLog;
using System.Collections.Generic;

namespace QS.Tdi.Gtk
{
	public class TdiSliderTab : HBox, ITdiTab, ITdiTabParent, ITdiTabWithPath
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ITdiJournal journal;
		private ITdiDialog activeDialog;
		private VBox boxSeparator;
		private VSeparator separatorUpper, separatorLower;
		private VBox dialogVBox;
		private Label dialogTilteLabel;
		private Button buttonHide;

		public ITdiTabParent TabParent { set; get; }

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;
		public HandleSwitchIn HandleSwitchIn { get; private set; }
		public HandleSwitchOut HandleSwitchOut { get; private set; }

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
						journal.CloseTab -= OnJournalClose;
						this.Remove((Widget)journal);
					}
					journal = value;
					journal.TabNameChanged += OnJournalTabNameChanged;
					journal.CloseTab += OnJournalClose;
					this.PackStart((Widget)Journal);
					(Journal as Widget).Show();
					journal.TabParent = this;
				}
			}
		}

		public ITdiDialog ActiveDialog {
			get {
				return activeDialog;
			}
			set {
				if(activeDialog != value) {
					//Remove
					if(activeDialog != null) {
						activeDialog.CloseTab -= OnDialogClose;
						activeDialog.TabNameChanged -= ActiveDialog_TabNameChanged;
						dialogVBox.Destroy();
						this.Remove(dialogVBox);
						this.Remove(boxSeparator);
						(TabParent as TdiNotebook).OnSliderTabClosed(this, activeDialog);
					}
					//Add
					if(value != null) {
						value.CloseTab += OnDialogClose;
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
						dialogVBox.PackStart((Widget)value);
						this.PackEnd(dialogVBox);
						this.PackEnd(boxSeparator, false, true, 6);
						value.TabParent = this;
						value.TabNameChanged += ActiveDialog_TabNameChanged;

						buttonHide.Show();
						boxSeparator.Show();
						dialogVBox.Show();
						(value as Widget).Show();

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
					ITdiTab currentTab = ActiveDialog != null ? ActiveDialog as ITdiTab : this as ITdiTab;
					(TabParent as TdiNotebook).OnSliderTabSwitched(this, currentTab);

					//I-867 Открыть "контрагенты" при создании нового заказа из журнала.
					(value as ITdiTabAddedNotifier)?.OnTabAdded();
				}
			}
		}
		public bool IsHideJournal {
			get {
				return Journal == null || !((Widget)Journal).Visible;
			}
			set {
				if(Journal == null)
					return;
				((Widget)Journal).Visible = !value;
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

		protected void OnJournalClose(object sender, TdiTabCloseEventArgs arg)
		{
			if(CloseTab != null)
				CloseTab(this, arg);
		}

		protected void OnDialogClose(object sender, TdiTabCloseEventArgs arg)
		{
			if(TabParent.CheckClosingSlaveTabs(this as ITdiTab))
				return;

			ITdiDialog dlg = sender as ITdiDialog;
			if(arg.AskSave && dlg.HasChanges) {
				string Message = "Объект изменён. Сохранить изменения перед закрытием?";
				MessageDialog md = new MessageDialog((Window)this.Toplevel, DialogFlags.Modal,
									   MessageType.Question,
									   ButtonsType.YesNo,
									   Message);
				md.AddButton("Отмена", ResponseType.Cancel);
				int result = md.Run();
				md.Destroy();
				if(result == (int)ResponseType.Cancel)
					return;
				if(result == (int)ResponseType.Yes) {
					if(!dlg.Save()) {
						logger.Warn("Объект не сохранён. Отмена закрытия...");
						return;
					}
				}
			}
			ActiveDialog = null;
			(dlg as Widget).Destroy();
			OnSladerTabChanged();
		}

		public TdiSliderTab(ITdiJournal jour)
		{
			Journal = jour;
			HandleSwitchIn = OnSwitchIn;
			HandleSwitchOut = OnSwitchOut;
		}

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
		/// <param name="CanSlided">Если true, то открываются в одной вкладке.</param>
		public void AddTab(ITdiTab tab, ITdiTab afterTab, bool CanSlided = true)
		{
			if(tab.FailInitialize) {
				logger.Warn("Вкладка <{0}> не добавлена, так как сообщает что построена с ошибкой(Свойство FailInitialize) .",
					tab.TabName
				);
				return;
			}

			if(CanSlided && afterTab == Journal && tab is ITdiDialog) {
				ActiveDialog = (ITdiDialog)tab;
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

		public ITdiTab OpenTab(string hashName, Func<ITdiTab> newTabFunc, ITdiTab afterTab = null)
		{
			ITdiTab tab = FindTab(hashName);

			if(tab != null) {
				SwitchOnTab(tab);
				return tab;
			}

			if(afterTab == Journal && ActiveDialog != null) {
				OnDialogClose(ActiveDialog, new TdiTabCloseEventArgs(true));
				if(ActiveDialog != null)
					return null;
			}

			tab = newTabFunc();
			AddTab(tab, afterTab);
			return tab;
		}

		public override void Destroy()
		{
			if(ActiveDialog != null)
				(ActiveDialog as Widget).Destroy();
			(Journal as Widget).Destroy();
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

