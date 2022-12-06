using System;
using System.Collections.Generic;

namespace QS.Project.Journal
{
	public class JournalAction : IJournalAction
	{
		/// <summary>
		/// Инициализирует новый объект класса JournalAction
		/// во всех делегатах object[] - это Node-ы(=строки) журнала
		/// </summary>
		/// <param name="title">Название действия.</param>
		/// <param name="sensitiveFunc">Функция проверки sensitive(отклика кнопки на нажатие), при выделенных Node-ах.</param>
		/// <param name="visibleFunc">Функция проверки Visible(видно ли действие,к примеру,как объект выпадающего меню), при выделенных Node-ах.</param>
		/// <param name="executeAction">Выполняемая функция, при активировании с выделенными Node-ами</param>
		/// <param name="hotKeys">Горячая клавиша выполнения действия JournalAction. Записывается в виде строки: Insert, Delete, Tab</param>
		public JournalAction(string title, Func<object[], bool> sensitiveFunc, Func<object[], bool> visibleFunc,
			Action<object[]> executeAction = null, string hotKeys = null)
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
		/// Функция проверки sensitive(отклика кнопки на нажатие), при выделенных Node-ах.
		/// </summary>
		private readonly Func<object[], bool> sensitiveFunc;
		/// <summary>
		/// Функция проверки Visible(видно ли действие,к примеру,как объект выпадающего меню), при выделенных Node-ах.
		/// </summary>
		private readonly Func<object[], bool> visibleFunc;
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
		public Action<object[]> ExecuteAction { get; set; }

        public bool GetSensitivity(object[] selectedNodes)
		{
			return sensitiveFunc.Invoke(selectedNodes);
		}

		public bool GetVisibility(object[] selectedNodes)
		{
			return visibleFunc.Invoke(selectedNodes);
		}

		public string HotKeys { get; }
	}
}
