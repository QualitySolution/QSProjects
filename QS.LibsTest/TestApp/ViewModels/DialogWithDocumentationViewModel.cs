using QS.Navigation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;

namespace QS.Test.TestApp.ViewModels {
	public class DialogWithDocumentationViewModel : DialogViewModelBase, IDialogDocumentation {
		public DialogWithDocumentationViewModel(INavigationManager navigation) : base(navigation)
		{
		}

		public string DocumentationUrl { get; } = "TestUrl";
		public string ButtonTooltip { get; } = "TestTooltip";
	}
}
