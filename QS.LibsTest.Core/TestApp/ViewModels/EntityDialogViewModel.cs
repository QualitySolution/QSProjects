﻿using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Test.TestApp.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;

namespace QS.Test.TestApp.ViewModels
{
	public class EntityDialogViewModel : EntityDialogViewModelBase<SimpleEntity>
	{
		public EntityDialogViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWork unitOfWork, INavigationManager navigation, IValidator validator = null) : base(uowBuilder, unitOfWork, navigation, validator)
		{
		}
	}
}
