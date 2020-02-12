using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Gdk;
using Gtk;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Control.ESVM;

namespace QS.Views.Control
{
	[ToolboxItem(true)]
	[Category("QS.Control")]
	public partial class EntitySearch : Gtk.Bin
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public static uint QueryDelay = 0;

		public EntitySearch()
		{
			this.Build();
			ConfigureEntryComplition();
		}

		#region Свойства виджета

		private IEntitySearchViewModel viewModel;
		public IEntitySearchViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				if(viewModel != null)
					ViewModel.PropertyChanged += ViewModel_PropertyChanged;

				btnClear.Sensitive = ViewModel.SensetiveCleanButton;
				tbTextForSearch.IsEditable = ViewModel.SensetiveAutoCompleteEntry;

				viewModel.AutocompleteListSize = 20;
				viewModel.AutoCompleteListUpdated += ViewModel_AutoCompleteListUpdated;
				tbTextForSearch.Binding.AddBinding(ViewModel, vm => vm.SearchText, w => w.Text).InitializeFromSource();
			}
		}

		#endregion

		#region Обработка событий

		void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(IEntitySearchViewModel.SensetiveCleanButton):
					btnClear.Sensitive = ViewModel.SensetiveCleanButton;
					break;
			case nameof(IEntitySearchViewModel.SensetiveAutoCompleteEntry):
					tbTextForSearch.IsEditable = ViewModel.SensetiveAutoCompleteEntry;
					break;
			default:
					break;
			}
		}

		protected void OnButtonClearClicked(object sender, EventArgs e)
		{
			viewModel.CleanEntity();
		}

		#endregion

		#region Внутренние методы

		private void InternalSetEntryText(string text)
		{
			isInternalTextSet = true;
			tbTextForSearch.Text = text ?? String.Empty;
			isInternalTextSet = false;
		}

		#endregion

		#region AutoCompletion

		private bool isInternalTextSet;
		private ListStore completionListStore;
		uint timerId;

		private void ConfigureEntryComplition()
		{
			tbTextForSearch.Completion = new EntryCompletion();
			tbTextForSearch.Completion.MatchSelected += Completion_MatchSelected;
			tbTextForSearch.Completion.MatchFunc = Completion_MatchFunc;
			var cell = new CellRendererText();
			tbTextForSearch.Completion.PackStart(cell, true);
			tbTextForSearch.Completion.SetCellDataFunc(cell, OnCellLayoutDataFunc);

		}

		bool Completion_MatchFunc(EntryCompletion completion, string key, TreeIter iter)
		{
			return true;
		}

		void OnCellLayoutDataFunc(CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var title = viewModel.GetAutocompleteTitle(tree_model.GetValue(iter, 0)) ?? String.Empty;
			var words = tbTextForSearch.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			foreach(var word in words) {
				string pattern = String.Format("{0}", Regex.Escape(word));
				title = Regex.Replace(title, pattern, (match) => String.Format("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
			}
			(cell as CellRendererText).Markup = title;
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected(object o, MatchSelectedArgs args)
		{
			var node = args.Model.GetValue(args.Iter, 0);
			Application.Invoke(delegate {
				viewModel.SelectNode(node);
			});
			args.RetVal = true;
		}

		void ViewModel_AutoCompleteListUpdated(object sender, AutocompleteUpdatedEventArgs e)
		{
			Application.Invoke((s, arg) => {
				FillAutocomplete(e.List);
			});
		}

		private void FillAutocomplete(IList list)
		{
			logger.Info("Запрос данных для автодополнения...");
			completionListStore = new ListStore(typeof(object));

			foreach(var item in list) {
				completionListStore.AppendValues(item);
			}

			tbTextForSearch.Completion.Model = completionListStore;
			tbTextForSearch.Completion.PopupCompletion = true;
			logger.Debug("Получено {0} строк автодополения...", completionListStore.IterNChildren());
		}

		protected void OnEntryObjectFocusOutEvent(object o, FocusOutEventArgs args)
		{
			if(string.IsNullOrWhiteSpace(tbTextForSearch.Text)) {
				viewModel.CleanEntity();
			}
		}

		protected void OnEntryObjectChanged(object sender, EventArgs e)
		{
			if(isInternalTextSet)
				return;

			if(QueryDelay != 0) {
				GLib.Source.Remove(timerId);
				timerId = GLib.Timeout.Add(QueryDelay, new GLib.TimeoutHandler(RunSearch));
			}
			else
				RunSearch();
		}

		bool RunSearch()
		{
			viewModel.AutocompleteTextEdited(tbTextForSearch.Text);
			timerId = 0;
			return false;
		}

		protected void OnEntryObjectWidgetEvent(object o, WidgetEventArgs args)
		{
			if(args.Event.Type == EventType.KeyPress && timerId != 0) {
				EventKey eventKey = args.Args.OfType<EventKey>().FirstOrDefault();
				if(eventKey != null && (eventKey.Key == Gdk.Key.Return || eventKey.Key == Gdk.Key.KP_Enter)) {
					GLib.Source.Remove(timerId);
					RunSearch();
				}
			}
		}

		#endregion

	}
}
