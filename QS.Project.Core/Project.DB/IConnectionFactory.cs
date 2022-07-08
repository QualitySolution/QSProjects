using System;
using System.Data.Common;

namespace QS.Project.DB
{
	public interface IConnectionFactory
	{
		DbConnection OpenConnection();
	}
}
