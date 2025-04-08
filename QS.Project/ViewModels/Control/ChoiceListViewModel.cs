using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;

namespace QS.ViewModels.Control {
	public class ChoiceListViewModel<TEntity> : PropertyChangedBase, IChoiceListViewModel, IDisposable {
		public ChoiceListViewModel(IList<TEntity> itemsList) {
			foreach(var item in itemsList) {
				Items.Add(new SelectedEntity(++itemCount, Id(item), Title(item), item));
	        }
	        Items.PropertyOfElementChanged += OnPropertyOfElementChanged;
		}

		private int itemCount = 0;
		public Func<TEntity, int?> Id = e => DomainHelper.GetIdOrNull(e);
		public Func<TEntity, string> Title = e => DomainHelper.GetTitle(e);
		
		private ObservableList<SelectedEntity> items = new ObservableList<SelectedEntity>();
		public ObservableList<SelectedEntity> Items => items;
		private void OnPropertyOfElementChanged(object sender, PropertyChangedEventArgs e) {
			OnPropertyChanged(nameof(AllSelected));
			OnPropertyChanged(nameof(AllUnSelected));
		}
		
		public int[] SelectedIds {
			get => Items.Where(x => x.Select && x.EntityId != null).Select(x => (int)x.EntityId).Distinct().ToArray();
		}
		
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

		public IEnumerable<object> SelectedEntities =>
			 Items.Where(x => x.Select).Select(x => x.Entity);
		
		public IEnumerable<object> UnSelectedEntities =>
			Items.Where(x => !x.Select).Select(x => x.Entity);
		
		private bool visibleNullValue = false;
		public bool VisibleNullValue {
			get => visibleNullValue;
			set => SetField(ref visibleNullValue, value);
		}
		
		public bool NullIsSelected {
			get => Items.Any(x => x.Select && x.ItemId == -1);
		}

		public void ShowNullValue(bool show, string title) {
			if(Items.Any(x => x.ItemId == -1)) {
				if(!show)
					items.RemoveAll(x => x.ItemId == -1);
			} else if(show)
				items.Add(new SelectedEntity(-1, null, title, null));
			OnPropertyChanged(nameof(Items));
			VisibleNullValue = show;
		}
		
		
		public bool AllSelected {
			get => Items.All(x => x.Select);
		}
		
		public bool AllUnSelected {
			get => Items.All(x => !x.Select);
		}

		public void SelectAll() {
			foreach (var pt in Items)
				pt.Select = true;
		}
		 
		public void UnSelectAll() {
			foreach (var e in Items)
				e.Select = false;
		}
		
		public void SelectLike(string maskLike) {
			foreach(var line in Items)
				line.Highlighted = line.Name.ToLower().Contains(maskLike.ToLower());
			Items.Sort(Comparison);
		}
		
		private int Comparison(SelectedEntity x, SelectedEntity y) {
			if(x.Highlighted == y.Highlighted)
				return x.Name.CompareTo(y.Name);
			return x.Highlighted ? -1 : 1;
		}
		
		public void Dispose() {
////28569 разобрать
// TODO release managed resources here
		}
	}
	
	public class SelectedEntity : PropertyChangedBase
	{
		public SelectedEntity(int itemId, int? entityId, string name, object entity) {
			ItemId = itemId;
			EntityId = entityId;
			Name = name;
			Entity = entity;
		}
		
		public int ItemId { get; }
		public int? EntityId { get; }
		public object Entity { get; }
		public string Name { get; }
		
		private bool select = true;
		public virtual bool Select {
			get => select;
			set => SetField(ref select, value); 
		}

		private bool highlighted = true;
		public bool Highlighted {
			get => highlighted;
			set => SetField(ref highlighted, value);
		}
	}
}
