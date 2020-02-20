using System;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace QS.Test.TestApp.ViewModels
{
	public class SlideableViewModel : DialogViewModelBase, ISlideableViewModel
	{
		public SlideableViewModel(INavigationManager navigation, bool useSlider = false, bool alwaysNewPage = false) : base(navigation)
		{
			UseSlider = useSlider;
			AlwaysNewPage = alwaysNewPage;
		}

		public bool UseSlider { get; set; }

		public bool AlwaysNewPage { get; set; }

	}
}
