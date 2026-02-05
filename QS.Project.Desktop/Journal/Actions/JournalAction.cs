using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;

namespace QS.Journal.Actions
{
	/// <summary>
	/// Действие журнала
	/// </summary>
	/// <typeparam name="TNode">Тип узла (строки) журнала</typeparam>
	public class JournalAction<TNode> : PropertyChangedBase, IJournalActionView
	{
		/// <summary>
		/// Инициализирует новый объект класса JournalAction
		/// </summary>
		/// <param name="title">Название действия.</param>
		/// <param name="executeAction">Выполняемая функция, при активировании с выделенными Node-ами</param>
		/// <param name="sensitiveFunc">Функция проверки sensitive(отклика кнопки на нажатие), при выделенных Node-ах.</param>
		/// <param name="visibleFunc">Функция проверки Visible(видно ли действие, к примеру, как объект выпадающего меню), при выделенных Node-ах.</param>
		/// <param name="hotkeys">Горячие клавиши</param>
		public JournalAction(
			string title,
			Action<IList<TNode>> executeAction,
			Func<IList<TNode>, bool> sensitiveFunc = null,
			Func<IList<TNode>, bool> visibleFunc = null,
			string hotkeys = null)
		{
			SensitiveFunc = sensitiveFunc;
			VisibleFunc = visibleFunc;
			this.title = title;
			ExecuteAction = executeAction;
			HotKeys = hotkeys;
			ChildActions = new List<JournalAction<TNode>>();
		}

		/// <summary>
		/// Инициализирует новый объект класса JournalAction с динамическим заголовком
		/// </summary>
		/// <param name="titleFunc">Функция для получения названия действия в зависимости от выбранных Node-ов.</param>
		/// <param name="executeAction">Выполняемая функция, при активировании с выделенными Node-ами</param>
		/// <param name="sensitiveFunc">Функция проверки sensitive(отклика кнопки на нажатие), при выделенных Node-ах.</param>
		/// <param name="visibleFunc">Функция проверки Visible(видно ли действие, к примеру, как объект выпадающего меню), при выделенных Node-ах.</param>
		/// <param name="hotkeys">Горячие клавиши</param>
		public JournalAction(
			Func<IList<TNode>, string> titleFunc,
			Action<IList<TNode>> executeAction,
			Func<IList<TNode>, bool> sensitiveFunc = null,
			Func<IList<TNode>, bool> visibleFunc = null,
			string hotkeys = null)
		{
			TitleFunc = titleFunc;
			SensitiveFunc = sensitiveFunc;
			VisibleFunc = visibleFunc;
			ExecuteAction = executeAction;
			HotKeys = hotkeys;
			ChildActions = new List<JournalAction<TNode>>();
		}

		/// <summary>
		/// Функция для получения динамического заголовка
		/// </summary>
		public Func<IList<TNode>, string> TitleFunc { get; set; }

		/// <summary>
		/// Функция проверки sensitive(отклика кнопки на нажатие), при выделенных Node-ах.
		/// </summary>
		public Func<IList<TNode>, bool> SensitiveFunc { get; set; }
		
		/// <summary>
		/// Функция проверки Visible(видно ли действие, к примеру, как объект выпадающего меню), при выделенных Node-ах.
		/// </summary>
		public Func<IList<TNode>, bool> VisibleFunc { get; set; }

		#region Свойства View
		private string title;
		public string Title {
			get => title;
			set => SetField(ref title, value);
		}

		private bool sensitive;
		public virtual bool Sensitive {
			get => sensitive;
			set => SetField(ref sensitive, value);
		}

		private bool visible;
		public virtual bool Visible {
			get => visible;
			set => SetField(ref visible, value);
		}
		#endregion

		/// <summary>
		/// Выполняемое действие при вызове.
		/// </summary>
		/// <value>Аргумент - это выделенные Node-ы журнала</value>
		public Action<IList<TNode>> ExecuteAction { get; set; }

		/// <summary>
		/// Функция для получения текущих выбранных узлов (устанавливается извне)
		/// </summary>
		public Func<IList<TNode>> GetSelectedNodesFunc { get; set; }

		/// <summary>
		/// Дочерние действия.
		/// </summary>
		public IList<JournalAction<TNode>> ChildActions { get; }
		
		public string HotKeys { get; }

		#region IJournalActionView

		/// <summary>
		/// Дочерние действия для View (не дженерик)
		/// </summary>
		public IEnumerable<IJournalActionView> ChildActionsView => ChildActions;

		/// <summary>
		/// Выполнить действие (для IJournalActionView)
		/// </summary>
		public void Execute()
		{
			var selectedNodes = GetSelectedNodesFunc?.Invoke() ?? new List<TNode>();
			ExecuteAction?.Invoke(selectedNodes);
		}

		#endregion

		#region Обработка

		public void OnSelectionChanged(IList<TNode> selectedNodes)
		{
			// Обновляем заголовок если есть TitleFunc
			if (TitleFunc != null)
				Title = TitleFunc.Invoke(selectedNodes);
			
			Sensitive = SensitiveFunc?.Invoke(selectedNodes) ?? true;
			Visible = VisibleFunc?.Invoke(selectedNodes) ?? true;
			
			foreach (var child in ChildActions)
				child.OnSelectionChanged(selectedNodes);
		}

		#endregion
	}
}
