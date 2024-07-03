using System;
namespace QS.Project.Versioning.Product
{
	public interface IProductService
	{
		string CurrentEditionName { get; }

		string GetEditionName(int editionId);
	}
}
