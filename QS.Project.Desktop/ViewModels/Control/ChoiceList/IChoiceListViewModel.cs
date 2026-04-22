using System.ComponentModel;
using QS.Extensions.Observable.Collections.List;

namespace QS.ViewModels.Control {
	public interface IChoiceListViewModel : INotifyPropertyChanged {

		ObservableList<ISelectableEntity> Items { get; }

		/// <summary>
		/// Выдделить все сущности списка
		/// </summary>
		void SelectAll ();
		 
		/// <summary>
		///  Снять выддение со всех сущностей списка
		/// </summary>
		void UnSelectAll ();
		
		/// <summary>
		///  Подсветить все сущности списка содержащие в названии указанную строку
		/// </summary>
		void HighlightLike(string maskLike);
	}
}
