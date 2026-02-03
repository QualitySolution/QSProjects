using System;

namespace QS.Journal.Search
{
	public interface IJournalSearch
	{
		string[] SearchValues { get; set; }
		event EventHandler OnSearch;
		void Update();
	}
}
