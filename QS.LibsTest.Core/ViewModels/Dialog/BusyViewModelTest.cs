using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace QS.Test.ViewModels.Dialog
{
	[TestFixture]
	public class BusyViewModelTest
	{
		[Test]
		public void BeginBusyOperation_ChangesStateAndDisposeRestoresIt()
		{
			var viewModel = CreateViewModel();
			var changedProperties = new List<string>();
			viewModel.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

			var operation = viewModel.BeginBusyOperation("Загрузка");

			Assert.That(viewModel.IsBusy, Is.True);
			Assert.That(viewModel.BusyOperationTitle, Is.EqualTo("Загрузка"));
			Assert.That(viewModel.CanCancelBusyOperation, Is.False);
			Assert.That(changedProperties, Does.Contain(nameof(viewModel.IsBusy)));

			operation.Dispose();

			Assert.That(viewModel.IsBusy, Is.False);
			Assert.That(viewModel.BusyOperationTitle, Is.Null);
			Assert.That(viewModel.BusyOperationToken.CanBeCanceled, Is.False);
		}

		[Test]
		public void RequestCancelBusyOperation_CancelsOperationToken()
		{
			var viewModel = CreateViewModel();

			using(var operation = viewModel.BeginBusyOperation("Загрузка", canCancel: true)) {
				Assert.That(viewModel.CanCancelBusyOperation, Is.True);
				Assert.That(viewModel.RequestCancelBusyOperation(), Is.True);
				Assert.That(operation.CancellationToken.IsCancellationRequested, Is.True);
				Assert.That(viewModel.IsBusyCancellationRequested, Is.True);
				Assert.That(viewModel.CanCancelBusyOperation, Is.False);
				Assert.That(viewModel.RequestCancelBusyOperation(), Is.False);
			}
		}

		[Test]
		public void BeginBusyOperation_WhenAlreadyBusy_Throws()
		{
			var viewModel = CreateViewModel();

			using(viewModel.BeginBusyOperation("Первая")) {
				Assert.Throws<InvalidOperationException>(() => viewModel.BeginBusyOperation("Вторая"));
			}
		}

		private static TestDialogViewModel CreateViewModel() =>
			new TestDialogViewModel(Substitute.For<INavigationManager>());

		private class TestDialogViewModel : DialogViewModelBase
		{
			public TestDialogViewModel(INavigationManager navigationManager)
				: base(navigationManager)
			{
			}
		}
	}
}
