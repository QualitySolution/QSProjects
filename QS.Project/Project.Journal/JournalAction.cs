using System;
using System.Collections.Generic;

namespace QS.Project.Journal
{
	public class JournalAction : IJournalAction
	{
		/// <summary>
		/// Инициализирует новый объект класса JournalAction
		/// во всех делегатах objetc[] - это Node-ы(=строки) журнала
		/// </summary>
		/// <param name="title">Название действия.</param>
		/// <param name="sensitiveFunc">Функция проверки sensetive(отклика кнопки на нажатие), при выделенных Node-ах.</param>
		/// <param name="visibleFunc">Функция проверки Visible(видно ли действие,к примеру,как объект выпадающего меню), при выделенных Node-ах.</param>
		/// <param name="executeAction">Выполняемая функция, при активировании с выделенными Node-ами</param>
		public JournalAction(string title, Func<IList<object>, bool> sensitiveFunc, Func<IList<object>, bool> visibleFunc, Action<IList<object>> executeAction = null, string hotKeys = null)
		{
			ChildActionsList = new List<IJournalAction>();
			Title = title;
			this.sensitiveFunc = sensitiveFunc;
			this.visibleFunc = visibleFunc;
			ExecuteAction = executeAction;
			HotKeys = hotKeys;
		}

		public string Title { get; }
		/// <summary>
		/// Функция проверки sensetive(отклика кнопки на нажатие), при выделенных Node-ах.
		/// </summary>
		private readonly Func<IList<object>, bool> sensitiveFunc;
		/// <summary>
		/// Функция проверки Visible(видно ли действие,к примеру,как объект выпадающего меню), при выделенных Node-ах.
		/// </summary>
		private readonly Func<IList<object>, bool> visibleFunc;
		/// <summary>
		/// Дочерние действия.
		/// </summary>
		/// <value>The child actions.</value>
		public IEnumerable<IJournalAction> ChildActions => ChildActionsList;
		public List<IJournalAction> ChildActionsList { get; set; }
		/// <summary>
		/// Выполняемое действие при вызове.
		/// </summary>
		/// <value>Объекты - в данном случае это Node-ы журнала</value>
		public Action<IList<object>> ExecuteAction { get; }

        public bool GetSensitivity(IList<object> selectedNodes)
		{
			return sensitiveFunc.Invoke(selectedNodes);
		}

		public bool GetVisibility(IList<object> selectedNodes)
		{
			return visibleFunc.Invoke(selectedNodes);
		}

		public string HotKeys { get; }
	}
}
