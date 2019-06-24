using System;
namespace QS.Project.Journal
{
	public interface IJournalFilter
	{
		event EventHandler OnFiltered;
		void Update();
	}
}
