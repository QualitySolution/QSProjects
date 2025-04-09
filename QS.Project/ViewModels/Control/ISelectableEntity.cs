namespace QS.ViewModels.Control {
	public interface ISelectableEntity {
		int ItemId { get; }
		string Label { get; }
		bool Select { get; set; }
		bool Highlighted { get; set; }
	}

}
