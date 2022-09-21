using System;

namespace QS.ViewModels.Widgets {
	public class LeftRightListItemViewModel<TContent> : LeftRightListItemViewModel {
		private readonly Func<TContent, string> displayFunc;

		public TContent Content { get; }

		public override string Name => displayFunc.Invoke(Content);

		public LeftRightListItemViewModel(TContent content, Func<TContent, string> displayFunc) {
			Content = content;
			this.displayFunc = displayFunc ?? throw new ArgumentNullException(nameof(displayFunc));
		}
	}

	public abstract class LeftRightListItemViewModel {
		public abstract string Name { get; }

		internal int LeftIndex { get; set; }
	}
}
