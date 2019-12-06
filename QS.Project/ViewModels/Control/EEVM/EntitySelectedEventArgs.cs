using System;
namespace QS.ViewModels.Control.EEVM
{
	public class EntitySelectedEventArgs : EventArgs
	{
		public object Entity;

		public EntitySelectedEventArgs(object entity)
		{
			Entity = entity;
		}
	}
}
