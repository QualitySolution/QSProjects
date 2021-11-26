using System;
using System.ComponentModel;
using System.Reflection;
using QS.Dialog;
using QS.Navigation;
using QS.Services;
using QS.Tdi;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;

namespace QS.ViewModels
{
	[Obsolete("Эта ветка базовых кассов будет удалена с окончательным выпиливанием TDI из ViewModel. Может быть оставлена только для обратной совместимости Водовозовских диалогов.")]
	public abstract class TabViewModelBase : DialogViewModelBase, ITdiTab, IDisposable
	{
		protected TabViewModelBase(IInteractiveService interactiveService, INavigationManager navigation) : base(navigation)
		{
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		public override string Title { get => TabName; set => TabName = value; }

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

		private ITdiTabParent tabParent;
		public ITdiTabParent TabParent
		{
			get => tabParent;
			set => SetField(ref tabParent, value);
		}

		public bool FailInitialize { get; protected set; }

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
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
			OnPropertyChanged(nameof(Title));
		}

		public override void Close(bool askSave, CloseSource source)
		{
			if(askSave)
				TabParent?.AskToCloseTab(this, source);
			else
				TabParent?.ForceCloseTab(this, source);
			
			base.Close(askSave, source);
		}

		public void OnTabClosed()
		{
			TabClosed?.Invoke(this, EventArgs.Empty);
		}

		#region Перенесено из ViewModelBase для поддержания обратной совместимости. умрет здесь вместе с TabViewModelBase

		private readonly IInteractiveService interactiveService;

		protected virtual void ShowInfoMessage(string message, string title = null)
		{
			interactiveService.ShowMessage(ImportanceLevel.Info, message, title);
		}

		protected virtual void ShowWarningMessage(string message, string title = null)
		{
			interactiveService.ShowMessage(ImportanceLevel.Warning, message, title);
		}

		protected virtual void ShowErrorMessage(string message, string title = null)
		{
			interactiveService.ShowMessage(ImportanceLevel.Error, message, title);
		}

		protected virtual bool AskQuestion(string question, string title = null)
		{
			return interactiveService.Question(question, title);
		}

		#endregion

		public virtual void Dispose() { }
	}
}
