using System;
namespace QS.Project.Journal.Search
{
	public class SearchModel : IJournalSearch
	{
		public event EventHandler OnSearch;

		public string[] SearchValues { get; set; }

		public void Update()
		{
			OnSearch?.Invoke(this, EventArgs.Empty);
		}
	}
}
