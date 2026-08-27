using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Gamma.Binding.Core;
using Gdk;
using Gtk;
using QS.Extensions;
using QS.ViewModels.Control.EEVM;

namespace QS.Views.Control
{
	[ToolboxItem(true)]
	[Category("QS.Control")]
	public partial class EntityEntry : Gtk.Bin
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly string _normalEntryToolTipMarkup;
		private readonly string _dangerEntryToolTipMarkup = "Введён текст для поиска, но не выбрана сущность из справочника или выпадающего списка.";

		#region Настройка
		/// <summary>
		/// Задержка в передачи запроса на поиск во view model.
		/// Измеряется в миллисекундах.
		/// </summary>
		public static uint QueryDelay = 0;
		#endregion

		public BindingControler<EntityEntry> Binding { get; private set; }

		public EntityEntry()
		{
			this.Build();
			Binding = new BindingControler<EntityEntry>(this);
			ConfigureEntryComplition();
			_normalEntryToolTipMarkup = entryObject.TooltipMarkup;
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
		private bool isDestroyed;
		private ListStore completionListStore;
		private CellRendererText entryCompletionCell;
		uint timerId;

		private void ConfigureEntryComplition()
		{
			entryObject.Completion = new EntryCompletion();
			entryObject.Completion.MatchSelected += Completion_MatchSelected;
			entryObject.Completion.MatchFunc = Completion_MatchFunc;
			entryCompletionCell = new CellRendererText();
			entryObject.Completion.PackStart(entryCompletionCell, true);
			entryObject.Completion.SetCellDataFunc(entryCompletionCell, OnCellLayoutDataFunc);
		}

		bool Completion_MatchFunc(EntryCompletion completion, string key, TreeIter iter)
		{
			return true;
		}

		void OnCellLayoutDataFunc(CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			if(!(cell is CellRendererText cellRenderer))
				return;

			if(viewModel == null || !TryGetAutocompleteNode(tree_model, iter, out var node)) {
				cellRenderer.Markup = String.Empty;
				return;
			}

			var title = viewModel.GetAutocompleteTitle(node) ?? String.Empty;
			var words = (entryObject?.Text ?? String.Empty)
				.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			foreach(var word in words) {
				string pattern = String.Format("{0}", Regex.Escape(word));
				title = Regex.Replace(title, pattern, (match) => String.Format("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
			}
			cellRenderer.Markup = title;
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected(object o, MatchSelectedArgs args)
		{
			if(viewModel == null || !TryGetAutocompleteNode(args.Model, args.Iter, out var node)) {
				args.RetVal = false;
				return;
			}

			viewModel.AutocompleteSelectNode(node);
			args.RetVal = true;
		}

		private bool TryGetAutocompleteNode(TreeModel model, TreeIter iter, out object node)
		{
			node = null;
			if(isDestroyed || model == null)
				return false;

			// GTK оборачивает Completion.Model в TreeModelFilter извлекаем ListStore через child iter.
			if(model is TreeModelFilter filterModel) {
				model = filterModel.ChildModel;
				iter = filterModel.ConvertIterToChildIter(iter);
			}

			// После отложенной перерисовки GTK может передать TreeIter от уже обновлённой модели.
			if(!(model is ListStore listStore) || !listStore.IterIsValid(iter))
				return false;

			node = listStore.GetValue(iter, 0);
			return node != null;
		}

		void ViewModel_AutoCompleteListUpdated(object sender, AutocompleteUpdatedEventArgs e)
		{
			var list = e.List;
			Application.Invoke((s, arg) => FillAutocomplete(list));
		}

		private void FillAutocomplete(IList list)
		{
			if(isDestroyed || list == null)
				return;

			var completion = entryObject?.Completion;
			if(completion is null)
				return;

			logger.Info("Запрос данных для автодополнения...");
			// Не заменяем модель: отложенная отрисовка может ещё хранить её TreeIter.
			if(completionListStore == null)
				completionListStore = new ListStore(typeof(object));
			else
				completionListStore.Clear();

			foreach (var item in list) {
				if(item != null)
					completionListStore.AppendValues(item);
			}

			completion.Model = completionListStore;
			completion.PopupCompletion = true;
			logger.Debug("Получено {0} строк автодополения...", completionListStore.IterNChildren());
		}

		protected void OnEntryObjectFocusOutEvent(object o, FocusOutEventArgs args)
		{
			if(string.IsNullOrWhiteSpace(entryObject.Text)) {
				entryObject.ModifyText(StateType.Normal);
				entryObject.TooltipMarkup = _normalEntryToolTipMarkup;
				viewModel.CleanEntity();
			}
			else if(entryObject.Text != viewModel.EntityTitle) {
				entryObject.ModifyText(StateType.Normal, new Gdk.Color(255, 0, 0));
				entryObject.TooltipMarkup = _dangerEntryToolTipMarkup;
			}
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
		
		protected override void OnDestroyed()
		{
			// Нативный callback может быть уже поставлен в очередь, поэтому сначала
			// запрещаем всем обработчикам обращаться к состоянию виджета.
			isDestroyed = true;

			if(timerId != 0) {
				GLib.Source.Remove(timerId);
				timerId = 0;
			}

			var completion = entryObject?.Completion;
			if(completion != null) {
				completion.MatchSelected -= Completion_MatchSelected;
				completion.MatchFunc = null;
				if(entryCompletionCell != null)
					completion.SetCellDataFunc(entryCompletionCell, null);
				completion.Model = null;
			}

			if(viewModel != null) {
				viewModel.PropertyChanged -= ViewModel_PropertyChanged;
				viewModel.AutoCompleteListUpdated -= ViewModel_AutoCompleteListUpdated;

				if(viewModel.DisposeViewModel)
					viewModel.Dispose();

				viewModel = null;
			}

			completionListStore?.Dispose();
			completionListStore = null;

			Binding.CleanSources();
			var viewImage = buttonViewEntity.Image as Gtk.Image;
			viewImage.DisposeImagePixbuf();
			var selectImage = buttonSelectEntity.Image as Gtk.Image;
			selectImage.DisposeImagePixbuf();
			var clearImage = buttonClear.Image as Gtk.Image;
			clearImage.DisposeImagePixbuf();

			base.OnDestroyed();
		}
	}
}
