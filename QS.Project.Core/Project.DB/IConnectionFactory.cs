using System;
using System.Data.Common;

namespace QS.Project.DB
{
	/// <summary>
	/// Подумать возможно требует удаления так как интерфейс достаточно бестолковый м напрямую нигде не используется.
	/// </summary>
	public interface IConnectionFactory
	{
		DbConnection OpenConnection();
	}
}
