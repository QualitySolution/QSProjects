using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Test.GtkUI;
using QS.Test.TestApp.ViewModels;
using QS.Test.TestApp.Views;
using QS.Views.GtkUI;

namespace QS.Test.Views.GtkUI
{
	[TestFixture(TestOf = typeof(EntityTabViewBase<,>))]
	public class EntityTabViewBaseTest
	{
		[Test(Description = "Проверяем что автоматическая подписка на события нажатия кнопок Save и Cancel работает.")]
		public void CommonButtonSubscriptionTest()
		{
			GtkInit.AtOnceInitGtk();
			var commonService = Substitute.For<ICommonServices>();
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var entityBuilder = Substitute.For<IEntityUoWBuilder>();

			var viewModel = Substitute.For<EntityTabViewModel>(entityBuilder, uowFactory, commonService);

			var view = new ButtonSubscriptionTabView(viewModel);

			view.SaveButton.Click();
			viewModel.Received().SaveAndClose();

			view.CancelButton.Click();
			viewModel.Received().Close(false, CloseSource.Cancel);
		}
	}
}
