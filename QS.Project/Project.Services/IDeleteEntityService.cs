using System;
namespace QS.Project.Services
{
	public interface IDeleteEntityService
	{
		bool DeleteEntity<TEntity>(int id);
		bool DeleteEntity(Type clazz, int id);
	}
}
