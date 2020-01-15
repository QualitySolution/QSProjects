using System;
using System.Collections.Generic;

namespace QS.Project.Journal.Search
{
	public class MultipleEntrySearchViewModel : SearchViewModelBase
	{
		public override SearchModel SearchModel { get; }

		public MultipleEntrySearchViewModel(SearchModel searchModel)
		{
			SearchModel = searchModel ?? throw new ArgumentNullException(nameof(searchModel));
		}

		private string searchValue1;
		public virtual string SearchValue1 {
			get => searchValue1;
			set {
				if(SetField(ref searchValue1, value, () => SearchValue1)) {
					OnSearchValueUpdated();
				}
			}
		}

		private string searchValue2;
		public virtual string SearchValue2 {
			get => searchValue2;
			set {
				if(SetField(ref searchValue2, value, () => SearchValue2)) {
					OnSearchValueUpdated();
				}
			}
		}

		private string searchValue3;
		public virtual string SearchValue3 {
			get => searchValue3;
			set {
				if(SetField(ref searchValue3, value, () => SearchValue3)) {
					OnSearchValueUpdated();
				}
			}
		}

		private string searchValue4;
		public virtual string SearchValue4 {
			get => searchValue4;
			set {
				if(SetField(ref searchValue4, value, () => SearchValue4)) {
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
			List<string> values = new List<string>();
			if(!string.IsNullOrWhiteSpace(SearchValue1)) {
				values.Add(SearchValue1);
			}
			if(!string.IsNullOrWhiteSpace(SearchValue2)) {
				values.Add(SearchValue2);
			}
			if(!string.IsNullOrWhiteSpace(SearchValue3)) {
				values.Add(SearchValue3);
			}
			if(!string.IsNullOrWhiteSpace(SearchValue4)) {
				values.Add(SearchValue4);
			}
			UpdateSearchValues(values.ToArray());
		}

		public virtual void Clear()
		{
			if(string.IsNullOrWhiteSpace(SearchValue1)
			&& string.IsNullOrWhiteSpace(SearchValue2)
			&& string.IsNullOrWhiteSpace(SearchValue3)
			&& string.IsNullOrWhiteSpace(SearchValue4)) {
				return;
			}

			searchValue1 = string.Empty;
			OnPropertyChanged(nameof(SearchValue1));
			searchValue2 = string.Empty;
			OnPropertyChanged(nameof(SearchValue2));
			searchValue3 = string.Empty;
			OnPropertyChanged(nameof(SearchValue3));
			searchValue4 = string.Empty;
			OnPropertyChanged(nameof(SearchValue4));
			OnSearchValueUpdated();
		}
	}
}
