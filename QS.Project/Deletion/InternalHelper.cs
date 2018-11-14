using System;
using System.Data;

namespace QS.Deletion
{
	public static class InternalHelper
	{
		internal static void AddParameterWithId(IDbCommand cmd, uint id)
		{
			var parameterId = cmd.CreateParameter();
			parameterId.ParameterName = "@id";
			parameterId.Value = id;
			cmd.Parameters.Add(parameterId);
		}
	}
}
