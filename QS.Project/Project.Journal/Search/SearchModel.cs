using System;
using QS.DomainModel.Entity;
namespace QS.Project.Journal.Search
{
	public class SearchModel : PropertyChangedBase, IJournalSearch
	{
		public event EventHandler OnSearch;

		public virtual string[] SearchValues { get; set; }

		public virtual void Update()
		{
			RaiseOnSearch();
		}

		protected void RaiseOnSearch()
		{
			OnSearch?.Invoke(this, EventArgs.Empty);
		}
	}
}
