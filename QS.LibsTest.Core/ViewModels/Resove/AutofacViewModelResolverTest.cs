﻿using System;
using Autofac;
using NUnit.Framework;
using QS.Test.TestApp.Domain;
using QS.Test.TestApp.JournalViewModels;
using QS.Test.TestApp.ViewModels;
using QS.ViewModels;
using QS.ViewModels.Resolve;

namespace QS.Test.ViewModels.Resove
{
	[TestFixture(TestOf = typeof(AutofacViewModelResolver))]
	public class AutofacViewModelResolverTest
	{
		[Test(Description = "Можем найти класс журнала.")]
		public void ResolveJournalTest()
		{
			var builder = new ContainerBuilder();
			IContainer container = null;
			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(FullQuerySetEntityJournalViewModel)))
				.Where(t => t.IsAssignableTo<ViewModelBase>() && t.Name.EndsWith("ViewModel"))
				.AsSelf();
			container = builder.Build();

			var resolver = new AutofacViewModelResolver(container);
			var result = resolver.GetTypeOfViewModel(typeof(Document1), TypeOfViewModel.Journal);
			Assert.That(result, Is.EqualTo(typeof(FullQuerySetEntityJournalViewModel)));
		}

		[Test(Description = "Можем найти класс диалога.")]
		public void ResolveEntityDialogTest()
		{
			var builder = new ContainerBuilder();
			IContainer container = null;
			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(EntityDialogViewModel)))
				.Where(t => t.IsAssignableTo<ViewModelBase>() && t.Name.EndsWith("ViewModel"))
				.AsSelf();
			container = builder.Build();

			var resolver = new AutofacViewModelResolver(container);
			var result = resolver.GetTypeOfViewModel(typeof(SimpleEntity), TypeOfViewModel.EditDialog);
			Assert.That(result, Is.EqualTo(typeof(EntityDialogViewModel)));
		}
	}
}
