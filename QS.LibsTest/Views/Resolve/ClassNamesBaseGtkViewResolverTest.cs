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
		[Test(Description = "Проверяем корректность работы резольвера на тестовом классе")]
		public void CanResolveOneLevelViewTest()
		{
			var navigation = Substitute.For<INavigationManager>();
			var tab = new OneLevelTestViewModel(navigation);

			IGtkViewResolver resolver =null;
			var factory = new GtkViewFactory(() => resolver);
			resolver = new ClassNamesBaseGtkViewResolver(factory, typeof(OneLevelTestView));
			var view = resolver.Resolve(tab);

			Assert.That(view, Is.InstanceOf<OneLevelTestView>());
		}

		[Test(Description = "Проверяем корректность работы резольвера на тестовом классе")]
		public void CanResolveTwoLevelViewTest()
		{
			var navigation = Substitute.For<INavigationManager>();
			var tab = new SecondTestClassViewModel(navigation);

			IGtkViewResolver resolver =null;
			var factory = new GtkViewFactory(() => resolver);
			resolver = new ClassNamesBaseGtkViewResolver(factory, typeof(OneLevelTestView));
			var view = resolver.Resolve(tab);

			Assert.That(view, Is.InstanceOf<SecondTestClassView>());
		}

		[Test(Description = "Проверяем корректность работы резольвера на тестовом классе")]
		public void CanResolveZeroLevelViewTest()
		{
			var navigation = Substitute.For<INavigationManager>();
			var tab = new ZeroLevelTestViewModel(navigation);

			IGtkViewResolver resolver =null;
			var factory = new GtkViewFactory(() => resolver);
			resolver = new ClassNamesBaseGtkViewResolver(factory, typeof(OneLevelTestView));
			var view = resolver.Resolve(tab);

			Assert.That(view, Is.InstanceOf<ZeroLevelTestView>());
		}

		[Test(Description = "Проверяем что резольвер корректно работает с классами в именах которых есть цифры(Реальный баг)")]
		[Category("real case")]
		public void CanResolveClassNameWithDigitsTest()
		{
			var navigation = Substitute.For<INavigationManager>();
			var tab = new NumberName123987TestViewModel(navigation);

			IGtkViewResolver resolver =null;
			var factory = new GtkViewFactory(() => resolver);
			resolver = new ClassNamesBaseGtkViewResolver(factory, typeof(OneLevelTestView));
			var view = resolver.Resolve(tab);

			Assert.That(view, Is.InstanceOf<NumberName123987TestView>());
		}
	}
}
