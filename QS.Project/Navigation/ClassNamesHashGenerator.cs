using System;
using System.Collections.Generic;
using System.Linq;
using QS.Project.Domain;
using QS.ViewModels;

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

		public string GetHash<TViewModel>(ViewModelBase master, Type[] ctorTypes, object[] ctorValues)
		{
			return InternalGetHash(typeof(TViewModel), ctorValues);
		}

		public string GetHashNamedArgs<TViewModel>(ViewModelBase master, IDictionary<string, object> ctorArgs)
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
				hash = NameHash(typeViewModel) + ParametersHash(ctorValues);

			//Если в сгенерированом хеше есть ~ значит мы не хотим проверку по хешу, поэтому возвращаем null
			return hash.Contains("~") ? null : hash;
		}

		private string ParametersHash(object[] ctorValues)
		{
			var paramHash = String.Empty;

			foreach (var ctorArg in ctorValues) {
				if(ctorArg is IEntityUoWBuilder uowBuilder) {
					paramHash += uowBuilder.IsNewEntity ? "~" : $"#{uowBuilder.EntityOpenId}";
				}

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
