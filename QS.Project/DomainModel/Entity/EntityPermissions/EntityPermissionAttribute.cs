using System;
namespace QS.DomainModel.Entity.EntityPermissions
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class EntityPermissionAttribute : Attribute
	{
		public EntityPermissionAttribute()
		{
		}
	}
}
