using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Project.Domain;
using QS.Tdi;
using QS.ViewModels.Dialog;

namespace QS.Navigation
{
	public class ClassNamesHashGenerator : IPageHashGenerator
	{
		readonly IEnumerable<IExtraPageHashGenerator> extraHashGenerators;

		public ClassNamesHashGenerator(IEnumerable<IExtraPageHashGenerator> extraHashGenerators)
		{
			this.extraHashGenerators = extraHashGenerators;
		}

		#region IPageHashGenerator

		public string GetHash<TViewModel>(DialogViewModelBase master, Type[] ctorTypes, object[] ctorValues)
		{
			return InternalGetHash(typeof(TViewModel), ctorValues);
		}

		public string GetHashNamedArgs<TViewModel>(DialogViewModelBase master, IDictionary<string, object> ctorArgs)
		{
			return InternalGetHash(typeof(TViewModel), ctorArgs.Values.ToArray());
		}

		#endregion

		#region Внутренние

		private string InternalGetHash(Type typeViewModel, object[] ctorValues)
		{
			string hash = null;

			if (extraHashGenerators != null) {
				foreach (var generator in extraHashGenerators) {
					hash = generator.GetHash(typeViewModel, ctorValues);
					if (hash != null)
						break;
				}
			}
			if (hash == null)
				hash = NameHash(typeViewModel) + ParametersHash(typeViewModel, ctorValues);

			//Если в сгенерированом хеше есть ~ значит мы не хотим проверку по хешу, поэтому возвращаем null
			return hash.Contains("~") ? null : hash;
		}

		private string ParametersHash(Type typeViewModel, object[] ctorValues)
		{
			var paramHash = String.Empty;

			foreach (var ctorArg in ctorValues) {
				if(ctorArg is IEntityUoWBuilder uowBuilder) {
					paramHash += uowBuilder.IsNewEntity ? "~" : $"#{uowBuilder.EntityOpenId}";
				}

				//Только для диалогов TDI
				if(typeViewModel.IsAssignableTo<ITdiDialog>() && ctorArg is int entityId)
					paramHash += $"#{entityId}";

				if(ctorArg is Type type) {
					paramHash += $"#{type.Name}";
				}
			}
			return paramHash;
		}

		private string NameHash(Type type)
		{
			return type.FullName;
		}

		#endregion
	}
}
