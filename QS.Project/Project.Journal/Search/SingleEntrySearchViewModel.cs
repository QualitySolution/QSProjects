using System;

namespace QS.Project.Journal.Search
{
	public class SingleEntrySearchViewModel : SearchViewModelBase
	{
		public override SearchModel SearchModel { get; }

		public SingleEntrySearchViewModel(SearchModel searchModel)
		{
			SearchModel = searchModel ?? throw new ArgumentNullException(nameof(searchModel));
		}

		private string searchValue;
		public virtual string SearchValue {
			get => searchValue;
			set {
				if(SetField(ref searchValue, value, () => SearchValue)) {
					OnSearchValueUpdated();
				}
			}
		}

		public override void ManualSearchUpdate()
		{
			OnSearchValueUpdated();
		}

		protected virtual void OnSearchValueUpdated()
		{
			if(string.IsNullOrWhiteSpace(SearchValue)) {
				UpdateSearchValues(new string[0]);
			} else {
				UpdateSearchValues(SearchValue.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			}
		}

		public virtual void Clear()
		{
			SearchValue = string.Empty;
			OnSearchValueUpdated();
		}
	}
}
