using QS.Navigation;

namespace QS.Tdi
{
	public class ViewModelTdiJournalTab : ViewModelTdiTab, ITdiJournal
	{
		public bool? UseSlider => (ViewModel as ISlideableViewModel)?.UseSlider;
	}
}
