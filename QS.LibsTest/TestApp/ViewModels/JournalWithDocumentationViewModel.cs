using System;
using QS.Navigation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;

namespace QS.Test.TestApp.ViewModels
{
	public class JournalWithDocumentationViewModel : DialogViewModelBase, ISlideableViewModel, IDialogDocumentation
	{
		public JournalWithDocumentationViewModel(INavigationManager navigation, bool useSlider = false, bool alwaysNewPage = false) : base(navigation)
		{
			UseSlider = useSlider;
			AlwaysNewPage = alwaysNewPage;
		}

		public bool UseSlider { get; set; }

		public bool AlwaysNewPage { get; set; }
		
		public string DocumentationUrl { get; } = "JournalTestUrl";
		public string ButtonTooltip { get; } = "JournalTestTooltip";

	}
}
