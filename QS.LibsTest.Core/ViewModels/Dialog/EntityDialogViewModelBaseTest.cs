using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Test.TestApp.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;

namespace QS.Test.ViewModels.Dialog
{
	[TestFixture(TestOf = typeof(EntityDialogViewModelBase<>))]
	public class EntityDialogViewModelBaseTest
	{
		[Test(Description = "Проверяем что ViewModel сохраняет entity если проверка прошла.")]
		public void Validate_SaveAfterValidationTest()
		{
			var entity = Substitute.For<ValidatedEntity>();
			var uow = Substitute.For<IUnitOfWork>();
			var uowBuilder = Substitute.For<IEntityUoWBuilder>();
			uowBuilder.GetEntity<ValidatedEntity>(uow).Returns(entity);
			var navigation = Substitute.For<INavigationManager>();
			var validation = Substitute.For<IValidator>();
			validation.Validate(entity, Arg.Any<ValidationContext>()).Returns(true);
			validation.Validate(Arg.Any<IEnumerable<ValidationRequest>>()).Returns(true);

			var viewModel = new EntityDialogViewModelBase<ValidatedEntity>(uowBuilder, uow, navigation, validation);

			Assert.That(viewModel.Save(), Is.True);
			uow.Received().Save(entity);
			uow.Received().Commit();
		}

		[Test(Description = "Проверяем что ViewModel НЕ сохраняет entity если проверка НЕ прошла.")]
		public void Validate_DontSaveIfValidationFailTest()
		{
			var entity = Substitute.For<ValidatedEntity>();
			var uow = Substitute.For<IUnitOfWork>();
			var uowBuilder = Substitute.For<IEntityUoWBuilder>();
			uowBuilder.GetEntity<ValidatedEntity>(uow).Returns(entity);
			var navigation = Substitute.For<INavigationManager>();
			var validation = Substitute.For<IValidator>();
			validation.Validate(entity, Arg.Any<ValidationContext>()).Returns(false);
			validation.Validate(Arg.Any<IEnumerable<ValidationRequest>>()).Returns(false);

			var viewModel = new EntityDialogViewModelBase<ValidatedEntity>(uowBuilder, uow, navigation, validation);

			Assert.That(viewModel.Save(), Is.False);
			uow.DidNotReceive().Save(entity);
			uow.DidNotReceive().Commit();
		}

		[Test(Description = "Проверяем что ViewModel сохраняет entity без проверки если валидатор отсутствует.")]
		public void Validate_SaveWithoutValidatorTest()
		{
			var entity = Substitute.For<ValidatedEntity>();
			var uow = Substitute.For<IUnitOfWork>();
			var uowBuilder = Substitute.For<IEntityUoWBuilder>();
			uowBuilder.GetEntity<ValidatedEntity>(uow).Returns(entity);
			var navigation = Substitute.For<INavigationManager>();

			var viewModel = new EntityDialogViewModelBase<ValidatedEntity>(uowBuilder, uow, navigation);

			Assert.That(viewModel.Save(), Is.True);
			uow.Received().Save(entity);
			uow.Received().Commit();
		}
	}
}
