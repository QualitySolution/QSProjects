using System;
using QS.DomainModel.Entity;
using QS.ViewModels.Control.EEVM;

namespace QS.ViewModels.Control.ESVM
{
	public class EntitySearchViewModel<TEntity> : PropertyChangedBase, IEntitySearchViewModel, IDisposable
		where TEntity : class
	{
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public EntitySearchViewModel(IEntityAutocompleteSelector<TEntity> autocompleteSelector)
		{ 
			if(autocompleteSelector != null)
				this.AutocompleteSelector = autocompleteSelector;
		}

		#region События

		public event EventHandler<EntitySelectedEventArgs> EntitySelected;
		public event EventHandler<AutocompleteUpdatedEventArgs> AutoCompleteListUpdated;

		#endregion

		#region Публичные свойства

		bool isEditable = true;

		public bool IsEditable {
			get { return isEditable; }
			set {
				isEditable = value;
				OnPropertyChanged(nameof(SensetiveCleanButton));
				OnPropertyChanged(nameof(SensetiveAutoCompleteEntry));
			}
		}

		private string searchText;
		public virtual string SearchText {
			get => searchText;
			set => SetField(ref searchText, value);
		}


		#endregion

		#region Свойства для использования во View 

		public virtual bool SensetiveCleanButton => IsEditable;
		public virtual bool SensetiveAutoCompleteEntry => IsEditable && AutocompleteSelector != null;
		#endregion


		#region AutoCompletion

		public int AutocompleteListSize { get; set; }

		private IEntityAutocompleteSelector<TEntity> autocompleteSelector;
		public IEntityAutocompleteSelector<TEntity> AutocompleteSelector {
			get => autocompleteSelector;
			set {
				autocompleteSelector = value;
				OnPropertyChanged(nameof(SensetiveAutoCompleteEntry));
				autocompleteSelector.AutocompleteLoaded += AutocompleteSelector_AutocompleteLoaded;
			}
		}

		public void AutocompleteTextEdited(string searchText)
		{
			var words = searchText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			AutocompleteSelector?.LoadAutocompletion(words, AutocompleteListSize);
		}

		void AutocompleteSelector_AutocompleteLoaded(object sender, AutocompleteUpdatedEventArgs e)
		{
			AutoCompleteListUpdated?.Invoke(this, e);
		}

		public string GetAutocompleteTitle(object node)
		{
			return AutocompleteSelector.GetTitle(node);
		}

		public void SelectNode(object node)
		{
			EntitySelected?.Invoke(this, new EntitySelectedEventArgs(node));
			CleanEntity();
		}

		#endregion


		#region Команды View

		public void CleanEntity()
		{
			SearchText = "";
		}

		#endregion

		public void Dispose()
		{
			if(AutocompleteSelector is IDisposable asd)
				asd.Dispose();
		}
	}

}
