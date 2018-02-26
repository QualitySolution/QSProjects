using System;
namespace QSOrmProject.Permissions
{
	public interface IPermissionMatrix
	{
		string Title { get; }
		string DBField { get; }

		string GetJson();
		void ParseJson(string json);

		int PermissionCount {get;}
		string[] PermissionNames { get; }

		int ColumnCount { get; }
		string[] ColumnNames { get; }

		bool this[int permissionIx, int columnIx] { get; set; }
	}
}
