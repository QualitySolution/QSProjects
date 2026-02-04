using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace QS.Journal.Actions
{
	/// <summary>
	/// View model для кнопочной панели действий журнала
	/// </summary>
	/// <typeparam name="TNode">Тип узла (строки) журнала</typeparam>
	public class ButtonJournalActionsViewModel<TNode> : JournalActionsViewModelBase<TNode>, IButtonJournalActionsViewModel
	{
		private IList<TNode> selectedNodes = new List<TNode>();

		/// <summary>
		/// Список действий журнала
		/// </summary>
		public ObservableCollection<JournalAction<TNode>> Actions { get; }

		/// <summary>
		/// Коллекция действий для View (не дженерик)
		/// </summary>
		public ObservableCollection<IJournalActionView> ActionsView { get; }

		public ButtonJournalActionsViewModel()
		{
			Actions = new ObservableCollection<JournalAction<TNode>>();
			ActionsView = new ObservableCollection<IJournalActionView>();
		}

		/// <summary>
		/// Добавить действие
		/// </summary>
		public void AddAction(JournalAction<TNode> action)
		{
			// Устанавливаем функцию получения выбранных узлов
			action.GetSelectedNodesFunc = () => selectedNodes;
			
			// Инициализируем состояние действия
			action.OnSelectionChanged(selectedNodes);
			
			Actions.Add(action);
			ActionsView.Add(action);
		}

		/// <summary>
		/// Добавить действие с базовыми параметрами
		/// </summary>
		public JournalAction<TNode> AddAction(
			string title,
			System.Action<IList<TNode>> executeAction,
			System.Func<IList<TNode>, bool> sensitiveFunc = null,
			System.Func<IList<TNode>, bool> visibleFunc = null,
			string hotkeys = null)
		{
			var action = new JournalAction<TNode>(
				title,
				executeAction,
				sensitiveFunc,
				visibleFunc,
				hotkeys);
			// ВАЖНО: Мы должны использовать первый метод AddAction, чтобы установить GetSelectedNodesFunc и добавить в ActionsView
			AddAction(action);
			return action;
		}

		/// <summary>
		/// Получить текущие выбранные узлы
		/// </summary>
		public IList<TNode> GetSelectedNodes() => selectedNodes;

		#region IJournalEventsHandler

		public override void OnSelectionChanged(IList<object> nodes)
		{
			selectedNodes = nodes.Cast<TNode>().ToList();
			
			// Обновляем состояние всех действий
			foreach (var action in Actions)
			{
				action.OnSelectionChanged(selectedNodes);
			}
		}

		public override void OnKeyPressed(string key)
		{
			// Обработка горячих клавиш
			foreach (var action in Actions)
			{
				if (action.HotKeys == key && action.Sensitive && action.Visible)
				{
					action.ExecuteAction?.Invoke(selectedNodes);
					break;
				}
			}
		}

		#endregion
	}
}
