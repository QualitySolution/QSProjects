using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;

namespace QS.ViewModels.Control {
	public class ChoiceListViewModel<TEntity> : PropertyChangedBase, IChoiceListViewModel where TEntity : class
	{
		public ChoiceListViewModel(IList<TEntity> itemsList, Func<TEntity, int?> IdFunc = null, Func<TEntity, string> TitleFunc = null) {
			foreach(var item in itemsList) {
				Items.Add(new SelectableEntity<TEntity>(
					++itemCount,
					TitleFunc != null ? TitleFunc(item) : DomainHelper.GetTitle(item),
					IdFunc != null ? IdFunc(item) : DomainHelper.GetIdOrNull(item),
					item));
	        }
	        Items.PropertyOfElementChanged += OnPropertyOfElementChanged;
		}

		#region Содержимое

		private int itemCount = 0;
		
		private ObservableList<ISelectableEntity> items = new ObservableList<ISelectableEntity>();
		public ObservableList<ISelectableEntity> Items => items;
		
		private void OnPropertyOfElementChanged(object sender, PropertyChangedEventArgs e) {
			OnPropertyChanged(nameof(AllSelected));
			OnPropertyChanged(nameof(AllUnSelected));
		}

		#endregion

		#region Конфигурация
		
		private bool visibleNullValue = false;
		public bool VisibleNullValue {
			get => visibleNullValue;
			set => SetField(ref visibleNullValue, value);
		}

		/// <summary>
		/// Показывать в списке строку с сущностью null.
		/// Результат в спец. поле NullIsSelected
		/// </summary>
		public void ShowNullValue(bool show, string title) {
			if(Items.Any(x => x.ItemId == -1)) {
				if(!show)
					items.RemoveAll(x => x.ItemId == -1);
			} else if(show)
				items.Insert(0,new SelectableEntity<TEntity>(-1, title, null, null));
			OnPropertyChanged(nameof(Items));
			VisibleNullValue = show;
		}

		#endregion

		#region Вывод данных
		
		/// <summary>
		///  Список выбранных сущностей, в том числе null
		/// </summary>
		public IEnumerable<TEntity> SelectedEntities =>
			Items.Where(x => x.Select).Select(x => ((SelectableEntity<TEntity>)x).Entity);
		
		/// <summary>
		///  Список не выбранных сущностей, в том числе null
		/// </summary>
		public IEnumerable<TEntity> UnSelectedEntities =>
			Items.Where(x => !x.Select).Select(x => ((SelectableEntity<TEntity>)x).Entity);

		/// <summary>
		///  Массив id выбранных сущностей
		/// </summary>
		public int[] SelectedIds {
			get => Items.Where(x => 
							x.Select &&
							x is SelectableEntity<TEntity> &&
							((SelectableEntity<TEntity>)x).EntityId != null)
						.Select(x => (int)((SelectableEntity<TEntity>)x).EntityId).Distinct().ToArray();
		}
		
		/// <summary>
		/// Массив id сущностей со спецзначениями.
		/// Выводит массив id если что-то выбрано, либо массив с одним значением, 
		/// -1 если выбрано всё втом числе  null элемент,
		/// -2 если ничего не выбрано,
		/// -3 если выбран только null. 
		/// Никогда не возвращает пустой массив.
		/// </summary>
		public int[] SelectedIdsMod {
			get {
				if(AllSelected)
					return new[] { -1 };
				
				if(SelectedIds.Length == 0) 
					if(NullIsSelected) 
						return new[] { -3 };
					else 
						return new[] { -2 };
				else 
					return SelectedIds;
			} 
		}
		
		/// <summary>
		/// В списке выбрана сущность null
		/// </summary>
		public bool NullIsSelected {
			get => Items.Any(x => x.Select && x.ItemId == -1);
		}
		
		public bool AllSelected {
			get => Items.All(x => x.Select);
		}
		
		public bool AllUnSelected {
			get => Items.All(x => !x.Select);
		}

		#endregion

		#region Действия

		public void SelectAll() {
			foreach (var pt in Items)
				pt.Select = true;
		}
		 
		public void UnSelectAll() {
			foreach (var e in Items)
				e.Select = false;
		}
		
		/// <summary>
		/// Выделить и поднять в верх списка элементы содержищие текст в title
		/// </summary>
		/// <param name="maskLike"></param>
		public void HighlightLike(string maskLike) {
			foreach(var line in Items)
				line.Highlighted = line.Label.ToLower().Contains(maskLike.ToLower());
			Items.Sort(Comparison);
		}
		
		private int Comparison(ISelectableEntity x, ISelectableEntity y) {
			if(x.Highlighted == y.Highlighted)
				return x.Label.CompareTo(y.Label);
			return x.Highlighted ? -1 : 1;
		}

		#endregion
	}
}
