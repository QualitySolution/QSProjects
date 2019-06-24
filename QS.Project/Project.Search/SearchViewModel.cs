using System;
using QS.Services;
using QS.ViewModels;
using QS.Project.Journal;
namespace QS.Project.Search
{
	public class SearchViewModel : WidgetViewModelBase, IJournalSearch
	{
		#region IJournalSearch implementation

		public event EventHandler OnSearch;

		public string[] GetValuesToSearch() => new[] { SearchString };

		private string[] searchValues;
		public virtual string[] SearchValues {
			get => searchValues;
			set {
				if(SetField(ref searchValues, value, () => SearchValues)) {
					Update();
				}
			}
		}

		private DateTime lastUpdate = DateTime.Now;
		private bool searchStarted;

		public void Update()
		{
			OnSearch?.Invoke(this, new EventArgs());
		}

		#endregion IJournalSearch implementation

		public SearchViewModel(IInteractiveService interactiveService) : base(interactiveService)
		{
		}

		private void SetSearchValues()
		{
			SearchValues = new string[] { SearchString };
		}

		string searchString;
		public string SearchString {
			get => searchString;
			set {
				if(SetField(ref searchString, value, () => SearchString)) {
					SetSearchValues();
				}
			}
		}
	}
}
