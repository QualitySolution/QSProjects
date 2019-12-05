using System;
namespace QS.ViewModels.Control
{
	public interface IPropertyBinder<TProperty>
	{
		TProperty PropertyValue { get; set; }
		event EventHandler Changed;
	}
}
