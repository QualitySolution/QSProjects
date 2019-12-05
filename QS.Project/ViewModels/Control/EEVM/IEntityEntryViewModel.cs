using System;
using System.ComponentModel;

namespace QS.ViewModels.Control.EEVM
{
	public interface IEntityEntryViewModel : INotifyPropertyChanged
	{
		string EntityTitle { get; }

		bool SensetiveSelectButton { get; }
		bool SensetiveCleanButton { get; }
		bool SensetiveAutoCompleteEntry { get; }
		bool SensetiveViewButton { get; }

		void OpenSelectDialog();
		void CleanEntity();
		void OpenViewEntity();
	}
}
