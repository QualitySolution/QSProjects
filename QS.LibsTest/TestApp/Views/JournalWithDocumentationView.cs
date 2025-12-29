using System;
using QS.Test.TestApp.ViewModels;
using QS.Views.Dialog;

namespace QS.Test.TestApp.Views {

	public partial class JournalWithDocumentationView : DialogViewBase<JournalWithDocumentationViewModel> {
		public JournalWithDocumentationView(JournalWithDocumentationViewModel viewModel) : base(viewModel) {
			this.Build();
		}
	}
}
