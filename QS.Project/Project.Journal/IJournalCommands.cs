using QS.Commands;

namespace QS.Project.Journal
{
	public interface IJournalCommands
	{
		DelegateCommand SelectCommand { get; }
		DelegateCommand AddCommand { get; }
		DelegateCommand EditCommand { get; }
		DelegateCommand DeleteCommand { get; }
	}
}