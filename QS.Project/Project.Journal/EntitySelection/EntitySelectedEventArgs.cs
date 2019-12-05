using System;
namespace QS.Project.Journal.EntitySelection
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
