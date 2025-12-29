namespace QS.Permissions
{
	public interface IPermissionResult
	{
		bool CanCreate 	{ get; }
		bool CanRead 	{ get; }
		bool CanUpdate 	{ get; }
		bool CanDelete 	{ get; }
	}
}
