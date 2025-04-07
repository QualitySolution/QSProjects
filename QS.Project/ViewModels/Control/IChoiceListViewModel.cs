using System.ComponentModel;
using QS.Extensions.Observable.Collections.List;

namespace QS.ViewModels.Control {
	public interface IChoiceListViewModel : INotifyPropertyChanged {

		ObservableList<SelectedEntity> Items { get; }
		
		/// <summary>
		///  Выбраны все сущности
		/// </summary>
		bool AllSelected  { get; }

		/// <summary>
		///  Не выбрано ни одной сущности
		/// </summary>
		bool AllUnSelected { get; }

		/// <summary>
		/// Выдделить все сущности списка
		/// </summary>
		void SelectAll ();
		 
		/// <summary>
		///  Снять выддение со всех сущностей списка
		/// </summary>
		void UnSelectAll ();
		
////28569 не очевидное название
		/// <summary>
		///  Подсветить все сущности списка содержащие в названии указанную строку
		/// </summary>
		void SelectLike(string maskLike);
		
		/// <summary>
		///  Массив id сущностей
		/// </summary>
		int[] SelectedIds { get; }
		
////28569 Добавить только null
		/// <summary>
		/// Массив id сущностей со спецзначениями
		/// Выводит массив id если что-то выбрано, либо массив с одним значением
		/// -1 если выбрано всё втом числе  null элемент
		/// -2 если ничего не выбрано
		/// Никогда не возвращает пустой массив.
		/// </summary>
		int[] SelectedIdsMod { get; }
	}
}
