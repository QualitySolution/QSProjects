using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.Config;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Dialogs.GtkUI.JournalActions;
using QS.RepresentationModel.GtkUI;
using QS.Utilities.Text;

namespace QS.Project.Dialogs.GtkUI
{
	[Obsolete("Используется только в ВОДОВОЗЕ. Удалить, когда удалите из водовоза.")]
	public partial class RepresentationJournalDialog : SingleUowTabBase, IJournalDialog
	{
        private static Logger logger = LogManager.GetCurrentClassLogger ();
		private IRepresentationModel representationModel;

		public bool? UseSlider => EntityConfig?.DefaultUseSlider;

		#region Hash

		public bool CompareHashName(string hashName)
        {
            return GenerateHashName(RepresentationModel.GetType()) == hashName;
        }

        public static string GenerateHashName<TRepresentation>() where TRepresentation : IRepresentationModel
        {
            return GenerateHashName(typeof(TRepresentation));
        }

        public static string GenerateHashName(Type presentation)
        {
            if (!typeof(IRepresentationModel).IsAssignableFrom(presentation))
                throw new ArgumentException("Тип должен реализовывать интерфейс IRepresentationModel", "presentation");
            
            return String.Format("View_{0}", presentation.Name);
        }

		public static string GenerateHashName(IRepresentationModel model)
		{
			return GenerateHashName(model.GetType());
		}

		#endregion

		#region Actions

		protected List<IJournalActionButton> ActionButtons = new List<IJournalActionButton>();

		protected virtual void ConfigureActionButtons()
		{
			if(RepresentationModel.EntityType == null)
				return;

			ActionButtons.Add(new RepresentationAddButton(this, representationModel));
			ActionButtons.Add(new RepresentationEditButton(this, representationModel));
			ActionButtons.Add(new RepresentationDeleteButton(this, representationModel));

			DoubleClickAction = new RepresentationEditButton(this, representationModel);
		}

		public virtual IJournalAction DoubleClickAction { get; set; }
		public virtual List<IJournalPopupAction> PopupActions { get; set; }

		#endregion

		public event EventHandler<JournalObjectSelectedEventArgs> ObjectSelected;

		#region Фильтр

		private Widget filterWidget;

		private void SetFilter(Widget filter)
		{ 
            if (filterWidget == filter)
                return;
            if (filterWidget != null) {
                hboxFilter.Remove (filterWidget);
                filterWidget.Destroy ();
                checkShowFilter.Visible = true;
                filterWidget = null;
            }
            filterWidget = filter;
            checkShowFilter.Visible = filterWidget != null;
            hboxFilter.Add (filterWidget);
            filterWidget.ShowAll ();
        }

		public bool ShowFilter {
			get {
				return checkShowFilter.Active;
			}
			set {
				checkShowFilter.Active = value;
			}
		}

		protected void OnCheckShowFilterToggled(object sender, EventArgs e)
		{
			hboxFilter.Visible = checkShowFilter.Active;
		}

		#endregion

		#region Основные свойства

		protected IRepresentationModel RepresentationModel {
            get => representationModel;
            set {
                if (representationModel == value)
                    return;
                if(representationModel != null)
                    RepresentationModel.ItemsListUpdated -= RepresentationModel_ItemsListUpdated;
                representationModel = value;
                RepresentationModel.SearchStrings = null;
                RepresentationModel.ItemsListUpdated += RepresentationModel_ItemsListUpdated;
                tableview.RepresentationModel = RepresentationModel;
                
                RepresentationModel.PropertyChanged += (sender, args) => {
	                if(args.PropertyName == nameof(RepresentationModel.YTreeModel) && RepresentationModel.YTreeModel != null)
		                tableview.YTreeModel = RepresentationModel.YTreeModel;
                };
                if(RepresentationModel.YTreeModel != null)
					tableview.YTreeModel = RepresentationModel.YTreeModel;
                
				if(RepresentationModel.JournalFilter != null) {
					Widget resolvedFilterWidget = DialogHelper.FilterWidgetResolver.Resolve(RepresentationModel.JournalFilter);
					SetFilter(resolvedFilterWidget);
				}
                hboxSearch.Visible = RepresentationModel.SearchFieldsExist;
				UpdatePopupItems();
            }
        }

		private void UpdatePopupItems()
		{
			if(PopupActions == null) {
				PopupActions = new List<IJournalPopupAction>();
			} else {
				foreach(var pa in PopupActions) {
					pa.MenuItem.Destroy();
				}
				PopupActions.Clear();
			}

			foreach(var popupItem in RepresentationModel.PopupItems) {
				PopupActions.Add(new JournalPopupAction(popupItem));
			}
		}

		protected IEntityConfig EntityConfig => RepresentationModel.EntityType != null
			? DomainConfiguration.GetEntityConfig(RepresentationModel.EntityType)
			: null;

		public override IUnitOfWork UoW => RepresentationModel.UoW;

		public yTreeView TreeView => tableview;

		#endregion

		void RepresentationModel_ItemsListUpdated (object sender, EventArgs e)
		{
			UpdateSum();
		}

		public RepresentationJournalDialog(IRepresentationModel representation, string tilte) : this(representation)
        {
            TabName = tilte;
        }

        public RepresentationJournalDialog(IRepresentationModel representation)
        {
            this.Build ();
			RepresentationModel = representation;
			ConfigureDlg ();
        }

        void ConfigureDlg ()
        {
            Mode = JournalSelectMode.None;

			if(RepresentationModel.EntityType != null)
            {
                object[] att = RepresentationModel.EntityType.GetCustomAttributes (typeof(AppellativeAttribute), true);
                if (att.Length > 0) {
					this.TabName = (att[0] as AppellativeAttribute).NominativePlural.StringToTitleCase();
                }
			}

			ConfigureActionButtons();
			CreateButtons();
			tableview.Selection.Changed += OnTreeviewSelectionChanged;
			tableview.ButtonReleaseEvent += OnOrmtableviewButtonReleaseEvent;
			OnTreeviewSelectionChanged(null, EventArgs.Empty);
		}

		protected void CreateButtons()
		{
			foreach(var item in hboxButtons.Children) {
				if(item is IJournalActionButton) {
					item.Destroy();
				}
			}
			foreach(var action in ActionButtons) {
				hboxButtons.PackStart(action.Button, false, false, 0);
				action.Button.Show();
			}
			OnTreeviewSelectionChanged(null, EventArgs.Empty);
		}

		#region Обработка выбранных строк

		private JournalSelectMode mode;

		public JournalSelectMode Mode {
			get => mode;
			set {
				mode = value;
				hboxSelect.Visible = (mode == JournalSelectMode.Single || mode == JournalSelectMode.Multiple);
				tableview.Selection.Mode = (mode == JournalSelectMode.Multiple) ? SelectionMode.Multiple : SelectionMode.Single;
			}
		}

		void OnTreeviewSelectionChanged (object sender, EventArgs e)
        {
            var selected = tableview.GetSelectedObjects();
            buttonSelect.Sensitive = selected.Any();
			ActionButtons.ForEach(x => x.CheckSensitive(selected));
        }

		public object[] SelectedNodes => tableview.GetSelectedObjects();

		protected void OnButtonSelectClicked(object sender, EventArgs e)
		{
			OnObjectSelected(SelectedNodes);
		}

		public virtual void OnObjectSelected(params object[] nodes)
		{
			if(ObjectSelected != null) {
				logger.Debug("Выбрано {0} id:({1})", RepresentationModel.NodeType, String.Join(",", nodes.Select(DomainHelper.GetIdOrNull)));
				ObjectSelected(this, new JournalObjectSelectedEventArgs(nodes));
			}
			OnCloseTab(false, CloseSource.Self);
		}

		#endregion

        protected void UpdateSum ()
        {
			var itemsCount = RepresentationModel.ItemsList != null ? RepresentationModel.ItemsList.Count : 0;
			labelSum.LabelProp = $"Количество: {itemsCount}";
			logger.Debug("Количество обновлено {0}", itemsCount);
        }

        protected void OnOrmtableviewRowActivated (object o, RowActivatedArgs args)
        {
            if (Mode == JournalSelectMode.Single || Mode == JournalSelectMode.Multiple)
                buttonSelect.Click ();
            else if (DoubleClickAction != null && DoubleClickAction.Sensetive)
                DoubleClickAction.Execute ();
        }

		private Menu popupMenu;

		[GLib.ConnectBefore]
        protected void OnOrmtableviewButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
        {
            if(args.Event.Button == 3 && PopupActions.Any())
            {
				var selected = tableview.GetSelectedObjects();
				if(popupMenu == null) {
					popupMenu = new Menu();
					foreach(var popupAction in PopupActions) {
						popupMenu.Add(popupAction.MenuItem);
					}
				}
				foreach(var popupAction in PopupActions) {
					popupAction.SelectedItems = selected;
					popupAction.CheckSensitive(selected);
					popupAction.CheckVisibility(selected);
				}
				
				popupMenu.ShowAll();
                popupMenu.Popup();
            }
        }

        protected void OnButtonRefreshClicked(object sender, EventArgs e)
        {
            RepresentationModel.UpdateNodes();
        }

		#region FluentConfig

		public RepresentationJournalDialog CustomTabName(string title)
        {
            TabName = title;
            return this;
        }

		#endregion

		#region Реализация поиска

		private int searchEntryShown = 1;

		protected void OnButtonSearchClearClicked(object sender, EventArgs e)
		{
			entrySearch.Text = entrySearch2.Text = entrySearch3.Text = entrySearch4.Text = String.Empty;
		}

		protected void OnEntrySearchChanged(object sender, EventArgs e)
		{
			UpdateSearchString();
		}

		void UpdateSearchString()
		{
			var searchList = new List<string>();
			if(!String.IsNullOrEmpty(entrySearch.Text))
				searchList.Add(entrySearch.Text);
			if(!String.IsNullOrEmpty(entrySearch2.Text))
				searchList.Add(entrySearch2.Text);
			if(!String.IsNullOrEmpty(entrySearch3.Text))
				searchList.Add(entrySearch3.Text);
			if(!String.IsNullOrEmpty(entrySearch4.Text))
				searchList.Add(entrySearch4.Text);

			RepresentationModel.SearchStrings = tableview.SearchHighlightTexts = searchList.ToArray();
			
			if(searchList.All(String.IsNullOrWhiteSpace)) {
				tableview.YTreeModel = RepresentationModel.YTreeModel;
			}
		}

		protected void OnButtonAddAndClicked(object sender, EventArgs e)
        {
			SearchVisible(searchEntryShown + 1);
        }

		public void SetSearchTexts(params string[] strings)
		{
			int i = 0;
			foreach(var str in strings) {
				if(!String.IsNullOrWhiteSpace(str)) {
					i++;
					SetSearchText(i, str.Trim());
				}
			}
			SearchVisible(i);
		}

		private void SetSearchText(int n, string text)
		{
			switch(n) {
				case 1: 
					entrySearch.Text = text;
					break;
				case 2:
					entrySearch2.Text = text;
					break;
				case 3:
					entrySearch3.Text = text;
					break;
				case 4:
					entrySearch4.Text = text;
					break;
			}
		}

		protected void SearchVisible(int count)
		{
			entrySearch.Visible = count > 0;
			ylabelSearchAnd.Visible = entrySearch2.Visible = count > 1;
			ylabelSearchAnd2.Visible = entrySearch3.Visible = count > 2;
			ylabelSearchAnd3.Visible = entrySearch4.Visible = count > 3;
			buttonAddAnd.Sensitive = count < 4;
			searchEntryShown = count;
		}

		public override void Destroy()
		{
			Dispose();
			base.Destroy();
		}

		#endregion
	}

	public class JournalObjectSelectedEventArgs : EventArgs
    {
        public object[] Selected { get; private set; }

        public IEnumerable<TNode> GetNodes<TNode> ()
        {
            return Selected.Cast<TNode>();
        }

        public IEnumerable<int> GetSelectedIds ()
        {
            return Selected.Select (DomainHelper.GetId);
        }

        public JournalObjectSelectedEventArgs (params object[] selected)
        {
            Selected = selected;
        }
    }
}

