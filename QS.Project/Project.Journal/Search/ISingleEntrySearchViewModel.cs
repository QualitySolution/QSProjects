using System;
using System.ComponentModel;
namespace QS.Project.Journal.Search
{
	public interface ISingleEntrySearchViewModel : INotifyPropertyChanged
	{
		string SearchValue { get; set; }

		void Clear();
		void ManualSearchUpdate();
	}
}
