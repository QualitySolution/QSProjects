using QS.Commands;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;

namespace QS.ViewModels.Widgets {
	public sealed class LeftRightListViewModel<TItem> : LeftRightListViewModel {
		public void SetLeftItems(IEnumerable<TItem> items, Func<TItem, string> displayFunc) {
			LeftItems.Clear();
			RightItems.Clear();
			var itemViewModel = items.Select(x => new LeftRightListItemViewModel<TItem>(x, displayFunc)).Cast<LeftRightListItemViewModel>().ToList();
			LeftItems = new GenericObservableList<LeftRightListItemViewModel>(itemViewModel);
			foreach(var leftItem in LeftItems) {
				leftItem.LeftIndex = LeftItems.IndexOf(leftItem);
			}
		}

		public IEnumerable<TItem> GetRightItems() {
			return RightItems.Cast<LeftRightListItemViewModel<TItem>>().Select(x => x.Content);
		}
	}

	public abstract class LeftRightListViewModel : WidgetViewModelBase {
		private string leftLabel;
		private string rightLabel;
		private GenericObservableList<LeftRightListItemViewModel> leftItems;
		private GenericObservableList<LeftRightListItemViewModel> rightItems;
		private IEnumerable<LeftRightListItemViewModel> selectedLeftItems;
		private IEnumerable<LeftRightListItemViewModel> selectedRightItems;
		private DelegateCommand moveRightCommand;
		private DelegateCommand moveLeftCommand;
		private DelegateCommand moveUpCommand;
		private DelegateCommand moveDownCommand;

		public LeftRightListViewModel() {
			LeftItems = new GenericObservableList<LeftRightListItemViewModel>();
			RightItems = new GenericObservableList<LeftRightListItemViewModel>();
			selectedLeftItems = Enumerable.Empty<LeftRightListItemViewModel>();
			selectedRightItems = Enumerable.Empty<LeftRightListItemViewModel>();
		}

		public bool SelectOnDoubleClick { get; set; } = true; 

		public virtual string LeftLabel {
			get => leftLabel;
			set => SetField(ref leftLabel, value);
		}

		public virtual string RightLabel {
			get => rightLabel;
			set => SetField(ref rightLabel, value);
		}

		public virtual GenericObservableList<LeftRightListItemViewModel> LeftItems {
			get => leftItems;
			set => SetField(ref leftItems, value);
		}

		public virtual GenericObservableList<LeftRightListItemViewModel> RightItems {
			get => rightItems;
			set => SetField(ref rightItems, value);
		}

		public virtual IEnumerable<LeftRightListItemViewModel> SelectedLeftItems {
			get => selectedLeftItems;
			set {
				if(SetField(ref selectedLeftItems, value)) {
					OnPropertyChanged(nameof(CanMoveRight));
				}
			}
		}

		public virtual IEnumerable<LeftRightListItemViewModel> SelectedRightItems {
			get => selectedRightItems;
			set {
				if(SetField(ref selectedRightItems, value)) {
					OnPropertyChanged(nameof(CanMoveUp));
					OnPropertyChanged(nameof(CanMoveDown));
					OnPropertyChanged(nameof(CanMoveLeft));
				}
			}
		}

		#region Move right

		public virtual DelegateCommand MoveRightCommand {
			get {
				if(moveRightCommand == null) {
					moveRightCommand = new DelegateCommand(MoveRight, () => CanMoveRight);
					moveRightCommand.CanExecuteChangedWith(this, x => x.CanMoveRight);
				}
				return moveRightCommand;
			}
		}

		public virtual bool CanMoveRight => SelectedLeftItems.Any();

		protected virtual void MoveRight() {
			foreach(var leftItem in SelectedLeftItems) {
				LeftItems.Remove(leftItem);
				RightItems.Add(leftItem);
			}
		}

		#endregion Move right

		#region Move left

		public virtual DelegateCommand MoveLeftCommand {
			get {
				if(moveLeftCommand == null) {
					moveLeftCommand = new DelegateCommand(MoveLeft, () => CanMoveLeft);
					moveLeftCommand.CanExecuteChangedWith(this, x => x.CanMoveLeft);
				}
				return moveLeftCommand;
			}
		}

		public virtual bool CanMoveLeft => SelectedRightItems.Any();

		protected virtual void MoveLeft() {
			foreach(var rightItem in SelectedRightItems) {
				RightItems.Remove(rightItem);
				if(LeftItems.Count + 1 > rightItem.LeftIndex) {
					LeftItems.Insert(rightItem.LeftIndex, rightItem);
				}
				else {
					LeftItems.Add(rightItem);
				}
			}
		}

		#endregion  Move left

		#region Move up

		public virtual DelegateCommand MoveUpCommand {
			get {
				if(moveUpCommand == null) {
					moveUpCommand = new DelegateCommand(MoveUp, () => CanMoveUp);
					moveUpCommand.CanExecuteChangedWith(this, x => x.CanMoveUp);
				}
				return moveUpCommand;
			}
		}

		public virtual bool CanMoveUp {
			get {
				var count = SelectedRightItems.Count();
				if(count == 0) {
					return false;
				}
				else if(count == 1) {
					var itemIndex = RightItems.IndexOf(SelectedRightItems.First());
					return itemIndex > 0;
				}
				else {
					return true;
				}
			}
		}

		protected void MoveUp() {
			var selectedItemsBackup = SelectedRightItems.ToList();

			var selectedItemIndexes = SelectedRightItems
				.Select(item => RightItems.IndexOf(item))
				.OrderBy(x => x)
				.ToList();

			foreach(var rightItem in SelectedRightItems) {
				var itemIndex = RightItems.IndexOf(rightItem);

				var previousIndexes = selectedItemIndexes.Where(i => i < itemIndex);
				if(previousIndexes.Any()) {
					var previousIndex = previousIndexes.Max();
					if(previousIndex + 1 == itemIndex) {
						continue;
					}
				}

				var newIndex = Math.Max(0, itemIndex - 1);
				if(newIndex == itemIndex) {
					continue;
				}

				selectedItemIndexes[selectedItemIndexes.IndexOf(itemIndex)] = newIndex;
				selectedItemIndexes = selectedItemIndexes.OrderBy(x => x).ToList();

				RightItems.Remove(rightItem);
				RightItems.Insert(newIndex, rightItem);
			}

			SelectedRightItems = selectedItemsBackup;
		}

		#endregion  Move up

		#region Move down

		public virtual DelegateCommand MoveDownCommand {
			get {
				if(moveDownCommand == null) {
					moveDownCommand = new DelegateCommand(MoveDown, () => CanMoveDown);
					moveDownCommand.CanExecuteChangedWith(this, x => x.CanMoveDown);
				}
				return moveDownCommand;
			}
		}

		public virtual bool CanMoveDown {
			get {
				var count = SelectedRightItems.Count();
				if(count == 0) {
					return false;
				}
				else if(count == 1) {
					var itemIndex = RightItems.IndexOf(SelectedRightItems.First());
					var maxIndex = RightItems.Count - 1;
					return itemIndex < maxIndex;
				}
				else {
					return true;
				}
			}
		}

		protected virtual void MoveDown() {
			var selectedItemsBackup = SelectedRightItems.ToList();

			var selectedItemIndexes = SelectedRightItems
				.Select(item => RightItems.IndexOf(item))
				.OrderByDescending(x => x)
				.ToList();

			foreach(var rightItem in SelectedRightItems.Reverse()) {
				var itemIndex = RightItems.IndexOf(rightItem);

				var previousIndexes = selectedItemIndexes.Where(i => i > itemIndex);
				if(previousIndexes.Any()) {
					var previousIndex = previousIndexes.Min();
					if(previousIndex - 1 == itemIndex) {
						continue;
					}
				}
				var maxIndex = RightItems.Count() - 1;
				var newIndex = Math.Min(maxIndex, itemIndex + 1);
				if(newIndex == itemIndex) {
					continue;
				}

				selectedItemIndexes[selectedItemIndexes.IndexOf(itemIndex)] = newIndex;
				selectedItemIndexes = selectedItemIndexes.OrderByDescending(x => x).ToList();

				RightItems.Remove(rightItem);
				RightItems.Insert(newIndex, rightItem);
			}

			SelectedRightItems = selectedItemsBackup;
		}

		#endregion  Move down
	}
}
