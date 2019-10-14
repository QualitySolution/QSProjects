using System;
using QS.Tdi;
using System.Reflection;
using System.ComponentModel;
using QS.Services;

namespace QS.ViewModels
{
	public abstract class TabViewModelBase : ViewModelBase, ITdiTab
	{
		protected TabViewModelBase(IInteractiveService interactiveService) : base(interactiveService)
		{
		}

		#region ITdiTab implementation

		public HandleSwitchIn HandleSwitchIn { get; private set; }
		public HandleSwitchOut HandleSwitchOut { get; private set; }

		private string tabName = string.Empty;

		/// <summary>
		/// Имя вкладки может быть автоматически получено из атрибута DisplayNameAttribute у класса диалога.
		/// </summary>
		public virtual string TabName {
			get {
				if(string.IsNullOrWhiteSpace(tabName)) {
					return GetType().GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName;
				}
				return tabName;
			}
			set {
				if(tabName == value)
					return;
				tabName = value;
				OnTabNameChanged();
			}
		}

		public ITdiTabParent TabParent { set; get; }

		public bool FailInitialize { get; protected set; }

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;
		public event EventHandler TabClosed;

		public virtual bool CompareHashName(string hashName)
		{
			return GenerateHashName(this.GetType()) == hashName;
		}

		#endregion

		/// <summary>
		/// Отменяет открытие вкладки
		/// </summary>
		/// <param name="message">Сообщение пользователю при отмене открытия</param>
		protected void AbortOpening(string message, string title = "Невозможно открыть вкладку")
		{
			ShowErrorMessage(message, title);
			AbortOpening();
		}

		/// <summary>
		/// Отменяет открытие вкладки
		/// </summary>
		protected void AbortOpening()
		{
			FailInitialize = true;
		}

		public static string GenerateHashName<TTab>() where TTab : TabViewModelBase
		{
			return GenerateHashName(typeof(TTab));
		}

		public static string GenerateHashName(Type tabType)
		{
			if(!typeof(TabViewModelBase).IsAssignableFrom(tabType))
				throw new ArgumentException($"Тип должен наследоваться от {nameof(TabViewModelBase)}", nameof(tabType));

			return string.Format("Tab_{0}", tabType.Name);
		}

		protected virtual void OnTabNameChanged()
		{
			TabNameChanged?.Invoke(this, new TdiTabNameChangedEventArgs(TabName));
		}

		public virtual void Close(bool askSave)
		{
			CloseTab?.Invoke(this, new TdiTabCloseEventArgs(askSave));
		}

		public void OnTabClosed()
		{
			TabClosed?.Invoke(this, EventArgs.Empty);
		}
	}
}
