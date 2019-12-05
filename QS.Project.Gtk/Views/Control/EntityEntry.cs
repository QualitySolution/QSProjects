using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Gtk;
using QS.ViewModels.Control.EEVM;

namespace QS.GtkUI.Views.Control
{
	[ToolboxItem(true)]
	[Category("QS.Control")]
	public partial class EntityEntry : Gtk.Bin
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private bool entryChangedByUser;

		private ListStore completionListStore;

		public EntityEntry()
		{
			this.Build();
		}

		#region Свойства виджета

		private IEntityEntryViewModel viewModel;

		public IEntityEntryViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				if(viewModel != null)
					ViewModel.PropertyChanged += ViewModel_PropertyChanged;

				buttonSelectEntity.Sensitive = ViewModel.SensetiveSelectButton;
				buttonClear.Sensitive = ViewModel.SensetiveCleanButton;
				buttonViewEntity.Sensitive = ViewModel.SensetiveViewButton;
				entryObject.Sensitive = ViewModel.SensetiveAutoCompleteEntry;
				InternalSetEntryText(ViewModel.EntityTitle);
			}
		}

		#endregion

		#region Обработка событий

		void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof(IEntityEntryViewModel.SensetiveSelectButton):
					buttonSelectEntity.Sensitive = ViewModel.SensetiveSelectButton;
					break;
				case nameof(IEntityEntryViewModel.SensetiveCleanButton):
					buttonClear.Sensitive = ViewModel.SensetiveCleanButton;
					break;
				case nameof(IEntityEntryViewModel.SensetiveViewButton):
					buttonViewEntity.Sensitive = ViewModel.SensetiveViewButton;
					break;
				case nameof(IEntityEntryViewModel.SensetiveAutoCompleteEntry):
					entryObject.Sensitive = ViewModel.SensetiveAutoCompleteEntry;
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
			entryChangedByUser = false;
			entryObject.Text = text ?? String.Empty; //Тут если приходит null, то имеющееся текстовое значение не сбрасывается виджетом, поэтому null преобразуем в пустую строку.
			entryChangedByUser = true;
		}

		#endregion


		#region AutoCompletion

		private void ConfigureEntryComplition()
		{
			entryObject.Completion = new EntryCompletion();
	//		entryObject.Completion.MatchSelected += Completion_MatchSelected;
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
			var title = (string)tree_model.GetValue(iter, 0);
			string pattern = String.Format("{0}", Regex.Escape(entryObject.Text));
			(cell as CellRendererText).Markup =
				Regex.Replace(title, pattern, (match) => String.Format("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
		}

		//[GLib.ConnectBefore]
		//void Completion_MatchSelected(object o, MatchSelectedArgs args)
		//{
		//	var node = args.Model.GetValue(args.Iter, 1);
		//	SelectSubjectByNode(node);
		//	OnChangedByUser();
		//	args.RetVal = true;
		//}

		////private void FillAutocomplete()
		//{
		//	logger.Info("Запрос данных для автодополнения...");
		//	completionListStore = new ListStore(typeof(string), typeof(object));
		//	if(entitySelectorAutocompleteFactory == null) {
		//		return;
		//	}
		//	using(var autoCompleteSelector = entitySelectorAutocompleteFactory.CreateAutocompleteSelector()) {
		//		autoCompleteSelector.SearchValues(entryObject.Text);

		//		foreach(var item in autoCompleteSelector.Items) {
		//			if(item is JournalNodeBase) {
		//				completionListStore.AppendValues(
		//					(item as JournalNodeBase).Title,
		//					item
		//				);
		//			} else if(item is INodeWithEntryFastSelect) {
		//				completionListStore.AppendValues(
		//					(item as INodeWithEntryFastSelect).EntityTitle,
		//					item
		//				);
		//			}
		//		}
		//	}

		//	entryObject.Completion.Model = completionListStore;
		//	entryObject.Completion.PopupCompletion = true;
		//	logger.Debug("Получено {0} строк автодополения...", completionListStore.IterNChildren());
		//}

		//protected void OnEntryObjectFocusOutEvent(object o, FocusOutEventArgs args)
		//{
		//	if(string.IsNullOrWhiteSpace(entryObject.Text)) {
		//		Entity = null;
		//		OnChangedByUser();
		//	}
		//}

		//DateTime lastChangedTime = DateTime.Now;
		//bool fillingInProgress = false;
		//private CancellationTokenSource cts = new CancellationTokenSource();

		//protected void OnEntryObjectChanged(object sender, EventArgs e)
		//{
		//	buttonClear.Sensitive
		//	lastChangedTime = DateTime.Now;
		//	if(!fillingInProgress && entryChangedByUser) {
		//		Task.Run(() => {
		//			fillingInProgress = true;
		//			try {
		//				while((DateTime.Now - lastChangedTime).TotalMilliseconds < 200) {
		//					if(cts.IsCancellationRequested) {
		//						return;
		//					}
		//				}
		//				Application.Invoke((s, arg) => {
		//					FillAutocomplete();
		//				});
		//			} catch(Exception ex) {
		//				logger.Error(ex, $"Ошибка во время формирования автодополнения для {nameof(EntityViewModelEntry)}");
		//			} finally {
		//				fillingInProgress = false;
		//			}
		//		});
		//	}
		//}

		//protected void SelectSubjectByNode(object node)
		//{
		//	Entity = UoW.GetById(EntityType, DomainHelper.GetId(node));
		//}

		#endregion
	}
}
