using System;
using System.Collections;
using System.ComponentModel;

namespace QS.ViewModels.Control.EEVM
{
	public interface IEntityEntryViewModel : INotifyPropertyChanged, IDisposable {
		bool DisposeViewModel { get; set; }
		#region Выбранная сущьность
		string EntityTitle { get; }
		object Entity { get; set; }
		TEntity GetEntity<TEntity>() where TEntity : class;
		#endregion

		#region События для внешних подписчиков
		event EventHandler Changed;
		event EventHandler ChangedByUser;
		event EventHandler<BeforeChangeEventArgs> BeforeChangeByUser;
		#endregion

		#region Настройки виджета
		bool IsEditable { get; set; }
		#endregion

		#region Доступность функций View
		bool SensitiveSelectButton { get; }
		bool SensitiveCleanButton { get; }
		bool SensitiveAutoCompleteEntry { get; }
		bool SensitiveViewButton { get; }
		#endregion

		#region Команды от View
		void OpenSelectDialog();
		void CleanEntity();
		void OpenViewEntity();
		#endregion

		#region Автодополнение
		int AutocompleteListSize { get; set; }
		void AutocompleteTextEdited(string text);
		string GetAutocompleteTitle(object node);
		void AutocompleteSelectNode(object node);
		event EventHandler<AutocompleteUpdatedEventArgs> AutoCompleteListUpdated;
		#endregion
	}

	public class AutocompleteUpdatedEventArgs : EventArgs
	{
		public IList List;

		public AutocompleteUpdatedEventArgs(IList list)
		{
			this.List = list ?? throw new ArgumentNullException(nameof(list));
		}
	}
}
