using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Gamma.Binding.Core;
using Gdk;
using Gtk;
using QS.ViewModels.Control.EEVM;

namespace QS.Views.Control
{
	[ToolboxItem(true)]
	[Category("QS.Control")]
	public partial class EntityEntry : Gtk.Bin
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Настройка
		/// <summary>
		/// Задержка в передачи запроса на поиск во view model.
		/// Измеряется в милсекундах.
		/// </summary>
		public static uint QueryDelay = 0;
		#endregion

		public BindingControler<EntityEntry> Binding { get; private set; }

		public EntityEntry()
		{
			this.Build();
			Binding = new BindingControler<EntityEntry>(this);
			ConfigureEntryComplition();
		}

		#region Свойства виджета

		private IEntityEntryViewModel viewModel;

		public IEntityEntryViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				if(viewModel != null)
					ViewModel.PropertyChanged += ViewModel_PropertyChanged;

				buttonSelectEntity.Sensitive = ViewModel.SensitiveSelectButton;
				buttonClear.Sensitive = ViewModel.SensitiveCleanButton;
				buttonViewEntity.Sensitive = ViewModel.SensitiveViewButton;
				entryObject.IsEditable = ViewModel.SensitiveAutoCompleteEntry;
				InternalSetEntryText(ViewModel.EntityTitle);

				viewModel.AutocompleteListSize = 20;
				viewModel.AutoCompleteListUpdated += ViewModel_AutoCompleteListUpdated;
			}
		}

		#endregion

		#region Обработка событий

		void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof(IEntityEntryViewModel.SensitiveSelectButton):
					buttonSelectEntity.Sensitive = ViewModel.SensitiveSelectButton;
					break;
				case nameof(IEntityEntryViewModel.SensitiveCleanButton):
					buttonClear.Sensitive = ViewModel.SensitiveCleanButton;
					break;
				case nameof(IEntityEntryViewModel.SensitiveViewButton):
					buttonViewEntity.Sensitive = ViewModel.SensitiveViewButton;
					break;
				case nameof(IEntityEntryViewModel.SensitiveAutoCompleteEntry):
					entryObject.IsEditable = ViewModel.SensitiveAutoCompleteEntry;
					break;
				case nameof(IEntityEntryViewModel.EntityTitle):
					InternalSetEntryText(ViewModel.EntityTitle);
					break;

				default:
					break;
			}
		}

		protected void OnButtonSelectEntityClicked(object sender, EventArgs e)
		{
			viewModel.OpenSelectDialog();
		}

		protected void OnButtonClearClicked(object sender, EventArgs e)
		{
			viewModel.CleanEntity();
		}

		protected void OnButtonViewEntityClicked(object sender, EventArgs e)
		{
			ViewModel.OpenViewEntity();
		}

		#endregion

		#region Внутренние методы

		private void InternalSetEntryText(string text)
		{
			isInternalTextSet = true;
			entryObject.Text = text ?? String.Empty; //Тут если приходит null, то имеющееся текстовое значение не сбрасывается виджетом, поэтому null преобразуем в пустую строку.
			entryObject.ModifyText(StateType.Normal);
			isInternalTextSet = false;
		}

		#endregion


		#region AutoCompletion

		private bool isInternalTextSet;
		private ListStore completionListStore;
		uint timerId;

		private void ConfigureEntryComplition()
		{
			entryObject.Completion = new EntryCompletion();
			entryObject.Completion.MatchSelected += Completion_MatchSelected;
			entryObject.Completion.MatchFunc = Completion_MatchFunc;
			var cell = new CellRendererText();
			entryObject.Completion.PackStart(cell, true);
			entryObject.Completion.SetCellDataFunc(cell, OnCellLayoutDataFunc);
		}

		bool Completion_MatchFunc(EntryCompletion completion, string key, TreeIter iter)
		{
			return true;
		}

		void OnCellLayoutDataFunc(CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var title =  viewModel.GetAutocompleteTitle(tree_model.GetValue(iter, 0)) ?? String.Empty;
			var words = entryObject.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
			viewModel.AutocompleteSelectNode(node);
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

			foreach (var item in list) {
				completionListStore.AppendValues(item);
			}

			entryObject.Completion.Model = completionListStore;
			entryObject.Completion.PopupCompletion = true;
			logger.Debug("Получено {0} строк автодополения...", completionListStore.IterNChildren());
		}

		protected void OnEntryObjectFocusOutEvent(object o, FocusOutEventArgs args)
		{
			if(string.IsNullOrWhiteSpace(entryObject.Text)) {
				entryObject.ModifyText(StateType.Normal);
				viewModel.CleanEntity();
			}
			else if(entryObject.Text != viewModel.EntityTitle)
				entryObject.ModifyText(StateType.Normal, new Gdk.Color(255, 0, 0));

		}

		protected void OnEntryObjectChanged(object sender, EventArgs e)
		{
			if(isInternalTextSet)
				return;

			if (QueryDelay != 0) {
				GLib.Source.Remove(timerId);
				timerId = GLib.Timeout.Add(QueryDelay, new GLib.TimeoutHandler(RunSearch));
			}
			else
				RunSearch();
		}

		bool RunSearch()
		{
			viewModel.AutocompleteTextEdited(entryObject.Text);
			timerId = 0;
			return false;
		}

		protected void OnEntryObjectWidgetEvent(object o, WidgetEventArgs args)
		{
			if (args.Event.Type == EventType.KeyPress && timerId != 0) {
				EventKey eventKey = args.Args.OfType<EventKey>().FirstOrDefault();
				if (eventKey != null && (eventKey.Key == Gdk.Key.Return || eventKey.Key == Gdk.Key.KP_Enter)) {
					GLib.Source.Remove(timerId);
					RunSearch();
				}
			}
		}

		#endregion

		public override void Destroy()
		{
			GLib.Source.Remove(timerId);
			base.Destroy();
		}
	}
}
