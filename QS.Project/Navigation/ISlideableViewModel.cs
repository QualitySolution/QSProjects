using System;
namespace QS.Navigation
{
	public interface ISlideableViewModel
	{
		bool UseSlider { get; }
		bool AlwaysNewPage { get; }
	}
}