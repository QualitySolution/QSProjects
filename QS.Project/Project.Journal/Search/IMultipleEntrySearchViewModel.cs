using System;
using System.ComponentModel;
namespace QS.Project.Journal.Search
{
	public interface IMultipleEntrySearchViewModel : INotifyPropertyChanged
	{
		string SearchValue1 { get; set; }
		string SearchValue2 { get; set; }
		string SearchValue3 { get; set; }
		string SearchValue4 { get; set; }

		void ClearSearchValues();
		void ManualSearchUpdate();
	}
}
