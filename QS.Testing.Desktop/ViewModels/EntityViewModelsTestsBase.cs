using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Validation;
using QS.ViewModels.Dialog;

namespace QS.ViewModels
{
	public abstract class EntityViewModelsTestsBase
	{
		public static Assembly[] ScanAssemblies;

		public static IEnumerable AllEntityViewModels {
			get {
				foreach(var assembly in ScanAssemblies) {
					foreach(var type in assembly.GetTypes()) {
						if(GetEntityDialogViewModelBase(type) != null) {
							Console.WriteLine(type);
							yield return new TestCaseData(type)
								.SetDescription("Проверка всех EntityDialogViewModelBase")
								.SetArgDisplayNames(new[] { type.Name });
						}
					}
				}
			}
		}

		/// <summary>
		/// Проверяем что ViewModel-и принимает IValidator в конструкторе, если редактируемая сущность поддерживает валидацию. 
		/// </summary>
		public virtual void ViewModelForValidatableEntityHasValidatorDependenceTest(Type type)
		{
			var entityDialogBase = GetEntityDialogViewModelBase(type);
			var entityType = entityDialogBase.GenericTypeArguments.First();
			if(!NeedValidate(entityType))
				Assert.Ignore($"{entityType.Name} не требует валидации.");

			foreach(var constructor in type.GetConstructors()) {
				if(constructor.GetParameters().Any(x => x.ParameterType == typeof(IValidator)))
					Assert.Pass();
			}
			Assert.Fail($"Для {type.Name} отсутствует конструктор с параметром типа {typeof(IValidator).Name}, хотя редактируемый объект {entityType} требует валидации при сохранении.");
		}

		/// <summary>
		/// Проверяем что для всех диалогов редактирования с включенным отслеживанием изменений, доменный объект имеет родительный падеж для корректного отображения названия диалога.. 
		/// </summary>
		public virtual void ViewModelForHistoryLogEntityHasNamesTest(Type type)
		{
			var entityDialogBase = GetEntityDialogViewModelBase(type);
			var entityType = entityDialogBase.GenericTypeArguments.First();
			if(entityType.GetCustomAttributes(typeof(HistoryTraceAttribute), true).Length == 0)
				Assert.Ignore($"В классе {entityType.Name} изменения не отслеживаются.");

			var names = entityType.GetSubjectNames();
			Assert.That(names, Is.Not.Null, $"Для {entityType.Name} отсутствует атрибут AppellativeAttribute необходимый для отображения названия диалога в журнале изменений.");
			Assert.That(names.Genitive, Is.Not.Empty.And.Not.Null, $"Для {entityType.Name} отсутствует родительный падеж(Genitive) в AppellativeAttribute необходимый для отображения названия диалога в журнале изменений.");
		}
		
		#region private
		private static Type GetEntityDialogViewModelBase(Type viewmodelType)
		{
			if(viewmodelType.IsGenericType && viewmodelType.GetGenericTypeDefinition() == typeof(EntityDialogViewModelBase<>))
				return viewmodelType;
			if(viewmodelType.BaseType != null)
				return GetEntityDialogViewModelBase(viewmodelType.BaseType);
			else
				return null;
		}

		private bool NeedValidate(Type type)
		{
			if(typeof(IValidatableObject).IsAssignableFrom(type))
				return true;

			foreach(var prop in type.GetProperties()) {
				foreach(var attribute in prop.GetCustomAttributes(true)) {
					if(attribute is ValidationAttribute)
						return true;
				}
			}
			return false;
		}
		#endregion
	}
}
