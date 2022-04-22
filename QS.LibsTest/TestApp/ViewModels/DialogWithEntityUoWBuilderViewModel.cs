using System;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels.Dialog;

namespace QS.Test.TestApp.ViewModels
{
	/// <summary>
	/// Диалог с наличием в конструкторе IEntityUoWBuilder, что позволяет генератору хешей сгенерировать для тестов хеши с Id сущности.
	/// </summary>
	public class DialogWithEntityUoWBuilderViewModel : DialogViewModelBase
	{
		public DialogWithEntityUoWBuilderViewModel(INavigationManager navigation, IEntityUoWBuilder entityUoWBuilder) : base(navigation)
		{
		}
	}
}
