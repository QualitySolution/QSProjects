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

		#region SearchText для биндинга с UI

		private string searchText;
		/// <summary>
		/// Текст поиска для биндинга с UI.
		/// Автоматически разбивается на массив SearchValues по пробелам.
		/// </summary>
		public virtual string SearchText {
			get => searchText;
			set {
				if(SetField(ref searchText, value, () => SearchText)) {
					UpdateSearchValuesFromText();
				}
			}
		}

		private void UpdateSearchValuesFromText()
		{
			if(string.IsNullOrWhiteSpace(searchText)) {
				SearchValues = new string[0];
			}
			else {
				var allFields = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				SearchValues = allFields;
			}
		}

		/// <summary>
		/// Очистить поиск
		/// </summary>
		public void Clear()
		{
			SearchText = string.Empty;
		}

		#endregion
	}
}
