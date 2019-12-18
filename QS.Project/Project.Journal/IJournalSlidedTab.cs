using System;
namespace QS.Project.Journal
{
	public interface IJournalSlidedTab
	{
		SliderOption SliderOption { get; }
	}

	public enum SliderOption
	{
		UseSlider,
		UseSliderHided,
		WithoutSlider
	}
}
