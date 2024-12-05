using System;
using QS.ViewModels;
using QS.Project.Journal;
namespace QS.Project.Search
{
	public class SearchViewModel : WidgetViewModelBase, IJournalSearch
	{
		#region IJournalSearch implementation

		public event EventHandler OnSearch;

		private string[] searchValues;
		public virtual string[] SearchValues {
			get => searchValues;
			set {
				if(SetField(ref searchValues, value, () => SearchValues)) {
					Update();
				}
			}
		}

		public void Update()
		{
			OnSearch?.Invoke(this, new EventArgs());
		}

		#endregion IJournalSearch implementation
	}
}
