using System;
using QS.Test.TestApp.ViewModels;
using QS.Views.Dialog;

namespace QS.Test.TestApp.Views {

	public partial class DialogWithDocumentationView : DialogViewBase<DialogWithDocumentationViewModel> {
		public DialogWithDocumentationView(DialogWithDocumentationViewModel viewModel) : base(viewModel) {
			this.Build();
		}
	}
}
