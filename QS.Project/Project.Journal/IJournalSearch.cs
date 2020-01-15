using System;

namespace QS.Project.Journal
{
	public interface IJournalSearch
	{
		string[] SearchValues { get; set; }
		event EventHandler OnSearch;
		void Update();
	}
}
