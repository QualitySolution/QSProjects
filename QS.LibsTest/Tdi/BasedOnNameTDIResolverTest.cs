using System.Reflection;
using NSubstitute;
using NUnit.Framework;
using QS.Services;
using QS.Tdi.Gtk;
using QS.Test.TestNamespace.ViewModels.OneLevel;
using QS.Test.TestNamespace.ViewModels.Two.Level;
using QS.Test.TestNamespace.Views.OneLevel;
using QS.Test.TestNamespace.Views.Two.Level;

namespace QS.Test.Tdi
{
	[TestFixture(TestOf = typeof(BasedOnNameTDIResolver))]
	public class BasedOnNameTDIResolverTest
	{
		[Test(Description = "Проверяем коректность работы резольвера на тестовом классе")]
		public void CanResolveOneLevelViewTest()
		{
			var interactiveService = Substitute.For<IInteractiveService>();
			var tab = new OneLevelTestViewModel(interactiveService);

			var resolver = new BasedOnNameTDIResolver(Assembly.GetAssembly(typeof(OneLevelTestView)));
			var view = resolver.Resolve(tab);

			Assert.That(view, Is.InstanceOf<OneLevelTestView>());
		}

		[Test(Description = "Проверяем коректность работы резольвера на тестовом классе")]
		public void CanResolveTwoLevelViewTest()
		{
			var interactiveService = Substitute.For<IInteractiveService>();
			var tab = new SecondTestClassViewModel(interactiveService);

			var resolver = new BasedOnNameTDIResolver(Assembly.GetAssembly(typeof(OneLevelTestView)));
			var view = resolver.Resolve(tab);

			Assert.That(view, Is.InstanceOf<SecondTestClassView>());
		}
	}
}
