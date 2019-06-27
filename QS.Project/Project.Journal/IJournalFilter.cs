using System;
namespace QS.Project.Journal
{
	public interface IJournalFilter
	{
		bool HidenByDefault { get; set; }
		event EventHandler OnFiltered;
		void Update();
	}
}
