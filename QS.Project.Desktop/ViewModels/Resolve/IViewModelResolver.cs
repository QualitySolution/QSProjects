using System;
namespace QS.ViewModels.Resolve
{
	public interface IViewModelResolver
	{
		Type GetTypeOfViewModel(Type typeOfEntity, TypeOfViewModel typeOfViewModel);
	}

	public enum TypeOfViewModel
	{
		EditDialog,
		Journal
	}
}
