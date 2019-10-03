using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Test.GtkUI;
using QS.Test.TestApp.ViewModels;
using QS.Test.TestApp.Views;

namespace QS.Test.Views.GtkUI
{
	[TestFixture()]
	public class EntityTabViewBaseTest
	{
		[Test(Description = "Проверяем что автоматическая подписка на события нажатия кнопок Save и Cancel работает.")]
		public void CommonButtonSubscriptionTest()
		{
			GtkInit.AtOnceInitGtk();
			var commonService = Substitute.For<ICommonServices>();
			var uow = Substitute.For<IUnitOfWork>();

			var viewModel = Substitute.For<EntityViewModel>();

			var view = new ButtonSubscriptionView(viewModel);

			view.SaveButton.Click();
			viewModel.Received().SaveAndClose();

			view.CancelButton.Click();
			viewModel.Received().Close(false);
		}
	}
}
