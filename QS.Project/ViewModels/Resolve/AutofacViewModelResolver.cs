using System;
using System.Linq;
using Autofac;
using QS.Project.Journal;
using QS.ViewModels.Dialog;

namespace QS.ViewModels.Resolve
{
	public class AutofacViewModelResolver : IViewModelResolver
	{
		private readonly IContainer container;

		public AutofacViewModelResolver(IContainer container)
		{
			this.container = container;
		}

		public Type GetTypeOfViewModel(Type typeOfEntity, TypeOfViewModel typeOfViewModel)
		{
			switch(typeOfViewModel) {
				case TypeOfViewModel.Journal:
					return GetJournalViewModel(typeOfEntity);
				case TypeOfViewModel.EditDialog:
					return GetDialogViewModel(typeOfEntity);
			}
			throw new NotImplementedException($"для {typeOfViewModel} не реализовано!");
		}

		private Type GetJournalViewModel(Type typeOfEntity)
		{
			//Здесь пока поддеживаем только журналы построенные на EntityJournalViewModelBase
			var viewModels = container.ComponentRegistry.Registrations
				.Where(x => x.Activator.LimitType.Name.EndsWith("ViewModel"))
				.Select(x => x.Activator.LimitType)
				.Where(x => ViewModelMatch(x, typeof(EntityJournalViewModelBase<,,>), 0, typeOfEntity))
				.ToList();
			if (viewModels.Count == 0)
				return null;
			if (viewModels.Count > 1)
				throw new InvalidOperationException($"Для типа сущьности {typeOfEntity} найдено более одного журнала: {String.Join(", ", viewModels.Select(x => x.FullName))}.");
			return viewModels.First();
		}

		private Type GetDialogViewModel(Type typeOfEntity)
		{
			//Здесь пока поддеживаем только диалоги построенные на EntityDialogViewModelBase
			var viewModels = container.ComponentRegistry.Registrations
				.Where(x => x.Activator.LimitType.Name.EndsWith("ViewModel"))
				.Select(x => x.Activator.LimitType)
				.Where(x => ViewModelMatch(x, typeof(EntityDialogViewModelBase<>), 0, typeOfEntity))
				.ToList();
			if (viewModels.Count == 0)
				return null;
			if (viewModels.Count > 1)
				throw new InvalidOperationException($"Для типа сущьности {typeOfEntity} найдено более одного диалога: {String.Join(", ", viewModels.Select(x => x.FullName))}.");
			return viewModels.First();
		}

		private bool ViewModelMatch(Type clazz, Type openGeneric, int argnum, Type parameterType)
		{
			if (openGeneric.Name == clazz.Name && openGeneric.Namespace == clazz.Namespace)
				return clazz.GenericTypeArguments[argnum] == parameterType;

			if (clazz.BaseType != null)
				return ViewModelMatch(clazz.BaseType, openGeneric, argnum, parameterType);

			return false;
		}
	}
}
