﻿using System;
namespace QS.ViewModels.Control
{
	public interface IPropertyBinder<TProperty> : IDisposable
	{
		TProperty PropertyValue { get; set; }
		event EventHandler Changed;
	}
}
