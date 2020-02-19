using System;
using Autofac;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Test.TestApp.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;

namespace QS.Test.ViewModel.Dialog
{
	[TestFixture(TestOf = typeof(EntityDialogViewModelBase<>))]
	public class EntityDialogViewModelBaseTest
	{
		[Test(Description = "Проверяем что ViewModel сохраняет entity если проверка прошла.")]
		public void Validate_SaveAfterValidationTest()
		{
			var entity = Substitute.For<ValidatedEntity>();
			var uow = Substitute.For<IUnitOfWorkGeneric<ValidatedEntity>>();
			uow.Root.Returns(entity);
			uow.RootObject.Returns(entity);
			var uowFacrory = Substitute.For<IUnitOfWorkFactory>();
			var uowBuilder = Substitute.For<IEntityUoWBuilder>();
			uowBuilder.CreateUoW<ValidatedEntity>(uowFacrory).Returns(uow);
			var navigation = Substitute.For<INavigationManager>();
			var validation = Substitute.For<IValidator>();
			validation.Validate(entity, null).Returns(true);

			var viewModel = new EntityDialogViewModelBase<ValidatedEntity>(uowBuilder, uowFacrory, navigation, validation);

			Assert.That(viewModel.Save(), Is.True);
			uow.Received().Save();
		}

		[Test(Description = "Проверяем что ViewModel сохраняет entity если проверка прошла.")]
		public void Validate_DontSaveIfValidationFailTest()
		{
			var entity = Substitute.For<ValidatedEntity>();
			var uow = Substitute.For<IUnitOfWorkGeneric<ValidatedEntity>>();
			uow.Root.Returns(entity);
			uow.RootObject.Returns(entity);
			var uowFacrory = Substitute.For<IUnitOfWorkFactory>();
			var uowBuilder = Substitute.For<IEntityUoWBuilder>();
			uowBuilder.CreateUoW<ValidatedEntity>(uowFacrory).Returns(uow);
			var navigation = Substitute.For<INavigationManager>();
			var validation = Substitute.For<IValidator>();
			validation.Validate(entity, null).Returns(false);

			var viewModel = new EntityDialogViewModelBase<ValidatedEntity>(uowBuilder, uowFacrory, navigation, validation);

			Assert.That(viewModel.Save(), Is.False);
			uow.DidNotReceive().Save();
		}

		[Test(Description = "Проверяем что ViewModel сохраняет entity без проверки если валидатор отсутствует.")]
		public void Validate_SaveWithoutValidatorTest()
		{
			var entity = Substitute.For<ValidatedEntity>();
			var uow = Substitute.For<IUnitOfWorkGeneric<ValidatedEntity>>();
			uow.Root.Returns(entity);
			uow.RootObject.Returns(entity);
			var uowFacrory = Substitute.For<IUnitOfWorkFactory>();
			var uowBuilder = Substitute.For<IEntityUoWBuilder>();
			uowBuilder.CreateUoW<ValidatedEntity>(uowFacrory).Returns(uow);
			var navigation = Substitute.For<INavigationManager>();

			var viewModel = new EntityDialogViewModelBase<ValidatedEntity>(uowBuilder, uowFacrory, navigation);

			Assert.That(viewModel.Save(), Is.True);
			uow.Received().Save();
		}
	}
}
