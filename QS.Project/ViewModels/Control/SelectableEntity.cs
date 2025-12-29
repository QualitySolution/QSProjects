using QS.DomainModel.Entity;

namespace QS.ViewModels.Control {
	public class SelectableEntity<TEntity> : PropertyChangedBase, ISelectableEntity where TEntity : class
	{
		public SelectableEntity(int itemId, string name, int? entityId = null, TEntity entity = null) {
			ItemId = itemId;
			EntityId = entityId;
			Label = name;
			Entity = entity;
		}
		
		public int ItemId { get; }
		public int? EntityId { get; }
		public TEntity Entity { get; }
		public string Label { get; }
		
		private bool select = true;
		public bool Select {
			get => select;
			set => SetField(ref select, value); 
		}
		public bool Highlighted { get; set; } = true;
	}
}
