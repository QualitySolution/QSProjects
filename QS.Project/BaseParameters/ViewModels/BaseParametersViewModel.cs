using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace QS.BaseParameters.ViewModels
{
	public class BaseParametersViewModel : DialogViewModelBase
	{
		private readonly ParametersService service;
		public readonly List<ParameterValue> ParameterValues;

		public BaseParametersViewModel(INavigationManager navigation, ParametersService service) : base(navigation)
		{
			this.service = service;
			ParameterValues = service.All.Select(p => new ParameterValue { Name = p.Key, ValueStr = (string)p.Value }).ToList();
		}

		#region Действия View
		public void AddParameter()
		{
			ParameterValues.Add(new ParameterValue { Name = "Новый параметр", ValueStr = "Значение" });
		}

		public void RemoveParameter(ParameterValue parameter)
		{
			ParameterValues.Remove(parameter);
		}

		public void Save()
		{
			foreach (var parameter in ParameterValues) {
				service.UpdateParameter(parameter.Name, parameter.ValueStr);
			}
			foreach (var pair in service.All) {
				if (ParameterValues.All(x => x.Name != pair.Key))
					service.RemoveParameter(pair.Key);
			}
		}
		#endregion
	}

	public class ParameterValue : PropertyChangedBase
	{
		private string name;
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		private string valueStr;
		public virtual string ValueStr {
			get => valueStr;
			set => SetField(ref valueStr, value);
		}
	}
}
