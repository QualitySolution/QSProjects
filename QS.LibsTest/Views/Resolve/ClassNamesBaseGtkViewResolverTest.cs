using NSubstitute;
using NUnit.Framework;
using QS.Navigation;
using QS.Test.TestNamespace.ViewModels;
using QS.Test.TestNamespace.ViewModels.OneLevel;
using QS.Test.TestNamespace.ViewModels.Two.Level;
using QS.Test.TestNamespace.Views;
using QS.Test.TestNamespace.Views.OneLevel;
using QS.Test.TestNamespace.Views.Two.Level;
using QS.Views.Resolve;

namespace QS.Test.Views.Resolve
{
	[TestFixture(TestOf = typeof(ClassNamesBaseGtkViewResolver))]
	public class ClassNamesBaseGtkViewResolverTest
	{
		[Test(Description = "Проверяем коректность работы резольвера на тестовом классе")]
		public void CanResolveOneLevelViewTest()
		{
			var navigation = Substitute.For<INavigationManager>();
			var tab = new OneLevelTestViewModel(navigation);

			var resolver = new ClassNamesBaseGtkViewResolver(typeof(OneLevelTestView));
			var view = resolver.Resolve(tab);

			Assert.That(view, Is.InstanceOf<OneLevelTestView>());
		}

		[Test(Description = "Проверяем коректность работы резольвера на тестовом классе")]
		public void CanResolveTwoLevelViewTest()
		{
			var navigation = Substitute.For<INavigationManager>();
			var tab = new SecondTestClassViewModel(navigation);

			var resolver = new ClassNamesBaseGtkViewResolver(typeof(OneLevelTestView));
			var view = resolver.Resolve(tab);

			Assert.That(view, Is.InstanceOf<SecondTestClassView>());
		}

		[Test(Description = "Проверяем коректность работы резольвера на тестовом классе")]
		public void CanResolveZeroLevelViewTest()
		{
			var navigation = Substitute.For<INavigationManager>();
			var tab = new ZeroLevelTestViewModel(navigation);

			var resolver = new ClassNamesBaseGtkViewResolver(typeof(OneLevelTestView));
			var view = resolver.Resolve(tab);

			Assert.That(view, Is.InstanceOf<ZeroLevelTestView>());
		}
	}
}
