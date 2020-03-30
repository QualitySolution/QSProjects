using System;

namespace QS.Project.Journal.Search
{
	public class SingleEntrySearchViewModel<TSearchModel> : SearchViewModelBase<TSearchModel>, ISingleEntrySearchViewModel
		where TSearchModel : SearchModel
	{
		public override TSearchModel SearchModelGeneric { get; }

		public SingleEntrySearchViewModel(TSearchModel searchModel)
		{
			SearchModelGeneric = searchModel ?? throw new ArgumentNullException(nameof(searchModel));
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
				UpdateSearchModel(new string[0]);
			} else {
				UpdateSearchModel(SearchValue.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			}
		}

		public virtual void Clear()
		{
			SearchValue = string.Empty;
			OnSearchValueUpdated();
		}
	}
}
