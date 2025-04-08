using System;
using System.Collections.Generic;
using System.ComponentModel;
using QS.Extensions.Observable.Collections.List;

namespace QS.ViewModels.Control {
	public interface IChoiceListViewModel : INotifyPropertyChanged {

		ObservableList<SelectedEntity> Items { get; }

		/// <summary>
		/// Показывать в списке строку с сущностью null.
		/// Результат в спец. поле NullIsSelected
		/// </summary>
		void ShowNullValue(bool show, string title);

		/// <summary>
		/// В списке выбрана сущность null
		/// </summary>
		bool NullIsSelected { get; }
		
		/// <summary>
		///  Выбраны все сущности
		/// </summary>
		bool AllSelected { get; }

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
		
		/// <summary>
		///  Подсветить все сущности списка содержащие в названии указанную строку
		/// </summary>
		void SelectLike(string maskLike);
		
		/// <summary>
		///  Массив id сущностей
		/// </summary>
		int[] SelectedIds { get;}
		
		/// <summary>
		/// Массив id сущностей со спецзначениями.
		/// Выводит массив id если что-то выбрано, либо массив с одним значением, 
		/// -1 если выбрано всё втом числе  null элемент,
		/// -2 если ничего не выбрано,
		/// -3 если выбран только null. 
		/// Никогда не возвращает пустой массив.
		/// </summary>
		int[] SelectedIdsMod { get; }

		/// <summary>
		///  Список выбранных сущностей, в том числе null
		/// </summary>
		IEnumerable<object> SelectedEntities { get; }

		/// <summary>
		///  Список не выбранных сущностей, в том числе null
		/// </summary>
		IEnumerable<object> UnSelectedEntities { get; }
	}
}
