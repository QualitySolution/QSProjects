using System;
using System.ComponentModel;

namespace QS.Project.Journal {
	public interface IJournalFilterViewModel : INotifyPropertyChanged {
		void SetAndRefilterAtOnce<TJournalFilterViewModel>(Action<TJournalFilterViewModel> configuration) where TJournalFilterViewModel : class, IJournalFilterViewModel;
	}
}
