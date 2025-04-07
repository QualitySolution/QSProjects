using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;

namespace QS.ViewModels.Control {
	public class ChoiceListViewModel<TEntity> : PropertyChangedBase, IChoiceListViewModel, IDisposable
		where TEntity : class ////28569 разобрать
	{
		
		public ChoiceListViewModel(IList<TEntity> itemsList) {
			FillItemsFromList(itemsList);
			//this.UoW = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public Func<TEntity, int> Id = e => DomainHelper.GetId(e);
		public Func<TEntity, string> Name = e => DomainHelper.GetTitle(e);
		
		private ObservableList<SelectedEntity> items = new ObservableList<SelectedEntity>();
		public ObservableList<SelectedEntity> Items {
			get {
				if(items == null)
					FillItems();
				return items;
			}
		}

		private void FillItemsFromList(IList<TEntity> itemsList) {
			foreach(var item in itemsList) {
				items.Add(new SelectedEntity() {
					Id = Id(item),
					Name = Name(item)
				});
			}
			items.PropertyOfElementChanged += OnPropertyOfElementChanged;
		}
		
		void FillItems(IUnitOfWork uow = null) {
////28569 разобрать			
/*
			if(uow != null) { 
				SelectedEntity resultAlias = null;
				items = new ObservableList<SelectedEntity>(uow.Session.QueryOver<TEntity>()
					.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.Id)
						.Select(x => x.Name).WithAlias(() => resultAlias.Name)
						.Select(() => true).WithAlias(() => resultAlias.Select)
						.Select(() => true).WithAlias(() => resultAlias.Highlighted)
					).OrderBy(x => x.Name).Asc
					.TransformUsing(Transformers.AliasToBean<SelectedEntity>())
					.List<SelectedEntity>());
			}
*/			
//			items.PropertyOfElementChanged += OnPropertyOfElementChanged;
		}

		private void OnPropertyOfElementChanged(object sender, PropertyChangedEventArgs e) {
			OnPropertyChanged(nameof(AllSelected));
			OnPropertyChanged(nameof(AllUnSelected));
		}
		
		public int[] SelectedIds {
			get => Items.Where(x => x.Select && x.Id > 0).Select(x => x.Id).Distinct().ToArray();
		}
		
		public int[] SelectedIdsMod {
			get {
				if(AllSelected)
					return new[] { -1 };
				var ids = Items.Where(x => x.Select && x.Id > 0).Select(x => x.Id).Distinct().ToArray();
				if(ids.Length != 0)
					return ids;
				else return new[] { -2 };
			} 
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
		public int Id { get; set; }
		public string Name { get; set; }
		
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
