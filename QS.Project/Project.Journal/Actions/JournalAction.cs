using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;

namespace QS.Project.Journal.Actions
{
	public class JournalAction<TNode> : PropertyChangedBase
	{
		/// <summary>
		/// Инициализирует новый объект класса JournalAction
		/// во всех делегатах objetc[] - это Node-ы(=строки) журнала
		/// </summary>
		/// <param name="title">Название действия.</param>
		/// <param name="sensitiveFunc">Функция проверки sensetive(отклика кнопки на нажатие), при выделенных Node-ах.</param>
		/// <param name="visibleFunc">Функция проверки Visible(видно ли действие,к примеру,как объект выпадающего меню), при выделенных Node-ах.</param>
		/// <param name="executeAction">Выполняемая функция, при активировании с выделенными Node-ами</param>
		public JournalAction(
			string title,
			Func<IList<TNode>, bool> sensitiveFunc,
			Func<IList<TNode>, bool> visibleFunc,
			Action<IList<TNode>> executeAction,
			ActionType actionType,
			string hotkeys = null)
		{
			SensitiveFunc = sensitiveFunc;
			VisibleFunc = visibleFunc;
			Title = title;
			ExecuteAction = executeAction;
			ActionType = actionType;
			HotKeys = hotkeys;
			ChildActions = new List<JournalAction<TNode>>();
		}

		/// <summary>
		/// Функция проверки sensetive(отклика кнопки на нажатие), при выделенных Node-ах.
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
		/// <value>Аргумет это выделенные Node-ы журнала</value>
		public Action<IList<TNode>> ExecuteAction { get; set; }

		/// <summary>
		/// Дочерние действия.
		/// </summary>
		public IList<JournalAction<TNode>> ChildActions { get; }
		public ActionType ActionType { get; }
		public string HotKeys { get; }

		#region Обработка

		public void OnSelectionChanged(IList<TNode> selectedNodes)
		{
			Sensitive = SensitiveFunc?.Invoke(selectedNodes) ?? true;
			Visible = VisibleFunc?.Invoke(selectedNodes) ?? true;
			foreach (var child in ChildActions)
				child.OnSelectionChanged(selectedNodes);
		}

		#endregion
	}
}