using System;
using QS.Tdi;

namespace QS.DomainModel.Config
{
	public interface IEntityConfig
	{
		Type EntityClass { get; }

		bool? DefaultUseSlider { get; }
		bool SimpleDialog { get; }

		ITdiDialog CreateDialog(params object[] parameters);

		event EventHandler<EntityUpdatedEventArgs> EntityUpdated;
	}
}
