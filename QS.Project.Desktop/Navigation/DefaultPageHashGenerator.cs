using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QS.Project.Domain;

namespace QS.Navigation
{
	/// <summary>
	/// Базовый генератор хеша страниц для проектов без TDI
	/// </summary>
	public class DefaultPageHashGenerator : IPageHashGenerator
	{
		private readonly IEnumerable<IExtraPageHashGenerator> extraHashGenerators;

		public DefaultPageHashGenerator(IEnumerable<IExtraPageHashGenerator> extraHashGenerators = null)
		{
			this.extraHashGenerators = extraHashGenerators ?? Enumerable.Empty<IExtraPageHashGenerator>();
		}

		public string GetHash<TViewModel>(IDialogViewModel master, Type[] ctorTypes, object[] ctorValues)
			=> InternalGetHash(typeof(TViewModel), ctorValues);

		public string GetHashNamedArgs<TViewModel>(IDialogViewModel master, IDictionary<string, object> ctorArgs)
			=> InternalGetHash(typeof(TViewModel), ctorArgs.Values.ToArray());

		private string InternalGetHash(Type typeViewModel, object[] ctorValues)
		{
			string hash = null;
			foreach(var generator in extraHashGenerators) {
				hash = generator.GetHash(typeViewModel, ctorValues);
				if(hash != null)
					break;
			}

			if(hash == null)
				hash = typeViewModel.FullName + ParametersHash(ctorValues);

			return hash.Contains("~") ? null : hash;
		}

		private static string ParametersHash(object[] ctorValues)
		{
			var paramHash = new StringBuilder();
			foreach(var ctorArg in ctorValues) {
				if(ctorArg is IEntityUoWBuilder uowBuilder)
					paramHash.Append(uowBuilder.IsNewEntity ? "~" : $"#{uowBuilder.EntityOpenId}");

				if(ctorArg is Type type)
					paramHash.Append($"#{type.Name}");
			}
			return paramHash.ToString();
		}
	}
}
