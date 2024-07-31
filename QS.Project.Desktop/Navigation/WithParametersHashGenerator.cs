using System;
using System.Collections.Generic;
using System.Linq;
using QS.ViewModels;

namespace QS.Navigation
{
	/// <summary>
	/// Генератор позволяет создавать диалоги принимающие дополнительные параметры, которые должны использоваться для формирования хеша.
	/// </summary>
	public class WithParametersHashGenerator : IExtraPageHashGenerator
	{
		#region Настройка
		
		private readonly List<ViewModelParametersConfig> viewModelsConfigs = new List<ViewModelParametersConfig>();

		public ViewModelParametersConfig Configure<TViewModel>() where TViewModel : ViewModelBase {
			var config = new ViewModelParametersConfig(this, typeof(TViewModel));
			viewModelsConfigs.Add(config);
			return config;
		}

		#endregion 
		public string GetHash(Type typeViewModel, object[] ctorValues)
		{
			foreach(var config in viewModelsConfigs) {
				if(config.ViewModelType == typeViewModel) {
					var hashParts = new List<string>();
					foreach(var paramConfig in config.Parameters) {
						var paramValue = ctorValues.FirstOrDefault(x => paramConfig.IsMatch(x));
						if(paramValue == null)
							break;
						hashParts.Add(paramConfig.GetHashPart(paramValue));
					}
					if(hashParts.Count == config.Parameters.Count)
						return $"{typeViewModel.FullName}[{string.Join(";", hashParts)}]";
				}
			}
			return null;
		}
	}
	
	public class ViewModelParametersConfig
	{
		private readonly WithParametersHashGenerator generator;

		public ViewModelParametersConfig(WithParametersHashGenerator generator, Type viewModelType) {
			this.generator = generator ?? throw new ArgumentNullException(nameof(generator));
			ViewModelType = viewModelType ?? throw new ArgumentNullException(nameof(viewModelType));
		}

		public Type ViewModelType { get; }

		public readonly List<IParameterConfig> Parameters = new List<IParameterConfig>();

		#region Настройка
		public ViewModelParametersConfig AddParameter<TParameter>(Func<TParameter,string> hashPartFunc) {
			var param = new ParameterConfig<TParameter>(hashPartFunc);
			Parameters.Add(param);
			return this;
		}
		
		public ViewModelParametersConfig Configure<TViewModel>() where TViewModel : ViewModelBase {
			return generator.Configure<TViewModel>();
		}
		
		public WithParametersHashGenerator End() => generator;
		#endregion
	}
	
	public class ParameterConfig<TParameter> : IParameterConfig {
		public ParameterConfig(Func<TParameter, string> hashPartFunc) {
			this.hashPartFunc = hashPartFunc ?? throw new ArgumentNullException(nameof(hashPartFunc));
		}
		
		private readonly Func<TParameter, string> hashPartFunc;
		
		public bool IsMatch(object param) => param is TParameter;
		public string GetHashPart(object param) =>$"{typeof(TParameter).Name}={hashPartFunc((TParameter)param)}";
	}
	
	public interface IParameterConfig {
		bool IsMatch(object param);
		string GetHashPart(object param);
	}
}
