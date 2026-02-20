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
		where TNode : class
	{
		private IList<TNode> selectedNodes = new List<TNode>();

		/// <summary>
		/// Список действий журнала (слева)
		/// </summary>
		public ObservableCollection<JournalAction<TNode>> LeftActions { get; }

		/// <summary>
		/// Коллекция действий для View (не дженерик) (слева)
		/// </summary>
		public ObservableCollection<IJournalActionView> LeftActionsView { get; }

		/// <summary>
		/// Список действий журнала, расположенных справа на панели
		/// </summary>
		public ObservableCollection<JournalAction<TNode>> RightActions { get; }

		/// <summary>
		/// Коллекция действий для View (не дженерик), расположенных справа
		/// </summary>
		public ObservableCollection<IJournalActionView> RightActionsView { get; }

		private JournalAction<TNode> doubleClickAction;
		/// <summary>
		/// Действие, выполняемое при двойном клике на строке журнала
		/// </summary>
		public JournalAction<TNode> DoubleClickAction {
			get => doubleClickAction;
			set => SetField(ref doubleClickAction, value);
		}

		public ButtonJournalActionsViewModel()
		{
			LeftActions = new ObservableCollection<JournalAction<TNode>>();
			LeftActionsView = new ObservableCollection<IJournalActionView>();
			RightActions = new ObservableCollection<JournalAction<TNode>>();
			RightActionsView = new ObservableCollection<IJournalActionView>();
		}

		/// <summary>
		/// Добавить действие
		/// </summary>
		public void AddAction(JournalAction<TNode> action)
		{
			// Устанавливаем функцию получения выбранных узлов для действия и всех дочерних действий
			SetGetSelectedNodesFuncRecursively(action);
			
			// Инициализируем состояние действия
			action.OnSelectionChanged(selectedNodes);
			
			LeftActions.Add(action);
			LeftActionsView.Add(action);
		}

		/// <summary>
		/// Рекурсивно устанавливает GetSelectedNodesFunc для действия и всех его дочерних действий
		/// </summary>
		private void SetGetSelectedNodesFuncRecursively(JournalAction<TNode> action)
		{
			action.GetSelectedNodesFunc = () => selectedNodes;
			
			foreach (var childAction in action.ChildActions)
			{
				SetGetSelectedNodesFuncRecursively(childAction);
			}
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
		/// Добавить действие в правую часть панели
		/// </summary>
		public void AddRightAction(JournalAction<TNode> action)
		{
			// Устанавливаем функцию получения выбранных узлов для действия и всех дочерних действий
			SetGetSelectedNodesFuncRecursively(action);
			
			// Инициализируем состояние действия
			action.OnSelectionChanged(selectedNodes);
			
			RightActions.Add(action);
			RightActionsView.Add(action);
		}

		/// <summary>
		/// Добавить действие в правую часть панели с базовыми параметрами
		/// </summary>
		public JournalAction<TNode> AddRightAction(
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
			AddRightAction(action);
			return action;
		}

		/// <summary>
		/// Получить текущие выбранные узлы
		/// </summary>
		public IList<TNode> GetSelectedNodes() => selectedNodes;

		#region IJournalEventsHandler

		public override void OnCellDoubleClick(object node, object columnTag, object subcolumnTag)
		{
			// При двойном клике выполняем RowActivatedAction, если оно установлено
			if (DoubleClickAction != null && DoubleClickAction.Sensitive && DoubleClickAction.Visible) {
				DoubleClickAction.ExecuteAction?.Invoke(selectedNodes);
			}
		}

		public override void OnSelectionChanged(IList<object> nodes)
		{
			selectedNodes = nodes.Cast<TNode>().ToList();
			
			// Обновляем состояние всех действий (слева)
			foreach (var action in LeftActions)
			{
				action.OnSelectionChanged(selectedNodes);
			}
			
			// Обновляем состояние всех действий (справа)
			foreach (var action in RightActions)
			{
				action.OnSelectionChanged(selectedNodes);
			}
		}

		public override void OnKeyPressed(string key)
		{
			// Обработка горячих клавиш для действий слева
			foreach (var action in LeftActions)
			{
				if (action.HotKeys == key && action.Sensitive && action.Visible)
				{
					action.ExecuteAction?.Invoke(selectedNodes);
					return;
				}
			}
			
			// Обработка горячих клавиш для действий справа
			foreach (var action in RightActions)
			{
				if (action.HotKeys == key && action.Sensitive && action.Visible)
				{
					action.ExecuteAction?.Invoke(selectedNodes);
					return;
				}
			}
		}

		#endregion
	}
}
