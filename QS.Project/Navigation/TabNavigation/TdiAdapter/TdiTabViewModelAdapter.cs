using System;
using QS.ViewModels;
using QS.Services;
using QS.Tdi;
using QS.Project.Journal;
namespace QS.Navigation.TabNavigation.TdiAdapter
{
	public class TdiTabViewModelAdapter : TabViewModelBase
	{
		public ITdiTab Tab { get; }

		public TdiTabViewModelAdapter(ITdiTab tab, IInteractiveService interactiveService) : base(interactiveService)
		{
			Tab = tab ?? throw new ArgumentNullException(nameof(tab));
			if(tab.FailInitialize) {
				throw new AbortCreatingPageException("Попытка открыть вкладку, создание которой не было завершено", "");
				//Сообщение пользователю должно было быть уже отображено, поэтому просто прерываем создание
				return;
			}

			tab.TabNameChanged += (sender, e) => OnTabNameChanged();
			tab.TabClosed += Tab_TabClosed;
		}

		public override SliderOption SliderOption {
			get {
				if(Tab is IJournalSlidedTab slidedTab) {
					return slidedTab.SliderOption;
				}
				return base.SliderOption;
			}
		}

		void Tab_TabClosed(object sender, EventArgs e)
		{
			RaiseTabClosed();
		}

		public override void Close(bool askSave)
		{
			if(askSave)
				Tab.TabParent?.AskToCloseTab(Tab);
			else
				Tab.TabParent?.ForceCloseTab(Tab);
		}

		public override string TabName => Tab.TabName;

	}
}
